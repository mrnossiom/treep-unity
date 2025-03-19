use clap::{ArgAction, Parser, ValueEnum};
use std::sync::OnceLock;

pub static DEBUG_LEVEL: OnceLock<u8> = OnceLock::new();

#[derive(Debug, Parser)]
pub struct Args {
	pub mode: Option<Mode>,

	#[clap(short, long, action = ArgAction::Count)]
	pub debug: u8,

	#[clap(long)]
	pub server_ui: bool,
}

impl Args {
	/// Parse CLI arguments and sets globals. Can only be called once.
	///
	/// # Panics
	///
	/// - If this is called twice.
	pub fn parse_and_set_globals() -> Self {
		let args = Self::parse();

		assert!(
			DEBUG_LEVEL.set(args.debug).is_ok(),
			"could not set `DEBUG_LEVEL`. maybe `parse_and_set_globals` was called twice?"
		);

		args
	}
}

/// Which mode to start the game in
#[derive(Debug, Clone, Default, ValueEnum)]
pub enum Mode {
	#[cfg(feature = "client")]
	#[default]
	Client,

	#[cfg(feature = "server")]
	#[cfg_attr(not(feature = "client"), default)]
	Server,
}
