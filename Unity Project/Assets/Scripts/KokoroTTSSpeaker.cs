using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;

public class KokoroTTSSpeaker : MonoBehaviour, ILocalTTSSpeaker
{
    private string ttsUrl = "http://192.168.0.22:8880";  // TTS Server URL
    public string host = "localhost";
    public int port = 8880; // TTS Server Port
    [Header("Audio Settings")]
    public AudioSource audioSource;    // Audio source for playback
    public bool playInQueue = true; // Play audio immediately or queue
    private Queue<AudioClip> audioQueue = new Queue<AudioClip>(); // Queue for audio clips
    private bool isPlaying = false;

    public float gain = 4f;

    //public bool delayPlayback = false; // Delay playback for zoom demo

    [Header("Voice Settings")]
    public VoiceOption selectedVoice = VoiceOption.af_sky;  // Default voice in Inspector

    [Header("Tools")]
    public string text = "Hello, this is a test!";  // Default text to synthesize
    public Image statusIndicator; // UI element to show status

    private const int sampleRate = 24000;               // PCM sample rate

    // This needs to moved to the STTHandler later, but for now it is here
    [Header("LLM STT Handler")]
    public STTHandler sttHandler;
    public LLMHandler llmHandler;

    private ILocalTTSSpeaker.AudioStatus audioStatus = ILocalTTSSpeaker.AudioStatus.Idle; // Current audio status

    // Enum for voice options
    public enum VoiceOption
    {
        af_heart,  // Grade A
        af_bella,  // Grade A-
        af_sarah,  // Grade C+
        af_nicole, // Grade B-
        af_sky,    // Grade C-
        zf_xiaoxiao, // Grade C
        am_adam,   // Grade F+
        am_michael,// Grade C+
        am_fenrir, // Grade C+
        bf_emma,   // Grade B-
        bf_isabella,// Grade C
        bm_george, // Grade C
        bm_lewis // Grade D+
    }

    public void Speak(string textToSynthesize, string language)
    {
        text = textToSynthesize;
        if (language == "Chinese")
        {
            selectedVoice = VoiceOption.zf_xiaoxiao; // Set to Chinese voice
        }
        else if (language == "English")
        {
            selectedVoice = VoiceOption.af_heart; // Set to English voice
        }

        StartCoroutine(StreamTTS(text));
    }

    [ContextMenu("Speak Hello World")]
    // Call this function to start speaking text
    public void Speak()
    {
        StartCoroutine(StreamTTS(text));
    }

    private IEnumerator StreamTTS(string text)
    {
        // Initialize stopwatch for latency measurement
        DateTime startTime = DateTime.Now;

        // Prepare JSON payload
        string jsonPayload = JsonUtility.ToJson(new TTSRequest
        {
            input = text,
            voice = selectedVoice.ToString(),
            response_format = "pcm",
            stream = true
        });

        ttsUrl = $"http://{host}:{port}"; // Update TTS URL with host and port

        // Create POST request
        using (UnityWebRequest request = new UnityWebRequest(ttsUrl + "/v1/audio/speech", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for data
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("TTS Response received, processing audio...");
                var latency = DateTime.Now - startTime;
                Debug.Log($"Kokoro TTS Latency: {latency.Milliseconds} ms.");
                byte[] audioData = request.downloadHandler.data;

                // play immediatly or play in queue
                if (!playInQueue)
                {
                    PlayPCMStream(audioData);
                }
                else
                {
                    EnqueueAudio(audioData);
                }
            }
            else
            {
                Debug.LogError($"Error: {request.responseCode} - {request.error}");
                audioStatus = ILocalTTSSpeaker.AudioStatus.Error;
            }
        }
    }

    private void EnqueueAudio(byte[] audioData)
    {
         if (audioData == null || audioData.Length == 0)
        {
            Debug.LogWarning("EnqueueAudio: No audio data to enqueue.");
            return;
        }
        float[] floatData = ConvertPCMToFloat(audioData);
        AudioClip clip = AudioClip.Create("TTS_Audio", floatData.Length, 1, sampleRate, false);
        clip.SetData(floatData, 0);
        audioQueue.Enqueue(clip);

        if (!isPlaying && llmHandler.isSelf)
        {
            StartCoroutine(PlayQueuedAudio());
        }
    }

