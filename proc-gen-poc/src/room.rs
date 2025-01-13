use glam::{IVec2, UVec2};
use std::fmt;

#[derive(Debug)]
pub(crate) enum RoomKind {
	/// This should be the first room in a level graph
	Spawn,

	Normal,
	Reward,
	Shop,
	Boss,

	/// This should be the last room in a level graph
	Exit,
}

#[derive(Debug, Clone)]
pub(crate) enum RoomShape {
	/// A rectangle size
	Rectangle(UVec2),
}

impl fmt::Display for RoomShape {
	fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
		match self {
			Self::Rectangle(rect) => writeln!(f, "{rect}"),
		}
	}
}

#[derive(Debug)]
pub(crate) struct Door<'a> {
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
pub(crate) enum DoorSize {
	Vertical(u8),
	Horizontal(u8),
}

impl DoorSize {
	pub(crate) fn to_uvec2(&self) -> UVec2 {
		match &self {
			Self::Vertical(size) => UVec2::new(1, *size as u32),
			Self::Horizontal(size) => UVec2::new(*size as u32, 1),
		}
	}
	pub(crate) fn adj_door_deltas(&self) -> [IVec2; 2] {
		let pos = self.to_uvec2().as_ivec2();
		match &self {
			Self::Vertical(_) => [pos.with_y(0), -pos.with_y(0)],
			Self::Horizontal(_) => [pos.with_x(0), -pos.with_x(0)],
		}
	}
}

#[derive(Debug, Clone)]
pub(crate) struct RoomTemplate<'a> {
	pub(crate) name: &'a str,
	pub(crate) shape: RoomShape,
	pub(crate) doors: &'a [Door<'a>],
	// name? kind? allowed_transformations?
}

impl<'a> RoomTemplate<'a> {
	const fn new(name: &'a str, shape: RoomShape, doors: &'a [Door]) -> Self {
		Self { name, shape, doors }
	}
}

impl fmt::Display for RoomTemplate<'_> {
	fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
		let Self { name, shape, doors } = self;

		writeln!(f, r#"📦 "{name}" {shape}"#)?;
		for door in *doors {
			writeln!(f, "> {door}")?;
		}
		Ok(())
	}
}

pub(crate) trait RoomProvider {
	fn provide_of_kind<'a>(&self, kind: &RoomKind) -> &[RoomTemplate<'a>];
}

pub(crate) struct StaticRoomTable;

impl RoomProvider for StaticRoomTable {
	fn provide_of_kind<'a>(&self, kind: &RoomKind) -> &[RoomTemplate<'a>] {
		use blueprints::*;

		match kind {
			RoomKind::Spawn => &[BASIC_1],
			RoomKind::Normal => &[CORRIDOR_1, CORRIDOR_2],
			RoomKind::Reward => &[],
			RoomKind::Shop => &[],
			RoomKind::Boss => &[],
			RoomKind::Exit => &[BASIC_2],
		}
	}
}

pub(crate) mod blueprints {
	use super::*;
	use glam::UVec2;

	// const SPAWN_ROOM: RoomTemplate<'static> = RoomTemplate::new("spawn-1", (), ());
	// const BOSS_ROOM: RoomTemplate<'static> = RoomTemplate::new("boss-1", (), ());
	// const EXIT_ROOM: RoomTemplate<'static> = RoomTemplate::new("exit-1", (), ());

	/// # Caracteristics
	///
	/// - Size: (9,6)
	/// - Doors:
	///   - 2 units vertical at (8,3)
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
	pub(crate) const BASIC_1: RoomTemplate<'static> = RoomTemplate::new(
		"basic-1",
		RoomShape::Rectangle(UVec2::new(9, 6)),
		&[Door::new("right", UVec2::new(8, 3), DoorSize::Vertical(2))],
	);

	/// # Caracteristics
	///
	/// - Size: (6,9)
	/// - Doors:
	///   - 2 units vertical at (0,6)
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
	pub(crate) const BASIC_2: RoomTemplate<'static> = RoomTemplate::new(
		"basic-2",
		RoomShape::Rectangle(UVec2::new(6, 9)),
		&[Door::new("left", UVec2::new(0, 6), DoorSize::Vertical(2))],
	);

	/// # Caracteristics
	///
	/// - Size: (4,5)
	/// - Doors:
	///   - 3 units vertical at (0,1)
	///
	/// # Preview
	/// ```text
	/// 0WWW
	/// D  W
	/// D  W
	/// D  W
	/// WWWW
	/// ```
	pub(crate) const DOOR_UNMATCHED: RoomTemplate<'static> = RoomTemplate::new(
		"door-unmatched",
		RoomShape::Rectangle(UVec2::new(4, 5)),
		&[Door::new(
			"unmatched",
			UVec2::new(0, 1),
			DoorSize::Vertical(3),
		)],
	);

	/// # Caracteristics
	///
	/// - Size: (10,4)
	/// - Doors:
	///   - 2 units vertical at (0,1)
	///   - 2 units vertical at (9,1)
	///
	/// # Preview
	/// ```text
	/// 0WWWWWWWWW
	/// D        D
	/// D        D
	/// WWWWWWWWWW
	/// ```
	pub(crate) const CORRIDOR_1: RoomTemplate<'static> = RoomTemplate::new(
		"corridor-1",
		RoomShape::Rectangle(UVec2::new(10, 4)),
		&[
			Door::new("left", UVec2::new(0, 1), DoorSize::Vertical(2)),
			Door::new("right", UVec2::new(9, 1), DoorSize::Vertical(2)),
		],
	);

	pub(crate) const CORRIDOR_2: RoomTemplate<'static> = RoomTemplate::new(
		"corridor-2",
		RoomShape::Rectangle(UVec2::new(10, 5)),
		&[
			Door::new("left", UVec2::new(0, 1), DoorSize::Vertical(2)),
			Door::new("right", UVec2::new(9, 1), DoorSize::Vertical(2)),
		],
	);
}
