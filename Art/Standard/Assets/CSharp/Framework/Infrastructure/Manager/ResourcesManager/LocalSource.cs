using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Framework.Infrastructure
{
    public class LocalSource : ISource
    {
        private Dictionary<int, int[]> m_dependenciesChain = new Dictionary<int, int[]>();
        private Dictionary<int, string> m_guidPath = new Dictionary<int, string>();
        private Dictionary<int, int> m_guidDepend = new Dictionary<int, int>();

        void ISource.Initialize() {
            AnalizeSourceTable();
        }
        void ISource.Update() { }
        void ISource.LateUpdate() { }
        void ISource.Load(int assetId, DataHandler<UnityEngine.Object> loadHandler,EVariantType variant) {
            int guid = assetId * Const.variantMaxLength + (int)variant;
            if (m_guidPath.ContainsKey(guid))
            {
                string path = m_guidPath[guid];
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
        void ISource.Load(int assetId, DataHandler<UnityEngine.Object[]> loadHandler, EVariantType variant)
        {
            int guid = assetId * Const.variantMaxLength + (int)variant;
            if (m_guidPath.ContainsKey(guid))
            {
                string path = m_guidPath[guid];
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
                                            string path =Define.sourceUrl + ((EVariantType)variantId).ToString()+  assetPath ;
                                            path = path+ GetExtension(path);
                                            m_guidPath.Add(g, path);
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
                                        m_guidDepend.Add(guid, chainId);
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
            else
            {
                Debug.Log.i(Debug.ELogType.Error, "GetExtension  Dont has this path:" + path);
                return null;
            }
        }

    }
}