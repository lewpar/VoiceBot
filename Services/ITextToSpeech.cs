namespace VoiceBot.Services;

public interface ITextToSpeech
{
    public Task InitAsync();
    public Task SpeakAsync(string content);
}
