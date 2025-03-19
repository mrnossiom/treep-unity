//! A server is always in play mode. Start in a room lobby state

use crate::shared;
use bevy::prelude::*;

pub fn plugin(app: &mut App) {
	app.add_plugins((
		shared::ground::plugin,
		shared::level::plugin,
		shared::player::plugin,
		shared::room::plugin,
	));
}
