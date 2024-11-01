using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace VoiceBot;

public class OpenAICompletion : IChatCompletion
{
    private OpenAIAPI api;
    private Conversation chat;

    public OpenAICompletion(Model model, string apiKey)
    {
        api = new OpenAIAPI(apiKey);

        chat = api.Chat.CreateConversation(new ChatRequest()
        {
            Temperature = 0.9
        });

        chat.Model = model;

        chat.AppendSystemMessage("You are a friendly AI that likes conversation. Keep your answers short and succint.");
    }

    public async Task<string> PromptAsync(string prompt)
    {
        chat.AppendUserInput(prompt);

        return await chat.GetResponseFromChatbotAsync();
    }
}
