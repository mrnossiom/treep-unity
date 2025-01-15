use crate::room::RoomKind;
use core::fmt;
use petgraph::{graph::NodeIndex, Graph};

pub(crate) type LevelBlueprint = Graph<Room, Connection>;

/// This represent a room instance in the [`LevelBlueprint`]
#[derive(Debug)]
pub(crate) struct Room {
	pub(crate) name: &'static str,

	pub(crate) kind: RoomKind,
}

impl fmt::Display for Room {
	fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
		let Self { name, kind } = self;
		writeln!(f, r#"🥡 "{name}" {kind:?}"#)
	}
}

#[derive(Debug)]
pub(crate) struct Connection {}

impl fmt::Display for Connection {
	fn fmt(&self, _: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
		Ok(())
	}
}

impl Room {
	fn new(name: &'static str, kind: RoomKind) -> Self {
		Self { name, kind }
	}
}

pub(crate) mod blueprints {
	use super::*;

	/// Basic level graph
	///
	/// *(BEGIN)* `room-1` → `room-2` *(STOP)*
	pub(crate) fn basic_level_graph() -> (LevelBlueprint, NodeIndex) {
		let spawn = Room::new("spawn-1", RoomKind::Spawn);
		let exit = Room::new("exit-1", RoomKind::Exit);

		let corridor_1 = Room::new("corridor-1", RoomKind::Normal);
		let corridor_2 = Room::new("corridor-2", RoomKind::Normal);

		let mut graph = LevelBlueprint::new();

		let spawn = graph.add_node(spawn);
		let exit = graph.add_node(exit);
		let corridor_1 = graph.add_node(corridor_1);
		let corridor_2 = graph.add_node(corridor_2);

		graph.add_edge(spawn, corridor_1, Connection {});
		graph.add_edge(corridor_1, corridor_2, Connection {});
		graph.add_edge(corridor_2, exit, Connection {});

		(graph, spawn)
	}

	/// More advanced level graph
	///
	// / *(BEGIN)* `room-1` → `room-2` *(STOP)*
	pub(crate) fn next_level_graph() -> (LevelBlueprint, NodeIndex) {
		let spawn = Room::new("spawn-1", RoomKind::Spawn);
		let exit = Room::new("exit-1", RoomKind::Exit);

		let normal_1 = Room::new("normal-1", RoomKind::Normal);
		let normal_2 = Room::new("normal-2", RoomKind::Normal);
		let normal_3 = Room::new("normal-3", RoomKind::Normal);
		let normal_4 = Room::new("normal-4", RoomKind::Normal);

		let mut graph = LevelBlueprint::new();

		let spawn = graph.add_node(spawn);
		let exit = graph.add_node(exit);
		let normal_1 = graph.add_node(normal_1);
		let normal_2 = graph.add_node(normal_2);
		let normal_3 = graph.add_node(normal_3);
		let normal_4 = graph.add_node(normal_4);

		graph.add_edge(spawn, normal_1, Connection {});
		graph.add_edge(normal_1, normal_2, Connection {});
		graph.add_edge(normal_2, normal_3, Connection {});
		graph.add_edge(normal_3, normal_4, Connection {});
		graph.add_edge(normal_4, exit, Connection {});
		// graph.add_edge(normal_3, exit, Connection {});

		(graph, spawn)
	}

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

		let mut graph = LevelBlueprint::new();

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
}
