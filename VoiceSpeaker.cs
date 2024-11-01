using NAudio.Wave;

using OpenAI_API;
using OpenAI_API.Audio;
using OpenAI_API.Models;

namespace VoiceBot;

public class VoiceSpeaker : IVoiceSpeaker, IDisposable
{
    public bool IsSpeaking { get; private set; }

    private OpenAIAPI api;
    private WaveOutEvent waveOut;

    public VoiceSpeaker(string apiKey)
    {
        api = new OpenAIAPI(apiKey);

        waveOut = new WaveOutEvent();
        waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
    }

    private void WaveOut_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        IsSpeaking = false;
    }

    public async Task SpeakAsync(string content)
    {
        IsSpeaking = true;

        var audioStream = await api.TextToSpeech.GetSpeechAsStreamAsync(content, TextToSpeechRequest.Voices.Shimmer, null, null, Model.TTS_Speed);
        var reader = new Mp3FileReader(audioStream);

        waveOut.Init(reader);
        waveOut.Play();

        while (IsSpeaking)
        {
            await Task.Delay(100);
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
