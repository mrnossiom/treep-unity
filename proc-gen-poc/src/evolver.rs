use crate::{
	level::LevelBlueprint,
	render::render_room_graph,
	room::{RoomProvider, RoomTemplate},
};
use glam::UVec2;
use petgraph::graph::UnGraph;
use rand::rngs::SmallRng;
use rand::seq::SliceRandom;

pub(crate) type EvolvedGraph = UnGraph<PlacedRoom, ()>;

pub(crate) struct PlacedRoom {
	pub(crate) place: UVec2,
	pub(crate) template: RoomTemplate<'static>,
}

pub(crate) struct Evolver<R: RoomProvider> {
	pub(crate) room_provider: R,
	pub(crate) level_blueprint: LevelBlueprint,
	pub(crate) rng: SmallRng,

	pub(crate) evolved_graph: EvolvedGraph,
}

impl<R: RoomProvider> Evolver<R> {
	pub(crate) fn find_layout(&mut self) -> Option<EvolvedGraph> {
		let id = self.level_blueprint.node_indices().next().unwrap();
		let root = &self.level_blueprint[id];

		let templates = self.room_provider.provide_of_kind(&root.kind);
		// TODO: replace with random access struct implementing IntoIterator?
		let mut indices = (0..templates.len()).collect::<Vec<_>>();
		indices.shuffle(&mut self.rng);

		for index in indices {
			let template = &templates[index];

			let placed_room_id = self.evolved_graph.add_node(PlacedRoom {
				place: UVec2::new(0, 0),
				template: template.clone(),
			});

			if true {
				break;
			} else {
				self.evolved_graph.remove_node(placed_room_id).unwrap();
			}
		}

		render_room_graph(&self.evolved_graph);

		todo!()
	}
}
