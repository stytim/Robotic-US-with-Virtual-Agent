using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConversationHandler : MonoBehaviour
{
    [Header("Scripts")]
    public STTHandler sttHandler;
    public LLMHandler llmHandler;
    public TTSHandler ttsHandler;

    [Header("Settings")]
    public bool allowInterruptions = false;

    public OVRInput.Axis2D thumbstick;
    private bool isThumbstickactivated = false;
    public GameObject micIndicator;

    // [Header("Study Information")]
    // public string doctorName;
    // public string patientName;

    [Header("Conversation")]
    public AudioSource audioSource;
    // public string greeting;
    // public string startProcedure;
    // public string endProcedure;
    public bool procedureStarted;
    public bool procedureEnded;

    public Image statusIndicator; // UI element to show status

    public StartEndSignalPublisher startEndSignalPublisher;
    private Coroutine conversationRoutine;

    // Start is called before the first frame update
    void Start()
    {
       // TrrigerConversation();
    }

    // Update is called once per frame
    void Update()
    {
        // if (procedureStarted)
        // {
        //     if (ttsHandler != null)
        //     {
        //         ttsHandler.Speak(startProcedure);
        //     }

        //    //startEndSignalPublisher.SendStartSignal(true);
        //     procedureStarted = false;
        // }

        if (procedureEnded)
        {
            if (llmHandler != null)
            {
                llmHandler.PatientQuestion("System: Scan Done");
            }
           // startEndSignalPublisher.SendStartSignal(false);
            procedureEnded = false;
        }


        isThumbstickactivated = false;
        if (OVRInput.Get(thumbstick).y > 0.7f || OVRInput.Get(thumbstick).x > 0.7f)
        {

            isThumbstickactivated = true;
        }


        if (micIndicator == null)
        {
            Debug.LogWarning("Mic Indicator GameObject is not assigned.");
            return;
        }
        if (isThumbstickactivated)
        {
            micIndicator.SetActive(true);
        }
        else
        {
            micIndicator.SetActive(false);
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 140, 0, 140, 120));

        if (GUILayout.Button("Start Conversation"))
        {
            StartConversation();
        }
        //if (GUILayout.Button("Preparation Start"))
        //{
        //    llmHandler.Question("System: Preparation Start");
        //}
        //if (GUILayout.Button("Preparation Done"))
        //{
        //    llmHandler.Question("System: Preparation Done");
        //}

        if (GUILayout.Button("Stop Conversation"))
        {
            StopConversation();
        }

        GUILayout.EndArea();
    }

    public void TrrigerConversation()
    {
        if (conversationRoutine == null)
        {
            StartConversation();
            statusIndicator.color = Color.blue; 
        }
        else
        {
            StopConversation();
            statusIndicator.color = Color.white; 
        }
    }

    public void RedoLastRequest()
    {
        StopConversation();
        StartConversation();
        llmHandler.ReTry();

    }

    private void StartConversation()
    {
        //llmHandler.Question("System: Start the Conversation");

        if ((sttHandler == null) || (llmHandler == null) || (audioSource == null) || conversationRoutine != null) return;

        sttHandler.RequestConnect();
        conversationRoutine = StartCoroutine(ListeningRoutine());
    }

    private void StopConversation()
    {
        if (conversationRoutine != null)
        {
            StopCoroutine(conversationRoutine);
            conversationRoutine = null;
        }

        _ = sttHandler.Disconnect();

        try
        {
            if (llmHandler != null)
            {
                llmHandler.CancelReply();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error cancelling LLM Handler reply: {ex.Message}");
        }

        // llmHandler.CancelReply();
        ttsHandler.CancelSpeak();

    }

    public void HandleVoiceInterruption()
    {
        Debug.Log("Voice interruption detected.");
        llmHandler.CancelReply();
        ttsHandler.CancelSpeak();
    }

    string GetTimeOfDay()
    {
        System.DateTime now = System.DateTime.Now;
        int hour = now.Hour;

        if (hour >= 5 && hour < 12) return "Morning";
        else if (hour >= 12 && hour < 17) return "Afternoon";
        else if (hour >= 17 && hour < 21) return "Evening";
        else return "Night";
    }

    string GetGreeting(string timeOfDay, string yourName, string myName, string template)
    {
        string result = string.Format("Good {0}, {1}. I'm {2}. {3}", timeOfDay, yourName, myName, template);

        return result;
    }

    private IEnumerator ListeningRoutine()
    {
        while (true)
        {
            //if (allowInterruptions)
            //{
            //    // If interruptions are allowed, STT should generally always be recording.
            //    // The interruption is handled when STT detects speech while TTS is active.
            //    if (!sttHandler.isRecording)
            //    {
            //        sttHandler.StartRecording();
            //    }
            //}
            //else
            {
                if (audioSource.isPlaying || llmHandler.waitingForReply || ttsHandler.waitingForAudioSynthesize())
                {
                    sttHandler.StopRecording();
                }
                if (isThumbstickactivated)
                {
                    sttHandler.StartRecording();
                }
            }

            yield return null;
        }
    }
}