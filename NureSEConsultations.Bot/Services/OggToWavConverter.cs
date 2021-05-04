using Concentus.Oggfile;
using Concentus.Structs;
using NAudio.Wave;
using System;
using System.IO;

namespace NureSEConsultations.Bot.Services
{
    public class OggToWavConverter : IOggToWavConverter
    {
        private const int INPUT_FREQUENCY = 48_000;

        private const int OUTPUT_FREQUENCY = 16_000;

        private const int CHANNELS = 1;

        public void Convert(Stream inputOgg, Stream outputWav)
        {
            using MemoryStream pcmStream = new();
            OpusDecoder decoder = OpusDecoder.Create(INPUT_FREQUENCY, CHANNELS);
            OpusOggReadStream oggReader = new OpusOggReadStream(decoder, inputOgg);
            while (oggReader.HasNextPacket)
            {
                short[] packet = oggReader.DecodeNextPacket();
                if (packet != null)
                {
                    for (int i = 0; i < packet.Length; i++)
                    {
                        var bytes = BitConverter.GetBytes(packet[i]);
                        pcmStream.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            pcmStream.Position = 0;
            using var wavStream = new RawSourceWaveStream(pcmStream, new WaveFormat(INPUT_FREQUENCY, CHANNELS));
            using var resampler = new MediaFoundationResampler(wavStream, new WaveFormat(OUTPUT_FREQUENCY, CHANNELS));
            WaveFileWriter.WriteWavFileToStream(outputWav, resampler);
        }
    }
}
