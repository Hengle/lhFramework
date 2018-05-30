using UnityEditor;
using UnityEngine;

namespace Framework.Infrastructure
{
    [CustomEditor(typeof(TimelineEventAsset))]
    internal sealed class TimelineEventAssetEditor : UnityEditor.Editor
    {
        private static readonly string[] m_excludeFields = new string[] { "m_Script", "behavior" };
        private SerializedProperty m_controlProperty = null;
        private SerializedProperty m_behaivorProperty = null;
        private static readonly GUIContent kTimelineControlLabel
            = new GUIContent("Timeline Control", "The virtual camera to use for this shot");
        TimelineControl m_cachedReferenceObject;
        UnityEditor.Editor[] m_editors = null;
        private void OnEnable()
        {
            if (serializedObject != null)
            {
                m_controlProperty = serializedObject.FindProperty("controlRefer");
                m_behaivorProperty = serializedObject.FindProperty("behavior");
            }
        }

        private void OnDisable()
        {
            DestroyComponentEditors();
        }

        private void OnDestroy()
        {
            DestroyComponentEditors();
        }

        public override void OnInspectorGUI()
        {
            TimelineControl obj
                = m_controlProperty.exposedReferenceValue as TimelineControl;
            if (obj == null)
            {
                serializedObject.Update();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_controlProperty, kTimelineControlLabel, GUILayout.ExpandWidth(true));
                obj = m_controlProperty.exposedReferenceValue as TimelineControl;
                //if ((obj == null) && GUILayout.Button(new GUIContent("Create"), GUILayout.ExpandWidth(false)))
                //{
                //    TimelineControl vcam = CinemachineMenu.CreateDefaultVirtualCamera();
                //    mControlProperty.exposedReferenceValue = vcam;
                //}
                EditorGUILayout.EndHorizontal();
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                serializedObject.Update();
                DrawPropertiesExcluding(serializedObject, m_excludeFields);
                // Create an editor for each of the cinemachine virtual cam and its components
                UpdateComponentEditors(obj);
                if (m_editors != null)
                {
                    foreach (UnityEditor.Editor e in m_editors)
                    {
                        EditorGUILayout.Separator();
                        if (e.target.GetType() != typeof(Transform))
                        {
                            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
                            EditorGUILayout.LabelField(e.target.GetType().Name, EditorStyles.boldLabel);
                        }
                        e.OnInspectorGUI();
                    }
                }

                EditorGUILayout.Separator();
                GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
                EditorGUILayout.LabelField("Event", EditorStyles.boldLabel);
                var graphStartEvents = m_behaivorProperty.FindPropertyRelative("graphStartEvents");
                var graphEndEvents = m_behaivorProperty.FindPropertyRelative("graphEndEvents");
                var behaviorStartEvents = m_behaivorProperty.FindPropertyRelative("behaviorStartEvents");
                var behaviorEndEvents = m_behaivorProperty.FindPropertyRelative("behaviorEndEvents");
                var behaviorUpdateEvents = m_behaivorProperty.FindPropertyRelative("behaviorUpdateEvents");
                UpdateEvents(graphStartEvents, "GraphStart");
                UpdateEvents(graphEndEvents, "GraphEnd");
                UpdateEvents(behaviorStartEvents, "BehaviorStart");
                UpdateEvents(behaviorEndEvents, "BehaviorEnd");
                UpdateEvents(behaviorUpdateEvents, "BehaviorUpdate");
                serializedObject.ApplyModifiedProperties();
            }
        }

        void UpdateComponentEditors(TimelineControl obj)
        {
            MonoBehaviour[] components = null;
            if (obj != null)
                components = obj.gameObject.GetComponents<MonoBehaviour>();
            int numComponents = (components == null) ? 0 : components.Length;
            int numEditors = (m_editors == null) ? 0 : m_editors.Length;
            if (m_cachedReferenceObject != obj || (numComponents + 1) != numEditors)
            {
                DestroyComponentEditors();
                m_cachedReferenceObject = obj;
                if (obj != null)
                {
                    m_editors = new UnityEditor.Editor[components.Length + 1];
                    CreateCachedEditor(obj.gameObject.GetComponent<Transform>(), null, ref m_editors[0]);
                    for (int i = 0; i < components.Length; ++i)
                        CreateCachedEditor(components[i], null, ref m_editors[i + 1]);
                }
            }
        }

        void DestroyComponentEditors()
        {
            m_cachedReferenceObject = null;
            if (m_editors != null)
            {
                for (int i = 0; i < m_editors.Length; ++i)
                {
                    if (m_editors[i] != null)
                        UnityEngine.Object.DestroyImmediate(m_editors[i]);
                    m_editors[i] = null;
                }
                m_editors = null;
            }
        }
        void UpdateEvents(SerializedProperty property, string context)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(context, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+", EditorStyles.miniBoldLabel))
                {
                    property.arraySize++;
                    var a = property.GetArrayElementAtIndex(property.arraySize - 1);
                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < property.arraySize; i++)
            {
                var keyValue = property.GetArrayElementAtIndex(i);
                SerializedProperty key = keyValue.FindPropertyRelative("key");
                EditorGUILayout.BeginVertical(EditorStyles.textField);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.PropertyField(key, GUILayout.ExpandWidth(false));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("-", EditorStyles.miniBoldLabel))
                        {
                            property.DeleteArrayElementAtIndex(i);
                            i--;
                            serializedObject.ApplyModifiedProperties();
                            Repaint();
                            return;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    SerializedProperty value = keyValue.FindPropertyRelative("value");
                    EditorGUILayout.PropertyField(value);
                }
                EditorGUILayout.EndVertical();
            }

        }
    }
}