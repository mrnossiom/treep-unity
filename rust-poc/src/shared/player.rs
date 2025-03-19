//! Player

use crate::shared::ground::GroundDetection;
use bevy::prelude::*;
use bevy_aseprite_ultra::prelude::*;
use bevy_rapier2d::prelude::*;
use bevy_replicon::prelude::*;
use serde::{Deserialize, Serialize};

pub fn plugin(app: &mut App) {
	app
		// networking
		.replicate::<NetworkPlayer>()
		.replicate::<Transform>()
		// .replicate::<AnimationState>()
		.add_client_trigger::<MovePlayer>(ChannelKind::Ordered)
		.add_observer(apply_player_movement)
		.add_observer(spawn_clients)
		// player
		.register_type::<GroundDetection>();
}

const PLAYER_RUN_SPEED: f32 = 200.0;
const PLAYER_GROUND_SWITCH_COEF: f32 = 0.8;
const PLAYER_AIR_SWITCH_COEF: f32 = 0.1;
const PLAYER_FALLING_GAVITY_SCALE: f32 = 1.8;

const NO_FRICTION: Friction = Friction {
	coefficient: 0.0,
	combine_rule: CoefficientCombineRule::Min,
};

#[derive(Component, Default)]
#[require(Player)]
// physics
#[require(
	Velocity,
	LockedAxes(|| LockedAxes::ROTATION_LOCKED),
	Friction(|| NO_FRICTION),
	Collider(|| Collider::cuboid(10., 30.)),
	RigidBody(|| RigidBody::Dynamic),
	Ccd(Ccd::enabled),
	GravityScale,
)]
// controller
#[require(GroundDetection)]
pub(crate) struct LocalPlayer;

#[derive(Component, Deref, Reflect, Serialize, Deserialize)]
#[require(Player, Replicated)]
pub(crate) struct NetworkPlayer(pub Entity);

#[derive(Component, Reflect, Serialize, Deserialize, Default)]
#[require(Name(|| Name::new("Player")))]
#[require(Transform)]
// update animation locally based on lightweight player animation state
#[require(AseSpriteAnimation)]
pub(crate) struct Player;

/// Spawns a new box whenever a client connects.
fn spawn_clients(trigger: Trigger<OnAdd, ConnectedClient>, mut commands: Commands) {
	info!("spawning box for `{:?}`", trigger.entity());
	commands.spawn((LocalPlayer, NetworkPlayer(trigger.entity())));
}

fn apply_player_movement(
	trigger: Trigger<FromClient<MovePlayer>>,
	mut players: Query<(
		&NetworkPlayer,
		&mut Velocity,
		&mut GravityScale,
		// &mut Climber,
		&GroundDetection,
	)>,
) {
	// find the sender entity. we don't include the entity as a trigger target to save traffic, since the server knows
	// which entity to apply the input to. we could have a resource that maps connected clients to controlled entities,
	// but we didn't implement it for the sake of simplicity.
	let (_player, mut velocity, mut gravity_scale, GroundDetection { on_ground }) = players
		.iter_mut()
		.find(|(player, _, _, _)| ***player == trigger.client_entity)
		.unwrap();

	let target_speed = trigger.event.horizontal_delta * PLAYER_RUN_SPEED;
	velocity.linvel.x = velocity.linvel.x.lerp(
		target_speed,
		if *on_ground {
			PLAYER_GROUND_SWITCH_COEF
		} else {
			PLAYER_AIR_SWITCH_COEF
		},
	);

	// if climber.intersecting_climbables.is_empty() {
	// 	climber.climbing = false;
	// } else if input.just_pressed(KeyCode::KeyW) || input.just_pressed(KeyCode::KeyS) {
	// 	climber.climbing = true;
	// }

	// if climber.climbing {
	// 	let up = if input.pressed(KeyCode::KeyW) { 1. } else { 0. };
	// 	let down = if input.pressed(KeyCode::KeyS) { 1. } else { 0. };

	// 	velocity.linvel.y = (up - down) * 200.;
	// }

	if trigger.event.jumped
		&& (
			*on_ground
			// || climber.climbing
		) {
		velocity.linvel.y = 200.;
		// climber.climbing = false;
	}

	if velocity.linvel.y < 0.0 {
		gravity_scale.0 = PLAYER_FALLING_GAVITY_SCALE;
	} else {
		gravity_scale.0 = 1.0;
	}
}

/// A movement event for the controlled box.
#[derive(Event, Serialize, Deserialize)]
struct MovePlayer {
	horizontal_delta: f32,
	jumped: bool,
}
