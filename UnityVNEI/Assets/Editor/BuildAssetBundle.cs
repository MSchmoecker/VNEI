using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundle : MonoBehaviour {
    [MenuItem("Assets/Build AssetBundles")]
    private static void BuildAllAssetBundles() {
        BuildPipeline.BuildAssetBundles("AssetBundles/StandaloneWindows", BuildAssetBundleOptions.None,
                                        BuildTarget.StandaloneWindows);
        FileUtil.ReplaceFile("AssetBundles/StandaloneWindows/vnei_AssetBundle", "../VNEI/VNEI_AssetBundle");
    }
}
