using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//ModelImporter参数设置
public class ModelImportAsset : AssetPostprocessor
{
    private void OnPreprocessModel()
    {
        UnityEngine.Debug.Log(assetPath);
        var a = assetImporter;
    }
}