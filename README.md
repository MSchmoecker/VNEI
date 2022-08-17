# VNEI - Valheim Not Enough Items

## About
VNEI shows all items and recipes from the vanilla game and other mods inside an ingame GUI for easy access.

![screenshot](https://raw.githubusercontent.com/MSchmoecker/VNEI/master/Docs/WholeScreenshot.png)

![crafting](https://raw.githubusercontent.com/MSchmoecker/VNEI/master/Docs/MeatCrafting.png)

Basic Auga support

![Auga](https://raw.githubusercontent.com/MSchmoecker/VNEI/master/Docs/AugaSidebar.png)

## Manual Installation
This mod requires BepInEx and Jötunn.\
Extract the content of `VNEI/plugins` into the `BepInEx/plugins` folder

This is a client side mod and not everyone has to use it if you are playing in multiplayer.
If items or recipes from other mods are missing, please contact me.

## Controls and settings
### BepInEx config
The config file is generated at `BepInEx/config/com.maxsch.valheim.vnei.cfg` after the first launch, see there for detailed information.

Interesting configs:\
Show Only Known: Hide unknown items and recipes to discover them while playing the game

### Shortcuts
Press 'R' to view a recipe of an item while hovering over it.
Works in the inventory, too.

### Item Blacklist
Items can be blacklisted at `BepInEx/config/com.maxsch.valheim.vnei.blacklist.txt`.
The internal item name has to be used, one name on every line.
If an item is blacklisted it will not show up in the search and recipes using it will be disabled.
Requires a restart to take effect.

### Cheating
If devcommands are enabled, items can be right clicked to spawn them.
This is only possible at a local game or when the user is an admin at a server.
Shift + RightClick spawns a whole stack.

## Ingame Commands
Export all indexed items with `vnei_write_items_file [format]` as a file to the BepInEx root path

format:
- `csv` (default) -> csv can be used with excel or other tools
- `yaml` -> yaml creates a CLLC ItemConfig.yml like file
   yaml optional extra args (both provided as list, separated by ','):
   - a. strings to scan item prefab names for and exclude them from results e.g. RRR,someotherfilter
   - b. strings to scan mod names for and exclude them from results e.g. MonsterMobs,someothermod
- `text` -> simple text

## Development
See [contributing](https://github.com/MSchmoecker/VNEI/blob/master/CONTRIBUTING.md).

## API
Other mods can use events to include custom recipes that can't be indexed.
See more [here](https://github.com/MSchmoecker/VNEI/blob/master/API.md).

## Links
- Thunderstore: https://valheim.thunderstore.io/package/MSchmoecker/VNEI/
- Nexus: https://www.nexusmods.com/valheim/mods/1457
- Github: https://github.com/MSchmoecker/VNEI
- Discord: Margmas#9562

## Changelog
0.7.3
- Fixed plants (carrot/onion/...) were not shown in the search
- Fixed right-click on type toggle was not working when favorites were enabled

0.7.2
- Fixed error when player respawned while having 'Show Only Known' turned on
- Fixed tab buttons had to be pressed twice to correctly switch tabs
- Fixed view recipe shortcut has always switch to the VNEI tab, even when 'Attach To Crafting' is not enabled
- Fixed cheating piece items didn't work sometimes

0.7.1
- Added shortcut to quickly view recipes, hover over an item an press 'R'. Works in the inventory, too
- Added German translation
- Added needed crafting stations to piece recipes
- Fixed cheat was always enabled, regardless of devcommands (still only for admins)
- Changed cheating of pieces will spawn the resources in the player's inventory instead of spawning them in the world. Disabled spawning of items that are neither items or pieces.

0.7.0
- Added possibility to favorite items
- Added option to only show items that are known by the player
- Added option to switch switch between split/only optaining/only using view of recipes
- Added external blacklist for items
- Fixed a tooltip was not showing for Jewelcrafting gem items
- Fixed cheat were enabled even if the player is no admin
- Improved recipe calculation of trees, intermediate prefabs are not shown

0.6.5
- fixed tab button was not correctly working together with Jewelcrafting

0.6.4
- added copy-to-clipboard functionality for item/mod names
- fixed a few items were not showing up in the search
- removed VNEI patcher

0.6.3
- fixed some weird behaviour in Auga, had to wait for Auga to update

0.6.2
- added basic Auga support
- index creature growups
- search also takes description into account

0.6.1
- added render cache, this speeds up the loading time after the first world spawn drastically. Disabled when pre Jotunn 2.4.10 is used
- fixed items were categorized as Undefined not Item
- fixed some monster attacks where visible as items

0.6.0
- the mod name search now works for the vast majority of mods. This works via a BepInEx patcher that looks for AssetBundle loading and therefore doesn't function with procedural or changed GameObject names
- fixed errors with AllTamable prefabs and improved error logging
- fixed wrong level display with ingredients of tools and weapons

0.5.4
- fixed tabs from other mods sometimes needed to be clicked twice after VNEI tab was active, resulting in not correct active tabs

0.5.3
- fixed crash when AttachToCrafting was set to false
- fixed tooltip has not clamped inside screen with Valheim update
- fixed bugs related to wrong hidden/visible UI at startup/changes to AttachToCrafting at runtime

0.5.2
- fixed VNEI has hidden the update tab
- fixed incompatibility with Valheim Recycle on the crafting tab
- removed unnecessary harmony self unpatch

0.5.1
- fixed incompatibility with other mods on the crafting tab

0.5.0
- attach VNEI window to crafting menu, instead of floating separately (default, configurable). This is deactivated when Auga is installed
- added config option to hide the GUI at startup
- fixed the hovering tooltip was not clamped inside the screen borders
- API
    - changed Indexing.DisableItem to public

0.4.0
- added ingame command to export all prefab in the game to yaml, csv or text (thx @Flux)
- UI style change, items now have a background like in the inventory
- level of tool and needed crafting station are shown at the icon
- make indexing event even later to let more time for mods to add new prefabs
- removed alphabetically ordering of item, as it makes no sense with icons anyway

0.3.0
- added the last viewed items at top
- added UI scale config for row and columns
- added hotkey to open/close the UI (default LeftAlt + H)
- added option to set the transparency of the UI
- fixed that the tooltip was not shown sometimes
- fixed mod names of CustomPrefabs were not collected
- fixed spawning food, weapons and armor were not placed inside inventory when possible
- fixed rendered icons not always having full color
- added API
    - added the ability to hook events at Indexing
    - added a selection Popup where an item can be choosen

0.2.3
- added tree drops, trader and obliterator (incinerator) items
- added more item categories (food, armor, weapon) and right click can invert selection
- fixed mouse wheel scrolling not working sometimes
- removed cheat button (was basically useless, cheats are enabled when devcommands are active)
- item tooltip hover now shows immediately

0.2.2
- fixed incompatibility with H&H update

0.2.1
- improved sprite rendering
- added scrolling with mouse wheel to changes pages
- added more rocks and ores to indexing
- added fuel for smelters in recipes
- fixed double cultivated plants was not removed
- fixed recipe title was blurred and icon not hoverable
- fixed cheating was not working when connected to a server
- changed most logging to debug logs

0.2.0
- added ability to search with @ for a mod name. Only works with Jötunn mods
- added crafting station/build tool to recipe
- added quality levels for recipes
- added rendering of sprites from existing objects if none exists. It is not perfect, mainly because they are renders and not dedicated sprites but they are good enough. Works with other mods, too
- added valheim.exe process filter, this will stop the mod be executed by a server as it is fully client-side anyway
- removed scroll view in search, instead there are now pages which can be switched. This is done because of performance, especially heavily modded games will profit a lot
- find attack-prefabs automatically and not from blacklist, reduces number of empty items from mods that add monsters
- changed that items are sorted alphabetically
- changed only buildable pieces are shown
- cheating only works with right mouse button, allowing to still view recipes
- cheating spawns all prefabs, not just items
- renamed config keys to be more readable. The old are not working anymore and the new have their default values set

0.1.0
- added iron ore and trailership to blacklist (this is not iron scrap and longboat!)
- added toggleable categories: items, pieces, creatures and undefined
- added ability to cheat items when devcommands are enabled
- skip not enabled recipes
- greatly increased overall performance in UI
- changed styling of some GUI layouts
- fixed incompatibility with MonsterLabZ
- fixed incompatibility with ChaosArmor

0.0.4
- load search UI after a local player is present. This resolves a conflict with EpicLoot
- fixed text was wobbling inside recipe UI due to resizing

0.0.3
- added more items/recipes
  - mineable rocks (copper rock, ...)
  - DropOnDestroy items (feather from birds, tree stump, ...)
  - treasure chest drops
  - pickable items (stone on ground, bushes, ...)
- added a blacklist for items to not show up. This contains only not obtainable items/effects and the option can be toggled
- showing percentages when only one of a list of items can spawn
- fixed tooltip was not showing up if no localized name was present
- fixed recipe UI has shown last valid icon instead of the placeholder

0.0.2
- made the window draggable
- added placeholder sprite when there is no icon
- fixed the tooltip was rendered under on the right side of the UI
- improved logging. maybe

0.0.1
- Release
