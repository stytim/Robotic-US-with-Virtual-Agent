using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSHandler : MonoBehaviour
{
    public enum LocalTTSOption
    {
        KokoroTTS
    }

    [Header("Select Local TTS")]
    public LocalTTSOption selectedLocalTTS = LocalTTSOption.KokoroTTS;

    [Header("TTS Speakers")]
    public AudioSource TTSAudioSource;
    public KokoroTTSSpeaker kokoroTTSSpeaker;

    private ILocalTTSSpeaker TSSpeaker;


    [Header("Head")]
    public Transform head;

    // Start is called before the first frame update
    void Start()
    {
        TSSpeaker = GetSelectedLocalSpeaker();
        TestLocalTTSConnection();
    }

    private void TestLocalTTSConnection()
    {
        TSSpeaker.TestConnection();
    }

    private ILocalTTSSpeaker GetSelectedLocalSpeaker()
    {
        switch (selectedLocalTTS)
        {
            case LocalTTSOption.KokoroTTS:
                return kokoroTTSSpeaker;
            default:
                return kokoroTTSSpeaker;
        }
    }

    // Update is called once per frame
    void Update()
    {
        TTSAudioSource.transform.position = head.position;
    }

    public void Speak(string input, string language = "English")
    {
        TSSpeaker.Speak(input, language);
    }

    public bool waitingForAudioSynthesize()
    {
        return TSSpeaker.GetStatus() == ILocalTTSSpeaker.AudioStatus.Downloading;
    }

    public void ReportError()
    {
        TSSpeaker.Speak("I'm sorry, could you repeat that?");
    }

    public void PlayDelayedAudio()
    {
        TSSpeaker.SpeakDelayed();
    }

    public void CancelSpeak()
    {
        TSSpeaker.Stop();
        // sttHandler.NotifyTTSStopped();
    }
}