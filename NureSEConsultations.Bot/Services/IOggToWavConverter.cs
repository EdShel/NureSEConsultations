using System.IO;

namespace NureSEConsultations.Bot.Services
{
    public interface IOggToWavConverter
    {
        void Convert(Stream inputOgg, Stream outputWav);
    }
}
