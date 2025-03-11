//! Main Menu
//!
//! Taken from official example: <https://bevyengine.org/examples/games/game-menu/>

use crate::utils::despawn_screen;
use crate::{DEBUG_MODE, GameState};
use bevy::color::palettes::basic;
use bevy::prelude::*;
use bevy_inspector_egui::quick::StateInspectorPlugin;
use bevy_quinnet::client::QuinnetClient;
use bevy_quinnet::client::certificate::CertificateVerificationMode;
use bevy_quinnet::client::connection::{
	ClientEndpointConfiguration, ConnectionEvent, ConnectionFailedEvent,
};
use bevy_quinnet::server::certificate::CertificateRetrievalMode;
use bevy_quinnet::server::{QuinnetServer, ServerEndpointConfiguration};
use bevy_replicon::prelude::*;
use bevy_replicon_quinnet::ChannelsConfigurationExt;
use std::net::Ipv6Addr;

#[derive(Debug, Clone, Copy, Default, PartialEq, Eq, Hash, Reflect, States)]
enum MenuState {
	/// Greets the player when entering the game
	Main,

	/// Lets the player choose his save file and enter his world
	SinglePlayer,

	/// Lets the player host or rejoin a server
	MultiPlayer,
	/// Splash multiplayer loading screen
	MultiPlayerLoading,

	// TODO: extract as its own state to let the player edit keybinds mid-game?
	/// Lets the player edit settings
	Settings,

	#[default]
	Disabled,
}

pub fn plugin(app: &mut App) {
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
			(button_hover, menu_button_action).run_if(in_state(GameState::Menu)),
		)
		// Main menu
		.add_systems(OnEnter(Main), main::setup_ui)
		.add_systems(OnExit(Main), despawn_screen::<main::OnMainMenuScreen>)
		// Single player
		.add_systems(OnEnter(SinglePlayer), singleplayer::setup_ui)
		.add_systems(
			OnExit(SinglePlayer),
			despawn_screen::<singleplayer::OnSinglePlayerScreen>,
		)
		// Multi player
		.add_systems(OnEnter(MultiPlayer), multiplayer::setup_ui)
		.add_systems(
			OnExit(MultiPlayer),
			despawn_screen::<multiplayer::OnMultiPlayerScreen>,
		)
		.add_systems(Update, (handle_client_events,).run_if(client_connected))
		// Settings menu
		.add_systems(OnEnter(Settings), settings::setup_ui)
		.add_systems(
			OnExit(Settings),
			despawn_screen::<settings::OnSettingsScreen>,
		);

	if DEBUG_MODE {
		app.add_plugins(StateInspectorPlugin::<MenuState>::default());
	}
}

#[derive(Debug, Component)]
pub(crate) enum ButtonAction {
	/// Start a singleplayer game
	StartSinglePlayer,
	/// Show the muliplayer sub-menu
	StartMultiPlayer,

	/// Hosts a multiplayer room
	CreateRoom,
	/// Join a multiplayer room over the network
	JoinRoom,

	/// Show the settings panel
	ShowSettings,

	/// Quit the game
	ExitGame,
}

fn menu_button_action(
	interaction_query: Query<(&Interaction, &ButtonAction), (Changed<Interaction>, With<Button>)>,
	mut app_exit_events: EventWriter<AppExit>,
	mut menu_state: ResMut<NextState<MenuState>>,
	mut game_state: ResMut<NextState<GameState>>,
	// network
	commands: Commands,
	channels: Res<RepliconChannels>,
	mut server: ResMut<QuinnetServer>,
	mut client: ResMut<QuinnetClient>,
) {
	for (interaction, menu_button_action) in &interaction_query {
		const PORT: u16 = 6767;
		const DISTANT_IP: Ipv6Addr = Ipv6Addr::LOCALHOST;

		if *interaction == Interaction::Pressed {
			match menu_button_action {
				ButtonAction::StartSinglePlayer => {
					menu_state.set(MenuState::SinglePlayer);
				}
				ButtonAction::StartMultiPlayer => {
					menu_state.set(MenuState::MultiPlayer);
				}

				ButtonAction::CreateRoom => {
					let config = ServerEndpointConfiguration::from_ip(Ipv6Addr::LOCALHOST, PORT);
					let cert_mode = CertificateRetrievalMode::GenerateSelfSigned {
						server_hostname: DISTANT_IP.to_string(),
					};

					server
						.start_endpoint(config, cert_mode, channels.get_server_configs())
						.unwrap();

					game_state.set(GameState::InRoom);

					// TODO: spawn local player
				}
				ButtonAction::JoinRoom => {
					let endpoint_config = ClientEndpointConfiguration::from_ips(
						DISTANT_IP,
						PORT,
						Ipv6Addr::UNSPECIFIED,
						0,
					);

					client
						.open_connection(
							endpoint_config,
							CertificateVerificationMode::SkipVerification,
							channels.get_client_configs(),
						)
						.unwrap();

					menu_state.set(MenuState::MultiPlayerLoading);

					// TODO: spawn local player on sucessful join
				}

				ButtonAction::ShowSettings => {
					menu_state.set(MenuState::Settings);
				}
				ButtonAction::ExitGame => {
					app_exit_events.send(AppExit::Success);
				}
			}
		}
	}
}

