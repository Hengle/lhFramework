using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Framework.Tools.Bundle
{
    public class BundleBuildWindow : EditorWindow
    {
        private static BundleBuildWindow m_instance;
        private Vector2 m_filesScrollPos;
        private Vector2 m_tableScrollPos;
        private Vector2 m_paramsScrollPos;
        private BundleBuildManager m_bundleBuildManager;
        private string m_selectedCategory;
        private string m_selectedVariant;
        private string m_selectedFileName;
        private string m_selectedDirectoryName;
        private int m_paramMaxWidth = 350;
        private int m_categoryMaxWidth = 10;
        private bool m_drawSetting;
        private int m_table_idMaxWidth = 150;
        private int m_table_categoryMaxWidth = 10;
        private int m_table_fileNameMaxWidth = 10;
        private int m_table_variantMaxWidth = 100;
        private int m_table_dependenciedMaxWidth = 150;
        private int m_table_bundlePathMaxWidth = 10;
        private int m_table_maxHeight = 600;
        private string m_searchValue;
        private string m_searchOldValue;
        private ESearchType m_searchType = ESearchType.Id;
        [MenuItem("Tools/Bundle/BuildWindow")]
        static void Init()
        {
            m_instance = BundleBuildWindow.GetWindow<BundleBuildWindow>("BundleBuildWindow");
            m_instance.Initialize();
            m_instance.minSize = new Vector2(1300, 700);
            m_instance.Show();
        }

        void Initialize()
        {
            m_bundleBuildManager = new BundleBuildManager();
            if (m_bundleBuildManager.categoryData.Count <= 0)
            {
                m_selectedCategory = "";
                m_selectedFileName = "";
                m_selectedDirectoryName = "";
                m_selectedVariant = "";
                m_drawSetting = true;
            }
        }
        void OnEnable()
        {
        }
        void OnDestroy()
        {
        }
        void OnGUI()
        {
            if (m_instance == null)
            {
                m_bundleBuildManager = null;
                Close();
                Init();
                return;
            }
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("Build", EditorStyles.toolbarButton))
                {
                    m_bundleBuildManager.BuildPackage();
                    m_bundleBuildManager = null;
                    Close();
                    Init();
                    EditorGUILayout.EndHorizontal();
                    return;
                }
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                {
                    m_bundleBuildManager = null;
                    Close();
                    Init();
                    EditorGUILayout.EndHorizontal();
                    return;
                }
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                {
                    if (EditorUtility.DisplayDialog("Information","are you sure clear all bundleName?","Ok","Cancel"))
                    {
                        m_bundleBuildManager.Clear();
                        m_bundleBuildManager = null;
                        Close();
                        Init();
                        EditorGUILayout.EndHorizontal();
                        return;
                    }
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(m_bundleBuildManager.showTable ? "▤" : "▮", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    m_bundleBuildManager.showTable = !m_bundleBuildManager.showTable;
                }
                GUILayout.Space(8);
                if (GUILayout.Button("Settings", EditorStyles.toolbarButton))
                {
                    m_selectedCategory = "";
                    m_selectedFileName = "";
                    m_selectedDirectoryName = "";
                    m_selectedVariant = "";
                    m_drawSetting = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                if (m_bundleBuildManager.showTable)
                {
                    RenderTable();
                }
                else
                {
                    RenderHorizontal();
                }
                RenderParam();
            }
            EditorGUILayout.EndHorizontal();
        }
        void PingObject(string assetPath)
        {
            UnityEngine.Object[] obj = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            bool has = false;
            FileInfo info = new FileInfo(assetPath);
            string name = info.Name.Replace(info.Extension,"").ToLower();
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
            if(!has)
            {
                if (obj.Length>0)
                {
                    int instanceID = obj[0].GetInstanceID();
                    EditorGUIUtility.PingObject(instanceID);
                    Selection.activeObject = obj[0];
                }
            }
        }
        void RenderHorizontal()
        {
            float windowWidth = position.width;
            float windowHeight = position.height;
            EditorGUILayout.BeginVertical();
            {
                m_filesScrollPos = EditorGUILayout.BeginScrollView(m_filesScrollPos, true, false, GUILayout.Height(windowHeight - 20));
                {
                    if (m_bundleBuildManager.categoryData.Count > 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            var categoryEnumer = m_bundleBuildManager.categoryData.GetEnumerator();
                            while (categoryEnumer.MoveNext())
                            {
                                var r = EditorGUILayout.BeginVertical();
                                {
                                    Color c = GUI.color;
                                    if (Event.current.type == EventType.MouseUp)
                                    {
                                        if (new Rect(r.x, 0, 250, 20).Contains(Event.current.mousePosition))
                                        {
                                            m_selectedCategory = categoryEnumer.Current.Key;
                                            m_selectedFileName = "";
                                            m_selectedDirectoryName = "";
                                            m_selectedVariant = "";
                                            Repaint();
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(m_selectedCategory) && m_selectedCategory == categoryEnumer.Current.Key && string.IsNullOrEmpty(m_selectedFileName) && string.IsNullOrEmpty(m_selectedDirectoryName) && string.IsNullOrEmpty(m_selectedVariant))
                                    {
                                        GUI.color = Color.yellow;
                                    }
                                    else
                                    {
                                        GUI.color = Color.cyan;
                                    }

                                    GUILayout.Box(categoryEnumer.Current.Value.category, EditorStyles.helpBox, GUILayout.Width(250));
                                    string value = (categoryEnumer.Current.Value.baseId).ToString();
                                    GUI.Label(new Rect(r.x + 250 - value.Length * 9 - 10, 4, value.Length * 9, 30), value, EditorStyles.miniLabel);
                                    GUI.color = c;
                                    categoryEnumer.Current.Value.scrollPos = EditorGUILayout.BeginScrollView(categoryEnumer.Current.Value.scrollPos, EditorStyles.textField, GUILayout.Width(250), GUILayout.ExpandWidth(true));
                                    {
                                        int iFile = 0;
                                        EditorGUILayout.BeginVertical(GUILayout.Height(categoryEnumer.Current.Value.scrollHeight), GUILayout.Width(categoryEnumer.Current.Value.scrollWidth * 7 + 30));
                                        {
                                            var variantEnumer = categoryEnumer.Current.Value.variantDic.GetEnumerator();
                                            while (variantEnumer.MoveNext())
                                            {
                                                var re = EditorGUILayout.BeginHorizontal();
                                                {
                                                    if (Event.current.type == EventType.MouseUp)
                                                    {
                                                        if (new Rect(re.x, re.y + iFile * 20, 250, 20).Contains(Event.current.mousePosition))
                                                        {
                                                            variantEnumer.Current.Value.isOpen = !variantEnumer.Current.Value.isOpen;
                                                            Repaint();
                                                        }
                                                    }
                                                    Color color = GUI.color;
                                                    GUI.color = Color.white;
                                                    GUI.Label(new Rect(re.x , re.y + iFile * 20, 12, 20), variantEnumer.Current.Value.isOpen ? "▼" : "▶");
                                                    GUI.Label(new Rect(re.x+125- variantEnumer.Current.Key.Length*3, re.y + iFile * 20, 250, 20), variantEnumer.Current.Key);
                                                    GUI.Box(new Rect(re.x, re.y + iFile * 20, 250, 20), "");
                                                    GUI.color = color;
                                                }
                                                iFile++;
                                                EditorGUILayout.EndHorizontal();
                                                if (variantEnumer.Current.Value.isOpen)
                                                {
                                                    foreach (var item in variantEnumer.Current.Value.directoriesDic)
                                                    {
                                                        var rect = EditorGUILayout.BeginHorizontal();
                                                        {
                                                            if (Event.current.type == EventType.MouseUp)
                                                            {
                                                                if (new Rect(rect.x + 26, iFile * 20, 300, 20).Contains(Event.current.mousePosition))
                                                                {
                                                                    m_selectedCategory = categoryEnumer.Current.Key;
                                                                    m_selectedFileName = "";
                                                                    m_selectedDirectoryName = item.Key;
                                                                    m_selectedVariant = variantEnumer.Current.Key;
                                                                    Repaint();
                                                                }
                                                            }
                                                            if (Event.current.type == EventType.MouseUp)
                                                            {
                                                                if (new Rect(0, iFile * 20, 26, 20).Contains(Event.current.mousePosition))
                                                                {
                                                                    item.Value.isOpen = !item.Value.isOpen;
                                                                    Repaint();
                                                                }
                                                            }
                                                            Color color = GUI.color;
                                                            if (m_selectedCategory == categoryEnumer.Current.Key && m_selectedDirectoryName == item.Key && m_selectedVariant == variantEnumer.Current.Key && string.IsNullOrEmpty(m_selectedFileName))
                                                            {
                                                                GUI.color = Color.yellow;
                                                                GUI.Box(new Rect(rect.x, rect.y + iFile * 20, 300, 20), "");
                                                            }
                                                            GUI.Label(new Rect(rect.x + 12, rect.y + iFile * 20, 12, 20), item.Value.isOpen ? "▼" : "▶");
                                                            GUI.Label(new Rect(rect.x + 24, rect.y + iFile * 20, item.Value.fileName.Length * 7, 20), item.Value.fileName);
                                                            GUI.color = color;
                                                        }
                                                        EditorGUILayout.EndHorizontal();
                                                        iFile++;
                                                        if (item.Value.isOpen)
                                                        {
                                                            foreach (var it in item.Value.filesDic)
                                                            {
                                                                Color color = GUI.color;
                                                                categoryEnumer.Current.Value.scrollWidth = categoryEnumer.Current.Value.scrollWidth > it.Value.fileName.Length ? categoryEnumer.Current.Value.scrollWidth : it.Value.fileName.Length;
                                                                if (it.Value.dontDamage)
                                                                {
                                                                    if (Event.current.type == EventType.MouseUp)
                                                                    {
                                                                        if (new Rect(0, iFile * 20, 300, 20).Contains(Event.current.mousePosition))
                                                                        {
                                                                            m_selectedCategory = categoryEnumer.Current.Key;
                                                                            m_selectedFileName = it.Key;
                                                                            m_selectedDirectoryName = item.Key;
                                                                            m_selectedVariant = variantEnumer.Current.Key;
                                                                            Repaint();
                                                                        }
                                                                    }
                                                                    if (it.Value.dependenciedChainList.Count > m_bundleBuildManager.minBundleDependencyCount)
                                                                    {
                                                                        GUI.color = Color.yellow;
                                                                        GUI.Label(new Rect(rect.x + 24, rect.y + iFile * 20, 20, 20), it.Value.dependenciedChainList.Count.ToString());
                                                                        GUI.color = color;
                                                                    }
                                                                    if (it.Value.fileState == ESourceState.MainBundle)
                                                                    {
                                                                        if (!item.Value.dontDamage)
                                                                        {
                                                                            GUI.color = Color.green;
                                                                            GUI.Label(new Rect(rect.x + 24, rect.y + (iFile * 20) + 15, item.Value.fileName.Length * 7.5f, 0.2f), "", EditorStyles.textField);
                                                                        }
                                                                        GUI.color = m_bundleBuildManager.mainBundleColor;
                                                                    }
                                                                    else if (it.Value.fileState == ESourceState.SharedBundle)
                                                                    {
                                                                        if (!item.Value.dontDamage)
                                                                        {
                                                                            GUI.color = Color.green;
                                                                            GUI.Label(new Rect(rect.x + 24, rect.y + (iFile * 20) + 15, item.Value.fileName.Length * 7.5f, 0.2f), "", EditorStyles.textField);
                                                                        }
                                                                        GUI.color = m_bundleBuildManager.sharedBundleColor;
                                                                    }
                                                                    else if (it.Value.fileState == ESourceState.Damage)
                                                                    {
                                                                        GUI.color = m_bundleBuildManager.damageBundleColor;
                                                                        GUI.Label(new Rect(rect.x + 24 + 12, rect.y + iFile * 20, 20, 20), "x");
                                                                    }
                                                                    else if (it.Value.fileState == ESourceState.PrivateBundle)
                                                                    {
                                                                        GUI.color = m_bundleBuildManager.privateBundleColor;
                                                                    }
                                                                    if (m_selectedCategory == categoryEnumer.Current.Key && m_selectedFileName == it.Key && m_selectedVariant == variantEnumer.Current.Key)
                                                                    {
                                                                        GUI.color = Color.yellow;
                                                                        GUI.Box(new Rect(rect.x, rect.y + iFile * 20, 300, 20), "");
                                                                    }
                                                                    GUI.Label(new Rect(rect.x + 24 + 24, rect.y + iFile * 20, it.Value.fileName.Length * 7.5f, 20), it.Value.fileName);
                                                                }
                                                                else
                                                                {
                                                                    GUI.color = Color.red;
                                                                    GUI.Label(new Rect(rect.x + 24 + 12, rect.y + iFile * 20, 20, 20), "x");
                                                                    GUI.Label(new Rect(rect.x + 24 + 24, rect.y + iFile * 20, it.Value.fileName.Length * 7.5f, 20), it.Value.fileName);
                                                                }
                                                                GUI.color = color;
                                                                iFile++;
                                                            }
                                                        }
                                                    }
                                                    if (variantEnumer.Current.Value.directoriesDic.Count > 0)
                                                    {
                                                        GUI.TextField(new Rect(8, iFile * 20, 300, 1), "", EditorStyles.textField);
                                                        GUI.TextField(new Rect(8, iFile * 20 + 15, 300, 1), "", EditorStyles.textField);
                                                        iFile++;
                                                    }
                                                    foreach (var item in variantEnumer.Current.Value.filesDic)
                                                    {
                                                        var rect = EditorGUILayout.BeginHorizontal();
                                                        {
                                                            Color color = GUI.color;
                                                            categoryEnumer.Current.Value.scrollWidth = categoryEnumer.Current.Value.scrollWidth > item.Value.fileName.Length ? categoryEnumer.Current.Value.scrollWidth : item.Value.fileName.Length;
                                                            if (item.Value.dontDamage)
                                                            {
                                                                if (Event.current.type == EventType.MouseUp)
                                                                {
                                                                    if (new Rect(0, iFile * 20, 300, 20).Contains(Event.current.mousePosition))
                                                                    {
                                                                        m_selectedCategory = categoryEnumer.Current.Key;
                                                                        m_selectedFileName = item.Key;
                                                                        m_selectedDirectoryName = "";
                                                                        m_selectedVariant = variantEnumer.Current.Key;
                                                                        Repaint();
                                                                    }
                                                                }
                                                                if (item.Value.dependenciedChainList.Count > m_bundleBuildManager.minBundleDependencyCount)
                                                                {
                                                                    GUI.color = Color.yellow;
                                                                    GUI.Label(new Rect(rect.x, rect.y + (iFile * 20), 20, 20), item.Value.dependenciedChainList.Count.ToString());
                                                                    GUI.color = color;
                                                                }
                                                                if (item.Value.fileState == ESourceState.MainBundle)
                                                                {
                                                                    if (!item.Value.dontDamage)
                                                                    {
                                                                        GUI.color = Color.green;
                                                                        GUI.Label(new Rect(rect.x + 24, rect.y + (iFile * 20) + 15, item.Value.fileName.Length * 7.5f, 0.2f), "", EditorStyles.textField);
                                                                    }
                                                                    GUI.color = m_bundleBuildManager.mainBundleColor;
                                                                }
                                                                else if (item.Value.fileState == ESourceState.SharedBundle)
                                                                {
                                                                    if (!item.Value.dontDamage)
                                                                    {
                                                                        GUI.color = Color.green;
                                                                        GUI.Label(new Rect(rect.x + 24, rect.y + (iFile * 20) + 15, item.Value.fileName.Length * 7.5f, 0.2f), "", EditorStyles.textField);
                                                                    }
                                                                    GUI.color = m_bundleBuildManager.sharedBundleColor;
                                                                }
                                                                else if (item.Value.fileState == ESourceState.PrivateBundle)
                                                                {
                                                                    GUI.color = m_bundleBuildManager.privateBundleColor;
                                                                }
                                                                else if (item.Value.fileState == ESourceState.Damage)
                                                                {
                                                                    GUI.color = m_bundleBuildManager.damageBundleColor;
                                                                    GUI.Label(new Rect(rect.x + 12, rect.y + (iFile * 20), 20, 20), "x");
                                                                }
                                                                if (m_selectedCategory == categoryEnumer.Current.Key && m_selectedFileName == item.Key && m_selectedVariant == variantEnumer.Current.Key)
                                                                {
                                                                    GUI.color = Color.yellow;
                                                                    GUI.Box(new Rect(rect.x, rect.y * (iFile * 20), 300, 20), "");
                                                                    GUI.Label(new Rect(rect.x + 24, rect.y + (iFile * 20), item.Value.fileName.Length * 7.5f, 20), item.Value.fileName);
                                                                }
                                                                else
                                                                {
                                                                    GUI.Label(new Rect(rect.x + 24, rect.y + (iFile * 20), item.Value.fileName.Length * 7.5f, 20), item.Value.fileName);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                GUI.color = Color.red;
                                                                GUI.Label(new Rect(rect.x + 12, rect.y + (iFile * 20), 20, 20), "x");
                                                                GUI.Label(new Rect(rect.x + 24, rect.y + (iFile * 20), item.Value.fileName.Length * 7.5f, 20), item.Value.fileName);
                                                            }
                                                            GUI.color = color;
                                                        }
                                                        EditorGUILayout.EndHorizontal();
                                                        iFile++;
                                                    }
                                                }
                                            }
                                            categoryEnumer.Current.Value.scrollHeight = iFile * 20;
                                        }
                                        EditorGUILayout.EndVertical();
                                    }
                                    EditorGUILayout.EndScrollView();
                                    GUILayout.Space(7);
                                    GUILayout.TextField("", GUILayout.Height(1));
                                    GUILayout.Space(7);
                                }
                                EditorGUILayout.EndVertical();
                            }
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Label("1、设置资源根目录RootName");
                        GUILayout.Label("2、设置好对应的平台BuildTarget");
                        GUILayout.Label("3、按下Refresh");
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.TextField("", GUILayout.Height(windowHeight), GUILayout.Width(0.5f));
        }
        void RenderTable()
        {
            float windowWidth = position.width;
            float windowHeight = position.height;
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.FlexibleSpace();
                m_searchOldValue = EditorGUILayout.TextField(m_searchOldValue);
                m_searchType = (ESearchType)EditorGUILayout.EnumPopup(m_searchType, GUILayout.Width(100));
                if (m_searchOldValue != m_searchValue && string.IsNullOrEmpty(m_searchOldValue))
                {
                    m_searchValue = m_searchOldValue;
                    m_bundleBuildManager.ResetSearch();
                }
                if (GUILayout.Button("Search", EditorStyles.miniButton))
                {
                    if (!string.IsNullOrEmpty(m_searchOldValue))
                    {
                        m_searchValue = m_searchOldValue;
                        m_bundleBuildManager.Search(m_searchValue, m_searchType);
                    }
                }
                EditorGUILayout.EndHorizontal();
                m_tableScrollPos = EditorGUILayout.BeginScrollView(m_tableScrollPos, true, false, GUILayout.Height(windowHeight - 20));
                {
                    EditorGUILayout.BeginVertical(GUILayout.MinHeight(m_table_maxHeight), GUILayout.ExpandHeight(true));
                    {
                        GUILayout.Label("");
                        int y = 0;
                        RenderTitle(y);
                        y++;
                        if (string.IsNullOrEmpty(m_searchValue))
                        {
                            foreach (var categoryData in m_bundleBuildManager.categoryData)
                            {
                                foreach (var variantData in categoryData.Value.variantDic)
                                {
                                    foreach (var directories in variantData.Value.directoriesDic)
                                    {
                                        int dep = 0;
                                        foreach (var it in directories.Value.filesDic)
                                        {
                                            dep += it.Value.fileDependencied.Count;
                                        }
                                        bool selected = false;
                                        if (m_selectedCategory == categoryData.Key && m_selectedDirectoryName == directories.Key && string.IsNullOrEmpty(m_selectedFileName))
                                        {
                                            selected = true;
                                        }
                                        RenderRow(y, directories.Value.assetId, categoryData.Value.category, variantData.Key,  directories.Value.fileName, dep, directories.Value.bundleName, selected, directories.Value.fileState);
                                        if (Event.current.type == EventType.MouseUp && directories.Value.dontDamage)
                                        {
                                            if (new Rect(0, y * 20, 3000, 20).Contains(Event.current.mousePosition))
                                            {
                                                GUI.FocusControl("");
                                                m_selectedCategory = categoryData.Key;
                                                m_selectedFileName = "";
                                                m_selectedDirectoryName = directories.Key;
                                                m_selectedVariant = variantData.Key;
                                                Repaint();
                                            }
                                        }
                                        y++;
                                    }
                                    foreach (var files in variantData.Value.filesDic)
                                    {
                                        bool selected = false;
                                        if (m_selectedCategory == categoryData.Key && m_selectedFileName == files.Key && string.IsNullOrEmpty(m_selectedDirectoryName))
                                        {
                                            selected = true;
                                        }
                                        RenderRow(y, files.Value.assetId, categoryData.Value.category, variantData.Key, files.Value.fileName, files.Value.fileDependencied.Count, files.Value.bundleName, selected, files.Value.fileState);
                                        if (Event.current.type == EventType.MouseUp && files.Value.dontDamage)
                                        {
                                            if (new Rect(0, y * 20, 3000, 20).Contains(Event.current.mousePosition))
                                            {
                                                GUI.FocusControl("");
                                                m_selectedCategory = categoryData.Key;
                                                m_selectedFileName = files.Key;
                                                m_selectedDirectoryName = "";
                                                m_selectedVariant = variantData.Key;
                                                Repaint();
                                            }
                                        }
                                        y++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var category in m_bundleBuildManager.searchedDirectories)
                            {
                                foreach (var directories in category.Value)
                                {
                                    int dep = 0;
                                    foreach (var it in directories.filesDic)
                                    {
                                        dep += it.Value.fileDependencied.Count;
                                    }
                                    bool selected = false;
                                    if (!selected && m_selectedCategory == category.Key && m_selectedDirectoryName == directories.fileName.ToLower() && string.IsNullOrEmpty(m_selectedFileName))
                                    {
                                        selected = true;
                                    }
                                    RenderRow(y, directories.assetId, category.Key, directories.variantName, directories.fileName, dep, directories.bundleName, selected, directories.fileState);
                                    if (Event.current.type == EventType.MouseUp && directories.dontDamage)
                                    {
                                        if (new Rect(0, y * 20, 3000, 20).Contains(Event.current.mousePosition))
                                        {
                                            GUI.FocusControl("");
                                            m_selectedCategory = category.Key.ToLower();
                                            m_selectedFileName = "";
                                            m_selectedDirectoryName = directories.fileName.ToLower();
                                            m_selectedVariant = directories.variantName;
                                            Repaint();
                                        }
                                    }
                                    y++;
                                }
                            }
                            foreach (var category in m_bundleBuildManager.searchedFiles)
                            {
                                foreach (var files in category.Value)
                                {
                                    bool selected = false;
                                    if (!selected && m_selectedCategory == category.Key && m_selectedFileName == files.fileName.ToLower() && string.IsNullOrEmpty(m_selectedDirectoryName))
                                    {
                                        selected = true;
                                    }
                                    RenderRow(y, files.assetId, category.Key, files.variantName, files.fileName, files.fileDependencied.Count, files.bundleName, selected, files.fileState);
                                    if (Event.current.type == EventType.MouseUp && files.dontDamage)
                                    {
                                        if (new Rect(0, y * 20, 3000, 20).Contains(Event.current.mousePosition))
                                        {
                                            GUI.FocusControl("");
                                            m_selectedCategory = category.Key.ToLower();
                                            m_selectedFileName = files.fileName.ToLower();
                                            m_selectedDirectoryName = "";
                                            m_selectedVariant = files.variantName;
                                            Repaint();
                                        }
                                    }
                                    y++;
                                }
                            }
                        }
                        m_table_maxHeight = y * 20;
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        void RenderParam()
        {
            float windowWidth = position.width;
            float windowHeight = position.height;
            m_paramsScrollPos = EditorGUILayout.BeginScrollView(m_paramsScrollPos, GUILayout.MinWidth(300), GUILayout.ExpandWidth(true), GUILayout.Height(windowHeight - 20));
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(m_paramMaxWidth));
                //{
                if (!string.IsNullOrEmpty(m_selectedCategory) && m_bundleBuildManager.categoryData.ContainsKey(m_selectedCategory))
                {
                    if (string.IsNullOrEmpty(m_selectedDirectoryName) && string.IsNullOrEmpty(m_selectedFileName))// 单独选中分类框
                    {
                        GUILayout.Label("Params Settings", EditorStyles.boldLabel);
                        GUILayout.TextField("", GUILayout.Height(1));
                        EditorGUILayout.Separator();

                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                        var categoryData = m_bundleBuildManager.categoryData[m_selectedCategory];
                        if (categoryData.isBaseIdReset)
                        {
                            categoryData.oldId = EditorGUILayout.IntField("BaseID: ", categoryData.oldId);
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("✔", EditorStyles.miniButton))
                            {
                                if (categoryData.oldId != categoryData.baseId)
                                {
                                    if (EditorUtility.DisplayDialog("Information", "are you sure change baseId", "Ok", "Cancel"))
                                    {
                                        if (!m_bundleBuildManager.ResetBaseId(m_selectedCategory, categoryData.oldId))
                                        {
                                            EditorUtility.DisplayDialog("Warning", "修改失败！1、id必须大于idBase\n2、id不能重复\n3、id必须是基于idBase", "Ok");
                                            categoryData.oldId = categoryData.baseId;
                                        }
                                        else
                                        {
                                            categoryData.baseId = categoryData.oldId;
                                            categoryData.isBaseIdReset = false;
                                            Repaint();
                                        }
                                    }
                                    GUI.FocusControl("");
                                }
                                else
                                {
                                    GUI.FocusControl("");
                                    categoryData.isBaseIdReset = false;
                                }
                            }
                            if (GUILayout.Button("✘", EditorStyles.miniButton))
                            {
                                categoryData.oldId = categoryData.baseId;
                                categoryData.isBaseIdReset = false;
                            }
                        }
                        else
                        {
                            GUILayout.Label("BaseID: " + categoryData.baseId);
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(" ☚ ", EditorStyles.miniButton))
                            {
                                categoryData.isBaseIdReset = true;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        //GUILayout.Label("TotalCount: " + (categoryData.filesDic.Count + categoryData.directoriesDic.Count));

                        EditorGUILayout.EndVertical();
                    }
                    else if (!string.IsNullOrEmpty(m_selectedDirectoryName) && string.IsNullOrEmpty(m_selectedFileName))//单独选中文件夹
                    {
                        GUILayout.Label("Params Settings", EditorStyles.boldLabel);
                        GUILayout.TextField("", GUILayout.Height(1));
                        EditorGUILayout.Separator();

                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.BeginHorizontal();
                        var sourceFile = m_bundleBuildManager.categoryData[m_selectedCategory].variantDic[m_selectedVariant].directoriesDic[m_selectedDirectoryName];
                        if (sourceFile.isBaseIdReset)
                        {
                            sourceFile.oldAssetId = EditorGUILayout.IntField("ID: ", sourceFile.oldAssetId);
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("✔", EditorStyles.miniButton))
                            {
                                if (sourceFile.oldAssetId != sourceFile.assetId)
                                {
                                    if (EditorUtility.DisplayDialog("Information", "are you sure change baseId", "Ok", "Cancel"))
                                    {
                                        if (!m_bundleBuildManager.ResetBundleId(m_selectedCategory,m_selectedVariant, m_selectedDirectoryName, sourceFile.oldAssetId, true))
                                        {
                                            EditorUtility.DisplayDialog("Warning", "修改失败！1、id必须大于idBase\n2、id不能重复\n3、id必须是基于idBase", "Ok");
                                            sourceFile.oldAssetId = sourceFile.assetId;
                                        }
                                        else
                                        {
                                            sourceFile.assetId = sourceFile.oldAssetId;
                                            sourceFile.isBaseIdReset = false;
                                            Repaint();
                                        }
                                        GUI.FocusControl("");
                                    }
                                }
                                else
                                {
                                    GUI.FocusControl("");
                                    sourceFile.isBaseIdReset = false;
                                }

                            }
                            if (GUILayout.Button("✘", EditorStyles.miniButton))
                            {
                                sourceFile.oldAssetId = sourceFile.assetId;
                                sourceFile.isBaseIdReset = false;
                            }
                        }
                        else
                        {
                            GUILayout.Label("ID: " + sourceFile.assetId);
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(" ☚ ", EditorStyles.miniButton))
                            {
                                sourceFile.isBaseIdReset = true;
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Label("GUID: " + sourceFile.guid);
                        if (sourceFile.bundleSize > 0)
                        {
                            GUILayout.Label("BundleSize: " + sourceFile.bundleSize + "kb");
                        }
                        EditorGUILayout.EndVertical();

                        GUILayout.Label("All Asset", EditorStyles.boldLabel);
                        GUILayout.TextField("", GUILayout.Height(1));
                        EditorGUILayout.Separator();
                        foreach (var sFile in sourceFile.filesDic)
                        {
                            int l = RenderFileHorizontal(sFile.Value.mainAssetPath, sFile.Value.mainAssetName);
                            m_paramMaxWidth = m_paramMaxWidth > l ? m_paramMaxWidth : l;
                        }
                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();
                        EditorGUILayout.LabelField("Dependency", EditorStyles.boldLabel);
                        EditorGUILayout.TextField("", GUILayout.Height(1));
                        EditorGUILayout.Separator();
                        if (sourceFile.dependenciesChain >= 0)
                        {
                            var chainId = sourceFile.dependenciesChain;
                            var chain = m_bundleBuildManager.dependChains[chainId];
                            for (int j = chain.Count - 1; j >= 0; j--)
                            {
                                for (int k = 0; k < chain[j].Count; k++)
                                {
                                    var id = chain[j][k];
                                    var baseSource = m_bundleBuildManager.allSourcesGuid[id];
                                    int m = RenderFileHorizontal(j == chain.Count - 1 ? 38 : 62, j, baseSource.bundleName, baseSource.variantName, baseSource.mainAssetPath, sourceFile.guid == id, baseSource.fileState, baseSource.dependenciedChainList.Count);
                                    m_paramMaxWidth = m_paramMaxWidth > m ? m_paramMaxWidth : m;
                                }
                            }
                        }


                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();
                        EditorGUILayout.LabelField("Dependent Chain", EditorStyles.boldLabel);
                        EditorGUILayout.TextField("", GUILayout.Height(1));
                        EditorGUILayout.Separator();
                        if (sourceFile.fileState!=ESourceState.MainBundle)
                        {
                            for (int i = 0; i < sourceFile.dependenciedChainList.Count; i++)
                            {
                                var chainId = sourceFile.dependenciedChainList[i];
                                var chain = m_bundleBuildManager.dependChains[chainId];
                                for (int j = chain.Count - 1; j >= 0; j--)
                                {
                                    for (int k = 0; k < chain[j].Count; k++)
                                    {
                                        var id = chain[j][k];
                                        var baseSource = m_bundleBuildManager.allSourcesGuid[id];
                                        int m = RenderFileHorizontal(j == chain.Count - 1 ? 38 : 62, j, baseSource.bundleName, baseSource.variantName, baseSource.mainAssetPath, sourceFile.guid == id, baseSource.fileState, baseSource.dependenciedChainList.Count);
                                        m_paramMaxWidth = m_paramMaxWidth > m ? m_paramMaxWidth : m;
                                    }
                                }
                            }
                        }


                    }
                    else//只选中文件夹中单个文件或者文件夹外单个文件
                    {
                        SourceFile sourceFile = null;
                        if (string.IsNullOrEmpty(m_selectedDirectoryName))
                        {
                            if (m_bundleBuildManager.categoryData[m_selectedCategory].variantDic[m_selectedVariant].filesDic.ContainsKey(m_selectedFileName))
                            {
                                sourceFile = m_bundleBuildManager.categoryData[m_selectedCategory].variantDic[m_selectedVariant].filesDic[m_selectedFileName];
                            }
                        }
                        else
                        {
                            if (m_bundleBuildManager.categoryData[m_selectedCategory].variantDic[m_selectedVariant].directoriesDic.ContainsKey(m_selectedDirectoryName))
                            {
                                sourceFile = m_bundleBuildManager.categoryData[m_selectedCategory].variantDic[m_selectedVariant].directoriesDic[m_selectedDirectoryName].filesDic[m_selectedFileName];
                            }
                        }
                        if (sourceFile != null)
                        {

                            GUILayout.Label("Params Settings", EditorStyles.boldLabel);
                            GUILayout.TextField("", GUILayout.Height(1));
                            EditorGUILayout.Separator();

                            EditorGUILayout.BeginVertical();
                            if (sourceFile.assetId > 0)
                            {
                                EditorGUILayout.BeginHorizontal();
                                if (sourceFile.isBaseIdReset)
                                {
                                    sourceFile.oldAssetId = EditorGUILayout.IntField("ID: ", sourceFile.oldAssetId);
                                    GUILayout.FlexibleSpace();
                                    if (GUILayout.Button("✔", EditorStyles.miniButton))
                                    {
                                        if (sourceFile.oldAssetId != sourceFile.assetId)
                                        {
                                            if (EditorUtility.DisplayDialog("Information", "are you sure change baseId", "Ok", "Cancel"))
                                            {
                                                if (!m_bundleBuildManager.ResetBundleId(m_selectedCategory,m_selectedVariant, m_selectedFileName, sourceFile.oldAssetId, false))
                                                {
                                                    EditorUtility.DisplayDialog("Warning", "修改失败！1、id必须大于idBase\n2、id不能重复\n3、id必须是基于idBase", "Ok");
                                                    sourceFile.oldAssetId = sourceFile.assetId;
                                                }
                                                else
                                                {
                                                    sourceFile.assetId = sourceFile.oldAssetId;
                                                    sourceFile.isBaseIdReset = false;
                                                    Repaint();
                                                }
                                                GUI.FocusControl("");
                                            }
                                        }
                                        else
                                        {
                                            GUI.FocusControl("");
                                            sourceFile.isBaseIdReset = false;
                                        }

                                    }
                                    if (GUILayout.Button("✘", EditorStyles.miniButton))
                                    {
                                        GUI.FocusControl("");
                                        sourceFile.oldAssetId = sourceFile.assetId;
                                        sourceFile.isBaseIdReset = false;
                                    }
                                }
                                else
                                {
                                    GUILayout.Label("ID: " + sourceFile.assetId);
                                    GUILayout.FlexibleSpace();
                                    if (GUILayout.Button(" ☚ ", EditorStyles.miniButton))
                                    {
                                        sourceFile.isBaseIdReset = true;
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            GUILayout.Label("GUID: " + sourceFile.guid );
                            if (sourceFile.bundleSize > 0)
                            {
                                GUILayout.Label("BundleSize: " + sourceFile.bundleSize + "kb");
                            }
                            EditorGUILayout.EndVertical();

                            GUILayout.Label("Main Asset", EditorStyles.boldLabel);
                            GUILayout.TextField("", GUILayout.Height(1));
                            EditorGUILayout.Separator();
                            int l = RenderFileHorizontal(sourceFile.mainAssetPath, sourceFile.mainAssetName);
                            m_paramMaxWidth = m_paramMaxWidth > l ? m_paramMaxWidth : l;
                            EditorGUILayout.Separator();
                            EditorGUILayout.Separator();
                            EditorGUILayout.LabelField("Dependency", EditorStyles.boldLabel);
                            EditorGUILayout.TextField("", GUILayout.Height(1));
                            EditorGUILayout.Separator();
                            if (sourceFile.dependenciesChain >= 0)
                            {
                                var chainId = sourceFile.dependenciesChain;
                                var chain = m_bundleBuildManager.dependChains[chainId];
                                for (int j = chain.Count - 1; j >= 0; j--)
                                {
                                    for (int k = 0; k < chain[j].Count; k++)
                                    {
                                        var id = chain[j][k];
                                        var baseSource = m_bundleBuildManager.allSourcesGuid[id];
                                        int m = RenderFileHorizontal(j == chain.Count - 1 ? 38 : 62, j, baseSource.bundleName, baseSource.variantName, baseSource.mainAssetPath, sourceFile.guid == id, baseSource.fileState, baseSource.dependenciedChainList.Count);
                                        m_paramMaxWidth = m_paramMaxWidth > m ? m_paramMaxWidth : m;
                                    }
                                }
                            }

                            EditorGUILayout.Separator();
                            EditorGUILayout.Separator();
                            EditorGUILayout.LabelField("Dependent Chain", EditorStyles.boldLabel);
                            EditorGUILayout.TextField("", GUILayout.Height(1));
                            EditorGUILayout.Separator();

                            if (sourceFile.fileState != ESourceState.MainBundle)
                            {
                                for (int i = 0; i < sourceFile.dependenciedChainList.Count; i++)
                                {
                                    var chainId = sourceFile.dependenciedChainList[i];
                                    var chain = m_bundleBuildManager.dependChains[chainId];
                                    for (int j = chain.Count - 1; j >= 0; j--)
                                    {
                                        for (int k = 0; k < chain[j].Count; k++)
                                        {
                                            var id = chain[j][k];
                                            var baseSource = m_bundleBuildManager.allSourcesGuid[id];
                                            int m = RenderFileHorizontal(j == chain.Count - 1 ? 38 : 62, j, baseSource.bundleName, baseSource.variantName, baseSource.mainAssetPath, sourceFile.guid == id, baseSource.fileState, baseSource.dependenciedChainList.Count);
                                            m_paramMaxWidth = m_paramMaxWidth > m ? m_paramMaxWidth : m;
                                        }
                                    }
                                }
                            }
                            
                        }
                    }

                    m_drawSetting = false;
                }
                else
                {
                    if (m_drawSetting)
                    {
                        m_bundleBuildManager.rootName = EditorGUILayout.TextField("RootName", m_bundleBuildManager.rootName);
                        m_bundleBuildManager.bundleOutputFolder = EditorGUILayout.TextField("BundleOutputFolder", m_bundleBuildManager.bundleOutputFolder);
                        m_bundleBuildManager.bundleFolder = EditorGUILayout.TextField("bundleFolder", m_bundleBuildManager.bundleFolder);
                        m_bundleBuildManager.bundleStuff = EditorGUILayout.TextField("bundleStuff", m_bundleBuildManager.bundleStuff);
                        var minBundleDependencyCount = EditorGUILayout.IntField("minDependCount", m_bundleBuildManager.minBundleDependencyCount);
                        if (m_bundleBuildManager.minBundleDependencyCount != minBundleDependencyCount)
                        {
                            if (EditorUtility.DisplayDialog("Infomation", "确定要切换吗？\n切换需要手动删除Bundle目录，然后重新构建\n确定后会删除所有bundle", "确定", "取消"))
                            {
                                if (minBundleDependencyCount <= 0)
                                {
                                    EditorUtility.DisplayDialog("Infomation", "minBundleDependencyCount 必须大于1", "确定");
                                    GUI.FocusControl("");
                                }
                                else
                                {
                                    m_bundleBuildManager.minBundleDependencyCount = minBundleDependencyCount;
                                    m_bundleBuildManager.DeletePackge();
                                    m_bundleBuildManager = null;
                                    Close();
                                    Init();
                                }
                            }
                        }
                        var idBase = EditorGUILayout.IntField("idBase", m_bundleBuildManager.idBase);
                        if (m_bundleBuildManager.idBase != idBase)
                        {
                            if (EditorUtility.DisplayDialog("Infomation", "确定要切换吗？\n切换需要手动删除Bundle目录，然后重新构建\n确定后会删除所有bundle", "确定", "取消"))
                            {
                                if (idBase < 10000 || idBase % 10000 > 0)
                                {
                                    EditorUtility.DisplayDialog("Infomation", "idBase 必须大于10000，并且必须是10000的整数倍", "确定");
                                    GUI.FocusControl("");
                                }
                                else
                                {
                                    GUI.FocusControl("");
                                    m_bundleBuildManager.idBase = idBase;
                                    m_bundleBuildManager.DeletePackge();
                                    m_bundleBuildManager = null;
                                    Close();
                                    Init();
                                }

                            }
                        }

                        var bundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup("bundleOptions", (BuildAssetBundleOptions)m_bundleBuildManager.bundleOptions);
                        if (m_bundleBuildManager.bundleOptions != (int)bundleOptions)
                        {
                            if (EditorUtility.DisplayDialog("Infomation", "确定要切换吗？\n切换需要手动删除Bundle目录，然后重新构建\n确定后会删除所有bundle", "确定", "取消"))
                            {
                                m_bundleBuildManager.bundleOptions = (int)bundleOptions;
                                m_bundleBuildManager.DeletePackge();
                                m_bundleBuildManager = null;
                                Close();
                                Init();
                            }
                        }
                        var buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("buildTarget", (BuildTarget)m_bundleBuildManager.buildTarget);
                        if (m_bundleBuildManager.buildTarget != (int)buildTarget)
                        {
                            if (EditorUtility.DisplayDialog("Infomation", "确定要切换吗？\n切换需要手动删除Bundle目录，然后重新构建\n确定后会删除所有bundle", "确定", "取消"))
                            {
                                m_bundleBuildManager.buildTarget = (int)buildTarget;
                                m_bundleBuildManager.DeletePackge();
                                m_bundleBuildManager = null;
                                Close();
                                Init();
                            }
                        }
                        m_bundleBuildManager.mainBundleColor = EditorGUILayout.ColorField("MainBundleColor",m_bundleBuildManager.mainBundleColor);
                        m_bundleBuildManager.sharedBundleColor = EditorGUILayout.ColorField("SharedBundleColor", m_bundleBuildManager.sharedBundleColor);
                        m_bundleBuildManager.privateBundleColor = EditorGUILayout.ColorField("PrivateBundleColor", m_bundleBuildManager.privateBundleColor);
                        m_bundleBuildManager.damageBundleColor = EditorGUILayout.ColorField("DamageBundleColor", m_bundleBuildManager.damageBundleColor);
                    }
                }
                //}
                EditorGUILayout.EndVertical();

            }
            EditorGUILayout.EndScrollView();
        }
        int RenderFileHorizontal(string assetPath, string assetName)
        {
            int width = assetName.Length * 8;
            Rect re = EditorGUILayout.BeginHorizontal();
            //{
            EditorGUIUtility.SetIconSize(new Vector2(14f, 14f));
            var tex = AssetDatabase.GetCachedIcon(assetPath);
            if (tex == null)
            {
                Color color = GUI.color;
                GUI.color = Color.red;
                GUILayout.Label("");
                GUI.Label(new Rect(re.x + 24, re.y, width, 20), assetName);
                GUI.color = color;
            }
            else
            {
                GUILayout.Label(tex);
                GUI.Label(new Rect(re.x + 24, re.y, width, 20), assetName);
                if (Event.current.type == EventType.MouseUp)
                {
                    if (re.Contains(Event.current.mousePosition))
                    {
                        PingObject(assetPath);
                    }
                }
            }
            //}
            EditorGUILayout.EndHorizontal();
            return width;
        }
        int RenderFileHorizontal(int  c,int j,string assetName,string variantName,string assetPath,bool right,ESourceState state,int chainCount)
        {
            float height = 30;
            string name = assetName + "." + variantName;
            int width = name.Length * 8;
            Rect re = EditorGUILayout.BeginHorizontal();
            //{
            Color color = GUI.color;
            if (right)
            {
                GUI.color = Color.green;
                GUI.Label(new Rect(re.x, re.y, 14, height), "✔");
            }
            EditorGUIUtility.SetIconSize(new Vector2(14f, 14f));
            var tex = AssetDatabase.GetCachedIcon(assetPath);
            if (tex != null)
            {
                GUI.color = Color.white;
                GUI.Label(new Rect(re.x +c-14, re.y, 14, height), tex);
            }
            if (chainCount>m_bundleBuildManager.minBundleDependencyCount)
            {
                GUI.color = Color.yellow;
                GUI.Label(new Rect(re.x + c+22, re.y, 20, height), chainCount.ToString());
            }
            GUI.color = Color.white;
            GUI.Label(new Rect(re.x + c + 7, re.y, 20, height), j.ToString());

            if (state==ESourceState.MainBundle)
            {
                GUI.color = m_bundleBuildManager.mainBundleColor;
            }
            else if (state==ESourceState.SharedBundle)
            {
                GUI.color = m_bundleBuildManager.sharedBundleColor;
            }
            else if (state==ESourceState.PrivateBundle)
            {
                GUI.color = m_bundleBuildManager.privateBundleColor;
            }
            else if (state == ESourceState.Damage)
            {
                GUI.color = m_bundleBuildManager.damageBundleColor;
            }
            else
            {
                GUI.color = Color.white;
            }
            GUILayout.Label("");
            GUI.Label(new Rect(re.x +40+c, re.y, width, height), name);
            GUI.color = Color.white;
            //GUI.Label(new Rect(re.x + 24 + c, re.y*j+ 15, width, 1), "", EditorStyles.textField);
            GUI.color = color;
            if (Event.current.type == EventType.MouseUp)
            {
                if (re.Contains(Event.current.mousePosition))
                {
                    PingObject(assetPath);
                }
            }
            //}
            EditorGUILayout.EndHorizontal();
            return width;
        }
        void RenderTitle(int y)
        {
            float windowWidth = position.width;
            float windowHeight = position.height;
            GUI.Box(new Rect(0, y * 20, 3000, 20), "");
            GUI.Label(new Rect(8, y * 20, m_table_idMaxWidth, 20), "ID", EditorStyles.boldLabel);
            GUI.Label(new Rect(m_table_idMaxWidth, y * 20, m_table_categoryMaxWidth, 20), "Category", EditorStyles.boldLabel);
            GUI.Label(new Rect(m_table_idMaxWidth + m_table_categoryMaxWidth , y * 20, m_table_variantMaxWidth, 20), "Variant", EditorStyles.boldLabel);
            GUI.Label(new Rect(m_table_idMaxWidth + m_table_categoryMaxWidth+ m_table_variantMaxWidth, y * 20, m_table_fileNameMaxWidth, 20), "BundleName", EditorStyles.boldLabel);
            GUI.Label(new Rect(m_table_idMaxWidth + m_table_categoryMaxWidth + m_table_variantMaxWidth + m_table_fileNameMaxWidth, y * 20, m_table_dependenciedMaxWidth, 20), "DependenciedCount", EditorStyles.boldLabel);
            GUI.Label(new Rect(m_table_idMaxWidth + m_table_categoryMaxWidth + m_table_variantMaxWidth + m_table_fileNameMaxWidth + m_table_dependenciedMaxWidth, y * 20, m_table_bundlePathMaxWidth, 20), "BundlePath", EditorStyles.boldLabel);
        }
        void RenderRow(int y, int id, string category,string variant, string fileName, int dependenciedCount, string bundlePath, bool selected, ESourceState fileState)
        {
            float windowWidth = position.width;
            float windowHeight = position.height;
            m_table_categoryMaxWidth = m_table_categoryMaxWidth > category.Length * 8 ? m_table_categoryMaxWidth : category.Length * 8;
            m_table_variantMaxWidth = m_table_variantMaxWidth > variant.Length * 8 ? m_table_variantMaxWidth : variant.Length * 8;
            m_table_fileNameMaxWidth = m_table_fileNameMaxWidth > fileName.Length * 8 ? m_table_fileNameMaxWidth : fileName.Length * 8;
            m_table_bundlePathMaxWidth = m_table_bundlePathMaxWidth > bundlePath.Length * 8 ? m_table_bundlePathMaxWidth : bundlePath.Length * 8; GUI.Label(new Rect(0, y * 20, 3000, 0.5f), "", EditorStyles.textField);
            Color color = GUI.color;
            if (selected)
            {
                GUI.color = Color.yellow;
                GUI.Box(new Rect(0, y * 20, 3000, 20), "");
            }
            else
            {
                if (fileState == ESourceState.MainBundle)
                {
                    GUI.color = m_bundleBuildManager.mainBundleColor;
                }
                else if (fileState == ESourceState.PrivateBundle)
                {
                    GUI.color = m_bundleBuildManager.privateBundleColor;
                }
                else if (fileState == ESourceState.SharedBundle)
                {
                    GUI.color = m_bundleBuildManager.sharedBundleColor;
                }
                else if (fileState == ESourceState.Damage)
                {
                    GUI.color = m_bundleBuildManager.damageBundleColor;
                }
            }
            GUI.Label(new Rect(8, y * 20, m_table_idMaxWidth, 20), id.ToString(), EditorStyles.boldLabel);
            GUI.Label(new Rect(m_table_idMaxWidth, y * 20, m_table_categoryMaxWidth, 20), category, EditorStyles.boldLabel);
            GUI.Label(new Rect(m_table_idMaxWidth + m_table_categoryMaxWidth, y * 20, m_table_variantMaxWidth, 20), variant, EditorStyles.boldLabel);
            GUI.Label(new Rect(m_table_idMaxWidth + m_table_categoryMaxWidth+ m_table_variantMaxWidth, y * 20, m_table_fileNameMaxWidth, 20), fileName, EditorStyles.boldLabel);
            GUI.Label(new Rect(m_table_idMaxWidth + m_table_categoryMaxWidth+ m_table_variantMaxWidth + m_table_fileNameMaxWidth, y * 20, m_table_dependenciedMaxWidth, 20), dependenciedCount.ToString(), EditorStyles.boldLabel);
            GUI.Label(new Rect(m_table_idMaxWidth + m_table_categoryMaxWidth+ m_table_variantMaxWidth + m_table_fileNameMaxWidth + m_table_dependenciedMaxWidth, y * 20, m_table_bundlePathMaxWidth, 20), bundlePath, EditorStyles.boldLabel);
            GUI.color = color;
        }
    }

}