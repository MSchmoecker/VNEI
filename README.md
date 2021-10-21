# VNEI - Valheim Not Enough Items

## About
VNEI shows all items and recipes from the vanilla game and other mods inside an ingame GUI for easy access.

![screenshot](https://raw.githubusercontent.com/MSchmoecker/VNEI/master/Docs/WholeScreenshot.png)

![crafting](https://raw.githubusercontent.com/MSchmoecker/VNEI/master/Docs/MeatCrafting.png)

## Installation
This mod requires BepInEx and Jötunn.\
Extract the content of `VNEI` into the `BepInEx/plugins` folder.

This is a client side mod and not everyone has to use it if you are playing in multiplayer.
If items or recipes from other mods are missing, please contact me.

## Development
BepInEx must be setup at manual or with r2modman/Thunderstore Mod Manager.
Jötunn must be installed.
Everything else is setup to be as automatic as possible, no need to copy any files or folders.

Note the master branch will always use a stable Jötunn version while others may use a directly compiled one.

Create a file called `Environment.props` inside the project root.
Copy the example and change the Valheim install path to your location.
If you use r2modman/Tunderstore Mod Manager you can set the path too, but this is optional.

```
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <PropertyGroup>
        <!-- Needs to be your path to the base Valheim folder -->
        <VALHEIM_INSTALL>E:\Programme\Steam\steamapps\common\Valheim</VALHEIM_INSTALL>
        <!-- Optional, needs to be the path to a r2modmanPlus profile folder -->
        <R2MODMAN_INSTALL>C:\Users\[user]\AppData\Roaming\r2modmanPlus-local\Valheim\profiles\Develop</R2MODMAN_INSTALL>
        <USE_R2MODMAN_AS_DEPLOY_FOLDER>false</USE_R2MODMAN_AS_DEPLOY_FOLDER>
    </PropertyGroup>
</Project>
```
This project requires the publicized Valheim dlls present at `VALHEIM_INSTALL\valheim_Data\Managed\publicized_assemblies\assembly_[assembly]_publicized.dll`.
Use any publicizer, see here for example: https://github.com/CabbageCrow/AssemblyPublicizer

If the paths are set correctly, all assemblies are loaded and the project can be build.
Now you can run `deploy.sh`, this will copy the mod to your BepInEx plugin folder as you setup in `Environment.props`.

If you want to rebuild the asset bundle, open the Unity project.
Because VNEI.dll, Jotunn.dll and dependencies must be present inside Unity, `deploy.sh` had to be ran at least once.
Now you can run `Assets/Build AssetBundles` at the toolbar, this copies the asset bundle back to the C# project.


## API
### Indexing
At first world loading, VNEI indexes all available items and recipes at `DungeonDB.Start` to allow other mods to add there prefabs before.
You can hook events at `Indexing` to add custom items and recipes that were not collected by VNEI.
If these items are vanilla items instead, feel free to open a PR here.

The are called with every GameObject (or Recipe) so it can be tried to get a certain component or name.
The events are called in the following order:

- OnIndexingItems: `Action<GameObject>`
    - item names are registered here. Usually use `Indexing.AddItem(new Item(...))` to add a new item
- OnDisableItems: `Action<GameObject>`
    - disable items that should not be shown inside the UI. Usually use `Indexing.DisableItem(name, context)`
- OnIndexingRecipes: `Action<Recipe>`
    - provides a Valheim Recipe
- OnIndexingItemRecipes: `Action<GameObject>`
    - special recipes as item conversions (smelter, cooking station, ...) or drops are added here.
    Usually use `Indexing.AddRecipeToItems` with a new `RecipeInfo`.

See OnIndexingItemRecipes as example:
```
Indexing.OnIndexingItemRecipes += (prefab) => {
    if (prefab.TryGetComponent(out CookingStation cookingStation)) {
        foreach (CookingStation.ItemConversion conversion in cookingStation.m_conversion) {
            RecipeInfo recipeInfo = new RecipeInfo();
            recipeInfo.SetStation(cookingStation, 1);
            recipeInfo.AddIngredient(conversion.m_from, Amount.One, Amount.One, 1, prefab.name);
            recipeInfo.AddResult(conversion.m_to, Amount.One, Amount.One, 1, prefab.name);
            Indexing.AddRecipeToItems(recipeInfo);
        }
    }
};
```

### Selection Popup
You can use VNEI to open an item selection popup.
```cs
Action<string> onSelect = (prefabName) => {
    // called after selection
};
SelectUI selectUI = SelectUI.CreateSelection(parent, onSelect, Vector2.zero);
```
Type `vnei_toggle_select_test` inside the game console to open the example of `VNEI/APIExample/SelectUITest.cs`


## Links
- Thunderstore: https://valheim.thunderstore.io/package/MSchmoecker/VNEI/
- Nexus: https://www.nexusmods.com/valheim/mods/1457
- Github: https://github.com/MSchmoecker/VNEI
- Discord: Margmas#9562

## Changelog
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
