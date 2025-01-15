use crate::{
	level::Blueprint,
	room::{Door, Provider, Shape, Template},
};
use glam::IVec2;
use petgraph::{graph::NodeIndex, Direction, Graph};
use rand::seq::SliceRandom;
use rand::RngCore;
use std::fmt;

pub type EvolvedGraph<'a> = Graph<PlacedRoom<'a>, Empty>;

#[derive(Debug)]
pub struct PlacedRoom<'a> {
	pub(crate) place: IVec2,
	pub(crate) template: Template<'a>,
}

impl<'a> PlacedRoom<'a> {
	pub(crate) const fn new(place: IVec2, template: Template<'a>) -> Self {
		Self { place, template }
	}

	/// Return whether the shapes intersect
	fn has_overlap(&self, other: &Self) -> bool {
		match (&self.template.shape, &other.template.shape) {
			(Shape::Rectangle(self_rect), Shape::Rectangle(other_rect)) => {
				let self_rect = self_rect.as_ivec2();
				let other_rect = other_rect.as_ivec2();

				self.place.x < other.place.x + other_rect.x
					&& self.place.x + self_rect.x > other.place.x
					&& self.place.y < other.place.y + other_rect.y
					&& self.place.y + self_rect.y > other.place.y
			}
		}
	}
}

impl fmt::Display for PlacedRoom<'_> {
	fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
		let Self { place, template } = self;
		writeln!(f, "{place} {template}")
	}
}

/// Used to display `Graph`s with empty connections
pub struct Empty;

impl fmt::Display for Empty {
	fn fmt(&self, _: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
		Ok(())
	}
}

pub struct Evolver<R: Provider> {
	pub(crate) blueprint: Blueprint,

	pub(crate) room_provider: R,
}

/// Access slices by reference in a pseudo random manner.
///
/// See [`RandomAccessItem::new_with_rng`].
struct RandomAccessIter<'a, T> {
	slice: &'a [T],
	indices: Vec<usize>,
}

impl<'a, T> RandomAccessIter<'a, T> {
	fn new_with_rng(slice: &'a [T], rng: &mut impl RngCore) -> Self {
		let mut indices = (0..slice.len()).collect::<Vec<_>>();
		indices.shuffle(rng);

		Self { slice, indices }
	}
}

impl<'a, T> Iterator for RandomAccessIter<'a, T> {
	type Item = &'a T;

	fn next(&mut self) -> Option<Self::Item> {
		let index = self.indices.pop()?;
		Some(&self.slice[index])
	}
}

impl<R: Provider> Evolver<R> {
	/// Create and evolves a graph from the given root node in the level blueprint.
	///
	/// Returns `None` if graph is not solvable
	pub(crate) fn evolve_root(
		&self,
		root_id: NodeIndex,
		mut rng: &mut impl RngCore,
	) -> Option<EvolvedGraph> {
		let mut evolved_graph = EvolvedGraph::new();

		let root_room = &self.blueprint[root_id];
		let root_templates = &self.room_provider.provide_of_kind(&root_room.kind);

		for template in RandomAccessIter::new_with_rng(root_templates, rng) {
			let placed_root_room_id =
				evolved_graph.add_node(PlacedRoom::new(IVec2::ZERO, (*template).clone()));

			if !self.evolve_node(&mut evolved_graph, &mut rng, root_id, placed_root_room_id) {
				evolved_graph.remove_node(placed_root_room_id).unwrap();
				continue;
			};

			return Some(evolved_graph);
		}

		log::info!("(blueprint, room_provider) pair is not solvable");
		None
	}

	/// Evolves a placed node by trying to evolve all his doors.
	///
	/// Returns whether the graph have been sucessfully evolved.
	pub(crate) fn evolve_node(
		&self,
		graph: &mut EvolvedGraph,
		rng: &mut impl RngCore,
		room_id: NodeIndex,
		placed_room_id: NodeIndex,
	) -> bool {
		let placed_room = &graph[placed_room_id];
		let placed_room_name = placed_room.template.name.to_string();

		for door in RandomAccessIter::new_with_rng(placed_room.template.doors, rng) {
			log::debug!("testing room '{}' door '{}'", placed_room_name, door.label);
			if !self.evolve_node_door(graph, rng, room_id, placed_room_id, door) {
				continue;
			}

			return true;
		}

		false
	}

	/// Evolves a placed node door by getting the next neighbours and trying
	/// all the distant doors.
	///
	/// Returns whether the graph have been sucessfully evolved.
	pub(crate) fn evolve_node_door(
		&self,
		graph: &mut EvolvedGraph,
		rng: &mut impl RngCore,
		room_id: NodeIndex,
		placed_room_id: NodeIndex,
		door: &Door,
	) -> bool {
		let next_rooms = self
			.blueprint
			.neighbors_directed(room_id, Direction::Outgoing)
			.collect::<Vec<_>>();

		let next_room_id = match next_rooms.len() {
			0 => {
				log::info!("reached end of branch");
				return true;
			}
			1 => next_rooms[0],
			// TODO
			2.. => todo!("generalize to graph with branches"),
		};

		let next_room = &self.blueprint[next_room_id];
		let next_templates = &self.room_provider.provide_of_kind(&next_room.kind);

		for next_template in RandomAccessIter::new_with_rng(next_templates, rng) {
			for next_door in RandomAccessIter::new_with_rng(next_template.doors, rng) {
				let placed_room = &graph[placed_room_id];
				log::debug!(
					"> against '{}' with door '{}' (last: {})",
					next_template.name,
					next_door.label,
					placed_room.template.name
				);

				if door.size != next_door.size {
					log::debug!("< ABORT: door size mismatch");
					continue;
				}

				for delta in door.size.adjacent_deltas() {
					let placed_room = &graph[placed_room_id];
					let new_pos =
						placed_room.place + door.pos.as_ivec2() + delta - next_door.pos.as_ivec2();

					let next_placed_room = PlacedRoom {
						place: new_pos,
						template: (*next_template).clone(),
					};

					if next_placed_room.has_overlap(placed_room)
						|| graph
							.node_weights()
							.any(|pr| next_placed_room.has_overlap(pr))
					{
						log::debug!("< ABORT: rooms overlap");
						continue;
					}

					let next_placed_room_id = graph.add_node(next_placed_room);

					log::debug!(">> evolve next node");

					if !self.evolve_node(graph, rng, next_room_id, next_placed_room_id) {
						graph.remove_node(next_placed_room_id);
						log::debug!("<< ABORT: could not evolve node");
						continue;
					}

					graph.add_edge(placed_room_id, next_placed_room_id, Empty);

					return true;
				}
			}
		}

		false
	}
}
