use crate::room::RoomKind;
use petgraph::graph::UnGraph;

pub(crate) type LevelBlueprint = UnGraph<Room, Connection>;

/// This represent a room instance in the [`LevelGraph`]
#[derive(Debug)]
pub(crate) struct Room {
	pub(crate) name: &'static str,

	// corners: Vec<Vec2>,
	pub(crate) kind: RoomKind,
}

#[derive(Debug)]
pub(crate) struct Connection {}

impl Room {
	fn new(name: &'static str, kind: RoomKind) -> Self {
		Self { name, kind }
	}
}

pub(crate) mod blueprints {
	use super::*;

	/// Simple level graph
	///
	/// *(BEGIN)* `spawn` → `one` → `two` → `three` → `boss` → `exit` *(STOP)*
	pub(crate) fn full_level_graph() -> LevelBlueprint {
		let spawn_room = Room::new("spawn", RoomKind::Spawn);
		let one_room = Room::new("room-1", RoomKind::Normal);
		let two_room = Room::new("room-2", RoomKind::Normal);
		let three_room = Room::new("room-3", RoomKind::Normal);
		let boss_room = Room::new("boss", RoomKind::Boss);
		let exit_room = Room::new("exit", RoomKind::Exit);

		let mut graph = LevelBlueprint::new_undirected();

		let spawn_room = graph.add_node(spawn_room);
		let one_room = graph.add_node(one_room);
		let two_room = graph.add_node(two_room);
		let three_room = graph.add_node(three_room);
		let boss_room = graph.add_node(boss_room);
		let exit_room = graph.add_node(exit_room);

		graph.add_edge(spawn_room, one_room, Connection {});
		graph.add_edge(one_room, two_room, Connection {});
		graph.add_edge(two_room, three_room, Connection {});
		graph.add_edge(two_room, boss_room, Connection {});
		graph.add_edge(boss_room, exit_room, Connection {});

		graph
	}

	/// Basic level graph
	///
	/// *(BEGIN)* `room-1` → `room-2` *(STOP)*
	pub(crate) fn basic_level_graph() -> LevelBlueprint {
		let one_room = Room::new("room-1", RoomKind::Normal);
		let two_room = Room::new("room-2", RoomKind::Normal);

		let mut graph = LevelBlueprint::new_undirected();

		let one_room = graph.add_node(one_room);
		let two_room = graph.add_node(two_room);

		graph.add_edge(one_room, two_room, Connection {});

		graph
	}
}
