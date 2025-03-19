mod cli;
#[cfg(feature = "client")]
mod client;
#[cfg(feature = "server")]
mod server;
mod shared;

pub use cli::{Args, Mode};
#[cfg(feature = "client")]
pub use client::plugin as client_plugin;
#[cfg(feature = "server")]
pub use server::plugin as server_plugin;
