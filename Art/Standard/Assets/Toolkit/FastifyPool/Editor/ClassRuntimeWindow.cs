using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace lhFramework.Tools.Pools
{
    using Infrastructure.Managers;
    using System;

    public class ClassRuntimeWindow :EditorWindow
    {
        private Vector2 m_waitScrollPos;
        private Vector2 m_loadingScrollPos;
        private Vector2 m_usedScrollPos;
        private Vector2 m_paramScrollPos;
        private Vector2 m_commandScrollPos;
        private List<Dictionary<int, ClassPool>> m_runtimeData = new List<Dictionary<int, ClassPool>>();
        private List<string> m_commandData = new List<string>();
        private int m_maxStoreCommand = 120;
        private int m_currentCommand = 0;
        private int m_selectedCommand;
        private int m_selectedData;
        private float m_paramTextWidth = 150;
        private List<ClassPool> pool = new List<ClassPool>();
        private List<Dictionary<int, ClassPool>> dicPool = new List<Dictionary<int, ClassPool>>();
        private static ClassRuntimeWindow m_instance;
        [MenuItem("Tools/FastifyPool/ClassRuntime")]
        static void Init()
        {
            m_instance = ClassRuntimeWindow.GetWindow<ClassRuntimeWindow>("ClassRuntimeWindow");
            m_instance.Initialize();
            m_instance.minSize = new Vector2(1200, 650);
            m_instance.Show();
        }
        void Initialize()
        {
            ClassManager.getHandler = OnGet;
            ClassManager.storeHandler = OnStore;
            ClassManager.freeHandler = OnFree;
            ClassManager.clearHandler = OnClear;
        }
        void OnDestroy()
        {
            ClassManager.getHandler = null;
            ClassManager.storeHandler = null;
            ClassManager.freeHandler = null;
            ClassManager.clearHandler = null;
        }
        void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("RuntimeWindow must run player.....");
                return;
            }
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
                    RenderSourceData();
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
        void RenderSourceData()
        {
            float windowWidth = position.width;
            float windowHeight = position.height;
            m_waitScrollPos = EditorGUILayout.BeginScrollView(m_waitScrollPos, false, true, GUILayout.MinHeight(windowHeight - 180), GUILayout.ExpandHeight(false));
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(820),GUILayout.Height(m_runtimeData.Count > 0 ? m_runtimeData[m_selectedCommand].Count * 20 + 10 : 100));
                {
                    GUILayout.Label("");
                    Color color = GUI.color;
                    int iKey = 0;
                    GUI.Box(new Rect(0, 0, windowWidth - 200, 20), "");
                    GUI.Label(new Rect(8, 0, 100, 20), "id");
                    GUI.Label(new Rect(60, 0, 100, 20), "typeName");
                    GUI.Label(new Rect(220, 0, 100, 20), "guid");
                    GUI.Label(new Rect(320, 0, 100, 20), "group");
                    GUI.Label(new Rect(420, 0, 200, 20), "gettingCount");
                    GUI.Label(new Rect(520, 0, 200, 20), "freeingCount");
                    GUI.Label(new Rect(620, 0, 200, 20), "idleList");
                    GUI.Label(new Rect(720, 0, 200, 20), "allList");
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
                            GUI.Label(new Rect(60, 20 * iKey, 100, 20), data.typeName);
                            GUI.Label(new Rect(220, 20 * iKey, 100, 20), data.index.ToString());
                            GUI.Label(new Rect(320, 20 * iKey, 100, 20), data.group.ToString());
                            GUI.Label(new Rect(420, 20 * iKey, 100, 20), data.getCount.ToString());
                            GUI.Label(new Rect(520, 20 * iKey, 100, 20), data.freeCount.ToString());
                            GUI.Label(new Rect(620, 20 * iKey, 100, 20), data.freeList.Count.ToString());
                            GUI.Label(new Rect(720, 20 * iKey, 100, 20), data.allList.Count.ToString());
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
                            GUILayout.Label("Guid:  " + data.index);
                            GUILayout.Label("Group:  " + data.group);
                            GUILayout.Label("Type:  " + data.type.FullName);
                            GUILayout.Label("TypeName:  " + data.typeName);
                            GUILayout.Label("StoreCount:  " + data.storeCount);
                            GUILayout.Label("GettingCount:  " + data.getCount);
                            GUILayout.Label("FreeingCount:  " + data.freeCount);
                            GUILayout.Label("IdleList:  " + data.freeList.Count);
                            GUILayout.Label("AllList:  " + data.allList.Count);
                            m_paramTextWidth = m_paramTextWidth < data.type.FullName.Length * 8 ? data.type.FullName.Length * 8 : m_paramTextWidth;
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
        void OnGet(Type type)
        {
            AddCommand(type, "Get");
        }
        void OnFree(Type type)
        {
            AddCommand(type, "Free");
        }
        void OnClear(Type type)
        {
            AddCommand(type, "Clear");
        }
        void OnStore(Type type)
        {
            AddCommand(type, "Store");
        }
        void AddCommand(Type type, string command)
        {
            Dictionary<int, ClassPool> dic = GetDicPool();
            for (int j = 0; j < ClassManager.source.Length; j++)
            {
                var s = ClassManager.source[j];
                foreach (var source in s)
                {
                    var cls = GetPool();
                    cls.index = source.Value.index;
                    cls.freeCount = source.Value.freeCount;
                    for (int i = 0; i < source.Value.freeList.Count; i++)
                    {
                        cls.freeList.Add(null);
                    }
                    cls.getCount = source.Value.getCount;
                    cls.storeCount = source.Value.storeCount;
                    cls.type = source.Value.type;
                    cls.typeName = source.Value.typeName;
                    cls.group = source.Value.group;
                    for (int i = 0; i < source.Value.allList.Count; i++)
                    {
                        cls.allList.Add(null);
                    }
                    if (dic.ContainsKey(source.Key))
                    {
                        UnityEngine.Debug.LogWarning("has the same class in other classGroup:" + source.Value.type);
                        FreePool(cls);
                    }
                    else
                        dic.Add(source.Key, cls);
                }
            }
            m_runtimeData.Add(dic);
            m_commandData.Add(" ["+EditorApplication.timeSinceStartup+"] ["+ type.Name +"] "+ command);
            if (m_runtimeData.Count>m_maxStoreCommand)
            {
                FreeDicPool(m_runtimeData[0]);
                m_runtimeData.RemoveAt(0);
            }
            if (m_commandData.Count>m_maxStoreCommand)
            {
                m_commandData.RemoveAt(0);
            }
            if (m_selectedCommand == m_currentCommand)
            {
                m_selectedCommand = m_commandData.Count - 1;
            }
            m_currentCommand = m_commandData.Count - 1;
            Repaint();
        }
        ClassPool GetPool()
        {
            if (pool.Count>0)
            {
                var p = pool[0];
                pool.RemoveAt(0);
                return p;
            }
            else
            {
                return new ClassPool();
            }
        }
        void FreePool(ClassPool p)
        {
            p.Reset();
            pool.Add(p);
        }
        Dictionary<int,ClassPool> GetDicPool()
        {
            if (dicPool.Count > 0)
            {
                var p = dicPool[0];
                dicPool.RemoveAt(0);
                return p;
            }
            else
            {
                return new Dictionary<int, ClassPool>();
            }
        }
        void FreeDicPool(Dictionary<int, ClassPool> p)
        {
            foreach (var item in p)
            {
                FreePool(item.Value);
            }
            p.Clear();
            dicPool.Add(p);
        }
    }
}