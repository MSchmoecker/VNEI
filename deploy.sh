ModName="VNEI"

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
	ModDir="$R2MODMAN_INSTALL/BepInEx/plugins/$ModName"
else
	ModDir="$VALHEIM_INSTALL/BepInEx/plugins/$ModName"
fi

echo Coping to: $ModDir

# copy content
mkdir -p $ModDir
cp "$ModName/obj/Debug/$ModName.dll" $ModDir
