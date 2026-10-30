using System;
using System.IO;
using System.IO.Compression;

namespace Maui.Gtk.Sample;

/// <summary>Generates a small gradient PNG so the sample needs no image assets.</summary>
static class SampleImage
{
	public static byte[] CreatePng(int width, int height)
	{
		using var raw = new MemoryStream();
		for (int y = 0; y < height; y++)
		{
			raw.WriteByte(0);
			for (int x = 0; x < width; x++)
			{
				raw.WriteByte((byte)(255 * x / (width - 1)));
				raw.WriteByte((byte)(255 * y / (height - 1)));
				raw.WriteByte(160);
			}
		}

		using var compressed = new MemoryStream();
		using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
			zlib.Write(raw.ToArray());

		using var png = new MemoryStream();
		png.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
		var header = new byte[13];
		WriteBigEndian(header, 0, (uint)width);
		WriteBigEndian(header, 4, (uint)height);
		header[8] = 8;
		header[9] = 2;
		WriteChunk(png, "IHDR", header);
		WriteChunk(png, "IDAT", compressed.ToArray());
		WriteChunk(png, "IEND", Array.Empty<byte>());
		return png.ToArray();
	}

	static void WriteChunk(Stream stream, string type, byte[] data)
	{
		var buffer = new byte[4];
		WriteBigEndian(buffer, 0, (uint)data.Length);
		stream.Write(buffer);

		var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
		stream.Write(typeBytes);
		stream.Write(data);

		uint crc = 0xFFFFFFFF;
		foreach (var b in typeBytes)
			crc = Update(crc, b);
		foreach (var b in data)
			crc = Update(crc, b);
		WriteBigEndian(buffer, 0, ~crc);
		stream.Write(buffer);

		static uint Update(uint crc, byte value)
		{
			crc ^= value;
			for (int i = 0; i < 8; i++)
				crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
			return crc;
		}
	}

	static void WriteBigEndian(byte[] buffer, int offset, uint value)
	{
		buffer[offset] = (byte)(value >> 24);
		buffer[offset + 1] = (byte)(value >> 16);
		buffer[offset + 2] = (byte)(value >> 8);
		buffer[offset + 3] = (byte)value;
	}
}
