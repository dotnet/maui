using System.Buffers;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Controls.HybridWebApps;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class JsonHelperTests
{
	[Theory]
	[InlineData("null", false)]
	[InlineData("7", false)]
	[InlineData("true", false)]
	[InlineData("\"value\"", true)]
	public void StringPrimitiveEnforcesKindAndUseSiteNullability(string json, bool valid)
	{
		using var document = JsonDocument.Parse(json);
		if (valid)
			Assert.Equal("value", HybridJsonPrimitives.String.Read(document.RootElement, false));
		else
			Assert.ThrowsAny<Exception>(() => HybridJsonPrimitives.String.Read(document.RootElement, false));
		if (json == "null")
			Assert.Null(HybridJsonPrimitives.String.Read(document.RootElement, true));
	}

	[Fact]
	public void PrimitiveCodecsRejectOverflowFractionalIntegersNonfiniteAndInvalidChars()
	{
		using var overflow = JsonDocument.Parse("2147483648");
		using var fractional = JsonDocument.Parse("1.5");
		using var infinite = JsonDocument.Parse("1e400");
		using var text = JsonDocument.Parse("\"ab\"");
		Assert.Throws<FormatException>(() => HybridJsonPrimitives.Int32.Read(overflow.RootElement, false));
		Assert.Throws<FormatException>(() => HybridJsonPrimitives.Int32.Read(fractional.RootElement, false));
		Assert.Throws<JsonException>(() => HybridJsonPrimitives.Double.Read(infinite.RootElement, false));
		Assert.Throws<JsonException>(() => HybridJsonPrimitives.Char.Read(text.RootElement, false));
		Assert.Throws<JsonException>(() => Write(writer => HybridJsonPrimitives.String.Write(writer, null!, false)));
		Assert.Equal("null", Write(writer => HybridJsonPrimitives.String.Write(writer, null!, true)));
	}

	[Fact]
	public void CompiledCollectionsRejectDuplicateKeysAndValidateEveryNullableElement()
	{
		using var duplicate = JsonDocument.Parse("""{"same":1,"same":2}""");
		Assert.Throws<JsonException>(() => HybridJsonCollections.ReadDictionary(duplicate.RootElement, item => HybridJsonPrimitives.Int32.Read(item, false)));
		using var values = JsonDocument.Parse("""["a",null]""");
		Assert.Throws<JsonException>(() => HybridJsonCollections.Read<string, List<string>>(values.RootElement, () => new(), item => HybridJsonPrimitives.String.Read(item, false)));
		var result = HybridJsonCollections.Read<string, List<string>>(values.RootElement, () => new(), item => HybridJsonPrimitives.String.Read(item, true));
		Assert.Equal(new[] { "a", null }, result);
		Assert.Equal("""["a",null]""", Write(writer => HybridJsonCollections.Write(writer, result, (writer, item) => HybridJsonPrimitives.String.Write(writer, item, true))));
	}

	[Theory]
	[InlineData("""["read","write"]""", 3)]
	[InlineData("""["all"]""", 255)]
	[InlineData("[]", 0)]
	public void NamedFlagsUseCompilerProvidedValuesWithoutEnumReflection(string json, int expected)
	{
		using var document = JsonDocument.Parse(json);
		Assert.Equal((Permissions)expected, Flags.Read(document.RootElement, false));
		Assert.Equal(expected == 255 ? """["all"]""" : expected == 0 ? """["none"]""" : json,
			Write(writer => Flags.Write(writer, (Permissions)expected, false)));
	}

	[Theory]
	[InlineData("""["read","readAlias"]""")]
	[InlineData("""["none","read"]""")]
	[InlineData("""["all","write"]""")]
	[InlineData("""["Read"]""")]
	[InlineData("1")]
	[InlineData("""[1]""")]
	public void NamedFlagsRejectAliasesDuplicatesSentinelMixturesNumbersAndUnknownNames(string json)
	{
		using var document = JsonDocument.Parse(json);
		Assert.Throws<JsonException>(() => Flags.Read(document.RootElement, false));
		Assert.Throws<JsonException>(() => Write(writer => Flags.Write(writer, (Permissions)8, false)));
	}

	[Fact]
	public void CodecMetadataHasAnExplicitStrictSourceOwnerAndNullNeverSerializesArbitraryObjects()
	{
		var info = HybridJsonPrimitives.Int32.CreateInfo(ProtocolJsonContext.Default);
		CommandRegistry.ValidateMetadata(info);
		Assert.False(info.Options.AllowDuplicateProperties);
		Assert.Equal("7", JsonSerializer.Serialize(7, info));
		Assert.Equal("null", Write(writer => HybridJsonDefaults.Null.Write(writer, null, true)));
		Assert.Throws<JsonException>(() => Write(writer => HybridJsonDefaults.Null.Write(writer, new object(), true)));
	}

	enum Permissions : byte { None = 0, Read = 1, Write = 2, All = 255 }
	static readonly HybridJsonCodec<Permissions> Flags = HybridJsonEnums.Create(
		["all", "none", "read", "readAlias", "write"], [255UL, 0UL, 1UL, 1UL, 2UL],
		(Permissions value) => (ulong)value, value => (Permissions)value, true, 255, 3);

	static string Write(Action<Utf8JsonWriter> write)
	{
		var buffer = new ArrayBufferWriter<byte>();
		using (var writer = new Utf8JsonWriter(buffer))
			write(writer);
		return Encoding.UTF8.GetString(buffer.WrittenSpan);
	}
}
