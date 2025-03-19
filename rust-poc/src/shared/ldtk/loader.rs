use crate::shared::ldtk::{self, LevelCollection};
use bevy::{
	asset::{AssetLoader, LoadContext, io::Reader},
	prelude::*,
};
use std::io::ErrorKind;

pub struct LdtkLoader;

#[derive(Debug, thiserror::Error)]
pub enum LdtkAssetLoaderError {
	/// An IO Error
	#[error("Could not load LDtk file: {0}")]
	Io(#[from] std::io::Error),
}

impl AssetLoader for LdtkLoader {
	type Asset = LevelCollection;
	type Settings = ();
	type Error = LdtkAssetLoaderError;

	async fn load(
		&self,
		reader: &mut dyn Reader,
		_settings: &Self::Settings,
		_load_context: &mut LoadContext<'_>,
	) -> Result<Self::Asset, Self::Error> {
		let mut bytes = Vec::new();
		reader.read_to_end(&mut bytes).await?;

		let project = serde_json::from_slice::<ldtk::types::LdtkJson>(&bytes).map_err(|err| {
			let err = format!("Could not read contents of LDtk map: {err}");
			std::io::Error::new(ErrorKind::Other, err)
		})?;

		// dbg!(&project.defs.enums);

		let levels = project
			.worlds
			.into_iter()
			.map(TryInto::try_into)
			.collect::<Result<_, _>>()
			// TODO
			.unwrap();

		Ok(LevelCollection { levels })
	}

	fn extensions(&self) -> &[&str] {
		&["ldtk"]
	}
}
