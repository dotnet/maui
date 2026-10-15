using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui;

namespace Maui.HybridApp.UI;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	AllowDuplicateProperties = false,
	Converters = [typeof(NativeKeyboardConverter), typeof(NativeKeyboardFlagsConverter)],
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(Keyboard))]
[JsonSerializable(typeof(KeyboardFlags))]
internal partial class NativeKeyboardJsonContext : JsonSerializerContext;

internal sealed class NativeKeyboardConverter : JsonConverter<Keyboard>
{
	static readonly (string Name, Keyboard Value)[] Named =
	[
		("default", Keyboard.Default), ("plain", Keyboard.Plain), ("chat", Keyboard.Chat),
		("text", Keyboard.Text), ("email", Keyboard.Email), ("numeric", Keyboard.Numeric),
		("telephone", Keyboard.Telephone), ("url", Keyboard.Url), ("date", Keyboard.Date),
		("time", Keyboard.Time), ("password", Keyboard.Password)
	];

	public override Keyboard Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
	{
		using var document = JsonDocument.ParseValue(ref reader);
		var json = document.RootElement;
		if (json.ValueKind != JsonValueKind.Object) throw new JsonException("Expected a keyboard object.");
		var properties = json.EnumerateObject();
		if (!properties.MoveNext()) throw new JsonException("Expected a keyboard name or flags.");
		var property = properties.Current;
		if (properties.MoveNext()) throw new JsonException("Use exactly one keyboard name or flags property.");
		if (property.NameEquals("name") && property.Value.ValueKind == JsonValueKind.String)
		{
			foreach (var (name, keyboard) in Named)
				if (property.Value.GetString() == name) return keyboard;
		}
		else if (property.NameEquals("flags"))
			return Keyboard.Create(JsonSerializer.Deserialize(property.Value, NativeKeyboardJsonContext.Default.KeyboardFlags));
		throw new JsonException("Unknown keyboard representation.");
	}

	public override void Write(Utf8JsonWriter writer, Keyboard value, JsonSerializerOptions options)
	{
		foreach (var (name, keyboard) in Named)
		{
			if (!ReferenceEquals(value, keyboard)) continue;
			writer.WriteStartObject();
			writer.WriteString("name", name);
			writer.WriteEndObject();
			return;
		}
		if (value is not CustomKeyboard custom) throw new JsonException("Unknown native keyboard.");
		writer.WriteStartObject();
		writer.WritePropertyName("flags");
		JsonSerializer.Serialize(writer, custom.Flags, NativeKeyboardJsonContext.Default.KeyboardFlags);
		writer.WriteEndObject();
	}
}

internal sealed class NativeKeyboardFlagsConverter : JsonConverter<KeyboardFlags>
{
	static readonly (string Name, KeyboardFlags Value)[] Members =
	[
		("none", KeyboardFlags.None), ("capitalizeSentence", KeyboardFlags.CapitalizeSentence),
		("spellcheck", KeyboardFlags.Spellcheck), ("suggestions", KeyboardFlags.Suggestions),
		("capitalizeWord", KeyboardFlags.CapitalizeWord), ("capitalizeCharacter", KeyboardFlags.CapitalizeCharacter),
		("capitalizeNone", KeyboardFlags.CapitalizeNone), ("all", KeyboardFlags.All)
	];
	const KeyboardFlags Known = KeyboardFlags.CapitalizeSentence | KeyboardFlags.Spellcheck | KeyboardFlags.Suggestions |
		KeyboardFlags.CapitalizeWord | KeyboardFlags.CapitalizeCharacter | KeyboardFlags.CapitalizeNone;

	public override KeyboardFlags Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
	{
		using var document = JsonDocument.ParseValue(ref reader);
		var json = document.RootElement;
		if (json.ValueKind != JsonValueKind.Array) throw new JsonException("Expected a flag-name array.");
		var result = KeyboardFlags.None;
		var seen = new HashSet<KeyboardFlags>();
		foreach (var item in json.EnumerateArray())
		{
			if (item.ValueKind != JsonValueKind.String) throw new JsonException("Expected a flag name.");
			var found = false;
			foreach (var (name, flag) in Members)
			{
				if (item.GetString() != name) continue;
				if (!seen.Add(flag) || ((flag is KeyboardFlags.None or KeyboardFlags.All) && json.GetArrayLength() != 1))
					throw new JsonException("Use distinct flags; none and all must stand alone.");
				result |= flag;
				found = true;
				break;
			}
			if (!found) throw new JsonException("Unknown keyboard flag name.");
		}
		return result;
	}

	public override void Write(Utf8JsonWriter writer, KeyboardFlags value, JsonSerializerOptions options)
	{
		if (value != KeyboardFlags.All && (value & ~Known) != 0) throw new JsonException("Undefined keyboard flag bits.");
		writer.WriteStartArray();
		if (value is KeyboardFlags.None or KeyboardFlags.All)
			writer.WriteStringValue(value == KeyboardFlags.None ? "none" : "all");
		else
			foreach (var (name, flag) in Members)
				if (flag is not (KeyboardFlags.None or KeyboardFlags.All) && (value & flag) == flag)
					writer.WriteStringValue(name);
		writer.WriteEndArray();
	}
}
