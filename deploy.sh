ModName="VNEI"
ModNameUnity="UnityVNEI"

# function for xml reading
read_dom () {
    local IFS=\>
    read -d \< ENTITY CONTENT
}

# read install folder from environment
while read_dom; do
	if [[ $ENTITY = "VALHEIM_INSTALL" ]]; then
		VALHEIM_INSTALL=$CONTENT
	fi
	if [[ $ENTITY = "R2MODMAN_INSTALL" ]]; then
		R2MODMAN_INSTALL=$CONTENT
	fi
	if [[ $ENTITY = "USE_R2MODMAN_AS_DEPLOY_FOLDER" ]]; then
		USE_R2MODMAN_AS_DEPLOY_FOLDER=$CONTENT
	fi
done < Environment.props

# set ModDir
if $USE_R2MODMAN_AS_DEPLOY_FOLDER; then
  BEPINEX_INSTALL="$R2MODMAN_INSTALL/BepInEx"
else
  BEPINEX_INSTALL="$VALHEIM_INSTALL/BepInEx"
fi

PluginFolder="$BEPINEX_INSTALL/plugins"
ModDir="$PluginFolder/$ModName"

# copy to unity
mkdir -p "$ModNameUnity/Assets/Assemblies"
mkdir -p "$ModNameUnity/AssetBundles/StandaloneWindows"

cp "$ModName/bin/Debug/$ModName.dll" "$ModNameUnity/Assets/Assemblies"

cp "$BEPINEX_INSTALL/core/BepInEx.dll" "$ModNameUnity/Assets/Assemblies"
cp "$BEPINEX_INSTALL/core/0Harmony.dll" "$ModNameUnity/Assets/Assemblies"
cp "$BEPINEX_INSTALL/core/Mono.Cecil.dll" "$ModNameUnity/Assets/Assemblies"
cp "$BEPINEX_INSTALL/core/MonoMod.Utils.dll" "$ModNameUnity/Assets/Assemblies"
cp "$BEPINEX_INSTALL/core/MonoMod.RuntimeDetour.dll" "$ModNameUnity/Assets/Assemblies"
[ -f "$PluginFolder/ValheimModding-Jotunn/Jotunn.dll" ] && cp "$PluginFolder/ValheimModding-Jotunn/Jotunn.dll" "$ModNameUnity/Assets/Assemblies"
[ -f "$PluginFolder/Jotunn/Jotunn.dll" ] && cp "$PluginFolder/Jotunn/Jotunn.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_valheim.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_utils.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_postprocessing.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_sunshafts.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_guiutils.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_steamworks.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_googleanalytics.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/PlayFab.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/PlayFabParty.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/Fishlabs.Core.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/Fishlabs.Common.dll" "$ModNameUnity/Assets/Assemblies"

echo "$ModDir"

# copy content
mkdir -p "$ModDir"
cp "$ModName/bin/Debug/$ModName.dll" "$ModDir"
cp "$ModName/bin/Debug/$ModName.xml" "$ModDir"
cp README.md "$ModDir"
cp CHANGELOG.md "$ModDir"
cp manifest.json "$ModDir"
cp icon.png "$ModDir"

# make zip files
cd "$ModDir" || exit

[ -f "$ModName.zip" ] && rm "$ModName.zip"
[ -f "$ModName-Nexus.zip" ] && rm "$ModName-Nexus.zip"

mkdir -p plugins
cp "$ModName.dll" plugins

zip "$ModName.zip" "$ModName.dll" "$ModName.xml" README.md CHANGELOG.md manifest.json icon.png
zip -r "$ModName-Nexus.zip" plugins

rm -r plugins
