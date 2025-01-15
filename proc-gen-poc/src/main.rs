use crate::evolver::Evolver;
use petgraph::dot::Dot;
use rand::{rngs::SmallRng, SeedableRng};
use render::{render_room_graph, render_room_provider_store};

mod evolver;
mod level;
mod render;
mod room;

const SEED: u64 = 0xDEAD_BEEF_FFFF_FFFF;
// const SEED: u64 = 0xDEAD_BEEF_FFFF_FFFF - 1;

fn main() -> Result<(), Box<dyn std::error::Error>> {
	env_logger::builder().format_timestamp(None).init();

	let (level_blueprint, root) = level::blueprints::next_level_graph();
	let evolver = Evolver {
		room_provider: room::StaticRoomTable,
		level_blueprint,
	};

	let mut rng = SmallRng::seed_from_u64(SEED);
	let evolved_graph = evolver
		.evolve_root(root, &mut rng)
		.expect("could not solve evolver");

	println!(
		"\nBlueprint graph:\n```dot\n{}```\n",
		Dot::new(&evolver.level_blueprint)
	);
	println!(
		"\nEvolved graph:\n```dot\n{}```\n",
		Dot::new(&evolved_graph)
	);

	let store_render_out = "target/out/all_rooms.svg";
	render_room_provider_store(store_render_out, room::StaticRoomTable)?;
	println!("Room graph was rendered to `{store_render_out}`");

	let graph_render_out = "target/out/graph.svg";
	render_room_graph(graph_render_out, &evolved_graph)?;
	println!("Room graph was rendered to `{graph_render_out}`");

	Ok(())
}
