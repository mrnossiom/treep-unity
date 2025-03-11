//! Player

use crate::{GameState, ground::GroundDetection, utils::despawn_screen};
use bevy::prelude::*;
use bevy_aseprite_ultra::prelude::*;
use bevy_rapier2d::prelude::*;

pub fn plugin(app: &mut App) {
	app.register_type::<GroundDetection>()
		// Setup and cleanup on change game state
		.add_systems(OnEnter(GameState::InRoom), (setup_player,))
		.add_systems(OnExit(GameState::InRoom), (despawn_screen::<Player>,))
		.add_systems(
			Update,
			(player_movement,).run_if(in_state(GameState::InRoom)),
		);
}

const PLAYER_RUN_SPEED: f32 = 200.0;
const PLAYER_GROUND_SWITCH_COEF: f32 = 0.8;
const PLAYER_AIR_SWITCH_COEF: f32 = 0.1;
const PLAYER_FALLING_GAVITY_SCALE: f32 = 1.8;

#[derive(Component)]
#[require(Name(|| Name::new("Player")))]
pub(crate) struct Player;

pub(crate) fn setup_player(mut commands: Commands, asset_server: Res<AssetServer>) {
	commands.spawn((
		Player,
		// sprite
		AseSpriteAnimation {
			animation: Animation::tag("walk-left"),
			aseprite: asset_server.load("ant-run.aseprite"),
		},
		// position and physics
		Transform::default(),
		Velocity::zero(),
		LockedAxes::ROTATION_LOCKED,
		Friction {
			coefficient: 0.,
			combine_rule: CoefficientCombineRule::Min,
		},
		// controller
		Collider::cuboid(10., 30.),
		RigidBody::Dynamic,
		GravityScale::default(),
		KinematicCharacterController::default(),
		GroundDetection::default(),
		Ccd::enabled(),
	));
}

pub fn player_movement(
	input: Res<ButtonInput<KeyCode>>,
	mut query: Query<
		(
			&mut Velocity,
			&mut GravityScale,
			// &mut Climber,
			&GroundDetection,
		),
		With<Player>,
	>,
) {
	for (mut velocity, mut gravity_scale, GroundDetection { on_ground }) in &mut query {
		let right = if input.pressed(KeyCode::KeyD) { 1. } else { 0. };
		let left = if input.pressed(KeyCode::KeyA) { 1. } else { 0. };

		let target_speed = (right - left) * PLAYER_RUN_SPEED;
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

		if input.just_pressed(KeyCode::Space)
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
}
