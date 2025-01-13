use crate::{
	evolver::{EvolvedGraph, PlacedRoom},
	room::RoomShape,
};
use std::io;
use svg::{
	node::element::{path::Data, Group, Path},
	Document, Node,
};

pub(crate) fn render_room_graph(graph: &EvolvedGraph) -> io::Result<()> {
	let mut document = Document::new().set("viewBox", (-50, -25, 100, 50));

	for room in graph.node_weights() {
		let shape = make_room_shape(room);
		document = document.add(shape);
	}

	svg::save("target/out/graph.svg", &document)?;

	Ok(())
}

fn make_room_shape(room: &PlacedRoom) -> Group {
	let [x, y] = room.place.as_vec2().to_array();

	let shape: Box<dyn Node> = match room.template.shape {
		RoomShape::Rectangle(rect) => {
			let [w, h] = rect.as_vec2().to_array();

			let data = Data::new()
				.move_to((x, y))
				.line_by((w, 0))
				.line_by((0, h))
				.line_by((-w, 0))
				.line_by((0, -h))
				.close();

			Path::new()
				.set("fill", "none")
				.set("stroke", "black")
				.set("stroke-width", 0.25)
				.set("d", data)
				.into()
		}
	};

	let mut group = Group::new().set("data-name", room.template.name).add(shape);

	for door in room.template.doors {
		let [dx, dy] = door.pos.as_vec2().to_array();
		let [w, h] = door.size.to_uvec2().as_vec2().to_array();

		let data = Data::new()
			.move_to((x + dx, y + dy))
			.line_by((w, 0))
			.line_by((0, h))
			.line_by((-w, 0))
			.line_by((0, -h))
			.close();

		group = group.add(
			Path::new()
				.set("data-name", door.label)
				.set("fill", "none")
				.set("stroke", "red")
				.set("stroke-width", 0.25)
				.set("d", data),
		);
	}

	group
}
