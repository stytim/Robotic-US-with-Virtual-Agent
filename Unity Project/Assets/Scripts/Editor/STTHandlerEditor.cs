using UnityEngine;
using UnityEditor;
using System.Collections.Generic; // Required for HashSet

[CustomEditor(typeof(STTHandler))]
public class STTHandlerEditor : Editor
{
    private STTHandler sttHandler;
    private SerializedProperty activeMicrophoneDeviceForInfoProperty;
    // We don't need to store every property here if we iterate,
    // but it's good to get specific ones if you need special handling.

    private void OnEnable()
    {
        sttHandler = (STTHandler)target; // Get the instance of STTHandler being inspected
        // Find the specific property for our custom read-only display
        activeMicrophoneDeviceForInfoProperty = serializedObject.FindProperty("activeMicrophoneDeviceForInfo");

        if (activeMicrophoneDeviceForInfoProperty == null)
        {
            Debug.LogWarning("STTHandlerEditor: Could not find SerializedProperty 'activeMicrophoneDeviceForInfo'.");
        }
    }

    public override void OnInspectorGUI()
    {
        // Always call this at the beginning
        serializedObject.Update();

        EditorGUILayout.LabelField("Microphone Configuration", EditorStyles.boldLabel);

        // --- Custom Microphone Selection Dropdown ---
        string[] microphoneDevices = Microphone.devices;
        if (microphoneDevices.Length == 0)
        {
            EditorGUILayout.HelpBox("No microphone devices found. Please ensure a microphone is connected.", MessageType.Warning);
        }
        else
        {
            int currentDeviceIndex = -1;
            // Get the currently stored microphone name from the STTHandler instance
            string currentDeviceName = sttHandler.selectedMicrophoneDeviceName;

            if (!string.IsNullOrEmpty(currentDeviceName))
            {
                for (int i = 0; i < microphoneDevices.Length; i++)
                {
                    if (microphoneDevices[i] == currentDeviceName)
                    {
                        currentDeviceIndex = i;
                        break;
                    }
                }
            }

            // If the stored name isn't in the current list (e.g., device unplugged)
            if (currentDeviceIndex == -1 && !string.IsNullOrEmpty(currentDeviceName))
            {
                EditorGUILayout.HelpBox($"Previously selected: '{currentDeviceName}' (now disconnected or not found). Please select an available device.", MessageType.Warning);
            }

            int newSelectedDeviceIndex = EditorGUILayout.Popup("Select Microphone", currentDeviceIndex, microphoneDevices);

            if (newSelectedDeviceIndex != currentDeviceIndex)
            {
                Undo.RecordObject(sttHandler, "Select Microphone Device"); // For Undo support
                if (newSelectedDeviceIndex >= 0 && newSelectedDeviceIndex < microphoneDevices.Length)
                {
                    // Call the method in STTHandler to update the name
                    sttHandler.OnMicrophoneSelectedInEditor(microphoneDevices[newSelectedDeviceIndex]);
                }
                else
                {
                    sttHandler.OnMicrophoneSelectedInEditor(string.Empty); // Handle deselection or error
                }
                EditorUtility.SetDirty(sttHandler); // Mark STTHandler as changed to ensure data saves
            }
        }

        // --- Custom Display for Active Microphone (Read-only) ---
        if (activeMicrophoneDeviceForInfoProperty != null)
        {
            GUI.enabled = false; // Temporarily disable GUI to make the field read-only
            EditorGUILayout.PropertyField(activeMicrophoneDeviceForInfoProperty, new GUIContent("Active Microphone (Info)"));
            GUI.enabled = true;  // Re-enable GUI for subsequent fields
        }

        EditorGUILayout.Space(); // Adds a little visual separation

        // --- Draw all other properties ---
        EditorGUILayout.LabelField("Other Settings & Events", EditorStyles.boldLabel);

        // Define a list of property names that we have already handled above
        // or that are handled by specific attributes like [HideInInspector]
        var manuallyHandledProperties = new HashSet<string>
        {
            "m_Script",                         // Default script field, always good to skip explicitly
            "selectedMicrophoneDeviceName",     // We handle this with the dropdown (it's also [HideInInspector])
            "activeMicrophoneDeviceForInfo"     // We draw this manually as read-only info
        };

        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true; // To ensure the first property is entered
        while (property.NextVisible(enterChildren))
        {
            enterChildren = false; // Subsequent calls should not re-enter children of the same top-level property

            if (!manuallyHandledProperties.Contains(property.name))
            {
                EditorGUILayout.PropertyField(property, true); // 'true' ensures that children (like lists, UnityEvents) are drawn
            }
        }

        // Always call this at the end
        serializedObject.ApplyModifiedProperties();
    }
}