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

        public void DecodeStream(Stream input, Stream output)
        {
            const int chunkSize = 8;

            byte[] buffer = new byte[chunkSize];

            // The data is written reversed, 8 bytes at a time
            for (int i = 0; i < Length; i += chunkSize)
            {
                int read = input.Read(buffer, 0, chunkSize);

                if (read != chunkSize)
                    throw new EndOfStreamException();

                Array.Reverse(buffer);

                output.Write(buffer, 0, chunkSize);
            }
        }

        public void EncodeStream(Stream input, Stream output) => DecodeStream(input, output);
    }
}