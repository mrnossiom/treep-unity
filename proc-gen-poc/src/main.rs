use crate::evolver::Evolver;
use evolver::EvolvedGraph;
use rand::{rngs::SmallRng, SeedableRng};

mod evolver;
mod level;
mod render;
mod room;

const SEED: u64 = 0xDEAD_BEEF_FFFF_FFFF;
// const SEED: u64 = 0xDEAD_BEEF_FFFF_FFF5;

fn main() {
	let mut evolver = Evolver {
		room_provider: room::StaticRoomTable,
		level_blueprint: level::blueprints::basic_level_graph(),
		rng: SmallRng::seed_from_u64(SEED),

		evolved: EvolvedGraph::new_undirected(),
	};

	let _final_layout = evolver.find_layout().unwrap();
}
