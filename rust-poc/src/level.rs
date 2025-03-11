use crate::player::Player;
use crate::room::RoomState;
use crate::{DEBUG_MODE, GameState};
use bevy::color::palettes::css;
use bevy::prelude::*;
use bevy::utils::{HashMap, HashSet};
use bevy_ecs_ldtk::prelude::*;
use bevy_rapier2d::prelude::*;

pub fn plugin(app: &mut App) {
	use RoomState::*;

	app.add_plugins(LdtkPlugin)
		.insert_resource(LdtkSettings {
			level_background: LevelBackground::Nonexistent,
			..default()
		})
		// Register markers for entities
		.register_ldtk_entity_for_layer::<SpawnPoint>("entities", "spawn")
		// Register markers for cells
		.register_ldtk_int_cell_for_layer::<Wall>("interactables", 1)
		.register_ldtk_int_cell_for_layer::<Wall>("interactables", 2)
		.register_ldtk_int_cell_for_layer::<Wall>("interactables", 3);

	app.add_systems(Startup, load_level)
		.add_systems(
			Update,
			(spawn_wall_collision, tp_to_new_spawn_point).run_if(in_state(GameState::InRoom)),
		)
		.add_systems(OnEnter(Lobby), (setup_lobby,))
		.add_systems(OnExit(Lobby), || todo!())
		.add_systems(OnEnter(Level), (setup_level,))
		.add_systems(OnExit(Level), || todo!());

	if DEBUG_MODE {
		app.add_systems(
			Update,
			(highlight_spawn_point,).run_if(in_state(GameState::InRoom)),
		);
	}
}

#[derive(Component, Default)]
struct SpawnPoint;

impl LdtkEntity for SpawnPoint {
	fn bundle_entity(
		_: &EntityInstance,
		_: &LayerInstance,
		_: Option<&Handle<Image>>,
		_: Option<&TilesetDefinition>,
		_: &AssetServer,
		_: &mut Assets<TextureAtlasLayout>,
	) -> Self {
		Self
	}
}

#[derive(Copy, Clone, Eq, PartialEq, Debug, Default, Component)]
pub struct Wall;

impl LdtkIntCell for Wall {
	fn bundle_int_cell(_: IntGridCell, _: &LayerInstance) -> Self {
		Self
	}
}

fn load_level(mut commands: Commands, asset_server: Res<AssetServer>) {
	commands.spawn(LdtkWorldBundle {
		ldtk_handle: asset_server.load("world.ldtk").into(),
		..default()
	});
}

fn setup_lobby(mut commands: Commands) {
	commands.insert_resource(LevelSelection::index(0));
}

fn tp_to_new_spawn_point(
	spawn_points: Query<&Transform, (Added<SpawnPoint>, Without<Player>)>,
	mut players: Query<&mut Transform, With<Player>>,
) {
	for spawn_point in &spawn_points {
		for mut player in &mut players {
			player.translation = spawn_point.translation;
		}
	}
}

fn setup_level(mut commands: Commands) {
	commands.insert_resource(LevelSelection::index(1));
}

fn highlight_spawn_point(
	mut gizmos: Gizmos<DefaultGizmoConfigGroup>,
	spawns_query: Query<&Transform, With<SpawnPoint>>,
) {
	for spawn in &spawns_query {
		gizmos.circle_2d(Isometry2d::from(spawn.translation.xy()), 5., css::GREEN);
	}
}

