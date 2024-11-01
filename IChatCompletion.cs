namespace VoiceBot;

public interface IChatCompletion
{
    public Task<string> PromptAsync(string prompt);
}
