using System.Diagnostics;
using System.Text;
using Whisper.net.Ggml;
using Whisper.net;

namespace VoiceBot.Services.Local;

public class WhisperTranscriber
{
    private StringBuilder sb;

    private WhisperFactory? factory;
    private WhisperProcessor? processor;

    private string model;

    public WhisperTranscriber(string model)
    {
        sb = new StringBuilder();
        this.model = model;
    }

    public async Task InitAsync()
    {
        if (!File.Exists(model))
        {
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.MediumEn);
            using var fileWriter = File.OpenWrite(model);
            await modelStream.CopyToAsync(fileWriter);
        }

        factory = WhisperFactory.FromPath(model, false, null, false, true);

        processor = factory.CreateBuilder()
            .WithLanguage("auto")
            .Build();
    }

    public async Task<string> TranscribeAsync(Stream audioStream)
    {
        if (processor is null)
        {
            throw new Exception("You need to call InitAsync before transcribing.");
        }

        sb.Clear();

        await foreach (var result in processor.ProcessAsync(audioStream))
        {
            sb.Append(result.Text);
        }

        return CleanTranscript(sb.ToString());
    }

    private string CleanTranscript(string transcript)
    {
        return transcript.Trim().Replace("[BLANK_AUDIO]", "");
    }
}
