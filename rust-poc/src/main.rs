//! Treep by *Kromm Studio*

use bevy::prelude::*;
use bevy::winit::WinitSettings;
use treep::TreepPlugin;

fn main() {
	App::new()
		.add_plugins(
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
		// Do not stop networking on lost focus
		.insert_resource(WinitSettings::game())
		// Game plugin
		.add_plugins(TreepPlugin)
		.run();
}
