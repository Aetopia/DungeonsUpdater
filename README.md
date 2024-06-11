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
- Download the latest release of Dungeons Updater from [GitHub Releases](https://github.com/Aetopia/DungeonsUpdater/releases/latest).
- Start Dungeons Updater & wait for the game files to be downloaded.
- The game will automatically launch once all required files are downloaded.

## Building
1. Download the following:
    - [.NET SDK](https://dotnet.microsoft.com/en-us/download)
    - [.NET Framework 4.8.1 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net481-developer-pack-offline-installer)

2. Run the following command to compile:

    ```cmd
    dotnet publish "src\DungeonsUpdater.csproj"
    ```