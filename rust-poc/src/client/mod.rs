use crate::cli::DEBUG_LEVEL;
use bevy::{
	diagnostic::{FrameTimeDiagnosticsPlugin, LogDiagnosticsPlugin},
	prelude::*,
};
use bevy_aseprite_ultra::prelude::*;
use bevy_inspector_egui::quick::{StateInspectorPlugin, WorldInspectorPlugin};
use bevy_rapier2d::prelude::*;

mod main_menu;

#[derive(Debug, Clone, Copy, Default, PartialEq, Eq, Hash, Reflect, States)]
enum GameState {
	// TODO: not implemented
	Splash,

	#[default]
	Menu,

	InRoom,
}

pub fn plugin(app: &mut App) {
	app
		// Aseprite files loader
		.add_plugins(AsepriteUltraPlugin)
		// Simulate physics
		.add_plugins(RapierPhysicsPlugin::<NoUserData>::pixels_per_meter(64.))
		// Game plugins
		.init_state::<GameState>()
		.enable_state_scoped_entities::<GameState>()
		.add_plugins((
			main_menu::plugin,
			// shared::room::plugin,
			// shared::level::plugin,
			// shared::player::plugin,
			// shared::ground::plugin,
		));

	// Debug plugins
	if let Some(level) = DEBUG_LEVEL.get().copied() {
		if level >= 1 {
			app.add_plugins(WorldInspectorPlugin::default())
				.add_plugins(LogDiagnosticsPlugin::default())
				.add_plugins(FrameTimeDiagnosticsPlugin);
		}
		if level >= 2 {
			app.add_plugins(RapierDebugRenderPlugin::default());
		}
		if level >= 3 {
			app.add_plugins(StateInspectorPlugin::<GameState>::default());
		}
	}
}
