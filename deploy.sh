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
	PluginFolder="$R2MODMAN_INSTALL/BepInEx/plugins"
else
	PluginFolder="$VALHEIM_INSTALL/BepInEx/plugins"
fi

ModDir="$PluginFolder/$ModName"

# copy to unity
mkdir -p "$ModNameUnity/Assets/Assemblies"
cp "$ModName/obj/Debug/$ModName.dll" "$ModNameUnity/Assets/Assemblies"

cp "$VALHEIM_INSTALL/BepInEx/core/BepInEx.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/BepInEx/core/0Harmony.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/BepInEx/core/Mono.Cecil.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/BepInEx/core/MonoMod.Utils.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/BepInEx/core/MonoMod.RuntimeDetour.dll" "$ModNameUnity/Assets/Assemblies"
[ -f "$PluginFolder/ValheimModding-Jotunn/Jotunn.dll" ] && cp "$PluginFolder/ValheimModding-Jotunn/Jotunn.dll" "$ModNameUnity/Assets/Assemblies"
[ -f "$PluginFolder/Jotunn/Jotunn.dll" ] && cp "$PluginFolder/Jotunn/Jotunn.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_valheim.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_utils.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_postprocessing.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_sunshafts.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_guiutils.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_steamworks.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/valheim_Data/Managed/assembly_googleanalytics.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/BepInEx/plugins/MMHOOK/MMHOOK_assembly_valheim.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/BepInEx/plugins/MMHOOK/MMHOOK_assembly_utils.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/BepInEx/plugins/MMHOOK/MMHOOK_assembly_postprocessing.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/BepInEx/plugins/MMHOOK/MMHOOK_assembly_sunshafts.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/BepInEx/plugins/MMHOOK/MMHOOK_assembly_guiutils.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/BepInEx/plugins/MMHOOK/MMHOOK_assembly_steamworks.dll" "$ModNameUnity/Assets/Assemblies"
cp "$VALHEIM_INSTALL/BepInEx/plugins/MMHOOK/MMHOOK_assembly_googleanalytics.dll" "$ModNameUnity/Assets/Assemblies"
echo Coping to: $ModDir

# copy content
mkdir -p $ModDir
cp "$ModName/obj/Debug/$ModName.dll" $ModDir
