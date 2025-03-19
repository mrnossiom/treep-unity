//! Room that can contain multiple players

use crate::shared::RoomState;
use bevy::prelude::*;

pub fn plugin(app: &mut App) {
	use RoomState::*;

	app.init_state::<RoomState>()
		.enable_state_scoped_entities::<RoomState>()
		// Setup and cleanup on change game state
		// .add_systems(
		// 	OnEnter(GameState::InRoom),
		// 	|mut menu_state: ResMut<NextState<RoomState>>| menu_state.set(Lobby),
		// )
		// .add_systems(
		// 	OnExit(GameState::InRoom),
		// 	|mut menu_state: ResMut<NextState<RoomState>>| menu_state.set(Disabled),
		// )
		// Main menu
		.add_systems(OnEnter(Lobby), lobby::setup_ui);
}

mod lobby {
	use super::RoomState;
	use bevy::color::palettes::basic;
	use bevy::prelude::*;

	pub(crate) fn setup_ui(mut commands: Commands) {
		let large_text_font = TextFont::from_font_size(33.);

		commands
			.spawn((
				StateScoped(RoomState::Lobby),
				Node {
					width: Val::Percent(100.),
					height: Val::Percent(100.),
					justify_content: JustifyContent::Start,
					align_items: AlignItems::End,
					..default()
				},
			))
			.with_children(|parent| {
				parent.spawn((
					Text::from("Ready?"),
					Button,
					large_text_font.clone(),
					TextColor(Color::from(basic::GREEN)),
				));
			});
	}
}
