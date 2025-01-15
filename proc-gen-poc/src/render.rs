use crate::{
	evolver::{EvolvedGraph, PlacedRoom},
	room::{RoomProvider, RoomShape},
};
use glam::IVec2;
use std::io;
use svg::{
	node::element::{path::Data, Group, Path, Style, Text},
	Document, Node,
};

pub(crate) fn render_room_graph(
	path: impl AsRef<std::path::Path>,
	graph: &EvolvedGraph,
) -> io::Result<()> {
	let style = Style::new(
		"
		text {
			font-size: 1px;
		}
		",
	);

	let mut document = Document::new()
		.set("viewBox", (-75, -25, 150, 50))
		.add(style);

	for room in graph.node_weights() {
		let shape = make_room_shape(room);
		document = document.add(shape);
	}

	svg::save(path, &document)?;

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

	let name_text = Text::new(room.template.name)
		.set("x", x + 0.2)
		.set("y", y + 1.1)
		.set("fill", "blue");

	let mut group = Group::new()
		.set("data-name", room.template.name)
		.add(shape)
		.add(name_text);

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

pub(crate) fn render_room_provider_store(
	path: impl AsRef<std::path::Path>,
	provider: impl RoomProvider,
) -> io::Result<()> {
	let style = Style::new(
		"
		text {
			font-size: 1px;
		}
		",
	);

	let rooms = provider.store();

	let mut document = Document::new()
		.set("viewBox", (-10, 0, 25 * rooms.len(), 50))
		.add(style);

	for (i, room) in rooms.iter().enumerate() {
		let r = PlacedRoom::new(IVec2::new(i as i32 * 20, 0), (*room).clone());
		let shape = make_room_shape(&r);
		document = document.add(shape);
	}

	svg::save(path, &document)?;

	Ok(())
}
