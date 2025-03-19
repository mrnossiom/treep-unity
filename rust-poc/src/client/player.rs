fn read_input(mut commands: Commands, input: Res<ButtonInput<KeyCode>>) {
	let right = if input.pressed(KeyCode::KeyD) { 1. } else { 0. };
	let left = if input.pressed(KeyCode::KeyA) { 1. } else { 0. };
	let jumped = input.just_pressed(KeyCode::Space);

	let horizontal_delta = right - left;

	commands.client_trigger(MovePlayer {
		horizontal_delta,
		jumped,
	});
}

fn update_player_animation(
	mut new_players: Query<&mut AseSpriteAnimation, Added<Player>>,
	asset_server: Res<AssetServer>,
) {
	for mut animation in &mut new_players {
		*animation = AseSpriteAnimation {
			animation: Animation::tag("walk-left"),
			aseprite: asset_server.load("ant.aseprite"),
		}
	}
}
