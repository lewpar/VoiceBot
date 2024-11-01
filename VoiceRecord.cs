using NAudio.Utils;
using NAudio.Wave;

namespace VoiceBot;

public class VoiceRecord : IDisposable
{
    public bool IsRecording { get; private set; }
    public MemoryStream? Stream { get; private set; }

    private WaveInEvent? waveIn;
    private WaveFileWriter? writer;

    public async Task<bool> RecordAsync(int lengthSeconds = 2, bool detectVoiceEnd = false)
    {
        waveIn = new WaveInEvent();
        waveIn.WaveFormat = new WaveFormat(16000, 1);

        Stream = new MemoryStream();

        // Writes RIFF header.
        writer = new WaveFileWriter(new IgnoreDisposeStream(Stream), waveIn.WaveFormat);

        IsRecording = true;

        var later = DateTime.Now.AddSeconds(lengthSeconds);

        int detectVoiceCutoffSeconds = 3;
        bool sampled = false;

        if(detectVoiceEnd)
        {
            later = DateTime.Now.AddSeconds(detectVoiceCutoffSeconds);
        }

        waveIn.DataAvailable += (s, e) =>
        {
            var now = DateTime.Now;

            if(detectVoiceEnd)
            {
                short noiseGate = 600;
                short sample = Math.Abs(BitConverter.ToInt16(e.Buffer));

                if(sample > noiseGate)
                {
                    sampled = true;
                    later = DateTime.Now.AddSeconds(detectVoiceCutoffSeconds);
                }

                if(now > later)
                {
                    waveIn.StopRecording();
                }
            }
            else
            {
                if (now > later)
                {
                    waveIn.StopRecording();
                }

                now = DateTime.Now;
            }

            writer.Write(e.Buffer, 0, e.BytesRecorded);
            writer.Flush();
        };

        waveIn.RecordingStopped += (s, e) =>
        {
            Stream.Position = 0;
            IsRecording = false;
        };

        waveIn.StartRecording();

        while(IsRecording)
        {
            await Task.Delay(100);
        }

        return sampled;
    }

    public void Dispose()
    {
        Stream?.Dispose();
        waveIn?.Dispose();
        writer?.Dispose();
    }
}
