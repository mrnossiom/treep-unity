use bevy::prelude::*;

pub(crate) mod ground;
pub(crate) mod level;
pub(crate) mod player;
pub(crate) mod room;

#[derive(Debug, Clone, Copy, Default, PartialEq, Eq, Hash, Reflect, States)]
pub(crate) enum RoomState {
	Lobby,

	Level,

	#[default]
	Disabled,
}
