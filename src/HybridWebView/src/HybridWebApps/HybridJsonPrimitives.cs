using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>Reusable strict primitive codecs; generated providers never duplicate these implementations.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class HybridJsonPrimitives
{
	/// <summary>Gets the string codec.</summary>
	public static HybridJsonCodec<string> String { get; } = new(j => j.GetString()!, (w, v) => w.WriteStringValue(v), true);
	/// <summary>Gets the Boolean codec.</summary>
	public static HybridJsonCodec<bool> Boolean { get; } = new(j => j.GetBoolean(), (w, v) => w.WriteBooleanValue(v));
	/// <summary>Gets the byte codec.</summary>
	public static HybridJsonCodec<byte> Byte { get; } = new(j => j.GetByte(), (w, v) => w.WriteNumberValue(v));
	/// <summary>Gets the signed byte codec.</summary>
	public static HybridJsonCodec<sbyte> SByte { get; } = new(j => j.GetSByte(), (w, v) => w.WriteNumberValue(v));
	/// <summary>Gets the Int16 codec.</summary>
	public static HybridJsonCodec<short> Int16 { get; } = new(j => j.GetInt16(), (w, v) => w.WriteNumberValue(v));
	/// <summary>Gets the UInt16 codec.</summary>
	public static HybridJsonCodec<ushort> UInt16 { get; } = new(j => j.GetUInt16(), (w, v) => w.WriteNumberValue(v));
	/// <summary>Gets the Int32 codec.</summary>
	public static HybridJsonCodec<int> Int32 { get; } = new(j => j.GetInt32(), (w, v) => w.WriteNumberValue(v));
	/// <summary>Gets the UInt32 codec.</summary>
	public static HybridJsonCodec<uint> UInt32 { get; } = new(j => j.GetUInt32(), (w, v) => w.WriteNumberValue(v));
	/// <summary>Gets the Int64 codec.</summary>
	public static HybridJsonCodec<long> Int64 { get; } = new(j => j.GetInt64(), (w, v) => w.WriteNumberValue(v));
	/// <summary>Gets the UInt64 codec.</summary>
	public static HybridJsonCodec<ulong> UInt64 { get; } = new(j => j.GetUInt64(), (w, v) => w.WriteNumberValue(v));
	/// <summary>Gets the finite Single codec.</summary>
	public static HybridJsonCodec<float> Single { get; } = new(j => float.IsFinite(j.GetSingle()) ? j.GetSingle() : throw new JsonException("Expected a finite number."), (w, v) => w.WriteNumberValue(v));
	/// <summary>Gets the finite Double codec.</summary>
	public static HybridJsonCodec<double> Double { get; } = new(j => double.IsFinite(j.GetDouble()) ? j.GetDouble() : throw new JsonException("Expected a finite number."), (w, v) => w.WriteNumberValue(v));
	/// <summary>Gets the decimal codec.</summary>
	public static HybridJsonCodec<decimal> Decimal { get; } = new(j => j.GetDecimal(), (w, v) => w.WriteNumberValue(v));
	/// <summary>Gets the single UTF-16 character codec.</summary>
	public static HybridJsonCodec<char> Char { get; } = new(j => j.GetString() is { Length: 1 } s ? s[0] : throw new JsonException("Expected one character."), (w, v) => w.WriteStringValue(v.ToString()));
	/// <summary>Gets the DateTime codec.</summary>
	public static HybridJsonCodec<DateTime> DateTime { get; } = new(j => j.GetDateTime(), (w, v) => w.WriteStringValue(v));
	/// <summary>Gets the DateTimeOffset codec.</summary>
	public static HybridJsonCodec<DateTimeOffset> DateTimeOffset { get; } = new(j => j.GetDateTimeOffset(), (w, v) => w.WriteStringValue(v));
	/// <summary>Gets the Guid codec.</summary>
	public static HybridJsonCodec<Guid> Guid { get; } = new(j => j.GetGuid(), (w, v) => w.WriteStringValue(v));
	/// <summary>Gets the invariant DateOnly codec.</summary>
	public static HybridJsonCodec<DateOnly> DateOnly { get; } = new(j => System.DateOnly.Parse(j.GetString()!, CultureInfo.InvariantCulture), (w, v) => w.WriteStringValue(v.ToString("O", CultureInfo.InvariantCulture)));
	/// <summary>Gets the invariant TimeOnly codec.</summary>
	public static HybridJsonCodec<TimeOnly> TimeOnly { get; } = new(j => System.TimeOnly.Parse(j.GetString()!, CultureInfo.InvariantCulture), (w, v) => w.WriteStringValue(v.ToString("O", CultureInfo.InvariantCulture)));
	/// <summary>Gets the invariant TimeSpan codec.</summary>
	public static HybridJsonCodec<TimeSpan> TimeSpan { get; } = new(j => System.TimeSpan.Parse(j.GetString()!, CultureInfo.InvariantCulture), (w, v) => w.WriteStringValue(v.ToString("c", CultureInfo.InvariantCulture)));
	/// <summary>Gets the original-string URI codec.</summary>
	public static HybridJsonCodec<Uri> Uri { get; } = new(j => new Uri(j.GetString()!, UriKind.RelativeOrAbsolute), (w, v) => w.WriteStringValue(v.OriginalString), true);
	/// <summary>Gets the detached JSON-element codec.</summary>
	public static HybridJsonCodec<JsonElement> JsonElement { get; } = new(j => j.Clone(), (w, v) => v.WriteTo(w));
}
