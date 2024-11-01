﻿namespace VoiceBot;

internal class Program
{
    static async Task Main(string[] args)
    {
        DotEnv.Ensure("OPENAI_API_KEY");

        var apiKey = DotEnv.Get("OPENAI_API_KEY");

        var recorder = new VoiceRecord();
        //var speaker = new VoiceSpeaker(apiKey);
        var speaker = new LocalVoiceSpeaker();
        var transcriber = new VoiceTranscribe("ggml-medium.bin");
        //var completion = new OpenAICompletion(Model.ChatGPTTurbo, apiKey);
        var completion = new LocalCompletion("llama3.2-kangan", "http://192.168.0.102:11434/api");

        await transcriber.InitAsync();

        while (true)
        {
            Console.WriteLine();

            Console.WriteLine("Recording Voice");
            var recorded = await recorder.RecordAsync(detectVoiceEnd: true);
            Console.WriteLine("Finished Recording");

            if(!recorded)
            {
                Console.WriteLine("No audio was sampled.");
                continue;
            }

            if(recorder.Stream is null)
            {
                Console.WriteLine("Failed to record voice.");
                return;
            }

            Console.WriteLine();

            Console.WriteLine("Transcribing Voice");
            var transcript = await transcriber.TranscribeAsync(recorder.Stream);
            Console.WriteLine("Finished Transcribing");

            if(string.IsNullOrWhiteSpace(transcript))
            {
                Console.WriteLine("No transcript could be created.");
                continue;
            }
            Console.WriteLine($"TRANSCRIPT: {transcript}");

            Console.WriteLine();
            
            Console.WriteLine("Prompting text completion..");
            var response = await completion.PromptAsync(transcript);
            Console.WriteLine("Finished Prompt");

            Console.WriteLine();

            Console.WriteLine("AI Speaking");
            await speaker.SpeakAsync(response);
            Console.WriteLine("Finished Speaking");
        }
    }
}