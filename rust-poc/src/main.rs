use bevy::prelude::*;
use bevy::winit::{UpdateMode, WinitSettings};
use bevy_aseprite_ultra::prelude::*;
use bevy_inspector_egui::quick::WorldInspectorPlugin;
use bevy_rapier2d::prelude::*;

mod level;
mod main_menu;
mod player;
mod room;
mod utils;

fn main() {
	build_game().run();
}

fn build_game() -> App {
	let mut app = App::new();

	app.add_plugins(
		DefaultPlugins
			// Display pixel art with sharp edges
			.set(ImagePlugin::default_nearest())
			.set(WindowPlugin {
				primary_window: Some(Window {
					title: "Treep".into(),
					..default()
				}),
				..default()
			}),
	)
	.insert_resource(WinitSettings {
		focused_mode: UpdateMode::Continuous,
		unfocused_mode: UpdateMode::Continuous,
	})
	// Load Aseprite files
	.add_plugins(AsepriteUltraPlugin)
	// Simulate physics
	.add_plugins(RapierPhysicsPlugin::<NoUserData>::pixels_per_meter(16.))
	.add_plugins(RapierDebugRenderPlugin::default())
	// Treep plugins
	.add_systems(Startup, setup_camera)
	.register_type::<GameState>()
	.init_state::<GameState>()
	.add_plugins((
		main_menu::plugin,
		room::plugin,
		level::plugin,
		player::plugin,
	));

	// Debug plugins
	if cfg!(debug_assertions) {
		app.add_plugins(WorldInspectorPlugin::default());
	}

	app
}

fn setup_camera(mut commands: Commands) {
	commands.spawn(Camera2d);
}

#[derive(Debug, Clone, Copy, Default, PartialEq, Eq, Hash, Reflect, States)]
enum GameState {
	// Splash?
	#[default]
	Menu,

	InRoom,
}
