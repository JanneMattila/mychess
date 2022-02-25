using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using MyChess.Interfaces;

namespace MyChess.Backend.Data
{
    public class Compactor
    {
        public byte[] Compact(MyChessGame game)
        {
            var json = JsonSerializer.Serialize(game);
            var buffer = Encoding.UTF8.GetBytes(json);

            using var input = new MemoryStream(buffer);
            using var output = new MemoryStream();
            using var compressor = new BrotliStream(output, CompressionMode.Compress);
            input.CopyTo(compressor);
            compressor.Flush();
            return output.ToArray();
        }

        public MyChessGame Decompress(byte[] data)
        {
            byte[] buffer;
            using (var input = new MemoryStream(data))
            using (var output = new MemoryStream())
            using (var decompressor = new BrotliStream(input, CompressionMode.Decompress))
            {
                decompressor.CopyTo(output);
                decompressor.Flush();
                buffer = output.ToArray();
            }

            var json = Encoding.UTF8.GetString(buffer);
            return JsonSerializer.Deserialize<MyChessGame>(json) ??
                   throw new ArgumentNullException("Cannot deserialize game data.");
        }
    }
}
