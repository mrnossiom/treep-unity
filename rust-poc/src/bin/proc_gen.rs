use petgraph::dot::Dot;
use rand::{SeedableRng, rngs::SmallRng};
use treep::shared::generation::{
	evolver::Evolver,
	level,
	render::{render_room_graph, render_room_provider_store},
	room,
};

const SEED: u64 = 0xDEAD_BEEF_FFFF_FFFF;
// const SEED: u64 = 0xDEAD_BEEF_FFFF_FFFF - 1;

// TODO: impl custom shape intersect
// TODO: mark matched doors to avoid useless computation

fn main() -> Result<(), Box<dyn std::error::Error>> {
	env_logger::builder().format_timestamp(None).init();

	let (blueprint, root) = level::blueprints::basic_level();
	let evolver = Evolver {
		blueprint,
		room_provider: room::StaticRoomTable,
	};

	let room_store_render_path = "target/out/rooms.svg";
	render_room_provider_store(room_store_render_path, &evolver.room_provider)?;
	println!("Room graph was rendered to `{room_store_render_path}`");

	let mut rng = SmallRng::seed_from_u64(SEED);
	let evolved_graph = evolver
		.evolve_root(root, &mut rng)
		.expect("could not solve evolver");

	println!(
		"\nBlueprint graph:\n```dot\n{}```\n",
		Dot::new(&evolver.blueprint)
	);
	println!(
		"\nEvolved graph:\n```dot\n{}```\n",
		Dot::new(&evolved_graph)
	);

	let evolved_render_path = "target/out/evolved.svg";
	render_room_graph(evolved_render_path, &evolved_graph)?;
	println!("Room graph was rendered to `{evolved_render_path}`");

	Ok(())
}
