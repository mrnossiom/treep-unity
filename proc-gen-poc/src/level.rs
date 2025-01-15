use crate::room::Kind;
use petgraph::{graph::NodeIndex, Graph};
use std::fmt;

pub type Blueprint = Graph<Room, Connection>;

/// This represent a room instance in the [`LevelBlueprint`]
#[derive(Debug)]
pub struct Room {
	pub(crate) name: &'static str,

	pub(crate) kind: Kind,
}

impl fmt::Display for Room {
	fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
		let Self { name, kind } = self;
		writeln!(f, r#"🥡 "{name}" {kind:?}"#)
	}
}

#[derive(Debug)]
pub struct Connection {}

impl fmt::Display for Connection {
	fn fmt(&self, _: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
		Ok(())
	}
}

impl Room {
	const fn new(name: &'static str, kind: Kind) -> Self {
		Self { name, kind }
	}
}

pub mod blueprints {
	use super::{Blueprint, Connection, Kind, NodeIndex, Room};

	pub fn basic_level() -> (Blueprint, NodeIndex) {
		let spawn = Room::new("spawn-1", Kind::Spawn);
		let exit = Room::new("exit-1", Kind::Exit);

		let corridor_1 = Room::new("corridor-1", Kind::Normal);
		let corridor_2 = Room::new("corridor-2", Kind::Normal);

		let mut graph = Blueprint::new();

		let spawn = graph.add_node(spawn);
		let exit = graph.add_node(exit);
		let corridor_1 = graph.add_node(corridor_1);
		let corridor_2 = graph.add_node(corridor_2);

		graph.add_edge(spawn, corridor_1, Connection {});
		graph.add_edge(corridor_1, corridor_2, Connection {});
		graph.add_edge(corridor_2, exit, Connection {});

		(graph, spawn)
	}
}
