using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace lhFramework.Infrastructure.Managers
{
    using Core;
    public class LocalSource : ISource
    {
        private Dictionary<int, int[]> m_dependenciesChain = new Dictionary<int, int[]>();
        private Dictionary<int, string> m_guidPaths = new Dictionary<int, string>();
        private Dictionary<int, int> m_variantChains = new Dictionary<int, int>();

        void ISource.Initialize() {
            //AnalizeSourceTable();
            AnalizeSourceTableBinary();
        }
        void ISource.Update() { }
        void ISource.LateUpdate() { }
        void ISource.Load(int assetId, DataHandler<UnityEngine.Object> loadHandler,EVariantType variant,bool toAsync) {
            int guid = assetId * Const.variantMaxLength + (int)variant;
            if (m_guidPaths.ContainsKey(guid))
            {
                string path = m_guidPaths[guid];
                UnityEngine.Object obj = null;
#if UNITY_EDITOR
                obj=AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
#endif
                loadHandler(obj);
            }
            else
            {
                Debug.Log.i(Debug.ELogType.Error, "LocalSource dont has this path of assetId:" + assetId + "   " + variant);
                loadHandler(null);
            }
        }
        void ISource.Load(int assetId, DataHandler<UnityEngine.Object[]> loadHandler, EVariantType variant, bool toAsync)
        {
            int guid = assetId * Const.variantMaxLength + (int)variant;
            if (m_guidPaths.ContainsKey(guid))
            {
                string path = m_guidPaths[guid];
                UnityEngine.Object[] obj = null;
#if UNITY_EDITOR
                obj = AssetDatabase.LoadAllAssetsAtPath(path);
#endif
                loadHandler(obj);
            }
            else
            {
                Debug.Log.i(Debug.ELogType.Error, "LocalSource dont has this path of assetId:" + assetId + "   " + variant);
                loadHandler(null);
            }
        }
        UnityEngine.Object[] ISource.Load(int assetId, EVariantType variant)
        {
            int guid = assetId * Const.variantMaxLength + (int)variant;
            if (m_guidPaths.ContainsKey(guid))
            {
                string path = m_guidPaths[guid];
                UnityEngine.Object[] obj = null;
#if UNITY_EDITOR
                obj = AssetDatabase.LoadAllAssetsAtPath(path);
#endif
                return obj;
            }
            else
            {
                Debug.Log.i(Debug.ELogType.Error, "LocalSource dont has this path of assetId:" + assetId + "   " + variant);
                return null;
            }
        }
        UnityEngine.Object ISource.Load(int assetId, string name, EVariantType variant)
        {
            int guid = assetId * Const.variantMaxLength + (int)variant;
            if (m_guidPaths.ContainsKey(guid))
            {
                string path = m_guidPaths[guid];
                UnityEngine.Object obj = null;
#if UNITY_EDITOR
                obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
#endif
                return obj;
            }
            else
            {
                Debug.Log.i(Debug.ELogType.Error, "LocalSource dont has this path of assetId:" + assetId + "   " + variant);
                return null;
            }
        }
        void ISource.UnLoad(int assetId, EVariantType variant) {

        }
        void ISource.Destroy(int assetId, EVariantType variant)
        {

        }
        void ISource.UnloadUnusedAsset()
        {
            Resources.UnloadUnusedAssets();
        }
        void ISource.Dispose()
        {

        }
        void AnalizeSourceTable()
        {
            string tablePath = Define.sourceTableUrl;
            if (!File.Exists(tablePath))
            {
                Debug.Log.i(Debug.ELogType.Error, "dont has sourceTable: " + tablePath);
            }
            using (FileStream fileStream = new FileStream(tablePath, FileMode.Open))
            {
                using (StreamReader sw = new StreamReader(fileStream))
                {
                    StringBuilder str = new StringBuilder();
                    string s = null;
                    int k = 0;
                    List<int> deps = new List<int>();
                    List<int> variant = new List<int>();
                    List<int> depends = new List<int>();
                    while (!string.IsNullOrEmpty((s = sw.ReadLine())))
                    {
                        int chainId = 0;
                        int assetId = 0;
                        string assetPath = "";
                        int guid = 0;
                        deps.Clear();
                        variant.Clear();
                        str.Clear();
                        for (int i = 0, j = 0; i < s.Length; i++)
                        {
                            char c = s[i];
                            if (i == 0 && c == '&')
                            {
                                k++;
                                break;
                            }
                            if (k == 0)//资源路径
                            {
                                if (c == ',' || i == s.Length - 1)
                                {
                                    if (i == s.Length - 1)
                                    {
                                        str.Append(c);
                                    }
                                    if (j == 0)
                                    {
                                        assetId = Convert.ToInt32(str.ToString());
                                    }
                                    else if (j == 1)
                                    {
                                        assetPath = str.ToString();
                                    }
                                    else
                                    {
                                        variant.Add(Convert.ToInt32(str.ToString()));
                                    }
                                    str.Clear();
                                    j++;
                                    if (i == s.Length - 1)
                                    {
                                        for (int m = 0; m < variant.Count; m++)
                                        {
                                            int variantId = variant[m];
                                            int g = assetId * Const.variantMaxLength + variantId;
                                            string path = Define.localSourceUrl + ((EVariantType)variantId).ToString() + "/" + assetPath;
                                            path = path + GetExtension(path);
                                            path = path.Replace(Application.dataPath, "");
                                            path = "Assets" + path;
                                            m_guidPaths.Add(g, path);
                                        }
                                    }
                                }
                                else
                                {
                                    str.Append(c);
                                }
                            }
                            else if (k == 1)//依赖链
                            {
                                if (c == ',' || i == s.Length - 1)
                                {
                                    if (i == s.Length - 1)
                                    {
                                        str.Append(c);
                                    }
                                    if (j == 0)
                                    {
                                        chainId = Convert.ToInt32(str.ToString());
                                    }
                                    else
                                    {
                                        var split = str.ToString().Split('-');
                                        for (int m = 0; m < split.Length; m++)
                                        {
                                            int g = Convert.ToInt32(split[m]) * Const.dependLayerDigit + (j - 1);
                                            deps.Add(g);
                                        }
                                    }
                                    str.Clear();
                                    j++;
                                    if (i == s.Length - 1)
                                    {
                                        m_dependenciesChain.Add(chainId, deps.ToArray());
                                    }
                                }
                                else
                                {
                                    str.Append(c);
                                }
                            }
                            else if (k == 2)//变体数据
                            {
                                if (c == ',' || i == s.Length - 1)
                                {
                                    if (i == s.Length - 1)
                                    {
                                        str.Append(c);
                                    }
                                    if (j == 0)
                                    {
                                        guid = Convert.ToInt32(str.ToString());
                                    }
                                    else
                                    {
                                        chainId = Convert.ToInt32(str.ToString());
                                    }
                                    str.Clear();
                                    j++;
                                    if (i == s.Length - 1)
                                    {
                                        m_variantChains.Add(guid, chainId);
                                    }
                                }
                                else
                                {
                                    str.Append(c);
                                }
                            }
                        }
                    }
                }
            }
        }
        void AnalizeSourceTableBinary()
        {
            string tablePath = Define.sourceTableUrl;
            if (!File.Exists(tablePath))
            {
                Debug.Log.i(Debug.ELogType.Error, "dont has sourceTable: " + tablePath);
            }
            SourceTable sourceTable = null;
            using (FileStream fileStream = new FileStream(tablePath, FileMode.Open))
            {
                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                sourceTable = Utility.ProtobufUtility.Deserialize<SourceTable>(bytes);
            }
            for (int i = 0; i < sourceTable.guidPaths.Count; i++)
            {
                m_guidPaths.Add(sourceTable.guidPaths[i].guid, Define.sourceBundleUrl + sourceTable.guidPaths[i].path);
            }
            for (int i = 0; i < sourceTable.dependenciesChains.Count; i++)
            {
                List<int> arr = new List<int>();
                for (int j = 0; j < sourceTable.dependenciesChains[i].depends.Count; j++)
                {
                    for (int x = 0; x < sourceTable.dependenciesChains[i].depends[j].deps.Count; x++)
                    {
                        arr.Add(sourceTable.dependenciesChains[i].depends[j].deps[x] * Const.dependLayerDigit + j);
                    }
                }
                m_dependenciesChain.Add(sourceTable.dependenciesChains[i].chainId, arr.ToArray());
            }
            for (int i = 0; i < sourceTable.variantChains.Count; i++)
            {
                m_variantChains.Add(sourceTable.variantChains[i].guid, sourceTable.variantChains[i].chainId);
            }
            sourceTable = null;
        }
        string GetExtension(string path)
        {
            if (File.Exists(path + ".fbx")) return ".fbx";
            else if (File.Exists(path + ".FBX")) return ".FBX";
            else if (File.Exists(path + ".mat")) return ".mat";
            else if (File.Exists(path + ".bytes")) return ".bytes";
            else if (File.Exists(path + ".png")) return ".png";
            else if (File.Exists(path + ".prefab")) return ".prefab";
            else if (File.Exists(path + ".exr")) return ".exr";
            else if (File.Exists(path + ".tga")) return ".tga";
            else if (File.Exists(path + ".txt")) return ".txt";
            else if (File.Exists(path + ".mesh")) return ".mesh";
            else if (File.Exists(path + ".anim")) return ".anim";
            else if (File.Exists(path + ".asset")) return ".asset";
            else if (File.Exists(path + ".shader")) return ".shader";
            else
            {
                Debug.Log.i(Debug.ELogType.Error, "GetExtension  Dont has this path:" + path);
                return null;
            }
        }

    }
}