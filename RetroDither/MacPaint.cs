using System.Buffers.Binary;
using System.Text;

namespace RetroDither;

public class MacPaint
{
    public const int MaxWidth = 576;
    public const int MaxHeight = 720;
    private const int MacbinaryLength = 128;
    private const int HeaderLength = 512;

    private byte[] BytesToBits(byte[] original)
    {
        var bitsArray = new List<byte>();

        for (var byteIndex = 0; byteIndex < original.Length; byteIndex += 8)
        {
            var nextByte = 0;
            for (var bitIndex = 0; bitIndex < 8; bitIndex++)
            {
                var nextBit = 1 - (original[byteIndex + bitIndex] & 1);
                nextByte = nextByte | (byteIndex << (7 - bitIndex));
                if (byteIndex + bitIndex + 1 >= original.Length)
                {
                    break;
                }
            }
            bitsArray.Add((byte)nextByte);
        }

        return bitsArray.ToArray();
    }

    public byte[] Prepare(byte[] data, int width, int height)
    {
        var bitsArray = new List<byte>();

        for (var row = 0; row < height; row++)
        {
            var imageLocation = row * width;
            var imageBits = BytesToBits(data[imageLocation..(imageLocation + width)]);
            bitsArray.AddRange(imageBits);
            var remainingWidth = MaxWidth - width;
            var whiteWidthBits = new byte[remainingWidth / 8];
            bitsArray.AddRange(whiteWidthBits);
        }
        var remainingHeight = MaxHeight - height;
        var whilteHeightBits = new byte[remainingHeight * MaxWidth / 8];
        bitsArray.AddRange(whilteHeightBits);

        return [.. bitsArray];
    }

    public byte[] RunLengthEncode(byte[] originalData)
    {
        int TakeSame(byte[] source, int start)
        {
            var count = 0;
            while (start + count + 1 < source.Length && source[start + count] == source[start + count + 1])
            {
                count++;
            }
            return count > 0 ? count + 1 : 0;
        }
        var rleData = new List<byte>();
        for (var lineStart = 0; lineStart < originalData.Length; lineStart += MaxWidth / 8)
        {
            var data = originalData[lineStart..Math.Min(originalData.Length, lineStart + MaxWidth / 8)];
            var index = 0;
            while (index < data.Length)
            {
                var notSame = 0;
                int same;
                while ((same = TakeSame(data, index + notSame)) == 0 && index + notSame < data.Length)
                {
                    notSame++;
                }
                if (notSame > 0)
                {
                    rleData.Add((byte)(notSame - 1));
                    rleData.AddRange(data[index..(index + notSame)]);
                    index += notSame;
                }
                if (same > 0)
                {
                    rleData.Add((byte)(257 - same));
                    rleData.Add(data[index]);
                    index += same;
                }
            }
        }

        return [.. rleData];
    }

    private byte[] MacbinaryHeader(string outfile, int dataSize)
    {
        var macbinaryArray = new byte[MacbinaryLength];
        var macbinary = macbinaryArray.AsSpan();
        var filename = Path.GetFileNameWithoutExtension(outfile);
        filename = filename.Length > 63 ? filename[..63] : filename;
        var timestamp = (uint)((DateTime.Now - new DateTime(1904, 1, 1)).TotalSeconds);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var macRomanEncoding = Encoding.GetEncoding("macintosh");

        macbinary[1] = (byte)filename.Length;
        macRomanEncoding.GetBytes(filename).CopyTo(macbinary[2..(2 + filename.Length)]);
        macRomanEncoding.GetBytes("PNTG").CopyTo(macbinary[65..69]);
        macRomanEncoding.GetBytes("MPNT").CopyTo(macbinary[69..73]);
        BinaryPrimitives.WriteInt32BigEndian(macbinary[83..87], dataSize);
        BinaryPrimitives.WriteUInt32BigEndian(macbinary[91..95], timestamp);
        BinaryPrimitives.WriteUInt32BigEndian(macbinary[95..99], timestamp);

        return macbinaryArray;
    }

    public void WriteMacpaintFile(byte[] data, string outFile, int width, int height)
    {
        var bitsArray = Prepare(data, width, height);
        var rle = RunLengthEncode(bitsArray);
        var dataSize = rle.Length + HeaderLength;
        byte[] output = [.. MacbinaryHeader(outFile, dataSize), .. new byte[HeaderLength], .. rle];
        output[MacbinaryLength + 3] = 2;
        var padding = 128 - (dataSize % 128);
        if (padding > 0)
        {
            output = [.. output, .. new byte[padding]];
        }
        File.WriteAllBytes($"{outFile}.bin", output);
    }
}
