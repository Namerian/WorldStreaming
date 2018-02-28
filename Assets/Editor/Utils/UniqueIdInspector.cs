using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Utilities
{
    [CustomEditor(typeof(UniqueId))]
    public class UniqueIdInspector : Editor
    {
        private UniqueId self;

        private SerializedProperty idProperty;

        private void OnEnable()
        {
            self = target as UniqueId;

            idProperty = serializedObject.FindProperty("id");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Id", idProperty.stringValue);
        }
    }
}