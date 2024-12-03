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

#[derive(Debug)]
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

#[derive(Debug)]
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
	fn provide_room(&self, kind: &RoomKind) -> Vec<RoomTemplate<'static>>;
}

pub(crate) struct StaticRoomTable;

impl RoomProvider for StaticRoomTable {
	fn provide_room(&self, kind: &RoomKind) -> Vec<RoomTemplate<'static>> {
		use blueprints::*;

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
	pub(crate) const BASIC_1_ROOM: RoomTemplate<'static> = RoomTemplate::new(
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
	pub(crate) const BASIC_2_ROOM: RoomTemplate<'static> = RoomTemplate::new(
		"basic-2",
		RoomShape::Rectangle(UVec2::new(5, 8)),
		&[Door::new(UVec2::new(0, 6), UVec2::new(0, 7))],
	);
}
