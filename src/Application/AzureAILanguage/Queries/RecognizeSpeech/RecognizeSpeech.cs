using AzureAI.Domain.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Media;

namespace AzureAI.Application.AzureAILanguage.Queries.RecognizeSpeech;
public class RecognizeSpeechQuery : IRequest<string>
{

}

public class RecognizeSpeechQueryValidator : AbstractValidator<RecognizeSpeechQuery>
{
    public RecognizeSpeechQueryValidator()
    {
        
    }
}

public class RecognizeSpeechQueryHandler : IRequestHandler<RecognizeSpeechQuery, string>
{
    private readonly AISettingsOption _aISettingsOption;
    private readonly ILogger<RecognizeSpeechQueryHandler> _logger;
    private readonly SpeechConfig speechConfig;

    public RecognizeSpeechQueryHandler(IOptions<AISettingsOption> options, 
        ILogger<RecognizeSpeechQueryHandler> logger)
    {
        _aISettingsOption = options.Value;
        _logger = logger;
        // Get config settings from AppSettings
        string aiSvcKey = _aISettingsOption.SpeechKey;
        string aiSvcRegion = _aISettingsOption.SpeechRegion;

        speechConfig = SpeechConfig.FromSubscription(aiSvcKey, aiSvcRegion);
    }

    public async Task<string> Handle(RecognizeSpeechQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Configure speech service

            speechConfig.SpeechSynthesisVoiceName = "en-US-ArialNeural";

            // Get spoken input
            string command = "";
            command = await TranscribeCommand();
            if (command.ToLower() == "what time is it?")
            {
                await TellTime();
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return string.Empty;
    }

    private async Task<string> TranscribeCommand()
    {
        string command = "";

        // Configure speech recognition
        //using AudioConfig audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        //using SpeechRecognizer speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

        string audioFile = "time.wav";
        SoundPlayer wavPlayer = new SoundPlayer(audioFile);
        wavPlayer.Play();

        using AudioConfig audioConfig = AudioConfig.FromWavFileInput(audioFile);
        using SpeechRecognizer speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

        // Process speech input
        SpeechRecognitionResult speech = await speechRecognizer.RecognizeOnceAsync();
        if (speech.Reason == ResultReason.RecognizedSpeech)
        {
            command = speech.Text;
        }
        else
        {
            if (speech.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(speech);
            }
        }

        // Return the command
        return command;
    }

    private async Task TellTime()
    {
        var now = DateTime.Now;
        string responseText = "The time is " + now.Hour.ToString() + ":" + now.Minute.ToString("D2");

        // Configure speech synthesis
        speechConfig.SpeechSynthesisVoiceName = "en-GB-RyanNeural";
        using SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig);

        // Synthesize spoken output
        string responseSsml = $@"
            <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
                <voice name='en-GB-LibbyNeural'>
                    {responseText}
                    <break strength='weak'/>
                    Time to end this lab!
                </voice>
            </speak>";
        SpeechSynthesisResult speak = await speechSynthesizer.SpeakSsmlAsync(responseSsml);
        if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
        {
            Console.WriteLine(speak.Reason);
        }

        // Print the response
        Console.WriteLine(responseText);
    }
}
