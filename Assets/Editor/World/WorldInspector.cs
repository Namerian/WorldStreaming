using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.World
{
    [CustomEditor(typeof(World))]
    public class WorldInspector : Editor
    {
        private World self;

        private SerializedProperty worldSizeProperty;
        private SerializedProperty renderDistanceFarProperty;
        private SerializedProperty renderDistanceInactiveProperty;
        private SerializedProperty drawBoundsProperty;
        private SerializedProperty subScenesLoaded;

        private void OnEnable()
        {
            self = target as World;

            worldSizeProperty = serializedObject.FindProperty("worldSize");
            renderDistanceFarProperty = serializedObject.FindProperty("renderDistanceFar");
            renderDistanceInactiveProperty = serializedObject.FindProperty("renderDistanceInactive");
            drawBoundsProperty = serializedObject.FindProperty("drawBounds");
            subScenesLoaded = serializedObject.FindProperty("editorSubScenesLoaded");
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            EditorGUILayout.LabelField("----");

            worldSizeProperty.vector3Value = EditorGUILayout.Vector3Field("World Size", worldSizeProperty.vector3Value);

            EditorGUILayout.LabelField("Render Distances");

            renderDistanceFarProperty.floatValue = EditorGUILayout.FloatField("  Far", renderDistanceFarProperty.floatValue);
            renderDistanceInactiveProperty.floatValue = EditorGUILayout.FloatField("  Inactive", renderDistanceInactiveProperty.floatValue);

            EditorGUILayout.LabelField("----");

            drawBoundsProperty.boolValue = EditorGUILayout.Toggle("Draw Bounds", drawBoundsProperty.boolValue);

            if (!Application.isPlaying)
            {
                GUILayout.Label("");

                if (subScenesLoaded.boolValue)
                {
                    if (GUILayout.Button("Export SubScenes"))
                    {
                        self.ExportSubScenes();
                    }

                    if (GUILayout.Button("Clear SubScene Folder"))
                    {
                        self.ClearSubSceneFolder();
                    }
                }
                else
                {
                    if (GUILayout.Button("Import SubScenes"))
                    {
                        self.ImportSubScenes();
                    }
                }

                GUILayout.Label("");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}