# VNEI API

## Using the API
`VNEI.dll` needs to be referenced inside the C# project by either copying it to your mod or set the path to your Valheim mod folder.
When coping it, make sure to also have `VNEI.xml` alongside for documentation.
Please don't inlcude `VNEI.dll` in your final mod when publishing to avoid multiple versions, but feel free to upload it to a git repository of course.

Inside your `.csproj`:
```
<Reference Include="VNEI">
    <HintPath>...Path.../VNEI.dll</HintPath>
</Reference>
```

If you use VNEI as a soft dependency, it throws an exception when not installed on a client.
To avoid this, make sure all API calls are spread inside functions that are only called when VNEI is present.

```cs
// global or local function
void UseVNEI() {
  RecipeInfo recipeInfo = new RecipeInfo();
}

// call it only when VNEI is present
if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.maxsch.valheim.vnei")) {
  UseVNEI();
}
```
This will prevent an error, when VNEI isn't installed.

## Indexing
At first world loading, VNEI indexes all available items and recipes at `DungeonDB.Start` to allow other mods to add their prefabs before.
You can hook events at `Indexing` to add custom items and recipes that were not collected by VNEI.
If these items/recipes use vanilla components instead of custom ones, feel free to open an issue or PR here.

The events are called with every GameObject (or Recipe) so it can be tried to get a certain component or name.
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

See OnIndexingItemRecipes as an example in:
```cs
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
This gets every GameObject with a CookingStation component and adds the ItemConversions as a new RecipeInfo respectively.
With `Indexing.AddRecipeToItems(recipeInfo)` the corresponding items (`conversion.m_from`, `conversion.m_to`, `cookingStation`)
know the RecipeInfo and can show them in the UI.
Notice these items are ItemDrops/Pieces and therefore already registered as Items by VNEI.
If this is not the case with your prefabs, they have to be added at `Indexing.OnIndexingItems`.

## Mod Name
If you don't use Jotunn, the mod name cannot be collected automatically due to no common insertion time of prefabs.
But this can be easily be done manual:
```cs
// Mod is your BaseUnityPlugin, with an static reference to it
Indexing.SetModOfPrefab("my_prefab_id", Mod.Instance.Info.Metadata);

// or inside your Mods Awake/Start
Indexing.SetModOfPrefab("my_prefab_id", Info.Metadata);
```

## Selection Popup
You can use VNEI to open an item selection popup.
```cs
Action<string> onSelect = (prefabName) => {
    // called after selection
};
SelectUI selectUI = SelectUI.CreateSelection(parent, onSelect, Vector2.zero);
```
Type `vnei_toggle_select_test` inside the game console to open the example of `VNEI/APIExample/SelectUITest.cs`
