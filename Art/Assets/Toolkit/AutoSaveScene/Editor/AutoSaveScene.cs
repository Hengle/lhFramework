using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using UnityEditor.SceneManagement;

public class AutoSaveScene : EditorWindow {
    private bool autoSaveScene = true;
    private bool showMessage = true;
    private bool isStarted = false;
    private int intervalScene;
    private DateTime lastSaveTimeScene = DateTime.Now;

    private string projectPath;
    private UnityEngine.SceneManagement.Scene scene;

    [MenuItem("Tools/AutoSaveSceneWindow")]
    static void Init()
    {
        AutoSaveScene saveWindow = (AutoSaveScene)EditorWindow.GetWindow(typeof(AutoSaveScene));
        saveWindow.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Info:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("       Saving scene:", "" + scene);
        GUILayout.Label("Options:", EditorStyles.boldLabel);
        autoSaveScene = EditorGUILayout.BeginToggleGroup("Auto save", autoSaveScene);
        intervalScene = EditorGUILayout.IntSlider("Interval (minutes)", intervalScene, 1, 10);
        if (isStarted)
        {
            EditorGUILayout.LabelField("Last save:", "" + lastSaveTimeScene);
        }
        EditorGUILayout.EndToggleGroup();
        showMessage = EditorGUILayout.BeginToggleGroup("Show Message", showMessage);
        EditorGUILayout.EndToggleGroup();
    }
    void Update()
    {
        scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (autoSaveScene)
        {
            if (DateTime.Now.Minute >= (lastSaveTimeScene.Minute + intervalScene) || DateTime.Now.Minute == 59 && DateTime.Now.Second == 59)
            {
                saveScene();
            }
        }
        else
        {
            isStarted = false;
        }
    }

    void saveScene()
    {
        EditorSceneManager.SaveScene(scene);
        //EditorApplication.SaveScene(scene);
        lastSaveTimeScene = DateTime.Now;
        isStarted = true;
        if (showMessage)
        {
            Debug.Log("AutoSave saved: " + scene + " on " + lastSaveTimeScene);
        }
        AutoSaveScene repaintSaveWindow = (AutoSaveScene)EditorWindow.GetWindow(typeof(AutoSaveScene));
        repaintSaveWindow.Repaint();
    }
}
