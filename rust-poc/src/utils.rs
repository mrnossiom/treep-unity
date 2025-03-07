use bevy::prelude::*;

pub fn despawn_screen<T: Component>(mut commands: Commands, to_despawn: Query<Entity, With<T>>) {
	for entity in &to_despawn {
		commands.entity(entity).despawn_recursive();
	}
}
