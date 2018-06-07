using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace lhFramework.Tools.Bundle
{
    using Infrastructure.Managers;
    using System.IO;
    using Infrastructure.Core;

    public class BundleRuntimeWindow : EditorWindow
    {
        private BundleSource m_bundleLocal;
        private Vector2 m_waitScrollPos;
        private Vector2 m_loadingScrollPos;
        private Vector2 m_usedScrollPos;
        private Vector2 m_paramScrollPos;
        private Vector2 m_commandScrollPos;
        private List<Dictionary<int, SourceData>> m_runtimeData = new List<Dictionary<int, SourceData>>();
        private List<string> m_commandData = new List<string>();
        private int m_maxStoreCommand = 120;
        private int m_currentCommand = 0;
        private int m_selectedCommand;
        private int m_selectedData;
        private float m_paramTextWidth = 150;
        private static BundleRuntimeWindow m_instance;
        [MenuItem("Tools/Bundle/RuntimeWindow")]
        static void Init()
        {
            m_instance = BundleRuntimeWindow.GetWindow<BundleRuntimeWindow>("BundleRuntimeWindow");
            m_instance.Initialize();
            //m_instance.minSize = new Vector2(1000, 1000);
            m_instance.Show();
        }
        void Initialize()
        {
#if !DEVELOPMENT
            if (m_bundleLocal == null && ResourcesManager.source != null)
            {
                m_bundleLocal = ResourcesManager.source as BundleSource;
                m_bundleLocal.loadEventHandler += OnLoad;
                m_bundleLocal.unLoadEventHandler += OnUnload;
                m_bundleLocal.destroyEventHandler += ONDestroy;
                m_bundleLocal.addToLoadEventHandler += OnAddToLoad;
                m_bundleLocal.loadedEventHandler += OnLoaded;
            }
#endif
        }
        void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("RuntimeWindow must run player.....");
                m_bundleLocal = null;
                return;
            }
#if !DEVELOPMENT
            Initialize();
#endif
            if (m_bundleLocal == null) return;
            if (m_instance == null)
            {
                Close();
                Init();
                return;
            }
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                //if (GUILayout.Button("Build", EditorStyles.toolbarButton))
                //{
                //    Close();
                //    Init();
                //    EditorGUILayout.EndHorizontal();
                //    return;
                //}
                GUILayout.FlexibleSpace();

                GUILayout.Space(8);
                if (GUILayout.Button("Settings", EditorStyles.toolbarButton))
                {

                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                {
                    RenderSourceData(m_bundleLocal.waitLoadSources);
                    GUILayout.Label("", EditorStyles.textField, GUILayout.Height(1));
                    RenderCommand();
                }
                EditorGUILayout.EndVertical();
                GUILayout.Label("", EditorStyles.textField, GUILayout.Width(1), GUILayout.Height(position.height));
                //GUI.Label(new Rect(position.width - 300 + 1, 20, 1, position.height - 180 + 5), "", EditorStyles.textField);
                RenderParam();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("", GUILayout.Height(position.height));
            //GUI.Label(new Rect(0, position.height - 198 + 40, 400, 1), "", EditorStyles.textField);
        }
        void RenderSourceData(Dictionary<int, SourceData> sourceDic)
        {
            float windowWidth = position.width;
            float windowHeight = position.height;
            m_waitScrollPos = EditorGUILayout.BeginScrollView(m_waitScrollPos, false, true, GUILayout.MinHeight(windowHeight - 180), GUILayout.ExpandHeight(false));
            {
                EditorGUILayout.BeginVertical(GUILayout.Height(m_runtimeData.Count > 0 ? m_runtimeData[m_selectedCommand].Count * 20 + 10 : 100));
                {
                    GUILayout.Label("");
                    Color color = GUI.color;
                    int iKey = 0;
                    GUI.Box(new Rect(0, 0, windowWidth - 200, 20), "");
                    GUI.Label(new Rect(8, 0, 100, 20), "id");
                    GUI.Label(new Rect(60, 0, 100, 20), "guid");
                    GUI.Label(new Rect(220, 0, 100, 20), "state");
                    GUI.Label(new Rect(320, 0, 200, 20), "dependCount");
                    iKey++;
                    if (m_runtimeData.Count > 0)
                    {
                        var dataList = m_runtimeData[m_selectedCommand];
                        foreach (var dataVa in dataList)
                        {
                            var data = dataVa.Value;
                            if (m_selectedData == dataVa.Key)
                            {
                                GUI.color = Color.yellow;
                                GUI.Box(new Rect(0, 20 * iKey, 3000, 20), "");
                            }
                            GUI.Label(new Rect(8, 20 * iKey, 100, 20), iKey.ToString());
                            GUI.Label(new Rect(60, 20 * iKey, 100, 20), data.guid.ToString());
                            GUI.Label(new Rect(220, 20 * iKey, 100, 20), data.state.ToString());
                            GUI.Label(new Rect(320, 20 * iKey, 100, 20), data.dependencieds.Count.ToString());
                            if (Event.current.type == EventType.MouseUp)
                            {
                                if (new Rect(0, iKey * 20, windowWidth - 200, 20).Contains(Event.current.mousePosition))
                                {
                                    m_selectedData = dataVa.Key;
                                    Repaint();
                                }
                            }
                            GUI.color = color;
                            iKey++;
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }
        void RenderParam()
        {
            m_paramScrollPos = EditorGUILayout.BeginScrollView(m_paramScrollPos, false, true, GUILayout.Width(300), GUILayout.Height(position.height - 20), GUILayout.ExpandWidth(false));
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(m_paramTextWidth));
                {
                    if (m_runtimeData.Count > m_selectedCommand)
                    {
                        if (m_runtimeData[m_selectedCommand].ContainsKey(m_selectedData))
                        {
                            var data = m_runtimeData[m_selectedCommand][m_selectedData];
                            GUILayout.Label("Guid:  " + data.guid);
                            GUILayout.Label("AssetPath:  " + data.assetPath);
                            GUILayout.Label("State:  " + data.state);
                            GUILayout.Label("AssetBundle:  " + data.bundle);
                            m_paramTextWidth = m_paramTextWidth < data.assetPath.Length * 8 ? data.assetPath.Length * 8 : m_paramTextWidth;
                            GUILayout.Label("Dependencied");
                            GUILayout.Label("", EditorStyles.textField, GUILayout.Height(1));
                            for (int i = 0; i < data.dependencieds.Count; i++)
                            {
                                GUILayout.Label("  " + data.dependencieds[i].ToString());
                            }
                            GUILayout.Label("", EditorStyles.textField, GUILayout.Height(1));
                            EditorGUILayout.Separator();
                            EditorGUILayout.ObjectField("mainAsset", data.mainAsset, typeof(UnityEngine.Object), true, GUILayout.Width(280));
                            EditorGUILayout.Separator();
                            GUILayout.Label("AllAssets");
                            GUILayout.Label("", EditorStyles.textField, GUILayout.Height(1));
                            if (data.allAssets != null)
                            {
                                for (int i = 0; i < data.allAssets.Length; i++)
                                {
                                    EditorGUILayout.ObjectField("", data.allAssets[i], typeof(UnityEngine.Object), true, GUILayout.Width(280));
                                }
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label("");
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }
        void RenderCommand()
        {
            m_commandScrollPos = EditorGUILayout.BeginScrollView(m_commandScrollPos, GUILayout.Height(150));
            {
                EditorGUILayout.BeginVertical(GUILayout.Height(m_commandData.Count * 20));
                {
                    GUILayout.Label("", GUILayout.Height(20));
                    for (int i = 0; i < m_commandData.Count; i++)
                    {
                        Color color = GUI.color;
                        if (m_selectedCommand == i)
                        {
                            GUI.color = Color.yellow;
                            GUI.Box(new Rect(0, 20 * i, 3000, 20), "");
                        }
                        GUI.Label(new Rect(8, i * 20, position.width, 20), m_commandData[i]);
                        GUI.color = color;
                        if (Event.current.type == EventType.MouseUp)
                        {
                            if (new Rect(0, i * 20, position.width, 20).Contains(Event.current.mousePosition))
                            {
                                m_selectedCommand = i;
                                Repaint();
                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }
        void OnLoad(int guid)
        {
            AddCommand(guid, "Load");
        }
        void OnUnload(int guid)
        {
            AddCommand(guid, "UnLoad");
        }
        void ONDestroy(int guid)
        {
            AddCommand(guid, "Destroy");
        }
        void OnLoaded(int guid)
        {
            AddCommand(guid, "Loaded");
        }
        void OnAddToLoad(int guid)
        {
            AddCommand(guid, "AddToLoad");
        }
        void AddCommand(int guid, string command)
        {
            var waitValues = m_bundleLocal.waitLoadSources.Values;
            SourceData[] waitData = new SourceData[waitValues.Count];
            waitValues.CopyTo(waitData, 0);
            var loadingValues = m_bundleLocal.loadingSources.Values;
            SourceData[] loadingData = new SourceData[loadingValues.Count];
            loadingValues.CopyTo(loadingData, 0);
            var usedValues = m_bundleLocal.usedSources.Values;
            SourceData[] usedData = new SourceData[usedValues.Count];
            usedValues.CopyTo(usedData, 0);

            Dictionary<int, SourceData> data = new Dictionary<int, SourceData>();
            foreach (var item in m_bundleLocal.waitLoadSources)
            {
                SourceData d = new SourceData();
                d.guid = item.Value.guid;
                d.mainAsset = item.Value.mainAsset;
                d.state = item.Value.state;
                d.allAssets = item.Value.allAssets;
                d.bundle = item.Value.bundle;
                d.assetPath = item.Value.assetPath.Replace(Define.sourceUrl, "");
                d.dependencieds = new List<int>();
                for (int i = 0; i < item.Value.dependencieds.Count; i++)
                {
                    d.dependencieds.Add(item.Value.dependencieds[i]);
                }
                data.Add(item.Value.guid, d);
            }
            foreach (var item in m_bundleLocal.loadingSources)
            {
                SourceData d = new SourceData();
                d.guid = item.Value.guid;
                d.mainAsset = item.Value.mainAsset;
                d.state = item.Value.state;
                d.allAssets = item.Value.allAssets;
                d.bundle = item.Value.bundle;
                d.assetPath = item.Value.assetPath.Replace(Define.sourceUrl, "");
                d.dependencieds = new List<int>();
                for (int i = 0; i < item.Value.dependencieds.Count; i++)
                {
                    d.dependencieds.Add(item.Value.dependencieds[i]);
                }
                data.Add(item.Value.guid, d);
            }
            foreach (var item in m_bundleLocal.usedSources)
            {
                SourceData d = new SourceData();
                d.guid = item.Value.guid;
                d.mainAsset = item.Value.mainAsset;
                d.state = item.Value.state;
                d.allAssets = item.Value.allAssets;
                d.bundle = item.Value.bundle;
                d.assetPath = item.Value.assetPath.Replace(Define.sourceUrl, "");
                d.dependencieds = new List<int>();
                for (int i = 0; i < item.Value.dependencieds.Count; i++)
                {
                    d.dependencieds.Add(item.Value.dependencieds[i]);
                }
                data.Add(item.Value.guid, d);
            }

            m_runtimeData.Add(data);
            m_commandData.Add("[" + Time.realtimeSinceStartup + "] " + " [" + guid + "] " + command);
            if (m_runtimeData.Count > m_maxStoreCommand)
            {
                m_runtimeData.RemoveAt(0);
                m_commandData.RemoveAt(0);
            }
            if (m_selectedCommand == m_currentCommand)
            {
                m_selectedCommand = m_commandData.Count - 1;
            }
            m_currentCommand = m_commandData.Count - 1;
            Repaint();
        }
        void PingObject(UnityEngine.Object obj)
        {
            int instanceID = obj.GetInstanceID();
            EditorGUIUtility.PingObject(instanceID);
            Selection.activeObject = obj;
        }
    }
}