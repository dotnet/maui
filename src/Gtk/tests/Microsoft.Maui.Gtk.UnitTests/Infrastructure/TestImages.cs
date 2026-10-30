using System;
using System.IO;
using System.IO.Compression;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>Creates small PNG files/streams without any imaging dependency.</summary>
	public static class TestImages
	{
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