fn handle_client_events(
	mut connection_events: EventReader<ConnectionEvent>,
	mut connection_failed_events: EventReader<ConnectionFailedEvent>,
	// mut client: ResMut<QuinnetClient>,
	mut game_state: ResMut<NextState<GameState>>,
	mut menu_state: ResMut<NextState<MenuState>>,
) {
	for ConnectionEvent { .. } in connection_events.read() {
		// when connected, proceed to game state
		game_state.set(GameState::InRoom);
	}

	for ConnectionFailedEvent { .. } in connection_failed_events.read() {
		// if connection failed, go back to multiplayer
		menu_state.set(MenuState::MultiPlayer);
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
					flex_direction: FlexDirection::Column,
					row_gap: Val::Px(20.),
					..default()
				},
			))
			.with_children(|parent| {
				parent.spawn((
					Text::from("Single player"),
					Button,
					ButtonAction::StartSinglePlayer,
					large_text_font.clone(),
					TextColor(Color::from(basic::GREEN)),
				));
				parent.spawn((
					Text::from("Multi player"),
					Button,
					ButtonAction::StartMultiPlayer,
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

mod singleplayer {
	use super::ButtonAction;
	use bevy::color::palettes::basic;
	use bevy::prelude::*;

	#[derive(Component)]
	pub(crate) struct OnSinglePlayerScreen;

	pub(crate) fn setup_ui(mut commands: Commands) {
		commands
			.spawn((
				OnSinglePlayerScreen,
				Node {
					width: Val::Percent(100.),
					height: Val::Percent(100.),
					justify_content: JustifyContent::Center,
					align_items: AlignItems::Center,
					flex_direction: FlexDirection::Column,
					row_gap: Val::Px(20.),
					..default()
				},
			))
			.with_children(|parent| {
				parent.spawn((
					Text::from("Start"),
					Button,
					ButtonAction::JoinRoom,
					TextColor(Color::from(basic::GREEN)),
				));
				parent.spawn((
					Text::from("Select Save"),
					Button,
					TextColor(Color::from(basic::GREEN)),
				));
			});
	}
}

mod multiplayer {
	use super::ButtonAction;
	use bevy::color::palettes::basic;
	use bevy::prelude::*;

	#[derive(Component)]
	pub(crate) struct OnMultiPlayerScreen;

	pub(crate) fn setup_ui(mut commands: Commands) {
		commands
			.spawn((
				OnMultiPlayerScreen,
				Node {
					width: Val::Percent(100.),
					height: Val::Percent(100.),
					justify_content: JustifyContent::Center,
					align_items: AlignItems::Center,
					flex_direction: FlexDirection::Column,
					row_gap: Val::Px(20.),
					..default()
				},
			))
			.with_children(|parent| {
				parent.spawn((
					Text::from("Join Room"),
					Button,
					ButtonAction::JoinRoom,
					TextColor(Color::from(basic::GREEN)),
				));
				parent.spawn((
					Text::from("Create Room"),
					Button,
					ButtonAction::CreateRoom,
					TextColor(Color::from(basic::GREEN)),
				));
			});
	}
}

mod settings {
	use bevy::color::palettes::basic;
	use bevy::prelude::*;

	#[derive(Component)]
	pub(crate) struct OnSettingsScreen;

	pub(crate) fn setup_ui(mut commands: Commands) {
		commands
			.spawn((
				OnSettingsScreen,
				Node {
					width: Val::Percent(100.),
					height: Val::Percent(100.),
					justify_content: JustifyContent::Center,
					align_items: AlignItems::Center,
					flex_direction: FlexDirection::Column,
					row_gap: Val::Px(20.),
					..default()
				},
			))
			.with_children(|parent| {
				parent.spawn((
					Text::from("Settings"),
					Button,
					TextColor(Color::from(basic::GREEN)),
				));
			});
	}
}
