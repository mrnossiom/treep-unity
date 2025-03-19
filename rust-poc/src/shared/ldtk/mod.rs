//! Custom `LDtk` Asset loader for procedural generation
//!
//! `LDtk` Terms to Treep terms
//! - `World` → `Level`
//! - `Level` → `Room`

use bevy::{log, prelude::*, utils::HashMap};
use std::str::FromStr;

/// Automatically generated from the ldtk.io JSON Schema.
/// Taken from <https://ldtk.io/files/quicktype/LdtkJson.rs>.
///
/// Entrypoint is [`LdtkJson`](types::LdtkJson).
#[allow(
	clippy::all,
	clippy::pedantic,
	clippy::restriction,
	clippy::nursery,
	rustdoc::all
)]
mod types;

mod loader;

pub struct LdtkPlugin;

impl Plugin for LdtkPlugin {
	fn build(&self, app: &mut App) {
		app.init_asset::<LevelCollection>()
			.register_asset_loader(loader::LdtkLoader);
	}
}
#[derive(Debug, TypePath, Asset)]
pub struct LevelCollection {
	pub levels: Vec<Level>,
}

#[derive(Debug)]
pub struct Level {
	/// Unique identifier
	iid: String,
	/// Arbitrary name
	identifier: String,
	/// Level rooms separated by type
	rooms: HashMap<RoomType, Vec<Room>>,

	/// Background image layers for paralax
	background: Vec<()>,
}

impl TryFrom<types::World> for Level {
	type Error = &'static str;
	fn try_from(world: types::World) -> Result<Self, Self::Error> {
		let mut rooms = HashMap::<RoomType, Vec<Room>>::new();

		for room in world.levels {
			let level_kind_field = room
				.field_instances
				.iter()
				.find(|field| field.identifier == "level_kind")
				.expect("`level_kind` should be present");

			let level_kind = level_kind_field.value.clone().unwrap();

			rooms
				.entry(level_kind.try_into()?)
				.or_default()
				.push(room.try_into()?);
		}

		Ok(Self {
			iid: world.iid,
			identifier: world.identifier,
			rooms,
			// TODO
			background: vec![],
		})
	}
}

#[derive(Debug, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub enum RoomType {
	Lobby,
	Spawn,
	Normal,
	Shop,
	Boss,
	Exit,
}

impl TryFrom<serde_json::Value> for RoomType {
	type Error = &'static str;
	fn try_from(value: serde_json::Value) -> Result<Self, Self::Error> {
		if let serde_json::Value::String(s) = value {
			Self::from_str(&s)
		} else {
			Err("encountered a non valid type for room type")
		}
	}
}

impl FromStr for RoomType {
	type Err = &'static str;
	fn from_str(s: &str) -> Result<Self, Self::Err> {
		match s {
			"lobby" => Ok(Self::Lobby),
			"spawn" => Ok(Self::Spawn),
			"normal" => Ok(Self::Normal),
			"shop" => Ok(Self::Shop),
			"boss" => Ok(Self::Boss),
			"exit" => Ok(Self::Exit),
			_ => Err("encountered a non valid room type"),
		}
	}
}

#[derive(Debug)]
pub struct Room {
	/// Unique identifier
	uid: i64,
	/// Arbitrary name
	identifier: String,

	layer_instances: Vec<types::LayerInstance>,

	height: u64,
	width: u64,
}

impl TryFrom<types::Level> for Room {
	type Error = &'static str;
	fn try_from(level: types::Level) -> Result<Self, Self::Error> {
		Ok(Self {
			uid: level.uid,
			identifier: level.identifier,
			layer_instances: level
				.layer_instances
				.ok_or("`layer_instances` should be present")?,
			height: u64::try_from(level.px_hei).map_err(|_| "`px_hei` is negative")?,
			width: u64::try_from(level.px_wid).map_err(|_| "`px_wid` is negative")?,
		})
	}
}
