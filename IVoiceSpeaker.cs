namespace VoiceBot;

public interface IVoiceSpeaker
{
    public Task SpeakAsync(string content);
}
