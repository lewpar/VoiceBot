using NAudio.Wave;
using PiperSharp;
using PiperSharp.Models;

namespace VoiceBot.Services.Local;

public class PiperTextToSpeech : ITextToSpeech
{
    public bool IsSpeaking { get; private set; }

    private string model;
    private WaveOutEvent waveOut;
    private PiperProvider? provider;

    public PiperTextToSpeech(string model)
    {
        this.model = model;

        waveOut = new WaveOutEvent();
        waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
    }

    private void WaveOut_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        IsSpeaking = false;
    }

    public async Task SpeakAsync(string content)
    {
        if (provider is null)
        {
            throw new Exception("You need to call InitAsync before speaking.");
        }

        IsSpeaking = true;

        // Generate audio, currently supported formats are Mp3, Wav, Raw
        var result = await provider.InferAsync(content, AudioOutputType.Wav); // Returns byte[]
        var stream = new MemoryStream(result);
        var reader = new WaveFileReader(stream);

        waveOut.Init(reader);
        waveOut.Play();

        while (IsSpeaking)
        {
            await Task.Delay(100);
        }
    }

    public async Task InitAsync()
    {
        var cwd = Directory.GetCurrentDirectory();
        if (!Directory.Exists(Path.Combine(cwd, "piper")))
        {
            await PiperDownloader.DownloadPiper().ExtractPiper(cwd);
        }

        var modelsLocation = Path.Combine(cwd, "piper", "models");
        if(!Directory.Exists(modelsLocation))
        {
            Directory.CreateDirectory(modelsLocation);
        }

        var modelLocation = Path.Combine(modelsLocation, model);
        VoiceModel? voiceModel;

        if(!Directory.Exists(modelLocation))
        {
            voiceModel = await PiperDownloader.GetModelByKey(model);

            if(voiceModel is null)
            {
                throw new Exception("Failed to download model with key " + model);
            }
            voiceModel = await voiceModel.DownloadModel(modelsLocation);
        }
        else
        {
            voiceModel = await VoiceModel.LoadModel(modelLocation);
        }

        if (voiceModel is null)
        {
            throw new Exception("Failed to get model.");
        }

        // To start generating audio use PiperProvider
        provider = new PiperProvider(new PiperConfiguration()
        {
            ExecutableLocation = Path.Join(cwd, "piper", "piper.exe"), // Path to piper executable
            WorkingDirectory = Path.Join(cwd, "piper"), // Path to piper working directory
            Model = voiceModel, // Loaded/downloaded VoiceModel
        });
    }
}
