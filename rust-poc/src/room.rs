use crate::{GameState, utils::despawn_screen};
use bevy::prelude::*;

#[derive(Debug, Clone, Copy, Default, PartialEq, Eq, Hash, Reflect, States)]
pub(crate) enum RoomState {
	Lobby,

	Level,

	#[default]
	Disabled,
}

pub(crate) fn plugin(app: &mut App) {
	use RoomState::*;

	app.init_state::<RoomState>()
		// Setup and cleanup on change game state
		.add_systems(
			OnEnter(GameState::InRoom),
			|mut menu_state: ResMut<NextState<RoomState>>| menu_state.set(Lobby),
		)
		.add_systems(
			OnExit(GameState::InRoom),
			|mut menu_state: ResMut<NextState<RoomState>>| menu_state.set(Disabled),
		)
		// Main menu
		.add_systems(OnEnter(Lobby), lobby::setup_ui)
		.add_systems(OnExit(Lobby), despawn_screen::<lobby::OnLobbyScreen>);
}

mod lobby {
	use bevy::color::palettes::basic;
	use bevy::prelude::*;

	#[derive(Component)]
	pub(crate) struct OnLobbyScreen;

	pub(crate) fn setup_ui(mut commands: Commands) {
		let large_text_font = TextFont::from_font_size(33.);

		commands
			.spawn((
				OnLobbyScreen,
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
