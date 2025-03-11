use bevy::prelude::*;
use bevy_aseprite_ultra::prelude::*;
use bevy_inspector_egui::quick::{StateInspectorPlugin, WorldInspectorPlugin};
use bevy_rapier2d::prelude::*;
use bevy_replicon::RepliconPlugins;
use bevy_replicon_quinnet::RepliconQuinnetPlugins;

mod ground;
mod level;
mod main_menu;
mod player;
mod room;
mod utils;

pub use ground::plugin as ground_plugin;
pub use level::plugin as level_plugin;
pub use main_menu::plugin as main_menu_plugin;
pub use player::plugin as player_plugin;
pub use room::plugin as room_plugin;

pub const DEBUG_MODE: bool = cfg!(feature = "debug");

#[derive(Debug, Clone, Copy, Default, PartialEq, Eq, Hash, Reflect, States)]
pub enum GameState {
	// Splash?
	#[default]
	Menu,

	InRoom,
}

fn setup_camera(mut commands: Commands) {
	commands.spawn(Camera2d);
}

pub struct TreepPlugin;

impl Plugin for TreepPlugin {
	fn build(&self, app: &mut App) {
		app
			// Aseprite files loader
			.add_plugins(AsepriteUltraPlugin)
			// Simulate physics
			.add_plugins(RapierPhysicsPlugin::<NoUserData>::pixels_per_meter(64.))
			// Networking
			.add_plugins(RepliconPlugins)
			.add_plugins(RepliconQuinnetPlugins)
			// Treep plugins
			.add_systems(Startup, setup_camera)
			.register_type::<GameState>()
			.init_state::<GameState>()
			.add_plugins((
				main_menu_plugin,
				room_plugin,
				level_plugin,
				player_plugin,
				ground_plugin,
			));

		if DEBUG_MODE {
			// Debug plugins
			app.add_plugins(WorldInspectorPlugin::default())
				.add_plugins(StateInspectorPlugin::<GameState>::default())
				.add_plugins(RapierDebugRenderPlugin::default());
		}
	}
}
