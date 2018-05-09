using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleWindow : EditorWindow {

    //private static BundleWindow m_instance;
    //private Vector2 m_filesScrollPos;
    //private Vector2 m_paramScrollPos;
    //private BundleManager m_bundleManager;
    //private string m_selectedFolder;
    //private int m_selectedFileIndex;
    //[MenuItem("Tools/BundleWindow %k")]
    //static void Init(){
    //    m_instance = GetWindow<BundleWindow>();
    //    m_instance.initialize();
    //    m_instance.Show();
    //}
    //void initialize(){
    //    m_bundleManager = new BundleManager();
    //}
    //void OnGUI(){
    //    if (m_instance==null)
    //    {
    //        m_bundleManager = null;
    //        Close();
    //        Init();
    //        return;
    //    }
    //    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);{
    //        if (GUILayout.Button("Apply",EditorStyles.toolbarButton))
    //        {
    //            m_bundleManager.BuildPackage();
    //            m_bundleManager = null;
    //            Close();
    //            Init();
    //            EditorGUILayout.EndHorizontal();
    //            return;
    //        }
    //        if (GUILayout.Button("Refresh",EditorStyles.toolbarButton))
    //        {
    //            m_bundleManager = null;
    //            Close();
    //            Init();
    //            EditorGUILayout.EndHorizontal();
    //            return;
    //        }
    //        GUILayout.FlexibleSpace();
    //        if (GUILayout.Button("Settings",EditorStyles.toolbarButton))
    //        {

    //        }
    //    }EditorGUILayout.EndHorizontal();
    //    float windowWidth = position.width;
    //    float windowHeight = position.height;
    //    EditorGUILayout.BeginHorizontal();
    //    {
    //        EditorGUILayout.BeginVertical();{
    //            m_filesScrollPos = EditorGUILayout.BeginScrollView(EditorGUILayout, true, false, GUILayout.Height(windowHeight - 20));
    //            {
    //                EditorGUILayout.BeginHorizontal();
    //                {
    //                    var enumer = m_bundleManager.sources.GetEnumerator();
    //                    while(enumer.MoveNext()){
    //                        EditorGUILayout.BeginVertical();{
    //                            Color c = GUI.color;
    //                            GUI.color = Color.cyan;
    //                            GUILayout.Box(enumer.Current.Key, EditorStyles.helpBox, GUILayout.Width(250));
    //                            GUI.color = c;
    //                            enumer.Current.Value.scrollPos = EditorGUILayout.BeginScrollVoew(enumer.Current.Value.scrollPos, EditorStyles.textField, GUILayout.Width(250));
    //                            {
    //                                for (int i = 0; i < enumer.Current.Value.files.Count; i++)
    //                                {
    //                                    var rect = EditorGUILayout.BeginHorizontal();
    //                                    {
    //                                        Color color = GUI.color;
    //                                        if (enumer.Current.Value.files[j].state=BundleManager.ESourceState.MainBundle)
    //                                        {

    //                                        }
    //                                    }EditorGUILayout.EndHorizontal();
    //                                }
    //                            }
    //                        }EditorGUILayout.EndVertical();
    //                    }
    //                }EditorGUILayout.EndHorizontal();
    //            }EditorGUILayout.EndScrollView();
    //        }EditorGUILayout.EndVertical();
    //    }EditorGUILayout.EndHorizontal();

    //}
}