pub fn spawn_wall_collision(
	mut commands: Commands,
	wall_query: Query<(&GridCoords, &Parent), Added<Wall>>,
	parent_query: Query<&Parent, Without<Wall>>,
	level_query: Query<(Entity, &LevelIid)>,
	ldtk_projects: Query<&LdtkProjectHandle>,
	ldtk_project_assets: Res<Assets<LdtkProject>>,
) {
	/// Represents a wide wall that is 1 tile tall
	/// Used to spawn wall collisions
	#[derive(Clone, Eq, PartialEq, Debug, Default, Hash)]
	struct Plate {
		left: i32,
		right: i32,
	}

	/// A simple rectangle type representing a wall of any size
	struct Rect {
		left: i32,
		right: i32,
		top: i32,
		bottom: i32,
	}

	// Consider where the walls are storing them as GridCoords in a HashSet for quick, easy lookup.
	//
	// The key of this map will be the entity of the level the wall belongs to.
	// This has two consequences in the resulting collision entities:
	// 1. it forces the walls to be split along level boundaries
	// 2. it lets us easily add the collision entities as children of the appropriate level entity
	let mut level_to_wall_locations: HashMap<Entity, HashSet<GridCoords>> = HashMap::new();

	wall_query.iter().for_each(|(&grid_coords, parent)| {
		// An intgrid tile's direct parent will be a layer entity, not the level entity
		// To get the level entity, you need the tile's grandparent.
		// This is where parent_query comes in.
		if let Ok(grandparent) = parent_query.get(parent.get()) {
			level_to_wall_locations
				.entry(grandparent.get())
				.or_default()
				.insert(grid_coords);
		}
	});

	if !wall_query.is_empty() {
		level_query.iter().for_each(|(level_entity, level_iid)| {
			if let Some(level_walls) = level_to_wall_locations.get(&level_entity) {
				let ldtk_project = ldtk_project_assets
					.get(ldtk_projects.single())
					.expect("Project should be loaded if level has spawned");

				let level = ldtk_project
					.as_standalone()
					.get_loaded_level_by_iid(&level_iid.to_string())
					.expect("Spawned level should exist in LDtk project");

				let LayerInstance {
					c_wid: width,
					c_hei: height,
					grid_size,
					..
				} = level.layer_instances()[0];

				// combine wall tiles into flat "plates" in each individual row
				let mut plate_stack: Vec<Vec<Plate>> = Vec::new();

				for y in 0..height {
					let mut row_plates: Vec<Plate> = Vec::new();
					let mut plate_start = None;

					// +1 to the width so the algorithm "terminates" plates that touch the right edge
					for x in 0..=width {
						match (plate_start, level_walls.contains(&GridCoords { x, y })) {
							(Some(s), false) => {
								row_plates.push(Plate {
									left: s,
									right: x - 1,
								});
								plate_start = None;
							}
							(None, true) => plate_start = Some(x),
							_ => (),
						}
					}

					plate_stack.push(row_plates);
				}

				// combine "plates" into rectangles across multiple rows
				let mut rect_builder: HashMap<Plate, Rect> = HashMap::new();
				let mut prev_row: Vec<Plate> = Vec::new();
				let mut wall_rects: Vec<Rect> = Vec::new();

				// an extra empty row so the algorithm "finishes" the rects that touch the top edge
				plate_stack.push(Vec::new());

				for (y, current_row) in plate_stack.into_iter().enumerate() {
					for prev_plate in &prev_row {
						if !current_row.contains(prev_plate) {
							// remove the finished rect so that the same plate in the future starts a new rect
							if let Some(rect) = rect_builder.remove(prev_plate) {
								wall_rects.push(rect);
							}
						}
					}
					for plate in &current_row {
						rect_builder
							.entry(plate.clone())
							.and_modify(|e| e.top += 1)
							.or_insert(Rect {
								bottom: y as i32,
								top: y as i32,
								left: plate.left,
								right: plate.right,
							});
					}
					prev_row = current_row;
				}

				commands.entity(level_entity).with_children(|level| {
					// Spawn colliders for every rectangle..
					// Making the collider a child of the level serves two purposes:
					// 1. Adjusts the transforms to be relative to the level for free
					// 2. the colliders will be despawned automatically when levels unload
					for wall_rect in wall_rects {
						level
							.spawn_empty()
							.insert(Collider::cuboid(
								(wall_rect.right as f32 - wall_rect.left as f32 + 1.)
									* grid_size as f32 / 2.,
								(wall_rect.top as f32 - wall_rect.bottom as f32 + 1.)
									* grid_size as f32 / 2.,
							))
							.insert(RigidBody::Fixed)
							.insert(Friction::new(1.0))
							.insert(Transform::from_xyz(
								(wall_rect.left + wall_rect.right + 1) as f32 * grid_size as f32
									/ 2.,
								(wall_rect.bottom + wall_rect.top + 1) as f32 * grid_size as f32
									/ 2.,
								0.,
							))
							.insert(GlobalTransform::default());
					}
				});
			}
		});
	}
}
