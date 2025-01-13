use crate::evolver::Evolver;
use petgraph::dot::Dot;
use rand::{rngs::SmallRng, SeedableRng};
use render::render_room_graph;

mod evolver;
mod level;
mod render;
mod room;

const SEED: u64 = 0xDEAD_BEEF_FFFF_FFFF;
// const SEED: u64 = 0xDEAD_BEEF_FFFF_FFFF - 1;

fn main() -> Result<(), Box<dyn std::error::Error>> {
	env_logger::builder().format_timestamp(None).init();

	let (level_blueprint, root) = level::blueprints::basic_level_graph();
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

	let render_out = "target/out/graph.svg";
	render_room_graph(&evolved_graph, render_out)?;
	println!("Room graph was rendered to `{render_out}`");

	Ok(())
}
