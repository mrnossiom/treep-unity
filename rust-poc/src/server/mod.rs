//! A server is always in play mode.
//!
//! Start in lobby state.

use crate::shared;
use bevy::prelude::*;
use bevy_rapier2d::prelude::*;

pub fn plugin(app: &mut App) {
	app
		// Simulate physics
		.add_plugins(RapierPhysicsPlugin::<NoUserData>::pixels_per_meter(64.))
		// Game
		.add_plugins((
			shared::ground::plugin,
			shared::level::plugin,
			shared::player::plugin,
			shared::room::plugin,
		));
}
