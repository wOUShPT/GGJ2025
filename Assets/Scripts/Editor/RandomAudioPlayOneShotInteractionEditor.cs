using AstralShift.QTI.Interactions;
using AstralShift.QTI.Interactions.Audio;
using UnityEditor;

[CustomEditor(typeof(RandomAudioPlayOneShotInteraction))]
public class RandomAudioPlayOneShotInteractionEditor : InteractionEditor
{
    public override void DrawProperties()
    {
        serializedObject.Update();

        SerializedProperty audioClipProp = serializedObject.FindProperty("audioClips");
        EditorGUILayout.PropertyField(audioClipProp);

        SerializedProperty volumeProp = serializedObject.FindProperty("volume");
        EditorGUILayout.PropertyField(volumeProp);

        SerializedProperty modeEnumProp = serializedObject.FindProperty("mode");
        EditorGUILayout.PropertyField(modeEnumProp);


        switch ((AudioPlayOneShotInteraction.AudioPlayOneShotInteractionMode)modeEnumProp.enumValueIndex)
        {
            case AudioPlayOneShotInteraction.AudioPlayOneShotInteractionMode.AudioSource:

                SerializedProperty audioSourceProp = serializedObject.FindProperty("audioSource");
                EditorGUILayout.PropertyField(audioSourceProp);
                break;


            case AudioPlayOneShotInteraction.AudioPlayOneShotInteractionMode.Position3D:

                SerializedProperty clipPositionProp = serializedObject.FindProperty("clipPosition");
                EditorGUILayout.PropertyField(clipPositionProp);
                break;

            // case Position2D ? Display nothing
        }

        serializedObject.ApplyModifiedProperties();
    }
}