using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace VoiceBot;

public class LocalCompletion : IChatCompletion
{
    private HttpClient _httpClient;

    private string model;
    private string apiUrl;

    public LocalCompletion(string model, string apiUrl)
    {
        _httpClient = new HttpClient();

        this.model = model;
        this.apiUrl = apiUrl;
    }

    public async Task<string> PromptAsync(string prompt)
    {
        var request = new LocalCompletionRequest()
        {
            Model = model,
            Prompt = prompt,
            Stream = false,
        };

        var response = await _httpClient.PostAsJsonAsync($"{apiUrl}/generate", request);
        if(!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to generate completion: Received status code '{response.StatusCode}'.");
        }

        var completion = await response.Content.ReadFromJsonAsync<LocalCompletionResponse>();
        if(completion is null)
        {
            throw new Exception("Failed to deserialize completion result.");
        }

        return completion.Response;
    }
}

public class LocalCompletionRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("prompt")]
    public required string Prompt { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}

public class LocalCompletionResponse
{
    [JsonPropertyName("response")]
    public required string Response { get; set; }
}
