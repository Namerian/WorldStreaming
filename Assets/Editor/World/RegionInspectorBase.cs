using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.World
{
    public abstract class RegionInspectorBase : Editor
    {
        protected RegionBase self;

        private SerializedProperty idProperty;
        private SerializedProperty boundsSizeProperty;
        private SerializedProperty overrideRenderDistanceFarProperty;
        private SerializedProperty overrideRenderDistanceInactiveProperty;
        private SerializedProperty localRenderDistanceFarProperty;
        private SerializedProperty localRenderDistanceInactiveProperty;
        private SerializedProperty drawBoundsProperty;

        private void OnEnable()
        {
            self = target as RegionBase;

            idProperty = serializedObject.FindProperty("id");
            boundsSizeProperty = serializedObject.FindProperty("boundsSize");
            overrideRenderDistanceFarProperty = serializedObject.FindProperty("overrideRenderDistanceFar");
            overrideRenderDistanceInactiveProperty = serializedObject.FindProperty("overrideRenderDistanceInactive");
            localRenderDistanceFarProperty = serializedObject.FindProperty("localRenderDistanceFar");
            localRenderDistanceInactiveProperty = serializedObject.FindProperty("localRenderDistanceInactive");
            drawBoundsProperty = serializedObject.FindProperty("drawBounds");

            if (string.IsNullOrEmpty(idProperty.stringValue))
            {
                idProperty.stringValue = Guid.NewGuid().ToString();

                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            EditorGUILayout.LabelField("----");

            EditorGUILayout.LabelField("Id", idProperty.stringValue);

            boundsSizeProperty.vector3Value = EditorGUILayout.Vector3Field("Bounds Size", boundsSizeProperty.vector3Value);

            EditorGUILayout.LabelField("Render Distances");

            overrideRenderDistanceFarProperty.boolValue = EditorGUILayout.Toggle("  Override Far", overrideRenderDistanceFarProperty.boolValue);

            if (overrideRenderDistanceFarProperty.boolValue)
            {
                localRenderDistanceFarProperty.floatValue = EditorGUILayout.FloatField("  Far", localRenderDistanceFarProperty.floatValue);
            }

            overrideRenderDistanceInactiveProperty.boolValue = EditorGUILayout.Toggle("  Override Inactive", overrideRenderDistanceInactiveProperty.boolValue);

            if (overrideRenderDistanceInactiveProperty.boolValue)
            {
                localRenderDistanceInactiveProperty.floatValue = EditorGUILayout.FloatField("  Inactive", localRenderDistanceInactiveProperty.floatValue);
            }

            EditorGUILayout.LabelField("----");

            drawBoundsProperty.boolValue = EditorGUILayout.Toggle("Draw Bounds", drawBoundsProperty.boolValue);

            if (!Application.isPlaying && self.transform.parent.GetComponent<World>().SubScenesLoaded)
            {
                GUILayout.Label("");

                if (GUILayout.Button("Create SubScenes"))
                {
                    self.SendMessage("CreateSubScenes");
                }

                GUILayout.Label("");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }


    //public abstract class RegionInspectorBase : Editor
    //{
    //    protected RegionEditorBase self;

    //    private void OnEnable()
    //    {
    //        self = target as RegionEditorBase;
    //    }

    //    /// <summary>
    //    /// Shows the Load/Save button for a group of SubScenes.
    //    /// </summary>
    //    /// <param name="name"></param>
    //    /// <param name="subSceneTypes"></param>
    //    protected void ShowSubSceneMenuGroup(string name, List<eSubSceneType> subSceneTypes)
    //    {
    //        EditorGUILayout.LabelField(name);

    //        if (self.IsAnySubSceneLoaded())
    //        {
    //            if (GUILayout.Button(string.Concat("Save ", name)))
    //            {
    //                foreach (var subSceneType in subSceneTypes)
    //                {
    //                    self.SaveSubScene(subSceneType);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            if (GUILayout.Button(string.Concat("Load ", name)))
    //            {
    //                foreach (var subSceneType in subSceneTypes)
    //                {
    //                    self.LoadSubScene(subSceneType);
    //                }
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Shows the Load/Save button for a SubScene.
    //    /// </summary>
    //    /// <param name="subSceneType"></param>
    //    protected void ShowSubSceneMenu(eSubSceneType subSceneType)
    //    {
    //        EditorGUILayout.LabelField(subSceneType.ToString());

    //        if (self.IsSubSceneLoaded(subSceneType))
    //        {
    //            if (GUILayout.Button("Save SubScene "))
    //            {
    //                self.SaveSubScene(subSceneType);
    //            }
    //        }
    //        else
    //        {
    //            if (GUILayout.Button("Load SubScene "))
    //            {
    //                self.LoadSubScene(subSceneType);
    //            }
    //        }
    //    }
    //}
}