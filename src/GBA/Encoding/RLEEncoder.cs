using System;
using System.IO;

namespace BinarySerializer.Nintendo.GBA
{
    // Implemented from dsdecmp, todo: refactor code to follow project coding style
    public class RLEEncoder : BaseEncoder
    {
        public override int ID => 3;
        public override string Name => "GBA_RLE";

        protected override void DecodeStream(Reader reader, Stream output, uint decompressedSize, byte headerValue)
        {
            /*      
                Data header (32bit)
                    Bit 0-3   Reserved
                    Bit 4-7   Compressed type (must be 3 for run-length)
                    Bit 8-31  Size of decompressed data
                Repeat below. Each Flag Byte followed by one or more Data Bytes.
                Flag data (8bit)
                    Bit 0-6   Expanded Data Length (uncompressed N-1, compressed N-3)
                    Bit 7     Flag (0=uncompressed, 1=compressed)
                Data Byte(s) - N uncompressed bytes, or 1 byte repeated N times
             */

            int currentOutSize = 0;

            while (currentOutSize < decompressedSize)
            {
                #region (try to) get the flag byte with the length data and compressed flag

                byte flag = reader.ReadByte();
                
                bool compressed = (flag & 0x80) > 0;
                int length = flag & 0x7F;

                if (compressed)
                    length += 3;
                else
                    length += 1;

                #endregion

                if (compressed)
                {
                    #region compressed: write the next byte (length) times.

                    byte data = reader.ReadByte(); 

                    if (currentOutSize + length > decompressedSize)
                        throw new InvalidDataException("The given stream is not a valid RLE stream; the "
                                                       + "output length does not match the provided plaintext length.");

                    for (int i = 0; i < length; i++)
                    {
                        output.WriteByte(data);
                        currentOutSize++;
                    }

                    #endregion
                }
                else
                {
                    #region uncompressed: copy the next (length) bytes.

                    byte[] bytes = reader.ReadBytes(length);

                    output.Write(bytes, 0, length);
                    currentOutSize += length;

                    #endregion
                }
            }
        }

        protected override void EncodeStream(Reader reader, Writer writer)
        {
            // at most 0x7F+3=130 bytes are compressed into a single block.
            // (and at most 0x7F+1=128 in an uncompressed block, however we need to read 2
            // more, since the last byte may be part of a repetition).
            byte[] dataBlock = new byte[130];
            // the length of the valid content in the current data block
            int currentBlockLength = 0;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                int repCount = 1;
                bool foundRepetition = false;

                int nextByte;
                while (currentBlockLength < dataBlock.Length && reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    nextByte = reader.ReadByte();

                    dataBlock[currentBlockLength++] = (byte)nextByte;
                    if (currentBlockLength > 1)
                    {
                        if (nextByte == dataBlock[currentBlockLength - 2])
                            repCount++;
                        else
                            repCount = 1;
                    }

                    foundRepetition = repCount > 2;
                    if (foundRepetition)
                        break;
                }

                int numUncompToCopy;

                if (foundRepetition)
                {
                    // if a repetition was found, copy block size - 3 bytes as compressed data
                    numUncompToCopy = currentBlockLength - 3;
                }
                else
                {
                    // if no repetition was found, copy min(block size, max block size - 2) bytes as uncompressed data.
                    numUncompToCopy = Math.Min(currentBlockLength, dataBlock.Length - 2);
                }

                #region insert uncompressed block

                if (numUncompToCopy > 0)
                {
                    byte flag = (byte)(numUncompToCopy - 1);
                    writer.Write(flag);
                    for (int i = 0; i < numUncompToCopy; i++)
                        writer.Write(dataBlock[i]);
                    // shift some possibly remaining bytes to the start
                    for (int i = numUncompToCopy; i < currentBlockLength; i++)
                        dataBlock[i - numUncompToCopy] = dataBlock[i];
                    currentBlockLength -= numUncompToCopy;
                }

                #endregion

                if (foundRepetition)
                {
                    // if a repetition was found, continue until the first different byte
                    // (or until the buffer is full)
                    while (currentBlockLength < dataBlock.Length && reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        nextByte = reader.ReadByte();

                        dataBlock[currentBlockLength++] = (byte)nextByte;

                        if (nextByte != dataBlock[0])
                            break;
                        else
                            repCount++;
                    }

                    // the next repCount bytes are the same.
                    #region insert compressed block
                    byte flag = (byte)(0x80 | (repCount - 3));
                    writer.Write(flag);
                    writer.Write(dataBlock[0]);
                    // make sure to shift the possible extra byte to the start
                    if (repCount != currentBlockLength)
                        dataBlock[0] = dataBlock[currentBlockLength - 1];
                    currentBlockLength -= repCount;
                    #endregion
                }
            }

            // write any reamaining bytes as uncompressed
            if (currentBlockLength > 0)
            {
                byte flag = (byte)(currentBlockLength - 1);
                writer.Write(flag);
                for (int i = 0; i < currentBlockLength; i++)
                    writer.Write(dataBlock[i]);
            }
        }
    }
}