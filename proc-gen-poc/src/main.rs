use petgraph::dot::Dot;
use rand::{rngs::SmallRng, SeedableRng};

#[derive(Debug)]
struct Vec2 {
	pub x: u32,
	pub y: u32,
}

impl Vec2 {
	const fn new(x: u32, y: u32) -> Self {
		Self { x, y }
	}
}

#[derive(Debug)]
enum RoomKind {
	/// This should be the first room in a level graph
	Spawn,

	Normal,
	Reward,
	Shop,
	Boss,

	/// This should be the last room in a level graph
	Exit,
}

mod level_blueprint {
	use crate::RoomKind;
	use petgraph::graph::UnGraph;

	#[derive(Debug)]
	pub struct Room {
		name: &'static str,

		// corners: Vec<Vec2>,
		kind: RoomKind,
	}

	#[derive(Debug)]
	pub struct Connection {}

	pub type LevelGraph = UnGraph<Room, Connection>;

	impl Room {
		fn new(name: &'static str, kind: RoomKind) -> Self {
			Self { name, kind }
		}
	}

	/// Simple level graph
	///
	/// *(BEGIN)* `spawn` → `one` → `two` → `three` → `boss` → `exit` *(STOP)*
	pub fn full_level_graph() -> LevelGraph {
		let spawn_room = Room::new("spawn", RoomKind::Spawn);
		let one_room = Room::new("room-1", RoomKind::Normal);
		let two_room = Room::new("room-2", RoomKind::Normal);
		let three_room = Room::new("room-3", RoomKind::Normal);
		let boss_room = Room::new("boss", RoomKind::Boss);
		let exit_room = Room::new("exit", RoomKind::Exit);

		let mut graph = LevelGraph::new_undirected();

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
	/// *(BEGIN)* `spawn` → `one` → `two` → `three` → `boss` → `exit` *(STOP)*
	pub fn basic_level_graph() -> LevelGraph {
		let one_room = Room::new("room-1", RoomKind::Normal);
		let two_room = Room::new("room-2", RoomKind::Normal);

		let mut graph = LevelGraph::new_undirected();

		let one_room = graph.add_node(one_room);
		let two_room = graph.add_node(two_room);

		graph.add_edge(one_room, two_room, Connection {});

		graph
	}
}

mod room_blueprint {
	use crate::{RoomKind, Vec2};

	#[derive(Debug)]
	enum RoomShape {
		Rectangle(Vec2),
	}

	#[derive(Debug)]
	pub struct Door {
		pos: Vec2,
		end: Vec2,
	}

	impl Door {
		pub const fn new(pos: Vec2, end: Vec2) -> Self {
			Self { pos, end }
		}
	}

	#[derive(Debug)]
	pub struct Room<'a> {
		name: &'a str,
		shape: RoomShape,
		doors: &'a [Door],
		// name? kind? allowed_transformations?
	}

	impl<'a> Room<'a> {
		const fn new(name: &'a str, shape: RoomShape, doors: &'a [Door]) -> Self {
			Self { name, shape, doors }
		}
	}

	// const SPAWN_ROOM: Room = Room::new("spawn-1", (), ());
	// const BOSS_ROOM: Room = Room::new("boss-1", (), ());
	// const EXIT_ROOM: Room = Room::new("exit-1", (), ());

	/// # Caracteristics
	///
	/// - Size: `(8,5)`
	/// - Doors:
	///   - `(8,3)-(8,4)`
	///
	/// # Preview
	/// ```text
	/// 0WWWWWWWW
	/// W       W
	/// W       W
	/// W       D
	/// W       D
	/// WWWWWWWWW
	/// ```
	const BASIC_1_ROOM: Room<'static> = Room::new(
		"basic-1",
		RoomShape::Rectangle(Vec2::new(8, 5)),
		&[Door::new(Vec2 { x: 0, y: 0 }, Vec2 { x: 0, y: 0 })],
	);

	/// # Caracteristics
	///
	/// - Size: `(5,8)`
	/// - Doors:
	///   - `(0,6)-(0,7)`
	///
	/// # Preview
	/// ```text
	/// 0WWWWW
	/// W    W
	/// W    W
	/// W    W
	/// W    W
	/// W    W
	/// D    W
	/// D    W
	/// WWWWWW
	/// ```
	const BASIC_2_ROOM: Room<'static> = Room::new(
		"basic-2",
		RoomShape::Rectangle(Vec2::new(5, 8)),
		&[Door::new(Vec2::new(0, 6), Vec2::new(0, 7))],
	);

	pub fn get_templates(kind: RoomKind) -> Vec<Room<'static>> {
		match kind {
			RoomKind::Spawn => vec![],
			RoomKind::Normal => vec![BASIC_1_ROOM, BASIC_2_ROOM],
			RoomKind::Reward => vec![],
			RoomKind::Shop => vec![],
			RoomKind::Boss => vec![],
			RoomKind::Exit => vec![],
		}
	}
}

fn main() {
	let lvl_graph = level_blueprint::basic_level_graph();
	let template = room_blueprint::get_templates;
	let rng = SmallRng::seed_from_u64(0xDEAD_BEEF_FFFF_FFFF);

	let graph_repr = Dot::new(&lvl_graph);
	println!("{:?}", graph_repr);
}