    public void SpeakDelayed()
    {
        if (!isPlaying && !llmHandler.isSelf)
            StartCoroutine(PlayQueuedAudio());
    }
    private IEnumerator PlayQueuedAudio()
    {
        yield return new WaitUntil(() => audioQueue.Count > 0);
        // This check ensures we only notify "started" once for a sequence of queued items
        if (!isPlaying && audioQueue.Count > 0 && sttHandler != null)
        {
            sttHandler.NotifyTTSStarted();
        }

        llmHandler.ShowSubtitle();

        while (audioQueue.Count > 0)
        {
            isPlaying = true;
            AudioClip clip = audioQueue.Dequeue();
            audioSource.clip = clip;
            audioSource.Play();
            yield return new WaitForSeconds(clip.length);
        }
        isPlaying = false;
        // Notify STTHandler that TTS has stopped after the queue is empty
        if (sttHandler != null)
        {
            sttHandler.NotifyTTSStopped();
        }
        MessageHandler.Instance?.SendMessageToNetwork("ttsp");
        llmHandler.TriggerSubtitleFadeOut();
    }
    private void PlayPCMStream(byte[] audioData)
    {
        // Convert byte[] PCM data to float[]
        float[] floatData = ConvertPCMToFloat(audioData);

        // Create AudioClip
        AudioClip clip = AudioClip.Create("TTS_Audio", floatData.Length, 1, sampleRate, false);
        clip.SetData(floatData, 0);

        // Play AudioClip
        audioSource.clip = clip;
        audioSource.Play();

    }
    public void Stop()
    {
        StopAllCoroutines();
        audioQueue.Clear();
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        isPlaying = false;
        if (sttHandler != null)
        {
            sttHandler.NotifyTTSStopped();
        }
    }
    private float[] ConvertPCMToFloat(byte[] pcmData)
    {
        int sampleCount = pcmData.Length / 2;  // 16-bit PCM
        float[] floatData = new float[sampleCount];


        for (int i = 0; i < sampleCount; i++)
        {
            short sample = (short)((pcmData[i * 2 + 1] << 8) | pcmData[i * 2]);
            float normalized = sample / 32768.0f;
            normalized *= gain;  // Apply gain multiplier
            normalized = Mathf.Clamp(normalized, -1f, 1f);  // Prevent clipping
            floatData[i] = normalized;
        }
        return floatData;
    }

    public ILocalTTSSpeaker.AudioStatus GetStatus()
    {
        // KokoroTTS does not have download tracking, so return Completed
        return ILocalTTSSpeaker.AudioStatus.Completed;
    }

    [ContextMenu("Test Connection")]
    public void TestConnection()
    {
        StartCoroutine(TestHealthCheck());
    }

    private IEnumerator TestHealthCheck()
    {
        ttsUrl = $"http://{host}:{port}"; // Update TTS URL with host and port
        using (UnityWebRequest request = UnityWebRequest.Get(ttsUrl + "/health"))
        {
            request.timeout = 3;  // Set timeout duration

            // Send GET request to /health endpoint
            yield return request.SendWebRequest();

            // Handle different response results
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Health check response: " + request.downloadHandler.text);

                // Check if the response matches {"status":"healthy"}
                if (request.downloadHandler.text.Contains("\"status\":\"healthy\""))
                {
                    Debug.Log("Server is healthy and responding.");
                    statusIndicator.color = Color.green; // Change status indicator to green
                }
                else
                {
                    Debug.LogWarning("Server responded, but health check failed.");
                    statusIndicator.color = Color.yellow; // Change status indicator to yellow
                }
            }
            else if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Failed to connect to server: {request.responseCode} - {request.error}");
                statusIndicator.color = Color.red; // Change status indicator to red
            }
            else if (request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError("Data processing error during the health check.");
                statusIndicator.color = Color.yellow; // Change status indicator to yellow
            }
            else
            {
                Debug.LogError("Unknown error during the health check.");
            }
        }
    }

    // Helper class to create JSON payload
    [System.Serializable]
    private class TTSRequest
    {
        public string input;
        public string voice;
        public string response_format;
        public bool stream = true;
    }
}