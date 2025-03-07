//! Taken from official example: <https://bevyengine.org/examples/games/game-menu/>

use crate::GameState;
use crate::utils::despawn_screen;
use bevy::color::palettes::basic;
use bevy::prelude::*;

#[derive(Debug, Clone, Copy, Default, PartialEq, Eq, Hash, Reflect, States)]
enum MenuState {
	Main,

	Settings,

	#[default]
	Disabled,
}

pub(crate) fn plugin(app: &mut App) {
	use MenuState::*;

	app.init_state::<MenuState>()
		// Setup and cleanup on change game state
		.add_systems(
			OnEnter(GameState::Menu),
			|mut menu_state: ResMut<NextState<MenuState>>| menu_state.set(Main),
		)
		.add_systems(
			OnExit(GameState::Menu),
			|mut menu_state: ResMut<NextState<MenuState>>| menu_state.set(Disabled),
		)
		// Global
		.add_systems(
			Update,
			(button_hover, ButtonAction::menu_action).run_if(in_state(GameState::Menu)),
		)
		// Main menu
		.add_systems(OnEnter(Main), main::setup_ui)
		.add_systems(OnExit(Main), despawn_screen::<main::OnMainMenuScreen>);
	// Settings menu
	// .add_systems(OnEnter(Settings), settings::setup)
	// .add_systems(OnExit(Settings), despawn_screen::<settings::OnSettingsMenuScreen>)
}

#[derive(Debug, Component)]
enum ButtonAction {
	LaunchRoom,
	RejoinRoom,
	ShowSettings,
	ExitGame,
}

mod main {
	use super::ButtonAction;
	use bevy::color::palettes::basic;
	use bevy::prelude::*;

	#[derive(Component)]
	pub(crate) struct OnMainMenuScreen;

	pub(crate) fn setup_ui(mut commands: Commands) {
		let large_text_font = TextFont::from_font_size(33.);

		commands
			.spawn((
				OnMainMenuScreen,
				Node {
					width: Val::Percent(100.),
					height: Val::Percent(100.),
					justify_content: JustifyContent::Center,
					align_items: AlignItems::Center,
					flex_direction: FlexDirection::Row,
					column_gap: Val::Px(20.),
					..default()
				},
			))
			.with_children(|parent| {
				parent.spawn((
					Text::from("Play"),
					Button,
					ButtonAction::LaunchRoom,
					large_text_font.clone(),
					TextColor(Color::from(basic::GREEN)),
				));

				parent.spawn((
					Text::from("Rejoin"),
					Button,
					ButtonAction::RejoinRoom,
					large_text_font.clone(),
					TextColor(Color::from(basic::GREEN)),
				));
				parent.spawn((
					Text::from("Settings"),
					Button,
					ButtonAction::ShowSettings,
					large_text_font.clone(),
					TextColor(Color::from(basic::GREEN)),
				));
				parent.spawn((
					Text::from("Exit"),
					Button,
					ButtonAction::ExitGame,
					large_text_font.clone(),
					TextColor(Color::from(basic::GREEN)),
				));
			});
	}
}

impl ButtonAction {
	fn menu_action(
		interaction_query: Query<(&Interaction, &Self), (Changed<Interaction>, With<Button>)>,
		mut app_exit_events: EventWriter<AppExit>,
		mut menu_state: ResMut<NextState<MenuState>>,
		mut game_state: ResMut<NextState<GameState>>,
	) {
		for (interaction, menu_button_action) in &interaction_query {
			if *interaction == Interaction::Pressed {
				match menu_button_action {
					Self::LaunchRoom => todo!(),
					Self::RejoinRoom => game_state.set(GameState::InRoom),
					Self::ShowSettings => menu_state.set(MenuState::Main),
					Self::ExitGame => {
						app_exit_events.send(AppExit::Success);
					}
				}
			}
		}
	}
}

fn button_hover(
	mut interaction_query: Query<
		(&Interaction, &mut TextColor),
		(Changed<Interaction>, With<Button>),
	>,
) {
	for (interaction, mut text_color) in &mut interaction_query {
		text_color.0 = match *interaction {
			Interaction::Pressed => Color::from(basic::RED),
			Interaction::Hovered => Color::from(basic::YELLOW),
			Interaction::None => Color::from(basic::GREEN),
		}
	}
}
