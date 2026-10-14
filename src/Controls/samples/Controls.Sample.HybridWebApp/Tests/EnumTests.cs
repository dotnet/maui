using System.Text.Json;
using System.Text.Json.Serialization;
using Maui.HybridApp;
using Microsoft.Maui;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class EnumTests
{
	static Task<JsonElement> Call(CommandRegistry registry, string command, string args)
	{
		using var json = JsonDocument.Parse(args);
		return registry.DispatchAsync(command, json.RootElement.Clone(), new Operation("session", "request", default, (_, _) => { }));
	}

	[Theory]
	[InlineData("first", "first")]
	[InlineData("alias", "alias")]
	[InlineData("second", "alias")]
	[InlineData("wire-value", "wire-value")]
	public async Task DeclaredNamesAliasesAndMemberOverridesRoundTrip(string input, string expected)
	{
		var provider = new EnumTestProvider();
		var registry = TestComposition.CreateRegistry(provider);
		Assert.Equal(expected, (await Call(registry, "enums.echo", $$"""["{{input}}"]""")).GetString());
		Assert.Equal(1, provider.Calls);
	}

	[Theory]
	[InlineData("0")]
	[InlineData("1")]
	[InlineData("999")]
	[InlineData("\"1\"")]
	[InlineData("\"First\"")]
	[InlineData("\"missing\"")]
	[InlineData("null")]
	[InlineData("[]")]
	public async Task InvalidEnumsAreRejectedBeforeCallingTheMethod(string input)
	{
		var provider = new EnumTestProvider();
		var registry = TestComposition.CreateRegistry(provider);
		var error = await Assert.ThrowsAsync<CommandException>(() => Call(registry, "enums.echo", "[" + input + "]"));
		Assert.Equal("invalid_arguments", error.Code);
		Assert.Equal(0, provider.Calls);
	}

	[Fact]
	public async Task NullableAndDefaultEnumsUseTheSameStringCodecInsideDtos()
	{
		var registry = TestComposition.CreateRegistry(new EnumTestProvider());
		Assert.Equal("alias", (await Call(registry, "enums.optional", "{}")).GetString());
		Assert.Equal(JsonValueKind.Null, (await Call(registry, "enums.optional", "[null]")).ValueKind);
		Assert.Equal("wire-value", (await Call(registry, "enums.optional", """["wire-value"]""")).GetString());
		Assert.Equal("alias", (await Call(registry, "enums.dto", "[{}]")).GetProperty("mode").GetString());
		Assert.Equal(JsonValueKind.Null, (await Call(registry, "enums.dto", """[{"mode":null}]""")).GetProperty("mode").ValueKind);
	}

	[Theory]
	[InlineData("[]", """["none"]""")]
	[InlineData("""["none"]""", """["none"]""")]
	[InlineData("""["all"]""", """["all"]""")]
	[InlineData("""["a","b"]""", """["both"]""")]
	[InlineData("""["a","high"]""", """["a","high"]""")]
	[InlineData("""["both","high"]""", """["a","b","high"]""")]
	[InlineData("""["alias"]""", """["a"]""")]
	public async Task SignedFlagsZeroAllAliasesAndCompositesRoundTrip(string input, string expected)
	{
		var registry = TestComposition.CreateRegistry(new EnumTestProvider());
		Assert.Equal(expected, (await Call(registry, "enums.flags", "[" + input + "]")).GetRawText());
	}

	[Theory]
	[InlineData("1")]
	[InlineData("\"a\"")]
	[InlineData("[1]")]
	[InlineData("""["missing"]""")]
	[InlineData("""["a","a"]""")]
	[InlineData("""["a","alias"]""")]
	[InlineData("""["all","a"]""")]
	[InlineData("""["none","a"]""")]
	public async Task InvalidFlagsAreRejectedBeforeCallingTheMethod(string input)
	{
		var provider = new EnumTestProvider();
		var registry = TestComposition.CreateRegistry(provider);
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			Call(registry, "enums.flags", "[" + input + "]"))).Code);
		Assert.Equal(0, provider.Calls);
	}

	[Fact]
	public async Task NativeAllSentinelDoesNotAuthorizeUndefinedBitsAndUndefinedOutputsFailClearly()
	{
		var registry = TestComposition.CreateRegistry(new EnumTestProvider());
		Assert.Equal("""["all"]""", (await Call(registry, "enums.keyboard", """[["all"]]""")).GetRawText());
		Assert.Equal("""["capitalizeSentence","spellcheck"]""",
			(await Call(registry, "enums.keyboard", """[["spellcheck","capitalizeSentence"]]""")).GetRawText());
		foreach (var command in new[] { "enums.undefined", "enums.undefinedFlags", "enums.undefinedKeyboard" })
			Assert.Contains("Undefined", (await Assert.ThrowsAsync<JsonException>(() => Call(registry, command, "{}"))).Message, StringComparison.Ordinal);
		Assert.Equal("""["top"]""", (await Call(registry, "enums.unsigned", """[["top"]]""")).GetRawText());
		Assert.Equal("""["all"]""", (await Call(registry, "enums.unsigned", """[["all"]]""")).GetRawText());
	}
}

internal enum EnumMode
{
	First,
	Second,
	Alias = Second,
	[JsonStringEnumMemberName("wire-value")] Named = 2,
}
[Flags]
internal enum EnumFlags : sbyte { None = 0, A = 1, Alias = A, B = 2, Both = A | B, High = -128, All = -1 }
[Flags]
internal enum UnsignedEnumFlags : ulong { None = 0, Top = 1UL << 63, All = ulong.MaxValue }
internal sealed record EnumDto(EnumMode? Mode = EnumMode.Second);
internal sealed class EnumTestProvider
{
	public EnumTestProvider() { }
	internal int Calls;
	[HybridAction("enums.echo")] internal EnumMode Echo(EnumMode value) { Calls++; return value; }
	[HybridAction("enums.flags")] internal EnumFlags Flags(EnumFlags value) { Calls++; return value; }
	[HybridAction("enums.optional")] internal static EnumMode? Optional(EnumMode? value = EnumMode.Second) => value;
	[HybridAction("enums.dto")] internal static EnumDto Dto(EnumDto value) => value;
	[HybridAction("enums.keyboard")] internal static KeyboardFlags Keyboard(KeyboardFlags value) => value;
	[HybridAction("enums.unsigned")] internal static UnsignedEnumFlags Unsigned(UnsignedEnumFlags value) => value;
	[HybridAction("enums.undefined")] internal static EnumMode Undefined() => (EnumMode)999;
	[HybridAction("enums.undefinedFlags")] internal static EnumFlags UndefinedFlags() => (EnumFlags)64;
	[HybridAction("enums.undefinedKeyboard")] internal static KeyboardFlags UndefinedKeyboard() => (KeyboardFlags)64;
}
