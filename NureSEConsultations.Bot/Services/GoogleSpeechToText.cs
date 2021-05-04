using Google.Cloud.Speech.V1;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NureSEConsultations.Bot.Services
{
    public class GoogleSpeechToText : ISpeechTranscriptor
    {
        private const string CREDENTIAL_ENV_VAR = "GOOGLE_APPLICATION_CREDENTIALS";

        private const string LANGUAGE = LanguageCodes.Ukrainian.Ukraine;

        public GoogleSpeechToText()
        {
            bool areCredentialsGiven = Environment.GetEnvironmentVariable(CREDENTIAL_ENV_VAR) != null;
            if (!areCredentialsGiven)
            {
                throw new InvalidOperationException($"Provide {CREDENTIAL_ENV_VAR} environment variable.");
            }
        }

        public async Task<string> TranscriptAsync(Stream wavVoiceStream)
        {
            var audio = await RecognitionAudio.FromStreamAsync(wavVoiceStream);
            var client = SpeechClient.Create();
            var config = new RecognitionConfig
            {
                LanguageCode = LANGUAGE
            };
            var response = await client.RecognizeAsync(config, audio);
            var alternative = response.Results.FirstOrDefault()?.Alternatives.FirstOrDefault();

            return alternative?.Transcript ?? string.Empty;
        }
    }
}
