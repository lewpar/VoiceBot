namespace VoiceBot.Services;

public interface ITextCompletion
{
    public Task<string> PromptAsync(string prompt);
}
