> [!CAUTION]
> **Not approved by or associated with Mojang or Microsoft.**<br>
> **This project doesn't allow you to pirate Minecraft Dungeons, you must own it.**

# Dungeons Updater
Download, install & update Minecraft Dungeons.

## Features
- Instantly update, download and install Minecraft Dungeons.
- Decouples the game from the Minecraft Launcher & Xbox App.<br>This makes it possible to deploy the game under a Windows compatibility layer.
- Portable, the game is placed alongside the updater.

## Prerequisites
- A Microsoft account that owns Minecraft Dungeons.
- Hardware & software that fulfill the [system requirements](https://help.minecraft.net/hc/en-us/articles/360038937032-Minecraft-Dungeons-Minimum-Specifications-for-Gameplay) for Minecraft Dungeons.

## Usage
- Install Dungeons Updater:
    - Install via [Scoop](https://scoop.sh):

        ```
        scoop bucket add games
        scoop install dungeonsupdater
        ```
    - Download the latest release of Dungeons Updater from [GitHub Releases](https://github.com/Aetopia/DungeonsUpdater/releases/latest).

- Start Dungeons Updater & wait for the game files to be downloaded.

- The game will automatically launch once all required files are downloaded.

## FAQ
- Can I play Minecraft Dungeons offline?<br>
    - Dungeons Updater downloads the launcher version of Minecraft Dungeons.<br>You will need an active internet connection when launching the game.<br>After that the game can be played offline.

- How can I repair Minecraft Dungeons?
    - Dungeons Updater automatically verifies & repairs the installation as needed.

- How can I skip the startup movies?
    - Open the following file:

        ```
        %LOCALAPPDATA%\Dungeons\Saved\Config\WindowsNoEditor\Game.ini
        ```
    - Add the following content and save:

        ```ini
        [/Script/MoviePlayer.MoviePlayerSettings]
        bWaitForMoviesToComplete=False
        bMoviesAreSkippable=True
        StartupMovies=
        ```

- How can I setup Minecraft Dungeons for modding?
    - [Refer to this help article on DokuCraft's website.]((https://stash.dokucraft.co.uk/?help=modding-dungeons-launcher))
    - The installation directory resides alongside Dungeons Updater with the folder name of `Content`.

## Building
1. Download the following:
    - [.NET SDK](https://dotnet.microsoft.com/en-us/download)
    - [.NET Framework 4.8.1 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net481-developer-pack-offline-installer)

2. Run the following command to compile:

    ```cmd
    dotnet publish "src\DungeonsUpdater.csproj"
    ```