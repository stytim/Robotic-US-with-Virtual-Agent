using LLMUnity;
using System;
using UnityEngine;
using System.Collections.Generic; // For more advanced queuing if needed

public class LLMTranslator : MonoBehaviour
{
    // --- Singleton Instance ---
    public static LLMTranslator Instance { get; private set; }

    public LLMCharacter llmTranslator;
    private DateTime startTime;
    public bool waitingForReply = false;
    public string inputForLLM; // Used by TestLLM via ContextMenu

    // --- Callback for the current translation request ---
    private Action<string> currentTranslationCallback;
    private string currentTargetLanguage; // To log which language was targeted
    // Optional: To store an error message if something goes wrong
    // private Action<string, string> currentTranslationCallback; // (translatedText, errorMessage)

    void Awake()
    {
        // --- Singleton Pattern Implementation ---
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional: if you want it to persist across scene loads
            // Be cautious with DontDestroyOnLoad if llmTranslator
            // or other dependencies are scene-specific.
        }
        else
        {
            Debug.LogWarning("Duplicate LLMTranslator instance found. Destroying new one.");
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    void Start()
    {
        if (llmTranslator != null)
        {
            _ = llmTranslator.Warmup(WarmUpCallback);
        }
        else
        {
            Debug.LogError("LLMCharacter (llmTranslator) not assigned in the Inspector for LLMTranslator Singleton.");
        }
    }

    private void WarmUpCallback()
    {
        Debug.Log("LLM Translator Warmed Up and ready.");
        // statusIndicator.color = Color.green; // Example
    }

    [ContextMenu("Test Translate")]
    private void TestLLM()
    {
        if (llmTranslator == null)
        {
            Debug.LogError("LLMCharacter (llmTranslator) not assigned. Cannot test translate.");
            return;
        }
        if (waitingForReply)
        {
            Debug.LogWarning("LLM is currently busy. TestLLM request ignored.");
            return;
        }

        Debug.Log("Testing translation for: " + inputForLLM);
        // For testing, we can use a simple Debug.Log callback
        Translate(inputForLLM, "English", (translatedText) => {
            Debug.Log("TestLLM - Translated text: " + translatedText);
        });
    }

    /// <summary>
    /// Initiates translation of the input string to the specified target language.
    /// </summary>
    /// <param name="inputString">The text to translate.</param>
    /// <param name="targetLanguage">The language to translate the text into (e.g., "Spanish", "French", "German").</param>
    /// <param name="onTranslationComplete">Callback action that receives the translated string.</param>
    /// <returns>True if the translation request was accepted, false if the translator is busy or an error occurred before sending.</returns>
    public bool Translate(string inputString, string targetLanguage, Action<string> onTranslationComplete)
    {
        if (llmTranslator == null)
        {
            Debug.LogError("LLMCharacter (llmTranslator) not assigned. Cannot translate.");
            onTranslationComplete?.Invoke(null); // Indicate error by passing null
            return false;
        }

        if (string.IsNullOrEmpty(targetLanguage))
        {
            Debug.LogError("Target language not specified. Cannot translate.");
            onTranslationComplete?.Invoke(null); // Indicate error
            return false;
        }

        if (waitingForReply)
        {
            Debug.LogWarning($"LLMTranslator is already waiting for a reply. New translation request for \"{inputString}\" to {targetLanguage} ignored.");
            // Optionally, notify callback about busy state: onTranslationComplete?.Invoke(null); // or a specific "busy" message
            return false;
        }

        waitingForReply = true;
        this.currentTranslationCallback = onTranslationComplete;
        this.currentTargetLanguage = targetLanguage; // Store for logging
        startTime = DateTime.Now;

        // Construct the prompt dynamically with the target language
        string prompt = $"Translate this text to {targetLanguage}: {inputString}";

        Debug.Log($"Requesting translation: \"{prompt}\"");
        _ = llmTranslator.Chat(prompt, ProcessLLMResponse, ReplyCompleted);
        return true;
    }

    private void ProcessLLMResponse(string response)
    {
        // Assuming 'response' is the final, complete translated text.
        // If the LLM streams responses, you would need to accumulate text here
        // and invoke the callback in ReplyCompleted with the full accumulated text.
        Debug.Log("Translated text: " + response);
        var latency = DateTime.Now - startTime;
        Debug.Log($"LLM Latency: {latency.Milliseconds} ms.");

        // Invoke the stored callback with the translated text
        currentTranslationCallback?.Invoke(response);
    }

    void ReplyCompleted()
    {
        Debug.Log("LLM Reply Completed.");
        waitingForReply = false;
        // It's good practice to clear the callback after it's used or when the operation completes,
        // to prevent it from being accidentally called again and to release references.
        // If ProcessLLMResponse is guaranteed to be called before ReplyCompleted with the full response,
        // clearing it here is fine.
        currentTranslationCallback = null;
    }

    // Optional: If you want to provide a way to check status from other scripts
    public bool IsBusy()
    {
        return waitingForReply;
    }
}