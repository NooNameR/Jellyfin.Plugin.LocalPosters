# Jellyfin.Plugin.LocalPosters

**Jellyfin.Plugin.LocalPosters** is a plugin for [Jellyfin](https://jellyfin.org/) that prioritizes the use of local poster images for your media library. This ensures that your personal artwork is displayed instead of automatically downloaded images.

Inspired by: https://github.com/Drazzilb08/daps (So make sure to check the project)

### Currently supported
Filename formats from [MediUX](https://mediux.pro/) or [TPDb](https://theposterdb.com/)

Media Types:
- `Movie`: you can test filename with: https://regex101.com/r/WwQIOJ/1 (make sure it matches jellyfin item metadata)
- `BoxSet`: you can test filename with: https://regex101.com/r/9kPyq5/1 (make sure it matches jellyfin item metadata)
- `Series`: you can test filename with: https://regex101.com/r/DQSHJF/2 (make sure it matches jellyfin item metadata)
- `Season`: you can test filename with: https://regex101.com/r/RTYOdo/2 (make sure it matches jellyfin item metadata)
- `Episode`: you can test filename with: https://regex101.com/r/oaKoZq/2 (make sure it matches jellyfin item metadata)

## Features

- **Local Artwork Priority**: The plugin ensures that any locally stored poster images are used as the primary artwork, providing a personalized experience.
- **GDrive Sync**: Sync GDrive folders using either (OAuth2 or Service Account)

![Screenshot 2025-02-17 at 15 38 08](https://github.com/user-attachments/assets/6a716f88-268d-4781-a2fb-cc1aefc723f3)


📌 Images shown are just a preview of the functionality and do not represent any local library

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

Once installed, you need to specify the folder locations where your posters are stored. `Local Posters` image fetcher should be enabled for desired libraries

Additionally, you can modify poster borders using the **border replacer** feature. To enable and configure this, navigate to the plugin settings within Jellyfin.

### Creating Client Secrets for GDrive Integration
To enable GDrive integration with `./auth/drive.file` scope:
1. Make sure [Known Proxy](https://jellyfin.org/docs/general/post-install/networking/#known-proxies) is set correctly before proceeding (Google unlikely to give refresh token to localhost)
2. **Google Cloud Console:** [console.cloud.google.com](https://console.cloud.google.com/)
3. **Create Project:** Click project dropdown, select **New Project**, and name it.
4. **Enable Google Drive API:** Navigate to **APIs & Services > Library** and enable **Google Drive API**.
5. **Create OAuth 2.0 Credentials:**
    - Under **APIs & Services > Credentials**, create **OAuth client ID**.
    - Configure consent screen (**External**, add app name, save).
    - Set **Authorized Redirect URI**: `{YOUR_JELLYFIN_ADDRESS}/LocalPosters/GoogleAuthorization/Callback`. NOTE: if you are using different addresses for local and external network you have to add both addresses. Make sure you initiate authorization from the whitelisted base address
    - Select **Web application**, download `client_secrets.json`.
6. **Set Scopes:** Add `https://www.googleapis.com/auth/drive.file`.
7. **Publish Application:** Publish the OAuth consent screen for external access. Please note if you use "Test Audience" refresh token will expire withing 7 days, with current scope application won't require "Verification"
8. **Upload Client Secrets:** Place `client_secrets.json` into directory visible for Jellyfin and change Plugin configuration accordingly.

Once configuration is done, plugin will keep syncing folders and searching for missing images in the selected libraries, no manual interaction is required! 😊

## Support

If you encounter any issues or have questions about the plugin, please open an [issue](https://github.com/NooNameR/Jellyfin.Plugin.LocalPosters/issues) in the GitHub Issues section of the repository.

While GitHub issues are the preferred way to report a bug, feel free to join the discussion on [Trash Discord](https://discord.com/channels/492590071455940612/1342175843069329448).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/NooNameR/Jellyfin.Plugin.LocalPosters/blob/master/LICENSE) file for details.
