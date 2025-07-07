using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks; // Required for Task

// Class to represent the JSON data to be sent
[System.Serializable]
public class TranslationRequestData
{
    public string q;
    public string source;
    public string target;
    public string format = "text";
    public string api_key = ""; // You can also make this configurable if needed
}

// Class to represent the expected JSON response
[System.Serializable]
public class TranslationResponseData
{
    public DetectedLanguage detectedLanguage;
    public string translatedText;
}

[System.Serializable]
public class DetectedLanguage
{
    public int confidence;
    public string language;
}

public class TextTranslator : MonoBehaviour
{
    [Header("Server Configuration")]
    [Tooltip("The IP address or hostname of the translation server (e.g., localhost, 127.0.0.1)")]
    public string serverAddress = "localhost";
    [Tooltip("The port number the translation server is listening on")]
    public int serverPort = 5050;

    [Header("Translation Settings")]
    [Tooltip("Source language code (e.g., auto, en, es, fr, zh-Hans). 'auto' for auto-detection.")]
    public string sourceLanguage = "auto";
    [Tooltip("Target language code (e.g., en, es, fr, de)")]
    public string targetLanguage = "en";

    [Header("Test Translation ")]
    public string textToTest = "我今年43"; // Example text to translate
    private string _endpoint = "/translate"; // The specific endpoint for translation

    /// <summary>
    /// Sends text to the translation server and returns the translated text.
    /// </summary>
    /// <param name="textToTranslate">The text string to be translated.</param>
    /// <returns>The translated text string, or null if an error occurs.</returns>
    public async Task<string> TranslateText(string textToTranslate)
    {
        if (string.IsNullOrEmpty(textToTranslate))
        {
            Debug.LogError("Text to translate cannot be empty.");
            return null;
        }

        string url = $"http://{serverAddress}:{serverPort}{_endpoint}";

        TranslationRequestData requestData = new TranslationRequestData
        {
            q = textToTranslate,
            source = sourceLanguage,
            target = targetLanguage
            // format and api_key are set by default in the class
        };

        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"Sending translation request to: {url}\nPayload: {jsonData}");

            var operation = webRequest.SendWebRequest();

            // Await the completion of the web request
            while (!operation.isDone)
            {
                await Task.Yield(); // Yield control to allow Unity to continue processing
            }

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError ||
                webRequest.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError($"Error: {webRequest.error}\nResponse Code: {webRequest.responseCode}\nResponse Text: {webRequest.downloadHandler.text}");
                return null;
            }
            else
            {
                string responseJson = webRequest.downloadHandler.text;
                Debug.Log($"Received response: {responseJson}");
                try
                {
                    TranslationResponseData responseData = JsonUtility.FromJson<TranslationResponseData>(responseJson);
                    if (responseData != null && !string.IsNullOrEmpty(responseData.translatedText))
                    {
                        return responseData.translatedText;
                    }
                    else
                    {
                        Debug.LogError("Failed to parse translated text from response or translatedText is empty.");
                        return null;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error parsing JSON response: {ex.Message}\nResponse JSON: {responseJson}");
                    return null;
                }
            }
        }
    }

    // --- Example Usage (can be called from another script or a UI button) ---

    [ContextMenu("Test Translation")]
    public async void TestTranslation()
    {
        Debug.Log($"Attempting to translate: '{textToTest}' from '{sourceLanguage}' to '{targetLanguage}'");

        //Timer to measure translation time
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        string translated = await TranslateText(textToTest);
        stopwatch.Stop();
        Debug.Log($"Translation took: {stopwatch.ElapsedMilliseconds} ms");

        if (translated != null)
        {
            Debug.Log($"Translation successful: {translated}");
        }
        else
        {
            Debug.Log("Translation failed.");
        }
    }
    /*
    // Example of how to call this from a UI button (create a button and link its OnClick event to this method)
    public void OnTranslateButtonPressed()
    {
        // You'd typically get this text from an InputField or similar
        string inputText = "你好世界";
        _ = TestSpecificTranslation(inputText); // Call the async method, discard the task if not awaiting
    }

    public async Task TestSpecificTranslation(string text)
    {
        string translated = await TranslateText(text);
        if (translated != null)
        {
            Debug.Log($"Input: '{text}', Translated: '{translated}'");
            // Update your UI here with the translated text
        }
        else
        {
            Debug.Log($"Failed to translate: '{text}'");
        }
    }
    */
}