using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Microsoft.Maui.Gtk.Fonts
{
	/// <summary>
	/// Reads the family name out of a TrueType / OpenType font file (<c>name</c> table), the same name Pango and
	/// fontconfig match on. OS independent: the fallback for hosts where fontconfig is absent or cannot parse the file.
	/// </summary>
	internal static class FontFileNames
	{
		const int FamilyNameId = 1;
		const int TypographicFamilyNameId = 16;

		/// <summary>The family name of the first font in <paramref name="filePath"/>, or <see langword="null"/>.</summary>
		internal static string? TryReadFamilyName(string filePath)
		{
			try
			{
				return ReadFamilyName(File.ReadAllBytes(filePath));
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
			{
				return null;
			}
		}

		/// <summary>The family name (name id 1, else 16) of the first font in <paramref name="data"/>, or <see langword="null"/>.</summary>
		internal static string? ReadFamilyName(byte[] data)
		{
			if (data is null || data.Length < 12)
				return null;

			var offset = 0;

			// TrueType Collection: take the first font.
			if (data[0] == 't' && data[1] == 't' && data[2] == 'c' && data[3] == 'f')
			{
				if (data.Length < 16)
					return null;

				var first = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(12, 4));
				if (first > data.Length - 12)
					return null;

				offset = (int)first;
			}

			var tableCount = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset + 4, 2));
			for (var i = 0; i < tableCount; i++)
			{
				var record = offset + 12 + i * 16;
				if (record + 16 > data.Length)
					return null;

				if (data[record] == 'n' && data[record + 1] == 'a' && data[record + 2] == 'm' && data[record + 3] == 'e')
				{
					var tableOffset = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(record + 8, 4));
					var tableLength = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(record + 12, 4));
					if (tableOffset > data.Length || tableLength > data.Length - tableOffset)
						return null;

					return ReadFromNameTable(data, (int)tableOffset, (int)tableLength);
				}
			}

			return null;
		}

		static string? ReadFromNameTable(byte[] data, int start, int length)
		{
			if (length < 6)
				return null;

			var count = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(start + 2, 2));
			var stringStorage = start + BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(start + 4, 2));

			string? best = null;
			var bestScore = -1;

			for (var i = 0; i < count; i++)
			{
				var record = start + 6 + i * 12;
				if (record + 12 > start + length)
					break;

				var platform = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(record, 2));
				var language = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(record + 4, 2));
				var nameId = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(record + 6, 2));

				if (nameId != FamilyNameId && nameId != TypographicFamilyNameId)
					continue;

				var stringLength = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(record + 8, 2));
				var stringOffset = stringStorage + BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(record + 10, 2));
				if (stringOffset + stringLength > data.Length)
					continue;

				// Windows Unicode / Mac Roman are the only encodings worth decoding; prefer English, then name id 1.
				string text;
				if (platform == 3 || platform == 0)
					text = Encoding.BigEndianUnicode.GetString(data, stringOffset, stringLength);
				else if (platform == 1)
					text = Encoding.ASCII.GetString(data, stringOffset, stringLength);
				else
					continue;

				if (string.IsNullOrWhiteSpace(text))
					continue;

				var score = (platform == 3 && language == 0x0409 ? 4 : platform == 1 && language == 0 ? 2 : 0) + (nameId == FamilyNameId ? 1 : 0);
				if (score > bestScore)
				{
					bestScore = score;
					best = text.Trim();
				}
			}

			return best;
		}
	}
}
