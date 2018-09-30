using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace lhFramework.Tools.PsdToUGUI
{
    public class PsdToWindow : EditorWindow
    {
        private int m_selectVersion=-1;
        private int m_selectDataIndex;
        private EFileType m_selectType;
        private string[] m_versionArr;
        private Vector2 m_versionScrollPos;
        private Vector2 m_panelScrollPos;
        private Vector2 m_commonScrollPos;
        private Vector2 m_fontScrollPos;
        private Vector2 m_imageScrollPos;
        private Vector2 m_panelSpriteScrollPos;
        private Vector2 m_commonSpriteScrollPos;
        private Vector2 m_paramScrollPos;
        private PsdToManager m_psdToManager;
        [MenuItem("Tools/PsdToUGUI/PsdToWindow")]
        static void Init()
        {
            PsdToWindow window = EditorWindow.GetWindow<PsdToWindow>("PsdToWindow");
            window.Initialize();
            window.Show();
        }
        void Initialize()
        {
            m_versionArr = new string[] { "1_0_0" , "1_0_1" , "1_0_2" };
        }
        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUI.enabled = m_selectVersion >= 0;
                if (GUILayout.Button("Refresh",EditorStyles.toolbarButton))
                {

                }
                if (GUILayout.Button("Build", EditorStyles.toolbarButton))
                {

                }
                GUI.enabled = true;
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Settings", EditorStyles.toolbarButton))
                {

                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            {
                m_versionScrollPos = EditorGUILayout.BeginScrollView(m_versionScrollPos,GUILayout.ExpandWidth(false),GUILayout.MaxWidth(200),GUILayout.MinWidth(100));
                {
                    GUILayout.TextField("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                    for (int i = 0; i < m_versionArr.Length; i++)
                    {
                        string sel = "       ";
                        if (m_selectVersion==i)
                        {
                            GUI.color = Color.green;
                            sel = "  ☛  ";
                        }
                        if (GUILayout.Button(sel + m_versionArr[i],EditorStyles.largeLabel, GUILayout.ExpandWidth(true)))
                        {
                            m_selectVersion = i;
                            m_psdToManager = new PsdToManager(m_versionArr[i]);
                            Repaint();
                        }
                        GUI.color = Color.white;
                        GUILayout.TextField("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                    }
                }EditorGUILayout.EndScrollView();
                GUILayout.Box("", GUILayout.Width(1),GUILayout.Height(position.height));
                if (m_selectVersion>=0)
                {
                    EditorGUILayout.BeginHorizontal(GUILayout.MaxHeight(500));
                    {
                        EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                        {
                            GUILayout.Box("Panels", GUILayout.ExpandWidth(true));
                            m_panelScrollPos = EditorGUILayout.BeginScrollView(m_panelScrollPos, GUILayout.ExpandWidth(false), GUILayout.MinWidth(350));
                            {
                                for (int i = 0; i < m_psdToManager.panels.Count; i++)
                                {
                                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                    {
                                        Color color = GUI.color;
                                        if (m_selectType == m_psdToManager.panels[i].type && m_selectDataIndex == i)
                                        {
                                            GUI.color = Color.green;
                                        };
                                        if (GUILayout.Button("❁", EditorStyles.label, GUILayout.Width(20)))
                                        {
                                            m_selectType = m_psdToManager.panels[i].type;
                                            m_selectDataIndex = i;
                                        }
                                        GUI.color = color;
                                        EditorGUILayout.LabelField(m_psdToManager.panels[i].name);
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    GUILayout.TextField("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                                }
                            }
                            EditorGUILayout.EndScrollView();
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.TextField("", GUILayout.Width(1), GUILayout.ExpandHeight(true));

                        EditorGUILayout.BeginVertical(GUILayout.MaxWidth(350));
                        {
                            GUILayout.Box("Commons", GUILayout.ExpandWidth(true));
                            m_commonScrollPos = EditorGUILayout.BeginScrollView(m_commonScrollPos, GUILayout.ExpandWidth(false), GUILayout.MinWidth(350), GUILayout.MinHeight(250));
                            {
                                for (int i = 0; i < m_psdToManager.commons.Count; i++)
                                {
                                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                    {
                                        if (GUILayout.Button("❁", EditorStyles.label, GUILayout.Width(20)))
                                        {
                                            m_selectType = m_psdToManager.commons[i].type;
                                            m_selectDataIndex = i;
                                        }
                                        Color color = GUI.color;
                                        if (m_selectType == m_psdToManager.commons[i].type && m_selectDataIndex == i)
                                        {
                                            color = Color.green;
                                        }
                                        EditorGUILayout.LabelField(m_psdToManager.commons[i].name);
                                        GUI.color = color;
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    GUILayout.TextField("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                                }
                            }
                            EditorGUILayout.EndScrollView();
                            GUILayout.Box("Fonts", GUILayout.ExpandWidth(true));
                            m_fontScrollPos = EditorGUILayout.BeginScrollView(m_fontScrollPos, GUILayout.ExpandWidth(false), GUILayout.MinWidth(350), GUILayout.MinHeight(150));
                            {
                                for (int i = 0; i < m_psdToManager.fonts.Count; i++)
                                {
                                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                    {
                                        if (GUILayout.Button("❁", EditorStyles.label, GUILayout.Width(20)))
                                        {
                                            m_selectType = m_psdToManager.fonts[i].type;
                                            m_selectDataIndex = i;
                                        }
                                        Color color = GUI.color;
                                        if (m_selectType == m_psdToManager.fonts[i].type && m_selectDataIndex == i)
                                        {
                                            color = Color.green;
                                        }
                                        EditorGUILayout.LabelField(m_psdToManager.fonts[i].name);
                                        GUI.color = color;
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    GUILayout.TextField("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                                }
                            }
                            EditorGUILayout.EndScrollView();
                            //GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                            GUILayout.Box("Textures", GUILayout.ExpandWidth(true));
                            m_imageScrollPos = EditorGUILayout.BeginScrollView(m_imageScrollPos, GUILayout.ExpandWidth(false), GUILayout.MinWidth(350), GUILayout.MinHeight(250));
                            {
                                for (int i = 0; i < m_psdToManager.textures.Count; i++)
                                {
                                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                    {
                                        if (GUILayout.Button("❁", EditorStyles.label, GUILayout.Width(20)))
                                        {
                                            m_selectType = m_psdToManager.textures[i].type;
                                            m_selectDataIndex = i;
                                        }
                                        Color color = GUI.color;
                                        if (m_selectType == m_psdToManager.textures[i].type && m_selectDataIndex == i)
                                        {
                                            color = Color.green;
                                        }
                                        EditorGUILayout.LabelField(m_psdToManager.textures[i].name);
                                        GUI.color = color;
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    GUILayout.TextField("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                                }
                            }
                            EditorGUILayout.EndScrollView();
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.TextField("", GUILayout.Width(1), GUILayout.ExpandHeight(true));
                    m_paramScrollPos = EditorGUILayout.BeginScrollView(m_paramScrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.Height(position.height - 30), GUILayout.MinWidth(200), GUILayout.MaxWidth(450));
                    {
                        if (m_selectType==EFileType.Panel)
                        {
                            DrawData(m_psdToManager.panels[m_selectDataIndex]);
                        }
                        else if (m_selectType==EFileType.Common)
                        {
                            DrawData(m_psdToManager.commons[m_selectDataIndex]);
                        }
                    }
                    EditorGUILayout.EndScrollView();
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.textField, GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true));
                    {
                        EditorGUILayout.LabelField("Please select version to edit............");
                    }EditorGUILayout.EndHorizontal();
                }
            } EditorGUILayout.EndHorizontal();
        }
        void DrawData(PsdToManager.SourceData data)
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true));
            {
                EditorGUILayout.LabelField("Name:  ", data.name);
                EditorGUILayout.LabelField("FileType:  ", data.type.ToString());
                EditorGUILayout.LabelField("Psd:");
                if (data.psd != null)
                {
                    var r = EditorGUILayout.BeginHorizontal();
                    {
                        var d = data.psd;
                        var tex = AssetDatabase.GetCachedIcon(d.assetPath);
                        GUILayout.Space(20);
                        if (tex == null)
                        {
                            GUILayout.Space(20);
                        }
                        else
                        {
                            GUILayout.Label(tex, GUILayout.Width(20));
                        }
                        GUILayout.Label(d.name, GUILayout.ExpandWidth(true));
                        if (Event.current.type == EventType.MouseUp && r.Contains(Event.current.mousePosition))
                        {
                            PingObject(d.assetPath);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.LabelField("Xml:");
                if (data.xml != null)
                {
                    var e = EditorGUILayout.BeginHorizontal();
                    {
                        var d = data.xml;
                        var tex = AssetDatabase.GetCachedIcon(d.assetPath);
                        GUILayout.Space(20);
                        if (tex == null)
                        {
                            GUILayout.Space(20);
                        }
                        else
                        {
                            GUILayout.Label(tex, GUILayout.Width(20),GUILayout.Height(20));
                        }
                        GUILayout.Label(d.name, GUILayout.ExpandWidth(true));
                        if (Event.current.type == EventType.MouseUp && e.Contains(Event.current.mousePosition))
                        {
                            PingObject(d.assetPath);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.LabelField("Prefab:");
                if (data.prefab != null)
                {
                    var c = EditorGUILayout.BeginHorizontal();
                    {
                        var d = data.prefab;
                        var tex = AssetDatabase.GetCachedIcon(d.assetPath);
                        GUILayout.Space(20);
                        if (tex == null)
                        {
                            GUILayout.Space(20);
                        }
                        else
                        {
                            GUILayout.Label(tex, GUILayout.Width(20), GUILayout.Height(20));
                        }
                        GUILayout.Label(d.name, GUILayout.ExpandWidth(true));
                        if (Event.current.type == EventType.MouseUp && c.Contains(Event.current.mousePosition))
                        {
                            PingObject(d.assetPath);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.LabelField("SpriteAtlas:");
                if (data.spriteAtlas != null)
                {
                    var c = EditorGUILayout.BeginHorizontal();
                    {
                        var d = data.spriteAtlas;
                        var tex = AssetDatabase.GetCachedIcon(d.assetPath);
                        GUILayout.Space(20);
                        if (tex == null)
                        {
                            GUILayout.Space(20);
                        }
                        else
                        {
                            GUILayout.Label(tex, GUILayout.Width(20),GUILayout.Height(20));
                        }
                        GUILayout.Label(d.name, GUILayout.ExpandWidth(true));
                        if (Event.current.type == EventType.MouseUp && c.Contains(Event.current.mousePosition))
                        {
                            PingObject(d.assetPath);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }


                EditorGUILayout.LabelField("Sprites:");
                for (int i = 0; i < data.sprites.Count; i++)
                {
                    var re = EditorGUILayout.BeginHorizontal();
                    {
                        var d = data.sprites[i];
                        var tex = AssetDatabase.GetCachedIcon(d.assetPath);
                        GUILayout.Space(20);
                        if (tex == null)
                        {
                            GUILayout.Space(20);
                        }
                        else
                        {
                            GUILayout.Label(tex, GUILayout.Width(20), GUILayout.Height(20));
                        }
                        GUILayout.Label(d.name, GUILayout.ExpandWidth(true));
                        if (Event.current.type == EventType.MouseUp && re.Contains(Event.current.mousePosition))
                        {
                            PingObject(d.assetPath);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("XmlToPrefab", GUILayout.ExpandWidth(true)))
                {

                }
                data.variant=(EVariantType)EditorGUILayout.EnumFlagsField(data.variant, GUILayout.Width(60));
            } EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        void DrawTexture()
        {

        }
        void PingObject(string assetPath)
        {
            UnityEngine.Object[] obj = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            bool has = false;
            FileInfo info = new FileInfo(assetPath);
            string name = info.Name.Replace(info.Extension, "").ToLower();
            for (int i = 0; i < obj.Length; i++)
            {
                if (obj[i].name.ToLower().Contains(name))
                {
                    has = true;
                    int instanceID = obj[i].GetInstanceID();
                    EditorGUIUtility.PingObject(instanceID);
                    Selection.activeObject = obj[i];
                    break;
                }
            }
            if (!has)
            {
                if (obj.Length > 0)
                {
                    int instanceID = obj[0].GetInstanceID();
                    EditorGUIUtility.PingObject(instanceID);
                    Selection.activeObject = obj[0];
                }
            }
        }
    }
}