using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Text;
using System;

namespace lhFramework.Tools.Bundle
{
    public class BundleBuildManager
    {
        public string rootName
        {
            get
            {
                if (string.IsNullOrEmpty(m_rootName))
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_rootName"))
                    {
                        m_rootName = EditorPrefs.GetString("BundleBuildManager_rootName");
                    }
                    else
                    {
                        m_rootName = "Resources";
                    }
                }
                return m_rootName;
            }
            set
            {
                EditorPrefs.SetString("BundleBuildManager_rootName", value);
                m_rootName = value;
            }
        }
        public string bundleOutputFolder
        {
            get
            {
                if (string.IsNullOrEmpty(m_bundleOutputFolder))
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_bundleOutputFolder"))
                    {
                        m_bundleOutputFolder = EditorPrefs.GetString("BundleBuildManager_bundleOutputFolder");
                    }
                    else
                    {
                        m_bundleOutputFolder = "Assets/StreamingAssets";
                    }
                }
                return m_bundleOutputFolder;
            }
            set
            {
                EditorPrefs.SetString("BundleBuildManager_bundleOutputFolder", value);
                m_bundleOutputFolder = value;
            }
        }
        public string bundleFolder
        {
            get
            {
                if (string.IsNullOrEmpty(m_bundleFolder))
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_bundleFolder"))
                    {
                        m_bundleFolder = EditorPrefs.GetString("BundleBuildManager_bundleFolder");
                    }
                    else
                    {
                        m_bundleFolder = "Bundle";
                    }
                }
                return m_bundleFolder;
            }
            set
            {
                EditorPrefs.SetString("BundleBuildManager_bundleFolder", value);
                m_bundleFolder = value;
            }
        }
        public int minBundleDependencyCount
        {
            get
            {
                if (m_minBundleDependencyCount == 0)
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_minBundleDependencyCount"))
                    {
                        m_minBundleDependencyCount = EditorPrefs.GetInt("BundleBuildManager_minBundleDependencyCount");
                    }
                    else
                    {
                        m_minBundleDependencyCount = 1;
                    }
                }
                return m_minBundleDependencyCount;
            }
            set
            {
                EditorPrefs.SetInt("BundleBuildManager_minBundleDependencyCount", value);
                m_minBundleDependencyCount = value;
            }
        }
        public int bundleOptions
        {
            get
            {
                if (m_bundleOptions == 0)
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_bundleOptions"))
                    {
                        m_bundleOptions = (BuildAssetBundleOptions)EditorPrefs.GetInt("BundleBuildManager_bundleOptions");
                    }
                    else
                    {
                        m_bundleOptions = 0;
                    }
                }
                return (int)m_bundleOptions;
            }
            set
            {
                EditorPrefs.SetInt("BundleBuildManager_bundleOptions", value);
                m_bundleOptions = (BuildAssetBundleOptions)value;
            }
        }
        public int buildTarget
        {
            get
            {
                if (m_buildTarget == BuildTarget.NoTarget)
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_buildTarget"))
                    {
                        m_buildTarget = (BuildTarget)EditorPrefs.GetInt("BundleBuildManager_buildTarget");
                    }
                    else
                    {
                        m_buildTarget = BuildTarget.NoTarget;
                    }
                }
                return (int)m_buildTarget;
            }
            set
            {
                EditorPrefs.SetInt("BundleBuildManager_buildTarget", value);
                m_buildTarget = (BuildTarget)value;
            }
        }
        public int idBase
        {
            get
            {
                if (m_idBase == 0)
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_idBase"))
                    {
                        m_idBase = EditorPrefs.GetInt("BundleBuildManager_idBase");
                    }
                    else
                    {
                        m_idBase = 10000;
                    }
                }
                return m_idBase;
            }
            set
            {
                EditorPrefs.SetInt("BundleBuildManager_idBase", value);
                m_idBase = value;
            }
        }
        public bool showTable
        {
            get
            {
                if (m_showTable == null)
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_showTable"))
                    {
                        m_showTable = EditorPrefs.GetBool("BundleBuildManager_showTable");
                    }
                    else
                    {
                        m_showTable = false;
                    }
                }
                return (bool)m_showTable;
            }
            set
            {
                EditorPrefs.SetBool("BundleBuildManager_showTable", value);
                m_showTable = value;
            }
        }

        public Color mainBundleColor
        {
            get
            {
                if (m_mainBundleColor == null)
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_mainBundleColor"))
                    {
                        var color = EditorPrefs.GetString("BundleBuildManager_mainBundleColor");
                        var split = color.Split('-');
                        m_mainBundleColor = new Color(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]), Convert.ToSingle(split[2]), Convert.ToSingle(split[3]));
                    }
                    else
                    {
                        m_mainBundleColor = Color.white;
                    }
                }
                return (Color)m_mainBundleColor;
            }
            set
            {
                if (m_mainBundleColor == null)
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_mainBundleColor"))
                    {
                        var color = EditorPrefs.GetString("BundleBuildManager_mainBundleColor");
                        var split = color.Split('-');
                        m_mainBundleColor = new Color(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]), Convert.ToSingle(split[2]), Convert.ToSingle(split[3]));
                    }
                    else
                    {
                        m_mainBundleColor = Color.white;
                    }
                }
                else
                {
                    if (m_sharedBundleColor!=value)
                    {
                        string color = value.r + "-" + value.g + "-" + value.b + "-" + value.a;
                        EditorPrefs.SetString("BundleBuildManager_mainBundleColor", color);
                        m_mainBundleColor = value;
                    }
                }
            }
        }
        public Color sharedBundleColor
        {
            get
            {
                if (m_sharedBundleColor == null)
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_sharedBundleColor"))
                    {
                        var color = EditorPrefs.GetString("BundleBuildManager_sharedBundleColor");
                        var split = color.Split('-');
                        m_sharedBundleColor = new Color(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]), Convert.ToSingle(split[2]), Convert.ToSingle(split[3]));
                    }
                    else
                    {
                        m_sharedBundleColor = new Color(1, 237.0f / 255.0f, 0, 1);
                    }
                }
                return  (Color)m_sharedBundleColor;
            }
            set
            {
                if (m_sharedBundleColor == null)
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_sharedBundleColor"))
                    {
                        var color = EditorPrefs.GetString("BundleBuildManager_sharedBundleColor");
                        var split = color.Split('-');
                        m_sharedBundleColor = new Color(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]), Convert.ToSingle(split[2]), Convert.ToSingle(split[3]));
                    }
                    else
                    {
                        m_sharedBundleColor = new Color(1, 237.0f / 255.0f, 0, 1);
                    }
                }
                else
                {
                    if (m_sharedBundleColor!=value)
                    {
                        string color = value.r + "-" + value.g + "-" + value.b + "-" + value.a;
                        EditorPrefs.SetString("BundleBuildManager_sharedBundleColor", color);
                        m_sharedBundleColor = value;
                    }
                }
            }
        }
        public Color privateBundleColor
        {
            get
            {
                if (EditorPrefs.HasKey("BundleBuildManager_privateBundleColor"))
                {
                    var color = EditorPrefs.GetString("BundleBuildManager_privateBundleColor");
                    var split = color.Split('-');
                    m_privateBundleColor = new Color(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]), Convert.ToSingle(split[2]), Convert.ToSingle(split[3]));
                }
                else
                {
                    m_privateBundleColor = new Color(161.0f / 255.0f, 161.0f / 255.0f, 161.0f / 255.0f, 1);
                }
                return (Color)m_privateBundleColor;
            }
            set
            {
                if (m_privateBundleColor == null)
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_privateBundleColor"))
                    {
                        var color = EditorPrefs.GetString("BundleBuildManager_privateBundleColor");
                        var split = color.Split('-');
                        m_privateBundleColor = new Color(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]), Convert.ToSingle(split[2]), Convert.ToSingle(split[3]));
                    }
                    else
                    {
                        m_privateBundleColor = new Color(161.0f/255.0f, 161.0f / 255.0f, 161.0f / 255.0f, 1);
                    }
                }
                else
                {
                    if (m_privateBundleColor!=value)
                    {
                        string color = value.r + "-" + value.g + "-" + value.b + "-" + value.a;
                        EditorPrefs.SetString("BundleBuildManager_privateBundleColor", color);
                        m_privateBundleColor = value;
                    }
                }
            }
        }
        public Color damageBundleColor
        {
            get
            {
                if (m_damageBundleColor == null)
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_damageBundleColor"))
                    {
                        var color = EditorPrefs.GetString("BundleBuildManager_damageBundleColor");
                        var split = color.Split('-');
                        m_damageBundleColor = new Color(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]), Convert.ToSingle(split[2]), Convert.ToSingle(split[3]));
                    }
                    else
                    {
                        m_damageBundleColor = Color.red;
                    }
                }
                return (Color)m_damageBundleColor;
            }
            set
            {
                if (m_damageBundleColor==null)
                {
                    if (EditorPrefs.HasKey("BundleBuildManager_damageBundleColor"))
                    {
                        var color = EditorPrefs.GetString("BundleBuildManager_damageBundleColor");
                        var split = color.Split('-');
                        m_damageBundleColor = new Color(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]), Convert.ToSingle(split[2]), Convert.ToSingle(split[3]));
                    }
                    else
                    {
                        m_damageBundleColor = Color.red;
                    }
                }
                else
                {
                    if (m_damageBundleColor != value)
                    {
                        string color = value.r + "-" + value.g + "-" + value.b + "-" + value.a;
                        EditorPrefs.SetString("BundleBuildManager_damageBundleColor", color);
                        m_damageBundleColor = value;
                    }
                }
            }
        }

        private Color? m_mainBundleColor = null;
        private Color? m_sharedBundleColor = null;
        private Color? m_privateBundleColor = null;
        private Color? m_damageBundleColor = null;

        private string m_rootName;
        private string m_bundleOutputFolder;
        private string m_bundleFolder;
        private string m_bundleStuff;
        private int m_minBundleDependencyCount;
        private BundlePack m_bundlePack = new BundlePack();
        private BuildAssetBundleOptions m_bundleOptions = BuildAssetBundleOptions.None;
        private BuildTarget m_buildTarget = BuildTarget.NoTarget;

        private bool? m_showTable;
        private string m_rootPath;
        private string m_bundleRootPath;
        private string m_manifestPath;
        private string m_tableFileName = "SourceTable.txt";
        private string m_editorTableFileName = "EditorSourceTable.txt";
        private int m_idBase;
        private UnityEngine.AssetBundleManifest m_manifest;
        public Dictionary<string, CategoryData> categoryData { get; private set; }
        public Dictionary<string, List<SourceDirectory>> searchedDirectories { get; private set; }
        public Dictionary<string, List<SourceFile>> searchedFiles { get; private set; }

        public List<List<List<int>>> dependChains = new List<List<List<int>>>();
        public Dictionary<int, BaseSource> allSourcesGuid = new Dictionary<int, BaseSource>();
        private Dictionary<string, BaseSource> m_allSourcesPath = new Dictionary<string, BaseSource>();
        public BundleBuildManager()
        {
        }
        public void Initialize()
        {
            if (buildTarget == (int)BuildTarget.NoTarget)
            {
                buildTarget = (int)EditorUserBuildSettings.activeBuildTarget;
            }
            m_rootPath = Application.dataPath + "/" + rootName + "/";
            m_bundleRootPath = Application.streamingAssetsPath + "/" + ((BuildTarget)buildTarget).ToString() + "/" + bundleFolder + "/";
            m_manifestPath = m_bundleRootPath + bundleFolder;
            categoryData = new Dictionary<string, CategoryData>();
            searchedDirectories = new Dictionary<string, List<SourceDirectory>>();
            searchedFiles = new Dictionary<string, List<SourceFile>>();
            if (File.Exists(m_manifestPath))
            {
                var manifestBundle = AssetBundle.LoadFromFile(m_manifestPath);
                m_manifest = (AssetBundleManifest)manifestBundle.LoadAsset("AssetBundleManifest");
                manifestBundle.Unload(false);
                var mainAssetBundles = m_manifest.GetAllAssetBundles();
                for (int i = 0; i < mainAssetBundles.Length; i++)
                {
                    AnalizeBundle(mainAssetBundles[i]);
                }
            }
            AnalizeFiles();
            AnalizeEditorTable();
            AnalizeSourceState();
            AnalizeDependency();
            AnalizeChain();

        }
        public void BuildPackage()
        {
            m_bundlePack.StartBuild();
            PackBundle(m_rootPath);
            PackSourceTable();
            PackEditorSourceTable();
            m_bundlePack.BuildOver();
            AssetDatabase.Refresh();
        }
        public void BuildTable()
        {
            PackSourceTable();
        }
        public void Clear()
        {
            string[] str=AssetDatabase.GetAllAssetBundleNames();
            for (int i = 0; i < str.Length; i++)
            {
                AssetDatabase.RemoveAssetBundleName(str[i],true);
            }
        }
        public void DeletePackge()
        {
            string outPath = Path.Combine(Path.Combine(bundleOutputFolder, ((BuildTarget)buildTarget).ToString()), bundleFolder);
            outPath = outPath.Replace("\\", "/");
            AssetDatabase.DeleteAsset(outPath);
        }
        public bool ResetBaseId(string category, int newId)
        {
            if (newId / m_idBase < 0) return false;
            if (newId % m_idBase > 0) return false;
            foreach (var item in categoryData)
            {
                if (item.Value.baseId == newId)
                {
                    return false;
                }
            }
            var ites = categoryData[category];
            foreach (var ite in ites.variantDic)
            {
                foreach (var it in ite.Value.directoriesDic)
                {
                    it.Value.assetId = it.Value.assetId % ites.baseId + newId;
                    it.Value.oldAssetId = it.Value.assetId;
                    it.Value.guid = it.Value.assetId * 10 + GetVariantInt(it.Value.variantName);
                }
                foreach (var it in ite.Value.filesDic)
                {
                    it.Value.assetId = it.Value.assetId % ites.baseId + newId;
                    it.Value.oldAssetId = it.Value.assetId;
                    it.Value.guid = it.Value.assetId * 10 + GetVariantInt(it.Value.variantName);
                }
            }
            ites.baseId = newId;
            ites.oldId = newId;
            return true;
        }
        public bool ResetBundleId(string category,string variantName, string fileName, int newId, bool isDir)
        {
            if (newId % idBase <= 0) return false;
            foreach (var item in categoryData)
            {
                foreach (var variant in item.Value.variantDic)
                {
                    foreach (var it in variant.Value.directoriesDic)
                    {
                        if (it.Value.assetId == newId)
                        {
                            return false;
                        }
                    }
                    foreach (var it in variant.Value.filesDic)
                    {
                        if (it.Value.assetId == newId)
                        {
                            return false;
                        }
                    }
                }
            }
            if (isDir)
            {
                foreach (var item in categoryData[category].variantDic)
                {
                    var sou = item.Value.directoriesDic[fileName];
                    sou.assetId = newId;
                    sou.oldAssetId = newId;
                    sou.guid = sou.assetId * 10 + GetVariantInt(sou.variantName);
                }
                return true;
            }
            else
            {
                foreach (var item in categoryData[category].variantDic)
                {
                    var sou = item.Value.filesDic[fileName];
                    sou.assetId = newId;
                    sou.oldAssetId = newId;
                    sou.guid = sou.assetId * 10 + GetVariantInt(sou.variantName);
                }
                return true;
            }
        }
        public void Search(string value, ESearchType searchType)
        {
            searchedDirectories.Clear();
            searchedFiles.Clear();
            if (searchType == ESearchType.Id)
            {
                try
                {
                    int id = Convert.ToInt32(value);
                    foreach (var category in categoryData)
                    {
                        foreach (var variant in category.Value.variantDic)
                        {
                            foreach (var dirs in variant.Value.directoriesDic)
                            {
                                if (dirs.Value.assetId == id)
                                {
                                    if (!searchedDirectories.ContainsKey(category.Key))
                                    {
                                        searchedDirectories.Add(category.Key, new List<SourceDirectory>());
                                    }
                                    searchedDirectories[category.Key].Add(dirs.Value);
                                }
                            }
                            foreach (var dirs in variant.Value.filesDic)
                            {
                                if (dirs.Value.assetId == id)
                                {
                                    if (!searchedFiles.ContainsKey(category.Key))
                                    {
                                        searchedFiles.Add(category.Key, new List<SourceFile>());
                                    }
                                    searchedFiles[category.Key].Add(dirs.Value);
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
            }
            else if (searchType == ESearchType.Category)
            {
                try
                {
                    foreach (var category in categoryData)
                    {
                        if (category.Key == value.ToLower())
                        {
                            foreach (var variant in category.Value.variantDic)
                            {
                                foreach (var dirs in variant.Value.directoriesDic)
                                {
                                    if (!searchedDirectories.ContainsKey(category.Key))
                                    {
                                        searchedDirectories.Add(category.Key, new List<SourceDirectory>());
                                    }
                                    searchedDirectories[category.Key].Add(dirs.Value);
                                }
                                foreach (var dirs in variant.Value.filesDic)
                                {
                                    if (!searchedFiles.ContainsKey(category.Key))
                                    {
                                        searchedFiles.Add(category.Key, new List<SourceFile>());
                                    }
                                    searchedFiles[category.Key].Add(dirs.Value);
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
            }
            else if (searchType == ESearchType.BundleName)
            {
                try
                {
                    foreach (var category in categoryData)
                    {
                        foreach (var variant in category.Value.variantDic)
                        {
                            foreach (var dirs in variant.Value.directoriesDic)
                            {
                                if (dirs.Value.fileName.ToLower().Contains(value.ToLower()))
                                {
                                    if (!searchedDirectories.ContainsKey(category.Key))
                                    {
                                        searchedDirectories.Add(category.Key, new List<SourceDirectory>());
                                    }
                                    searchedDirectories[category.Key].Add(dirs.Value);
                                }
                            }
                            foreach (var dirs in variant.Value.filesDic)
                            {
                                if (dirs.Value.fileName.ToLower().Contains(value.ToLower()))
                                {
                                    if (!searchedFiles.ContainsKey(category.Key))
                                    {
                                        searchedFiles.Add(category.Key, new List<SourceFile>());
                                    }
                                    searchedFiles[category.Key].Add(dirs.Value);
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
            }
        }
        public void ResetSearch()
        {
            searchedDirectories.Clear();
            searchedFiles.Clear();
        }
        private void PackBundle(string folderPaths)
        {
            string outPath = Path.Combine(Path.Combine(bundleOutputFolder, ((BuildTarget)(buildTarget)).ToString()), bundleFolder);
            outPath = outPath.Replace("\\", "/");
            foreach (var category in categoryData)
            {
                foreach (var variant in category.Value.variantDic)
                {
                    foreach (var it in variant.Value.directoriesDic)
                    {
                        if (it.Value.dontDamage)
                        {
                            m_bundlePack.PackEntire(category.Key, it.Value);
                        }
                        else
                        {
                            string str = outPath + "/" + it.Value.bundleName + "." + variant.Key;
                            str = str.Replace("\\", "/");
                            AssetDatabase.DeleteAsset(str);
                            AssetDatabase.DeleteAsset(str + ".manifest");
                        }
                    }
                    foreach (var it in variant.Value.filesDic)
                    {
                        if (it.Value.dontDamage)
                        {
                            m_bundlePack.PackSingle(category.Key, it.Value);
                        }
                        else
                        {
                            string str = outPath + "/" + it.Value.bundleName+"."+ variant.Key;
                            str = str.Replace("\\", "/");
                            AssetDatabase.DeleteAsset(str);
                            AssetDatabase.DeleteAsset(str + ".manifest");
                        }
                    }
                }
            }
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            BuildPipeline.BuildAssetBundles(outPath, (BuildAssetBundleOptions)bundleOptions, (BuildTarget)buildTarget);
        }
        private void PackSourceTable()
        {
            string tablePath = m_bundleRootPath + m_tableFileName;
            if (File.Exists(tablePath))
            {
                File.Delete(tablePath);
            }
            using (FileStream fileStream = new FileStream(tablePath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fileStream))
                {
                    Dictionary<int, string> pathDic = new Dictionary<int, string>();
                    Dictionary<int, List<int>> variantDic = new Dictionary<int, List<int>>();
                    foreach (var item in categoryData)
                    {
                        foreach (var its in item.Value.variantDic)
                        {
                            foreach (var it in its.Value.directoriesDic)
                            {
                                if (it.Value.filesDic.Count>0)
                                {
                                    if (it.Value.fileState == ESourceState.MainBundle || it.Value.fileState == ESourceState.SharedBundle)
                                    {
                                        if (!pathDic.ContainsKey(it.Value.assetId))
                                        {
                                            pathDic.Add(it.Value.assetId, it.Value.bundleName);
                                            variantDic.Add(it.Value.assetId, new List<int>() { GetVariantInt(it.Value.variantName) });
                                        }
                                        else
                                        {
                                            int variantId = GetVariantInt(it.Value.variantName);
                                            if (!variantDic[it.Value.assetId].Contains(variantId))
                                                variantDic[it.Value.assetId].Add(variantId);
                                        }
                                    }
                                }
                            }
                            foreach (var it in its.Value.filesDic)
                            {
                                if (it.Value.fileState == ESourceState.MainBundle || it.Value.fileState == ESourceState.SharedBundle)
                                {
                                    if (!pathDic.ContainsKey(it.Value.assetId))
                                    {
                                        pathDic.Add(it.Value.assetId, it.Value.bundleName);
                                        variantDic.Add(it.Value.assetId, new List<int>() { GetVariantInt(it.Value.variantName) });
                                    }
                                    else
                                    {
                                        int variantId = GetVariantInt(it.Value.variantName);
                                        if (!variantDic[it.Value.assetId].Contains(variantId))
                                            variantDic[it.Value.assetId].Add(variantId);
                                    }
                                }
                            }
                        }
                    }
                    foreach (var item in pathDic)
                    {
                        var varientList = variantDic[item.Key];
                        string vars = "";
                        for (int i = 0; i < varientList.Count; i++)
                        {
                            vars += "," + varientList[i];
                        }
                        sw.WriteLine(item.Key + "," + item.Value+vars);
                    }
                    sw.WriteLine("&");
                    for (int m = 0; m < dependChains.Count;m++)
                    {
                        string deps = "";
                        var chains = dependChains[m];
                        string str = null;
                        for (int i = 0; i < chains.Count; i++)
                        {
                            var chain = chains[i];
                            if (chain.Count>0)
                            {
                                for (int j = 0,k=0; j < chain.Count; j++)
                                {
                                    int chainId = chain[j];
                                    var source = allSourcesGuid[chainId];
                                    if(source.fileState==ESourceState.MainBundle|| source.fileState==ESourceState.SharedBundle)
                                    {
                                        if (k==0)
                                        {
                                            str += ",";
                                        }
                                        else
                                        {
                                            str += "-";
                                        }
                                        str += chainId;
                                        k++;
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(str))
                        {
                            deps += str;
                            sw.WriteLine(m + deps);
                        }
                    }
                    sw.WriteLine("&");
                    foreach (var item in allSourcesGuid)
                    {
                        if (item.Value.fileState==ESourceState.MainBundle|| item.Value.fileState==ESourceState.SharedBundle)
                        {
                            sw.WriteLine(item.Key + "," + item.Value.dependenciesChain);
                        }
                    }
                    sw.Flush();
                }
            }
        }
        private void PackEditorSourceTable()
        {
            string tablePath = m_bundleRootPath + m_editorTableFileName;
            if (File.Exists(tablePath))
            {
                File.Delete(tablePath);
            }
            using (FileStream fileStream = new FileStream(tablePath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fileStream))
                {
                    foreach (var item in categoryData)
                    {
                        Dictionary<string, BaseSource> sources = new Dictionary<string, BaseSource>();
                        foreach (var its in item.Value.variantDic)
                        {
                            foreach (var it in its.Value.directoriesDic)
                            {
                                if (!sources.ContainsKey(it.Key))
                                {
                                    sources.Add(it.Key,it.Value);
                                }
                            }
                            foreach (var it in its.Value.filesDic)
                            {
                                if (!sources.ContainsKey(it.Key))
                                {
                                    sources.Add(it.Key, it.Value);
                                }
                                sw.WriteLine(item.Key + "," + item.Value.baseId + "," + it.Key + "," + it.Value.assetId + ",0");
                            }
                        }
                        foreach (var its in sources)
                        {
                            sw.WriteLine(item.Key + "," + item.Value.baseId + "," + its.Key + "," + its.Value.assetId + ",1");
                        }
                    }
                    sw.Flush();
                }
            }
        }
        private void AnalizeBundle(string bundlePath)
        {
            string path = m_bundleRootPath + bundlePath;
            var b = AssetBundle.LoadFromFile(path);
            if (b != null)
            {
                int fileSize = Mathf.CeilToInt(new FileInfo(path).Length / 1024);
                var assets = b.GetAllAssetNames();
                if (assets.Length > 1)
                {
                    var info = new DirectoryInfo(path);
                    string category = info.Parent.Name;
                    string categoryLower = category.ToLower();
                    string dirName = info.Name.Substring(0, info.Name.LastIndexOf('.')); ;
                    string dirLowerName = dirName.ToLower();
                    string variantName = info.Name.LastIndexOf('.')<0?null: info.Name.Substring(info.Name.LastIndexOf('.')+1);
                    variantName = string.IsNullOrEmpty(variantName) ? EVariantType.n.ToString() : variantName;
                    if (!categoryData.ContainsKey(categoryLower))
                    {
                        categoryData.Add(categoryLower, new CategoryData());
                    }
                    if (!categoryData[categoryLower].variantDic.ContainsKey(variantName))
                    {
                        categoryData[categoryLower].variantDic.Add(variantName, new VariantData());
                    }
                    var sourDir = new SourceDirectory();
                    sourDir.fileName = dirName;
                    sourDir.bundleName = categoryLower + "/" + dirLowerName;
                    sourDir.variantName = variantName;
                    sourDir.category = category;
                    sourDir.bundleSize = fileSize;
                    sourDir.dontDamage = true;

                    categoryData[categoryLower].category = category;
                    var variant = categoryData[categoryLower].variantDic[variantName];
                    variant.directoriesDic.Add(dirLowerName, sourDir);
                    variant.variantName = variantName;
                    for (int j = 0; j < assets.Length; j++)
                    {
                        var filePath = "";
                        var split = assets[j].Split('/');
                        filePath = split[0].Substring(0, 1).ToUpper() + split[0].Substring(1);
                        for (int x = 1; x < split.Length; x++)
                        {
                            filePath += "/" + split[x].Substring(0, 1).ToUpper() + split[x].Substring(1);
                        }
                        FileInfo fileInfo = new FileInfo(filePath);
                        string fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                        string fileLowerName = fileName.ToLower();
                        sourDir.filesDic.Add(fileLowerName, new SourceFile()
                        {
                            fileName = fileName,
                            bundleName = categoryLower.ToLower() + '/' + dirName,
                            variantName = variantName,
                            category=category,
                            dontDamage = true
                        });
                    }
                }
                else
                {
                    FileInfo info = new FileInfo(path);
                    string category = info.DirectoryName.Substring(info.DirectoryName.LastIndexOf('\\') + 1);
                    string categoryLower = category.ToLower();
                    string variantName = info.Name.Substring(info.Name.LastIndexOf('.') + 1);
                    string fileName = info.Name.Substring(0,info.Name.LastIndexOf('.')).ToLower();
                    if (!categoryData.ContainsKey(categoryLower))
                    {
                        categoryData.Add(categoryLower, new CategoryData());
                    }
                    if (!categoryData[categoryLower].variantDic.ContainsKey(variantName))
                    {
                        categoryData[categoryLower].variantDic.Add(variantName, new VariantData());
                    }

                    if (!categoryData[categoryLower].variantDic[variantName].filesDic.ContainsKey(fileName))
                    {
                        categoryData[categoryLower].variantDic[variantName].filesDic.Add(fileName, new SourceFile());
                    }
                    categoryData[categoryLower].category = category;
                    var variant = categoryData[categoryLower].variantDic[variantName];
                    variant.variantName = variantName;
                    var sourceFile = variant.filesDic[fileName];
                    sourceFile.fileName = info.Name;
                    sourceFile.bundleSize = fileSize;
                    sourceFile.bundleName = categoryLower + "/" + fileName;
                    sourceFile.variantName = variantName;
                    sourceFile.category = category;
                }
                b.Unload(true);
            }
        }
        private void AnalizeFiles()
        {
            if (!Directory.Exists(m_rootPath)) return;
            var varis= Directory.GetDirectories(m_rootPath, "*", SearchOption.TopDirectoryOnly);
            for (int m = 0; m < varis.Length; m++)
            {
                var variDirInfo = new DirectoryInfo(varis[m]);
                var variantName = variDirInfo.Name;
                var directors = Directory.GetDirectories(varis[m], "*", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < directors.Length; i++)
                {
                    var dir = directors[i];
                    DirectoryInfo info = new DirectoryInfo(dir);
                    var directoryInfoArr = info.GetDirectories();
                    string categoryName = info.Name;
                    string categoryNameLower = categoryName.ToLower();
                    for (int j = 0; j < directoryInfoArr.Length; j++)
                    {
                        if (directoryInfoArr[j].Extension == ".meta") continue;
                        var dirInfo = directoryInfoArr[j];
                        string dirName = dirInfo.Name;
                        string dirLowerName = dirName.ToLower();
                        var childFilesInfoArr = directoryInfoArr[j].GetFiles();
                        if (categoryData.ContainsKey(categoryNameLower))
                        {
                            var category = categoryData[categoryNameLower];
                            if (category.variantDic.ContainsKey(variantName))
                            {
                                var variant = category.variantDic[variantName];
                                if (!variant.directoriesDic.ContainsKey(dirLowerName))
                                {
                                    variant.directoriesDic.Add(dirLowerName, new SourceDirectory()
                                    {
                                        fileName = dirName,
                                        bundleName = categoryNameLower + "/" + dirLowerName,
                                        variantName=variantName,
                                        category= category.category,
                                        dontDamage = true
                                    });
                                }
                            }
                            else
                            {
                                var variant = new VariantData();
                                variant.variantName = variantName;
                                variant.directoriesDic.Add(dirLowerName, new SourceDirectory()
                                {
                                    fileName = dirName,
                                    bundleName = categoryNameLower + "/" + dirLowerName,
                                    variantName=variantName,
                                    category=category.category,
                                    dontDamage = true
                                });
                                category.variantDic.Add(variantName, variant);
                            }
                        }
                        else
                        {
                            CategoryData sourceData = new CategoryData();
                            sourceData.category = categoryName;
                            sourceData.baseId = 0;
                            sourceData.oldId = 0;
                            var variant = new VariantData();
                            variant.variantName = variantName;
                            sourceData.variantDic.Add(variantName, variant);
                            variant.directoriesDic.Add(dirLowerName, new SourceDirectory()
                            {
                                fileName = dirName,
                                bundleName = categoryNameLower + "/" + dirLowerName,
                                variantName=variantName,
                                category=categoryName,
                                dontDamage = true
                            });
                            categoryData.Add(categoryNameLower, sourceData);
                        }
                        var sourDir = categoryData[categoryNameLower].variantDic[variantName].directoriesDic[dirLowerName];
                        for (int x = 0; x < childFilesInfoArr.Length; x++)
                        {
                            if (childFilesInfoArr[x].Extension == ".meta") continue;
                            if (childFilesInfoArr[x].Extension == ".cs") continue;
                            string ext = childFilesInfoArr[x].Name.Substring(childFilesInfoArr[x].Name.LastIndexOf("."));
                            string fileName = childFilesInfoArr[x].Name.Substring(0, childFilesInfoArr[x].Name.LastIndexOf("."));
                            string fileLowerName = fileName.ToLower();
                            string bundleName = info.Name.ToLower() + "/" + fileLowerName;
                            string fileAssetPath = "Assets/" + m_rootName + "/"+variantName+"/" + info.Name + "/" + dirName + "/" + fileName + ext; ;
                            string fileAssetName = info.Name + "/" + dirName + "/" + fileName + ext;
                            List<string> dependencies = new List<string>();
                            if (sourDir.filesDic.ContainsKey(fileLowerName))
                            {
                                sourDir.filesDic[fileLowerName].fileName = fileName;
                                sourDir.filesDic[fileLowerName].fileExtension = ext;
                                sourDir.filesDic[fileLowerName].mainAssetPath = fileAssetPath;
                                sourDir.filesDic[fileLowerName].mainAssetName = fileAssetName;
                                sourDir.filesDic[fileLowerName].bundleName = bundleName;
                                sourDir.filesDic[fileLowerName].dontDamage = true;
                            }
                            else
                            {
                                sourDir.filesDic.Add(fileLowerName, new SourceFile()
                                {
                                    fileName = fileName,
                                    fileExtension = ext,
                                    mainAssetPath = fileAssetPath,
                                    mainAssetName = fileAssetName,
                                    bundleName = bundleName,
                                    dontDamage = true,
                                    variantName=variantName,
                                    category=categoryName
                                });
                            }
                        }
                    }
                    var filesInfoArr = info.GetFiles();
                    for (int j = 0; j < filesInfoArr.Length; j++)
                    {
                        if (filesInfoArr[j].Extension == ".meta") continue;
                        if (filesInfoArr[j].Extension == ".cs") continue;
                        string ext = filesInfoArr[j].Name.Substring(filesInfoArr[j].Name.LastIndexOf("."));
                        string fileName = filesInfoArr[j].Name.Substring(0, filesInfoArr[j].Name.LastIndexOf("."));
                        string fileLowerName = fileName.ToLower();
                        string bundleName = info.Name.ToLower() + "/" + fileLowerName;
                        string fileAssetPath = "Assets/" + m_rootName + "/"+variantName+"/" + info.Name + "/" + fileName + ext; ;
                        string fileAssetName = info.Name + "/" + fileName + ext;
                        List<string> dependencies = new List<string>();
                        if (categoryData.ContainsKey(categoryNameLower))
                        {
                            if (categoryData[categoryNameLower].variantDic.ContainsKey(variantName))
                            {
                                var variant = categoryData[categoryNameLower].variantDic[variantName];
                                if (variant.filesDic.ContainsKey(fileLowerName))
                                {
                                    variant.filesDic[fileLowerName].fileName = fileName;
                                    variant.filesDic[fileLowerName].fileExtension = ext;
                                    variant.filesDic[fileLowerName].mainAssetPath = fileAssetPath;
                                    variant.filesDic[fileLowerName].mainAssetName = fileAssetName;
                                    variant.filesDic[fileLowerName].bundleName = bundleName;
                                    variant.filesDic[fileLowerName].dontDamage = true;
                                }
                                else
                                {
                                    variant.filesDic.Add(fileLowerName, new SourceFile()
                                    {
                                        fileName = fileName,
                                        fileExtension = ext,
                                        mainAssetPath = fileAssetPath,
                                        mainAssetName = fileAssetName,
                                        bundleName = bundleName,
                                        dontDamage = true,
                                        variantName=variantName,
                                        category=categoryName
                                    });
                                }
                            }
                            else
                            {
                                var variantData = new VariantData();
                                variantData.variantName = variantName;
                                variantData.filesDic.Add(fileLowerName, new SourceFile()
                                {
                                    fileName = fileName,
                                    fileExtension = ext,
                                    mainAssetPath = fileAssetPath,
                                    mainAssetName = fileAssetName,
                                    bundleName = bundleName,
                                    dontDamage = true,
                                    assetId = 0,
                                    oldAssetId = 0,
                                    variantName=variantName,
                                    category=categoryName
                                });
                                categoryData[categoryNameLower].variantDic.Add(variantName, variantData);
                            }
                        }
                        else
                        {
                            CategoryData data = new CategoryData();
                            data.category = categoryName;
                            data.baseId = 0;
                            data.oldId = 0;
                            var variant = new VariantData();
                            variant.variantName = variantName;
                            data.variantDic.Add(variantName, variant);
                            variant.filesDic.Add(fileLowerName, new SourceFile()
                            {
                                fileName = fileName,
                                fileExtension = ext,
                                mainAssetPath = fileAssetPath,
                                mainAssetName = fileAssetName,
                                bundleName = bundleName,
                                dontDamage = true,
                                variantName=variantName,
                                category=categoryName
                            });
                            categoryData.Add(categoryNameLower, data);
                        }

                    }

                }
            }

        }
        private void AnalizeDependency()
        {
            foreach (var sourceList in allSourcesGuid)
            {
                var source = sourceList.Value;
                if (source is SourceDirectory)
                {
                    var sourceDir = source as SourceDirectory;
                    foreach (var sourceFile in sourceDir.filesDic)
                    {
                        string fileAssetPath = sourceFile.Value.mainAssetPath;
                        var depArr = AssetDatabase.GetDependencies(fileAssetPath);
                        for (int x = 0; x < depArr.Length; x++)
                        {
                            if (depArr[x] == fileAssetPath) continue;
                            if (m_allSourcesPath.ContainsKey(depArr[x]))
                            {
                                var sources = m_allSourcesPath[depArr[x]];
                                sourceFile.Value.fileDependencies.Add(sources.guid);
                                sourceDir.fileDependencies.Add(sources.guid);
                                sources.fileDependencied.Add(sourceDir.guid);
                            }
                            else
                            {
                                //UnityEngine.Debug.LogWarning("dont has this path: of  Source:"+ fileAssetPath+"\n" + depArr[x]);
                            }
                        }
                    }
                }
                else
                {
                    var sourceFile = source as SourceFile;
                    string fileAssetPath = sourceFile.mainAssetPath;
                    var depArr = AssetDatabase.GetDependencies(fileAssetPath);
                    for (int x = 0; x < depArr.Length; x++)
                    {
                        if (depArr[x] == fileAssetPath) continue;
                        if (m_allSourcesPath.ContainsKey(depArr[x]))
                        {
                            var sources = m_allSourcesPath[depArr[x]];
                            sourceFile.fileDependencies.Add(sources.guid);
                            sources.fileDependencied.Add(sourceFile.guid);
                        }
                        else
                        {
                            //UnityEngine.Debug.LogWarning("dont has this path: of  Source:" + fileAssetPath + "\n" + depArr[x]);
                        }
                    }
                }
            }
        }
        private void AnalizeChain()
        {
            foreach (var source in allSourcesGuid)
            {
                var baseSource = source.Value;
                if (baseSource.fileDependencied.Count <= 0)
                {
                    baseSource.fileState = ESourceState.MainBundle;
                    List<int> chain = new List<int>();
                    chain.Add(source.Key);
                    dependChains.Add(new List<List<int>>() { chain});
                    int chainId = dependChains.Count-1;
                    baseSource.dependenciedChainList.Add(chainId);
                    baseSource.dependenciesChain = chainId;
                    for (int j = 0; j < baseSource.fileDependencies.Count; j++)
                    {
                        var depGuid = baseSource.fileDependencies[j];
                        if (!chain.Contains(depGuid))
                        {
                            chain.Add(depGuid);
                            if (!allSourcesGuid[depGuid].dependenciedChainList.Contains(depGuid))
                            {
                                allSourcesGuid[depGuid].dependenciedChainList.Add(chainId);
                                if (allSourcesGuid[depGuid].dependenciedChainList.Count > minBundleDependencyCount)
                                {
                                    allSourcesGuid[depGuid].fileState = ESourceState.SharedBundle;
                                }
                            }
                        }
                    }
                }
            }
            foreach (var source in allSourcesGuid)
            {
                var baseSource = source.Value;
                if (baseSource.fileState==ESourceState.SharedBundle)
                {
                    List<int> chain = new List<int>();
                    chain.Add(source.Key);
                    dependChains.Add(new List<List<int>>() { chain });
                    int chainId = dependChains.Count - 1;
                    baseSource.dependenciesChain=chainId;
                    for (int j = 0; j < baseSource.fileDependencies.Count; j++)
                    {
                        var depGuid = baseSource.fileDependencies[j];
                        if (!chain.Contains(depGuid))
                        {
                            chain.Add(depGuid);
                        }
                    }
                }
            }

            for (int i = 0; i < dependChains.Count; i++)
            {
                var chain = dependChains[i];
                var l = chain[0];
                for (int j = 1; j < l.Count; j++)
                {
                    var guid = l[j];
                    var baseSource = allSourcesGuid[guid];
                    var deps = baseSource.fileDependencies;
                    int guidLayer = 0;
                    for (int x = 0; x < deps.Count; x++)
                    {
                        var depSource = allSourcesGuid[deps[x]];
                        if (depSource.fileState == ESourceState.SharedBundle)
                        {
                            bool has = false;
                            int r = 0;
                            for (int m = 0; m < chain.Count; m++)
                            {
                                for (int n = 0; n < chain[m].Count; n++)
                                {
                                    if (m == 0 && n == 0) continue;
                                    if (chain[m][n] == guid) continue;
                                    if (chain[m][n] == deps[x])
                                    {
                                        r = m;
                                        has = true;
                                        break;
                                    }
                                }
                                if (has)
                                {
                                    break;
                                }
                            }
                            if (has)
                            {
                                int nextR = r + 1;
                                if (nextR>guidLayer)
                                {
                                    if (chain.Count <= nextR)
                                    {
                                        chain.Add(new List<int>() { guid });
                                    }
                                    else
                                    {
                                        chain[nextR].Add(guid);
                                    }
                                    chain[guidLayer].Remove(guid);
                                    guidLayer = nextR;
                                }
                                else
                                {

                                }
                                j =0;
                            }
                        }
                    }
                }
                chain.Add(new List<int>() { chain[0][0] });
                chain[0].RemoveAt(0);
                if (chain[0].Count<=0)
                {
                    chain.RemoveAt(0);
                }
            }
        }
        private void AnalizeEditorTable()
        {
            string tablePath = m_bundleRootPath + m_editorTableFileName;
            if (File.Exists(tablePath))
            {
                using (FileStream fileStream = new FileStream(tablePath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fileStream))
                    {
                        string s;
                        StringBuilder str = new StringBuilder();
                        while ((s = sr.ReadLine()) != null)
                        {
                            str.Clear();
                            string categoryName = "";
                            int baseId = 0;
                            string fileName = "";
                            int id = 0;
                            bool isDir = false;
                            for (int i = 0, j = 0; i < s.Length; i++)
                            {
                                char c = s[i];
                                if (c == ',' || i == s.Length - 1)
                                {
                                    if (j == 0)
                                    {
                                        categoryName = str.ToString();
                                    }
                                    else if (j == 1)
                                    {
                                        baseId = Convert.ToInt32(str.ToString());
                                    }
                                    else if (j == 2)
                                    {
                                        fileName = str.ToString();
                                    }
                                    else if (j == 3)
                                    {
                                        id = Convert.ToInt32(str.ToString());
                                    }
                                    else if (j == 4)
                                    {
                                        str.Append(c);
                                        isDir = str.ToString() == "1";
                                    }
                                    str.Clear();
                                    j++;
                                }
                                else
                                {
                                    str.Append(c);
                                }
                            }
                            if (categoryData.ContainsKey(categoryName))
                            {
                                categoryData[categoryName].baseId = baseId;
                                categoryData[categoryName].oldId = baseId;
                                foreach (var variantItem in categoryData[categoryName].variantDic)
                                {
                                    var variant = variantItem.Value;
                                    if (isDir)
                                    {
                                        if (variant.directoriesDic.ContainsKey(fileName))
                                        {
                                            int guid = id * 10 + GetVariantInt(variant.variantName);
                                            var dir = variant.directoriesDic[fileName];
                                            dir.assetId = id;
                                            dir.oldAssetId = id;
                                            dir.guid = guid;
                                        }
                                    }
                                    else
                                    {
                                        if (variant.filesDic.ContainsKey(fileName))
                                        {
                                            variant.filesDic[fileName].assetId = id;
                                            variant.filesDic[fileName].oldAssetId = id;
                                            variant.filesDic[fileName].guid = id * 10 + GetVariantInt(variant.variantName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (var category in categoryData)
            {
                if (category.Value.baseId<=0)
                {
                    int baseId = GetNextBaseId();
                    category.Value.baseId = baseId;
                    category.Value.oldId = baseId;
                }
                Dictionary<string, List<BaseSource>> sources = new Dictionary<string, List<BaseSource>>();
                foreach (var variant in category.Value.variantDic)
                {
                    foreach (var item in variant.Value.directoriesDic)
                    {
                        if (item.Value.assetId<=0)
                        {
                            if (sources.ContainsKey(item.Value.fileName))
                            {
                                sources[item.Value.fileName].Add(item.Value);
                            }
                            else
                            {
                                int id = GetNextBundleId(category.Value);
                                item.Value.assetId = id;
                                item.Value.oldAssetId = id;
                                item.Value.guid = id * 10 +GetVariantInt( variant.Value.variantName);
                                sources.Add(item.Value.fileName, new List<BaseSource>() { item.Value });
                            }
                        }
                    }
                    foreach (var item in variant.Value.filesDic)
                    {
                        if (item.Value.assetId <= 0)
                        {
                            if (sources.ContainsKey(item.Value.fileName))
                            {
                                sources[item.Value.fileName].Add(item.Value);
                            }
                            else
                            {
                                int id = GetNextBundleId(category.Value);
                                item.Value.assetId = id;
                                item.Value.oldAssetId = id;
                                item.Value.guid = id * 10 + GetVariantInt(variant.Value.variantName);
                                sources.Add(item.Value.fileName, new List<BaseSource>() { item.Value });
                            }
                        }
                    }
                }
                foreach (var item in sources)
                {
                    int id = 0;
                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        if (i==0)
                        {
                            id = item.Value[0].assetId;
                        }
                        else
                        {
                            item.Value[i].assetId = id;
                            item.Value[i].oldAssetId = id;
                            item.Value[i].guid = id * 10 + GetVariantInt(item.Value[i].variantName);
                        }
                    }
                }
            }
        }
        private void AnalizeSourceState()
        {
            foreach (var category in categoryData)
            {
                int id = category.Value.baseId;
                foreach (var variant in category.Value.variantDic)
                {
                    foreach (var dir in variant.Value.directoriesDic)
                    {
                        if (!dir.Value.dontDamage)
                        {
                            dir.Value.fileState = ESourceState.Damage;
                        }
                        else
                        {
                            allSourcesGuid.Add(dir.Value.guid,dir.Value);
                            foreach (var it in dir.Value.filesDic)
                            {
                                m_allSourcesPath.Add(it.Value.mainAssetPath, dir.Value);
                            }
                        }
                    }
                    foreach (var dir in variant.Value.filesDic)
                    {
                        if (!dir.Value.dontDamage)
                        {
                            dir.Value.fileState = ESourceState.Damage;
                        }
                        else
                        {
                            allSourcesGuid.Add(dir.Value.guid, dir.Value);
                            m_allSourcesPath.Add(dir.Value.mainAssetPath, dir.Value);
                        }
                    }
                }
            }
        }
        private int GetNextBundleId(CategoryData data)
        {
            int baseId = data.baseId;
            int nextId = baseId++;
            List<int> m_idList = new List<int>();
            foreach (var variant in data.variantDic)
            {
                foreach (var it in variant.Value.directoriesDic)
                {
                    if (it.Value.assetId>0)
                    {
                        m_idList.Add(it.Value.assetId);
                    }
                }
                foreach (var it in variant.Value.filesDic)
                {
                    if (it.Value.assetId > 0)
                    {
                        m_idList.Add(it.Value.assetId);
                    }
                }
            }
            m_idList.Sort();
            for (int i = 0; i < m_idList.Count; i++)
            {
                if (i == 0)
                {
                    nextId = m_idList[0];
                }
                if (m_idList[i] > nextId)
                {
                    break;
                }
                nextId++;
            }

            return nextId;
        }
        private int GetNextBaseId()
        {
            int baseId = idBase;
            List<int> m_idList = new List<int>();
            foreach (var item in categoryData)
            {
                m_idList.Add(item.Value.baseId);
            }
            m_idList.Sort();
            for (int i = 0; i < m_idList.Count; i++)
            {
                if (i == 0)
                {
                    baseId = m_idList[0];
                }
                if (m_idList[i] > baseId)
                {
                    break;
                }
                baseId += m_idBase;
            }
            return baseId;
        }
        private int GetVariantInt(string variantName)
        {
            return (int)Enum.Parse(typeof(EVariantType), variantName);
        }
    }

}