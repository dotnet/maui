using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>Creates small PNG files/streams without any imaging dependency.</summary>
	public static class TestImages
	{
		/// <summary>A 1x1 two-frame GIF (red, then blue), 50 ms per frame, looping forever.</summary>
		public static byte[] CreateAnimatedGif()
		{
			var gif = new List<byte>();
			gif.AddRange(System.Text.Encoding.ASCII.GetBytes("GIF89a"));
			gif.AddRange(new byte[] { 1, 0, 1, 0, 0x80, 0, 0 });                  // 1x1, 2-entry global palette
			gif.AddRange(new byte[] { 255, 0, 0, 0, 0, 255 });                    // red, blue
			gif.AddRange(new byte[] { 0x21, 0xFF, 0x0B });                        // NETSCAPE2.0 loop forever
			gif.AddRange(System.Text.Encoding.ASCII.GetBytes("NETSCAPE2.0"));
			gif.AddRange(new byte[] { 3, 1, 0, 0, 0 });
			foreach (byte lzw in new byte[] { 0x44, 0x4C })                       // pixel index 0, then index 1
			{
				gif.AddRange(new byte[] { 0x21, 0xF9, 4, 0, 5, 0, 0, 0 });        // graphic control: 5 cs delay
				gif.AddRange(new byte[] { 0x2C, 0, 0, 0, 0, 1, 0, 1, 0, 0 });     // image descriptor
				gif.AddRange(new byte[] { 2, 2, lzw, 1, 0 });                     // min code size, one block, end
			}
			gif.Add(0x3B);
			return gif.ToArray();
		}

		public static byte[] CreatePng(int width, int height, byte red, byte green, byte blue)
		{
			using var raw = new MemoryStream();
			for (int y = 0; y < height; y++)
			{
				raw.WriteByte(0); // filter: none
				for (int x = 0; x < width; x++)
				{
					raw.WriteByte(red);
					raw.WriteByte(green);
					raw.WriteByte(blue);
				}
			}

			byte[] compressed;
			using (var buffer = new MemoryStream())
			{
				using (var zlib = new ZLibStream(buffer, CompressionLevel.Optimal, leaveOpen: true))
					zlib.Write(raw.ToArray(), 0, (int)raw.Length);
				compressed = buffer.ToArray();
			}

			using var png = new MemoryStream();
			png.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

			var header = new byte[13];
			WriteBigEndian(header, 0, (uint)width);
			WriteBigEndian(header, 4, (uint)height);
			header[8] = 8; // bit depth
			header[9] = 2; // colour type: RGB
			WriteChunk(png, "IHDR", header);
			WriteChunk(png, "IDAT", compressed);
			WriteChunk(png, "IEND", Array.Empty<byte>());
			return png.ToArray();
		}

		public static string WritePng(int width, int height, byte red, byte green, byte blue)
		{
			var path = Path.Combine(Path.GetTempPath(), $"maui-gtk-test-{Guid.NewGuid():N}.png");
			File.WriteAllBytes(path, CreatePng(width, height, red, green, blue));
			return path;
		}

		static void WriteChunk(Stream stream, string type, byte[] data)
		{
			var length = new byte[4];
			WriteBigEndian(length, 0, (uint)data.Length);
			stream.Write(length);

			var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
			stream.Write(typeBytes);
			stream.Write(data);

			var crcInput = new byte[typeBytes.Length + data.Length];
			Buffer.BlockCopy(typeBytes, 0, crcInput, 0, typeBytes.Length);
			Buffer.BlockCopy(data, 0, crcInput, typeBytes.Length, data.Length);

			var crc = new byte[4];
			WriteBigEndian(crc, 0, Crc32(crcInput));
			stream.Write(crc);
		}

		static void WriteBigEndian(byte[] buffer, int offset, uint value)
		{
			buffer[offset] = (byte)(value >> 24);
			buffer[offset + 1] = (byte)(value >> 16);
			buffer[offset + 2] = (byte)(value >> 8);
			buffer[offset + 3] = (byte)value;
		}

		static uint Crc32(byte[] data)
		{
			uint crc = 0xFFFFFFFF;
			foreach (var b in data)
			{
				crc ^= b;
				for (int i = 0; i < 8; i++)
					crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
			}

			return ~crc;
		}
	}
}
