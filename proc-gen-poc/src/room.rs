use glam::{IVec2, UVec2};
use std::fmt;

mod doors {
	use crate::room::DoorSize;

	pub const VERTICAL_5: DoorSize = DoorSize::Vertical(5);
	pub const HORIZONTAL_3: DoorSize = DoorSize::Horizontal(3);
}

#[derive(Debug)]
pub enum Kind {
	/// This should be the first room in a level graph
	Spawn,

	Normal,
	Lobby,
	// Reward,
	// Shop,
	// Boss,
	/// This should be the last room in a level graph
	Exit,
}

impl Kind {
	pub(crate) const fn variants() -> &'static [Self] {
		use Kind::{Exit, Lobby, Normal, Spawn};
		&[Spawn, Normal, Lobby, Exit]
	}
}

#[derive(Debug, Clone)]
pub enum Shape {
	/// A rectangle size
	Rectangle(UVec2),
}

impl Shape {
	pub(crate) const fn bounding_box(&self) -> UVec2 {
		match self {
			Self::Rectangle(rect) => *rect,
		}
	}
}

impl fmt::Display for Shape {
	fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
		match self {
			Self::Rectangle(rect) => writeln!(f, "{rect}"),
		}
	}
}

#[derive(Debug)]
pub struct Door<'a> {
	pub(crate) label: &'a str,

	/// Offset in the Room coordinate space
	pub(crate) pos: UVec2,

	/// Door size
	pub(crate) size: DoorSize,
}

impl<'a> Door<'a> {
	pub(crate) const fn new(label: &'a str, pos: UVec2, size: DoorSize) -> Self {
		Self { label, pos, size }
	}
}

impl fmt::Display for Door<'_> {
	fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
		let Self { label, pos, size } = self;

		write!(f, r#"🚪 "{label}" {pos} {size:?}"#)
	}
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum DoorSize {
	Vertical(u8),
	Horizontal(u8),
}

impl DoorSize {
	pub const fn to_uvec2(&self) -> UVec2 {
		match &self {
			Self::Vertical(size) => UVec2::new(1, *size as u32),
			Self::Horizontal(size) => UVec2::new(*size as u32, 1),
		}
	}

	pub(crate) fn adjacent_deltas(&self) -> [IVec2; 2] {
		let pos = self.to_uvec2().as_ivec2();
		match &self {
			Self::Vertical(_) => [pos.with_y(0), -pos.with_y(0)],
			Self::Horizontal(_) => [pos.with_x(0), -pos.with_x(0)],
		}
	}
}

#[derive(Debug, Clone)]
pub struct Template<'a> {
	pub(crate) name: &'a str,
	pub(crate) shape: Shape,
	pub(crate) doors: &'a [Door<'a>],
	// allowed_transformations? (e.g. flip_x)
}

impl fmt::Display for Template<'_> {
	fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
		let Self { name, shape, doors } = self;

		writeln!(f, r#"📦 "{name}" {shape}"#)?;
		for door in *doors {
			writeln!(f, "> {door}")?;
		}
		Ok(())
	}
}

pub trait Provider {
	fn provide_of_kind<'a>(&self, kind: &Kind) -> &[&Template<'a>];
}

pub struct StaticRoomTable;

impl Provider for StaticRoomTable {
	fn provide_of_kind<'a>(&self, kind: &Kind) -> &[&Template<'a>] {
		match kind {
			Kind::Spawn => &[&blueprints::SPAWN_1],
			Kind::Normal => &[
				&blueprints::CORRIDOR_1,
				&blueprints::CORNER_1,
				&blueprints::CORNER_2,
				&blueprints::NORMAL_1,
			],
			Kind::Lobby => &[&blueprints::LOBBY_1],
			Kind::Exit => &[&blueprints::EXIT_1],
		}
	}
}

pub mod blueprints {
	use super::doors::{HORIZONTAL_3, VERTICAL_5};
	use super::{Door, Shape, Template};
	use glam::UVec2;

	macro_rules! room {
		($name:ident, ($x:expr, $y:expr), [$(($room_label:literal, ($room_x:expr, $room_y:expr), $room_size:expr)),*]) => {
			pub const $name: Template<'static> = Template {
				name: stringify!($name),
				shape: Shape::Rectangle(UVec2::new($x, $y)),
				doors: &[
					$(Door::new($room_label, UVec2::new($room_x, $room_y), $room_size)),*
				],
			};
		};
	}

	room!(SPAWN_1, (20, 12), [("right", (19, 7 - 1), VERTICAL_5)]);

	room!(EXIT_1, (20, 12), [("left", (0, 7 - 1), VERTICAL_5)]);

	room!(
		CORRIDOR_1,
		(20, 8),
		[
			("left", (0, 3 - 1), VERTICAL_5),
			("left", (19, 3 - 1), VERTICAL_5)
		]
	);

	room!(
		CORNER_1,
		(20, 20),
		[
			("left", (0, 15 - 2), VERTICAL_5),
			("top", (17 - 2, 0), HORIZONTAL_3)
		]
	);

	room!(
		CORNER_2,
		(30, 8),
		[
			("bottom-right", (27 - 4, 7), HORIZONTAL_3),
			("top-left", (4, 0), HORIZONTAL_3)
		]
	);

	room!(
		NORMAL_1,
		(40, 40),
		[
			("bottom", (3, 39), HORIZONTAL_3),
			("right", (39, 35 - 10), VERTICAL_5)
		]
	);

	room!(
		LOBBY_1,
		(30, 20),
		[
			("from", (0, 7), VERTICAL_5),
			("to-1", (29, 2), VERTICAL_5),
			("to-2", (29, 13), VERTICAL_5)
		]
	);
}
