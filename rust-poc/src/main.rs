//! Treep by *Kromm Studio*

use bevy::{
	app::ScheduleRunnerPlugin,
	diagnostic::DiagnosticsPlugin,
	log::{self, LogPlugin},
	prelude::*,
	state::app::StatesPlugin,
	winit::WinitSettings,
};
use bevy_replicon::{
	client::ClientPlugin,
	core::RepliconCorePlugin,
	prelude::{ClientEventPlugin, ServerEventPlugin},
	server::ServerPlugin,
};
use bevy_replicon_quinnet::{
	client::RepliconQuinnetClientPlugin, server::RepliconQuinnetServerPlugin,
};
use std::{process::Termination, time::Duration};
use treep::cli::{Args, Mode};

fn main() -> impl Termination {
	let args = Args::parse_and_set_globals();

	let mut app = App::new();

	match args.mode.unwrap_or_default() {
		#[cfg(feature = "client")]
		Mode::Client => client_plugins(&mut app),
		#[cfg(feature = "server")]
		Mode::Server => server_plugins(&mut app),
	};

	let exit = app.run();

	log::info!("Bye!");

	exit
}

const TICKS_PER_SECOND: u64 = 30;

#[cfg(feature = "client")]
fn client_plugins(app: &mut App) -> &mut App {
	app
		// Bevy plugins
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
		// TODO: it does not work
		// render: do not stop networking on focus lost
		.insert_resource(WinitSettings::game())
		// Networking
		.add_plugins((
			RepliconCorePlugin,
			ClientPlugin,
			ClientEventPlugin,
			// transport
			RepliconQuinnetClientPlugin,
		))
		// Game
		.add_plugins(treep::client::plugin)
}

#[cfg(feature = "server")]
fn server_plugins(app: &mut App) -> &mut App {
	let schedule_runner_plugin =
		ScheduleRunnerPlugin::run_loop(Duration::from_secs_f64(1.0 / TICKS_PER_SECOND as f64));

	app
		// Bevy Plugins
		.add_plugins((
			MinimalPlugins.set(schedule_runner_plugin),
			LogPlugin::default(),
			StatesPlugin,
			HierarchyPlugin,
			DiagnosticsPlugin,
			// for ldtk levels
			AssetPlugin::default(),
			ImagePlugin::default_nearest(),
		))
		// Networking
		.add_plugins((
			RepliconCorePlugin,
			ServerPlugin::default(),
			ServerEventPlugin,
			// transport
			RepliconQuinnetServerPlugin,
		))
		// Game
		.add_plugins(treep::server::plugin)
}
