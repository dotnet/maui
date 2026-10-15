using System.ComponentModel;
using System.Text.Json;

namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>Strict named-enum and flags codecs with compiler-provided names, bits and typed casts.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class HybridJsonEnums
{
	/// <summary>Creates an enum codec without discovering members at runtime.</summary>
	/// <typeparam name="T">The enum type.</typeparam><param name="names">Ordinally ordered wire names.</param>
	/// <param name="bits">Their masked underlying values.</param><param name="toBits">A generated numeric cast.</param>
	/// <param name="fromBits">A generated enum cast.</param><param name="flags">Whether this is a flags contract.</param>
	/// <param name="widthMask">The underlying integer width.</param><param name="knownBits">Declared nonsentinel bits.</param>
	/// <returns>The reusable typed codec.</returns>
	public static HybridJsonCodec<T> Create<T>(string[] names, ulong[] bits, Func<T, ulong> toBits,
		Func<ulong, T> fromBits, bool flags, ulong widthMask, ulong knownBits) where T : struct, Enum
	{
		if (names.Length != bits.Length)
			throw new ArgumentException("Enum names and values must align.");
		var canonical = Enumerable.Range(0, bits.Length).DistinctBy(i => bits[i]).ToArray();
		return new(Read, Write);

		T Read(JsonElement json)
		{
			if (!flags)
				return fromBits(Find(json));
			if (json.ValueKind != JsonValueKind.Array)
				throw new JsonException("Expected a flag-name array.");
			ulong value = 0;
			var seen = new HashSet<ulong>();
			foreach (var item in json.EnumerateArray())
			{
				var bit = Find(item);
				if (!seen.Add(bit) || ((bit == 0 || bit == widthMask) && json.GetArrayLength() != 1))
					throw new JsonException("Use distinct flag values; zero and all must stand alone.");
				value |= bit;
			}
			if (value != widthMask && (value & ~knownBits) != 0)
				throw new JsonException("Undefined flag bits.");
			return fromBits(value);
		}

		ulong Find(JsonElement json)
		{
			if (json.ValueKind != JsonValueKind.String)
				throw new JsonException("Expected an enum name.");
			var index = Array.IndexOf(names, json.GetString());
			return index >= 0 ? bits[index] : throw new JsonException("Unknown enum name.");
		}

		void Write(Utf8JsonWriter writer, T value)
		{
			var raw = toBits(value) & widthMask;
			var exact = Array.FindIndex(bits, bit => bit == raw);
			if (exact >= 0)
			{
				if (flags)
					writer.WriteStartArray();
				writer.WriteStringValue(names[exact]);
				if (flags)
					writer.WriteEndArray();
				return;
			}
			if (!flags || (raw & ~knownBits) != 0)
				throw new JsonException("Undefined native enum value.");
			ulong covered = 0;
			var selected = new List<string>();
			foreach (var i in canonical.OrderBy(i => bits[i]))
			{
				var bit = bits[i];
				if (bit == 0 || bit == widthMask || (raw & bit) != bit || (covered & bit) == bit)
					continue;
				covered |= bit;
				selected.Add(names[i]);
			}
			if (covered != raw)
				throw new JsonException("Native flags cannot be expressed using declared names.");
			writer.WriteStartArray();
			foreach (var name in selected)
				writer.WriteStringValue(name);
			writer.WriteEndArray();
		}
	}
}
