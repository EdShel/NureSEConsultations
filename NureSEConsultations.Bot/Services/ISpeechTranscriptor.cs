using System.IO;
using System.Threading.Tasks;

namespace NureSEConsultations.Bot.Services
{
    public interface ISpeechTranscriptor
    {
        Task<string> TranscriptAsync(Stream wavVoiceStream);
    }
}
