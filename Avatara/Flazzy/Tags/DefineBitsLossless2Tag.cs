using Flazzy;
using Flazzy.IO;
using Flazzy.IO.Compression;
using Flazzy.Records;
using System.Drawing;
using System.IO.Compression;

namespace Flazzy.Tags
{
    public class DefineBitsLossless2Tag : TagItem
    {
        public ushort Id { get; set; }

        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public byte ColorTableSize { get; }
        public BitmapFormat Format { get; set; }
        public byte[] CompressedData { get; set; }
        public byte[] UncompressedData { get; set; }

        private Color[,] _argbMap;

        public DefineBitsLossless2Tag()
            : base(TagKind.DefineBitsLossless2)
        {
            _argbMap = new Color[0, 0];
            CompressedData = Array.Empty<byte>();
        }
        public DefineBitsLossless2Tag(HeaderRecord header, FlashReader input)
            : base(header)
        {
            Id = input.ReadUInt16();
            Format = input.ReadByte() switch
            {
                3 => BitmapFormat.ColorMap8,
                4 when (int)TagKind.DefineBitsLossless2 == 1 => BitmapFormat.Rgb15,
                5 => BitmapFormat.Rgb32,

                _ => throw new InvalidDataException("Invalid bitmap format.")
            };

            Width = input.ReadUInt16();
            Height = input.ReadUInt16();
            _argbMap = new Color[Width, Height];

            if (Format == BitmapFormat.ColorMap8)
                ColorTableSize = input.ReadByte();

            CompressedData = input.ReadBytes(header.Length - GetHeaderSize());
        }

        public Color[,] GetARGBMap()
        {
            if (Format == BitmapFormat.ColorMap8)
            {
                throw new NotSupportedException(
                    "8-bit asset generator not supported.");
            }

            Compressor(CompressionMode.Decompress);
            for (int y = 0, i = 0; y < _argbMap.GetLength(1); y++)
            {
                for (int x = 0; x < _argbMap.GetLength(0); i += 4, x++)
                {
                    byte a = UncompressedData[i];
                    byte r = UncompressedData[i + 1];
                    byte g = UncompressedData[i + 2];
                    byte b = UncompressedData[i + 3];
                    _argbMap[x, y] = Color.FromArgb(a, r, g, b);
                }
            }

            UncompressedData = null;
            return _argbMap;
        }

        protected void Compressor(CompressionMode mode)
        {
            switch (mode)
            {
                case CompressionMode.Compress:
                    {
                        CompressedData =
                            ZLib.Compress(UncompressedData);
                        break;
                    }
                case CompressionMode.Decompress:
                    {
                        UncompressedData =
                            ZLib.Decompress(CompressedData);
                        break;
                    }
            }
        }

        private int GetHeaderSize()
        {
            int size = 0;
            size += sizeof(ushort);
            size += sizeof(byte);
            size += sizeof(ushort);
            size += sizeof(ushort);
            if (Format == BitmapFormat.ColorMap8)
            {
                size += sizeof(byte);
            }
            return size;
        }

        public override int GetBodySize()
        {
            int size = 0;
            size += GetHeaderSize();
            size += CompressedData.Length;
            return size;
        }
        protected override void WriteBodyTo(FlashWriter output)
        {
            byte format = Format switch
            {
                BitmapFormat.ColorMap8 => 3,
                BitmapFormat.Rgb15 when (int)TagKind.DefineBitsLossless2 == 1 => 4,
                BitmapFormat.Rgb32 => 5,

                BitmapFormat.Rgb15 when (int)TagKind.DefineBitsLossless2 == 2 => throw new Exception($"{BitmapFormat.Rgb15} is only supported on {nameof(DefineBitsLosslessTag)} version 1."),
                _ => throw new InvalidDataException("Invalid bitmap format.")
            };

            output.Write(Id);
            output.Write(format);
            output.Write(Width);
            output.Write(Height);
            if (Format == BitmapFormat.ColorMap8)
            {
                output.Write(ColorTableSize);
            }
            output.Write(CompressedData);
        }
    }
}