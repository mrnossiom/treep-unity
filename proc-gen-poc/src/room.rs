use glam::UVec2;

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
	Rectangle(UVec2),
}

#[derive(Debug)]
pub(crate) struct Door {
	pos: UVec2,
	end: UVec2,
}

impl Door {
	pub(crate) const fn new(pos: UVec2, end: UVec2) -> Self {
		Self { pos, end }
	}
}

#[derive(Debug, Clone)]
pub(crate) struct RoomTemplate<'a> {
	pub(crate) name: &'a str,
	pub(crate) shape: RoomShape,
	pub(crate) doors: &'a [Door],
	// name? kind? allowed_transformations?
}

impl<'a> RoomTemplate<'a> {
	const fn new(name: &'a str, shape: RoomShape, doors: &'a [Door]) -> Self {
		Self { name, shape, doors }
	}
}

pub(crate) trait RoomProvider {
	fn provide_of_kind(&self, kind: &RoomKind) -> &[RoomTemplate<'static>];
}

pub(crate) struct StaticRoomTable;

impl RoomProvider for StaticRoomTable {
	fn provide_of_kind(&self, kind: &RoomKind) -> &[RoomTemplate<'static>] {
		use blueprints::*;

		match kind {
			RoomKind::Spawn => &[],
			RoomKind::Normal => &[BASIC_1, BASIC_2],
			RoomKind::Reward => &[],
			RoomKind::Shop => &[],
			RoomKind::Boss => &[],
			RoomKind::Exit => &[],
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
	pub(crate) const BASIC_1: RoomTemplate<'static> = RoomTemplate::new(
		"basic-1",
		RoomShape::Rectangle(UVec2::new(8, 5)),
		&[Door::new(UVec2 { x: 0, y: 0 }, UVec2 { x: 0, y: 0 })],
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
	pub(crate) const BASIC_2: RoomTemplate<'static> = RoomTemplate::new(
		"basic-2",
		RoomShape::Rectangle(UVec2::new(5, 8)),
		&[Door::new(UVec2::new(0, 6), UVec2::new(0, 7))],
	);

	/// # Caracteristics
	///
	/// - Size: `(10,4)`
	/// - Doors:
	///   - `(0,1)-(0,2)`
	///   - `(9,1)-(9,2)`
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
			Door::new(UVec2::new(0, 6), UVec2::new(0, 7)),
			Door::new(UVec2::new(0, 6), UVec2::new(0, 7)),
		],
	);
}
