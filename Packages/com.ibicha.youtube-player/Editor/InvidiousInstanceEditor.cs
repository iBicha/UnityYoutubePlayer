using UnityEditor;
using UnityEngine;
using YoutubePlayer.Components;

namespace YoutubePlayer.Editor
{
    [CustomEditor(typeof(InvidiousInstance))]
    class InvidiousInstanceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var instanceTypeProperty = serializedObject.FindProperty("InstanceType");
            EditorGUILayout.PropertyField(instanceTypeProperty);

            var instanceType = (InvidiousInstance.InvidiousInstanceType)instanceTypeProperty.enumValueIndex;
            if (instanceType == InvidiousInstance.InvidiousInstanceType.Public)
            {
                EditorGUILayout.HelpBox("The public instances is fetched at runtime from https://api.invidious.io and the first from the list will be used.", MessageType.Info);
            }
            else if (instanceType == InvidiousInstance.InvidiousInstanceType.Custom)
            {
                var customInstanceUrlProperty = serializedObject.FindProperty("CustomInstanceUrl");
                EditorGUILayout.PropertyField(customInstanceUrlProperty);

                var customInstanceUrl = customInstanceUrlProperty.stringValue;
                if (!ValidateInstanceUrl(customInstanceUrl, out var errorMessage))
                {
                    EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
                }
            }

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        bool ValidateInstanceUrl(string instanceUrl, out string errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrEmpty(instanceUrl))
            {
                return true;
            }

            if (!instanceUrl.StartsWith("http://") && !instanceUrl.StartsWith("https://"))
            {
                errorMessage = "Instance URL must start with http:// or https://";
                return false;
            }

            if (instanceUrl.EndsWith("/"))
            {
                errorMessage = "Instance URL must not end with a forward slash (/)";
                return false;
            }

            return true;
        }
    }
}
