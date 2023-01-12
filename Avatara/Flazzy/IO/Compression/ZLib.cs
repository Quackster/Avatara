using Ionic.Zlib;
using System.Text;

namespace Flazzy.IO.Compression
{
    public static class ZLib
    {
        public static byte[] Compress(byte[] data)
        {
            using (var output = new MemoryStream())
            using (var compressor = new ZlibStream(output, CompressionMode.Compress, CompressionLevel.BestCompression))
            {
                CompressBuffer(data, compressor);
                return output.ToArray();
            }
        }
        public static byte[] Decompress(byte[] data)
        {
            using (var input = new MemoryStream(data))
            using (var decompressor = new ZlibStream(input, CompressionMode.Decompress))
            {
                return UncompressBuffer(decompressor);
            }
        }

        private static void CompressBuffer(byte[] b, Stream compressor)
        {
            using (compressor)
            {
                compressor.Write(b, 0, b.Length);
            }
        }

        private static byte[] UncompressBuffer(Stream decompressor)
        {
            // workitem 8460
            byte[] working = new byte[1024];
            using (var output = new MemoryStream())
            {
                using (decompressor)
                {
                    int n;
                    while ((n = decompressor.Read(working, 0, working.Length)) != 0)
                    {
                        output.Write(working, 0, n);
                    }
                }
                return output.ToArray();
            }
        }

        public static FlashReader WrapDecompressor(Stream input)
        {
            return new FlashReader(new ZlibStream(input, CompressionMode.Decompress));
        }
        public static FlashWriter WrapCompressor(Stream output, bool leaveOpen = false)
        {
            return new FlashWriter(new ZlibStream(output, CompressionMode.Compress, CompressionLevel.BestCompression, leaveOpen));
        }
    }
}