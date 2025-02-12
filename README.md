# Jellyfin.Plugin.LocalPosters

**Jellyfin.Plugin.LocalPosters** is a plugin for [Jellyfin](https://jellyfin.org/) that prioritizes the use of local poster images for your media library. This ensures that your personal artwork is displayed instead of automatically downloaded images.

It was heavily inspired by: https://github.com/Drazzilb08/daps (So make sure to check the project)

## Features

- **Local Artwork Priority**: The plugin ensures that any locally stored poster images in your media folders are used as the primary artwork, providing a personalized media browsing experience.

## Installation

### Using the Plugin Repository

1. Open Jellyfin and navigate to **Dashboard > Plugins**.
2. Click on **Repositories** and add the following URL: https://noonamer.github.io/Jellyfin.Plugin.LocalPosters/repository.json
3. Go to **Catalog**, find **Local Posters**, and install it.
4. Restart your Jellyfin server to apply the changes.

### Manual Installation

1. **Download the Plugin**:
    - Navigate to the [Releases](https://github.com/NooNameR/Jellyfin.Plugin.LocalPosters/releases) section of the repository.
    - Download the latest version of the plugin (`.dll` file).

2. **Install the Plugin**:
    - Place the downloaded `.dll` file into the `plugins` directory of your Jellyfin server.
    - Restart your Jellyfin server to recognize the new plugin.

## Configuration

Once installed, you need to specify the folder locations where your posters are stored. The latest folder in the list will have the highest priority, meaning Jellyfin will use posters from that folder first if duplicates exist.

Additionally, you can modify poster borders using the **border replacer** feature. To enable and configure this, navigate to the plugin settings within Jellyfin.

## Support

If you encounter any issues or have questions about the plugin, please open an issue in the [GitHub Issues](https://github.com/NooNameR/Jellyfin.Plugin.LocalPosters/issues) section of the repository.

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/NooNameR/Jellyfin.Plugin.LocalPosters/blob/master/LICENSE) file for details.
