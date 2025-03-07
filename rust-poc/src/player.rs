use bevy::{prelude::*, sprite::Anchor};
use bevy_aseprite_ultra::prelude::*;
use bevy_rapier2d::prelude::*;

use crate::GameState;

pub(crate) fn plugin(app: &mut App) {
	app
		// Setup and cleanup on change game state
		.add_systems(OnEnter(GameState::InRoom), (setup_player,))
		.add_systems(
			Update,
			(player_movement,).run_if(in_state(GameState::InRoom)),
		);
}

#[derive(Component)]
pub(crate) struct Player;

pub(crate) fn setup_player(mut commands: Commands, asset_server: Res<AssetServer>) {
	commands.spawn((
		Player,
		Name::new("Player"),
		AseSpriteAnimation {
			animation: Animation::tag("walk-left"),
			aseprite: asset_server.load("ant-run.aseprite"),
		},
		Sprite {
			anchor: Anchor::BottomCenter,
			..default()
		},
		Transform::default(),
		RigidBody::Dynamic,
		// RigidBody::KinematicVelocityBased,
		Velocity::zero(),
		Friction {
			coefficient: 0.0,
			combine_rule: CoefficientCombineRule::Min,
		},
		LockedAxes::ROTATION_LOCKED,
		Collider::cuboid(10., 20.),
	));
}

pub fn player_movement(
	input: Res<ButtonInput<KeyCode>>,
	mut query: Query<&mut Velocity, With<Player>>,
) {
	for mut velocity in &mut query {
		let right = if input.pressed(KeyCode::KeyD) { 1. } else { 0. };
		let left = if input.pressed(KeyCode::KeyA) { 1. } else { 0. };

		velocity.linvel.x = (right - left) * 200.;

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
		// && (ground_detection.on_ground || climber.climbing)
		{
			velocity.linvel.y = 150.;
			// climber.climbing = false;
		}
	}
}
