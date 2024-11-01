namespace VoiceBot.Services;

public interface ITextToSpeech
{
    public Task SpeakAsync(string content);
}
