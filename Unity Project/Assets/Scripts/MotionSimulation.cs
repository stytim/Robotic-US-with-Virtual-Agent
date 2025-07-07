using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MotionSimulation : MonoBehaviour
{
    public TTSHandler ttsHandler;
    public Transform[] robot_link;
    public Transform[] proxy_link;
    private Quaternion[] currentPose;

    private Quaternion[] startPose;
    private Quaternion[] endPose;

    public float moveSpeed = 1.0f;

    public Material proxymat;
    public Material holdermat;

    public bool isAnimating = false;
    public bool isDisappearing = false;

    // Start is called before the first frame update
    void Start()
    {
        startPose = ReadPoseFromFile("StartPose.txt");
        endPose = ReadPoseFromFile("EndPose.txt");
    }

    // Update is called once per frame
    void Update()
    {
        if (isAnimating)
        {
            // Gradually change the material transparency from 0 to 0.8 over 3 seconds
            Color color = proxymat.color;  // Get the current color
            float transparency = Mathf.Clamp(color.a + (Time.deltaTime / 3f) * 0.8f, 0, 0.8f);  // Adjust alpha gradually
            color.a = transparency;  // Set the new alpha
            proxymat.color = color;  // Apply the new color to the material


            Color colorholder = holdermat.color;
            colorholder.a = Mathf.Clamp(colorholder.a + (Time.deltaTime / 3f) * 0.8f, 0, 0.8f); ;
            holdermat.color = colorholder;

            // Stop animating once the transparency reaches 0.8
            if (transparency >= 0.8f)
            {
                isAnimating = false;
            }
        }

        if (isDisappearing)
        {
            // Gradually change the material transparency from 0.8 to 0 over 3 seconds
            Color color = proxymat.color;  // Get the current color
            float transparency = Mathf.Clamp(color.a - (Time.deltaTime / 3f) * 0.8f, 0, 0.8f);  // Adjust alpha gradually
            color.a = transparency;  // Set the new alpha
            proxymat.color = color;  // Apply the new color to the material

            Color colorholder = holdermat.color;
            colorholder.a = Mathf.Clamp(colorholder.a - (Time.deltaTime / 3f) * 0.8f, 0, 0.8f); ;
            holdermat.color = colorholder;

            // Stop animating once the transparency reaches 0.8
            if (transparency <= 0f)
            {
                isDisappearing = false;
                MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer renderer in renderers)
                {
                    renderer.enabled = false;
                }
            }
        }
    }

    [ContextMenu("Start Animation")]
    public void StartAnimation()
    {
        if (currentPose == null)
        {
            currentPose = new Quaternion[7];
            for (int i = 1; i <= 7; i++)
            {
                currentPose[i - 1] = robot_link[i - 1].localRotation;  // Store current rotation in the array
            }
        }
            
        for (int i = 1; i <= 7; i++)
        {
            proxy_link[i - 1].localRotation = currentPose[i - 1];
        }

        Color startColor = proxymat.color;
        startColor.a = 0;
        proxymat.color = startColor;

        Color startColorholder = holdermat.color;
        startColorholder.a = 0;
        holdermat.color = startColorholder;

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = true;
        }

        isAnimating = true;
        Debug.Log("Moving to First point");
        StartCoroutine(RotateLinksToPose(currentPose, startPose, 5, () =>
        {
            Debug.Log("Moving to Second point");
            StartCoroutine(RotateLinksToPose(startPose, endPose, moveSpeed, () =>
            {
                isDisappearing = true;
                Debug.Log("Fade out Animation");
                ttsHandler.Speak("Now that you have seen the planned trajectory, are you ready to start the procedure?");
            }));
            
        }));

    }

    [ContextMenu("Stop Animation")]
    void StopAnimation()
    {

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = false;
        }
    }

    [ContextMenu("Record Start Pose")]
    public void RecordStartPose()
    {

        for (int i = 1; i <= 7; i++)
        {
            startPose[i - 1] = robot_link[i - 1].localRotation;  // Store current rotation in the array
        }

        // Create a string to write to the file with only numbers
        string fileContent = "";
        for (int i = 0; i < 7; i++)
        {
            Quaternion rotation = startPose[i];
            // Concatenate the quaternion components as a single line of numbers
            fileContent += $"{rotation.x} {rotation.y} {rotation.z} {rotation.w}\n";
        }

        // Define the path where the file will be saved
        string filePath = Path.Combine(Application.persistentDataPath, "StartPose.txt");

        // Write the string to the file
        try
        {
            File.WriteAllText(filePath, fileContent);
            Debug.Log($"Start Pose Recorded and saved to {filePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to write start pose to file: {e.Message}");
        }


        Debug.Log("Start Pose Recorded");
    }

    [ContextMenu("Record End Pose")]
    public void RecordEndPose()
    {

        for (int i = 1; i <= 7; i++)
        {
            endPose[i - 1] = robot_link[i - 1].localRotation;  // Store current rotation in the array
        }

        // Create a string to write to the file with only numbers
        string fileContent = "";
        for (int i = 0; i < 7; i++)
        {
            Quaternion rotation = endPose[i];
            // Concatenate the quaternion components as a single line of numbers
            fileContent += $"{rotation.x} {rotation.y} {rotation.z} {rotation.w}\n";
        }

        // Define the path where the file will be saved
        string filePath = Path.Combine(Application.persistentDataPath, "EndPose.txt");

        // Write the string to the file
        try
        {
            File.WriteAllText(filePath, fileContent);
            Debug.Log($"End Pose Recorded and saved to {filePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to write start pose to file: {e.Message}");
        }

        Debug.Log("End Pose Recorded");
    }

    private IEnumerator RotateLinksToPose(Quaternion[] fromPose, Quaternion[] toPose, float duration, System.Action onComplete)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            for (int i = 0; i < 7; i++)
            {
                // Slerp rotation
                proxy_link[i].localRotation = Quaternion.Slerp(fromPose[i], toPose[i], elapsedTime / duration);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final rotation is exactly at the target pose
        for (int i = 0; i < 7; i++)
        {
            proxy_link[i].localRotation = toPose[i];
        }

        // Trigger the next phase if any
        onComplete?.Invoke();
    }


    public Quaternion[] ReadPoseFromFile(string fileName)
    {
        // Define the file path
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        // Check if the file exists
        if (!File.Exists(filePath))
        {
            Debug.Log($"File {filePath} not found!");
            return new Quaternion[7];
        }

        try
        {
            // Read all lines from the file
            string[] lines = File.ReadAllLines(filePath);

            // Create an array of Quaternions to hold the data
            Quaternion[] quaternions = new Quaternion[lines.Length];

            // Parse each line into a Quaternion
            for (int i = 0; i < lines.Length; i++)
            {
                // Split the line into individual string numbers
                string[] components = lines[i].Split(' ');

                // Ensure there are exactly 4 components to form a Quaternion
                if (components.Length == 4)
                {
                    float x = float.Parse(components[0]);
                    float y = float.Parse(components[1]);
                    float z = float.Parse(components[2]);
                    float w = float.Parse(components[3]);

                    // Create a Quaternion from the parsed values and store it in the array
                    quaternions[i] = new Quaternion(x, y, z, w);
                }
                else
                {
                    Debug.LogError($"Invalid data format on line {i + 1}");
                }
            }

            return quaternions;
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to read file {filePath}: {e.Message}");
            return null;
        }
    }

}
