using System;
using System.IO;

namespace BinarySerializer.Nintendo.GBA
{
    public class EEPROMEncoder : IStreamEncoder
    {
        public EEPROMEncoder(long length)
        {
            Length = length;
        }

        public long Length { get; }
        public string Name => "EEPROM";

        private void ReverseChunks(Stream input, Stream output, bool throwOnEndOfStream)
        {
            const int chunkSize = 8;

            byte[] buffer = new byte[Length];
            int read = input.Read(buffer, 0, buffer.Length);

            if (read != buffer.Length && throwOnEndOfStream)
                throw new EndOfStreamException();

            // Pad out with 0xFF
            for (int i = read; i < buffer.Length; i++)
                buffer[i] = 0xFF;

            // The data is written reversed, 8 bytes at a time
            for (int i = 0; i < Length; i += chunkSize)
                Array.Reverse(buffer, i, chunkSize);

            output.Write(buffer, 0, buffer.Length);
        }

        public void DecodeStream(Stream input, Stream output) => ReverseChunks(input, output, true);
        public void EncodeStream(Stream input, Stream output) => ReverseChunks(input, output, false);
    }
}