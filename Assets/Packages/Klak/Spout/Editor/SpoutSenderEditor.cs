// KlakSpout - Spout realtime video sharing plugin for Unity
// https://github.com/keijiro/KlakSpout
using UnityEngine;
using UnityEditor;

namespace Klak.Spout
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SpoutSender))]
    public class SpoutSenderEditor : Editor
    {
        SerializedProperty _senderName;
        SerializedProperty _clearAlpha;
        SerializedProperty _sourceTexture;

        void OnEnable()
        {
            _senderName = serializedObject.FindProperty("_senderName");
            _clearAlpha = serializedObject.FindProperty("_clearAlpha");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var sender = (SpoutSender)target;
            var camera = sender.GetComponent<Camera>();

            if (camera != null)
            {
                EditorGUILayout.HelpBox(
                    "Spout Sender is running in camera capture mode.",
                    MessageType.None
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "NDI Sender is running in render texture mode.",
                    MessageType.None
                );

                EditorGUILayout.PropertyField(_sourceTexture);
            }

            EditorGUILayout.PropertyField(_senderName);
            EditorGUILayout.PropertyField(_clearAlpha);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
