use crate::{evolver::EvolvedGraph, room::RoomShape};
use glam::UVec2;
use raqote::{DrawOptions, DrawTarget, PathBuilder, SolidSource, Source, StrokeStyle};

fn texture_from_pos(pos: &UVec2) -> Source<'static> {
	let r = pos.x / 2000 * 255;
	let g = pos.y / 1000 * 255;
	let b = pos.x + pos.y / 3000 * 255;

	let tex = SolidSource::from_unpremultiplied_argb(255, r as u8, g as u8, b as u8);
	Source::Solid(tex)
}

pub(crate) fn render_room_graph(graph: &EvolvedGraph) {
	let mut dt = DrawTarget::new(2000, 1000);
	let options = DrawOptions::new();

	let white_tex = Source::Solid(SolidSource::from_unpremultiplied_argb(255, 255, 255, 255));
	dt.fill_rect(0., 0., 2000., 1000., &white_tex, &options);

	for node in graph.raw_nodes() {
		let node = &node.weight;

		let path = match node.template.shape {
			RoomShape::Rectangle(rect) => {
				let (x, y) = (
					node.place.x as f32 * 10. + 1000.,
					node.place.y as f32 * 10. + 500.,
				);
				let (w, h) = (rect.x as f32 * 10., rect.y as f32 * 10.);

				let mut path = PathBuilder::new();
				path.move_to(x, y);
				path.line_to(x + w, y);
				path.line_to(x + w, y + h);
				path.line_to(x, y + h);
				path.line_to(x, y);
				path.finish()
			}
		};

		let texture = texture_from_pos(&node.place);
		dt.stroke(
			&path,
			&texture,
			&StrokeStyle {
				width: 2.,
				..Default::default()
			},
			&options,
		);
	}

	dt.write_png("target/out/graph.png").unwrap();
}
