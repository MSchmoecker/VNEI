# VNEI - Valheim Not Enough Items

## About
VNEI shows all items and recipes from the vanilla game and other mods inside an ingame GUI for easy access.

![screenshot](https://raw.githubusercontent.com/MSchmoecker/VNEI/master/Docs/WholeScreenshot.png)

![crafting](https://raw.githubusercontent.com/MSchmoecker/VNEI/master/Docs/Crafting.png)


## Manual Installation
This mod requires BepInEx and Jötunn.\
Extract the content of `VNEI/plugins` into the `BepInEx/plugins` folder

This is a client side mod and not everyone has to use it if you are playing in multiplayer.
If items or recipes from other mods are missing, please contact me.


## Feature Overview

### BepInEx config
The config file is generated at `BepInEx/config/com.maxsch.valheim.vnei.cfg` after the first launch, see there for detailed information.

Some interesting configs
- Show Only Known: Hide unknown items and recipes to discover them while playing the game


### Shortcuts
Press 'R' to view a recipe of an item while hovering over it.
Works in the inventory, too.

Press `LeftArrow`/`RightArrow` to navigate through the history of last viewed items.


### Search
The search bar can be used to filter items by name.
It is case insensitive and searches through the localized name, the internal ID and description of the item.
The search is separated by spaces and each word must be contained somewhere in the item to be displayed.
With `@ModName` the search can be limited to a specific mod.


### UI Changes
The mod name of every item is added to the tooltip.
Can be disabled in the config file.


### Item Blacklist
Items can be blacklisted at `BepInEx/config/com.maxsch.valheim.vnei.blacklist.txt`.
The internal item name has to be used, one name on every line.
If an item is blacklisted it will not show up in the search and recipes using it will be disabled.
Requires a restart to take effect.


### Cheating
If devcommands or the no build cost world modifier are enabled, items can be right clicked to spawn them.
Requires the config option `Allow Cheating` to be enabled as well.
If the world modifier are not set, it's only possible at a local game or when the user is an admin at a server.\
Shift + RightClick spawns a whole stack.\
Ctrl + RightClick spawns the item ingredients instead of the item itself.

## Ingame Commands
Export all indexed items as a file to the BepInEx root path:

| Command                                                                    | Description                                                                                                                                                                                                                                     |
|----------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| vnei_export_csv [comma&#124;semicolon&#124;tab]                            | table format, can be read by Excel or similar programs. The separator is optional and `comma` by default                                                                                                                                        |
| vnei_export_yaml [exclude_item1,...] [exclude_mod1,...] [include_mod1,...] | creates a CLLC ItemConfig.yml like file with the option to include certain items or exclude/include certain mods. `Valheim` are vanilla items, mod names can be partial. If include mods are specified, only items from those mods are exported |
| vnei_export_text                                                           | simple text                                                                                                                                                                                                                                     |
| vnei_export_icons [mod1,mod2]                                              | extracts all icons to the BepInEx root path as png files. Mod names are optional to limit the export, `Valheim` are vanilla icons                                                                                                               |


## Development
See [contributing](https://github.com/MSchmoecker/VNEI/blob/master/CONTRIBUTING.md).


## API
Other mods can use events to include custom recipes that can't be indexed.
See more [here](https://github.com/MSchmoecker/VNEI/blob/master/API.md).


## Links
- Thunderstore: https://valheim.thunderstore.io/package/MSchmoecker/VNEI/
- Nexus: https://www.nexusmods.com/valheim/mods/1457
- Github: https://github.com/MSchmoecker/VNEI
- Discord: Margmas. Feel free to DM or ping me, for example in the [Jötunn discord](https://discord.gg/DdUt6g7gyA)


## Changelog
See [here](https://github.com/MSchmoecker/VNEI/blob/master/CHANGELOG.md).

## Credits

- <a href="https://www.flaticon.com/free-icons/hello" title="hello icons">Hello icons created by Vitaly Gorbachev - Flaticon</a>
