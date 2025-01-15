use crate::{
	evolver::{EvolvedGraph, PlacedRoom},
	room::{self, Provider, Shape},
};
use glam::IVec2;
use std::io;
use svg::{
	node::element::{path::Data, Group, Path, Style, Text},
	Document, Node,
};

pub fn render_room_graph(
	path: impl AsRef<std::path::Path>,
	graph: &EvolvedGraph,
) -> io::Result<()> {
	const MARGIN: i32 = 5;

	// keep track of bounds to autocalc view box
	let (mut min_x, mut max_x) = (0, 0);
	let (mut min_y, mut max_y) = (0, 0);

	let style = Style::new(
		"
		text {
			font-size: 1px;
		}
		",
	);

	let mut document = Document::new().add(style);

	for room in graph.node_weights() {
		min_x = min_x.min(room.place.x);
		min_y = min_y.min(room.place.y);
		let [w, h] = room.template.shape.bounding_box().as_ivec2().to_array();
		max_x = max_x.max(room.place.x + w);
		max_y = max_y.max(room.place.y + h);

		let shape = make_room_shape(room);
		document = document.add(shape);
	}

	document = document.set(
		"viewBox",
		(
			min_x - MARGIN,
			min_y - MARGIN,
			max_x - min_x + MARGIN * 2,
			max_y - min_y + MARGIN * 2,
		),
	);

	svg::save(path, &document)?;

	Ok(())
}

fn make_room_shape(room: &PlacedRoom) -> Group {
	let [x, y] = room.place.as_vec2().to_array();

	let shape: Box<dyn Node> = match room.template.shape {
		Shape::Rectangle(rect) => {
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

	group = group.add(name_text);

	group
}

pub fn render_room_provider_store(
	path: impl AsRef<std::path::Path>,
	provider: &impl Provider,
) -> io::Result<()> {
	const PER_ROOM_PLACE: i32 = 50;

	let style = Style::new(
		"
		text {
			font-size: 1px;
		}
		",
	);

	let mut document = Document::new().add(style);

	let variants = room::Kind::variants();
	let mut max_length = 0;

	for (variants_idx, kind) in variants.iter().enumerate() {
		let rooms = provider.provide_of_kind(kind);
		max_length = max_length.max(rooms.len());

		for (room_idx, room) in rooms.iter().enumerate() {
			let r = PlacedRoom::new(
				IVec2::new(
					PER_ROOM_PLACE * room_idx as i32,
					PER_ROOM_PLACE * variants_idx as i32,
				),
				(*room).clone(),
			);
			let shape = make_room_shape(&r);
			document = document.add(shape);
		}
	}

	document = document.set(
		"viewBox",
		(
			-10,
			-10,
			PER_ROOM_PLACE * max_length as i32,
			PER_ROOM_PLACE * variants.len() as i32,
		),
	);

	svg::save(path, &document)?;

	Ok(())
}
