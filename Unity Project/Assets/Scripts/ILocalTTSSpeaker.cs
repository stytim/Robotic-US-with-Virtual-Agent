public interface ILocalTTSSpeaker
{
    public enum AudioStatus
    {
        Idle,
        Downloading,
        Playing,
        Completed,
        Error
    }
    void Speak(string text, string language = "English");
    void SpeakDelayed();
    void Stop();
    void TestConnection();
    AudioStatus GetStatus();
}
