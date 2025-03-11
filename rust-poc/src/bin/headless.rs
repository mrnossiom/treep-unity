//! Headless Server for Treep by *Kromm Studio*

use bevy::prelude::*;
use bevy::winit::WinitSettings;
use treep::TreepPlugin;

fn main() {
	App::new()
		.add_plugins(MinimalPlugins)
		// Do not stop networking on lost focus
		.insert_resource(WinitSettings::game())
		// Game plugin
		.add_plugins(TreepPlugin)
		.run();
}
