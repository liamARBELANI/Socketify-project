using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesServer
{
    class WavFileUtils
    {
        public static void TrimWavFile(string inPath, string outPath, TimeSpan cutFromStart, TimeSpan cutFromEnd, int segment)
        {
            outPath += segment + ".wav";
            using (WaveFileReader reader = new WaveFileReader(inPath))
            {
                using (WaveFileWriter writer = new WaveFileWriter(outPath, reader.WaveFormat))
                {
                    int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

                    int startPos = (int)cutFromStart.TotalMilliseconds * bytesPerMillisecond;
                    startPos = startPos - startPos % reader.WaveFormat.BlockAlign;
                    int endPos = (int)cutFromEnd.TotalMilliseconds * bytesPerMillisecond;
                    endPos = endPos - endPos % reader.WaveFormat.BlockAlign;
                    //int endBytes = (int)cutFromEnd.TotalMilliseconds * bytesPerMillisecond;
                    //endBytes = endBytes - endBytes % reader.WaveFormat.BlockAlign;
                    //int endPos = (int)reader.Length - endBytes;

                    TrimWavFile(reader, writer, startPos, endPos);
                }
            }
        }

        private static void TrimWavFile(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos)
        {
            reader.Position = (int)startPos;
            byte[] buffer = new byte[1024];
            while (reader.Position < endPos)
            {
                int bytesRequired = (int)(endPos - reader.Position);
                if (bytesRequired > 0)
                {
                    int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                    int bytesRead = reader.Read(buffer, 0, bytesToRead);
                    if (bytesRead > 0)
                    {
                        //writer.WriteData(buffer, 0, bytesRead);
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
    }
}
