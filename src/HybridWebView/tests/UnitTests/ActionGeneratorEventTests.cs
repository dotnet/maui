using System.Text;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed partial class ActionGeneratorTests
{
	[Fact]
	public void NullableDictionaryValueAnnotationsCompileWithWarningsAsErrors()
	{
		var generated = Generate("""
			using System.Collections.Generic;
			public sealed record Item(string Text);
			public class Actions {
				[HybridAction("dictionary.strings")]
				public static Dictionary<string, string?> Strings(Dictionary<string, string?> value) => value;
				[HybridAction("dictionary.items")]
				public static Dictionary<string, Item?> Items(Dictionary<string, Item?> value) => value;
				[HybridAction("dictionary.readOnlyStrings")]
				public static IReadOnlyDictionary<string, string?> ReadOnlyStrings(IReadOnlyDictionary<string, string?> value) => value;
				[HybridAction("dictionary.readOnlyItems")]
				public static IReadOnlyDictionary<string, Item?> ReadOnlyItems(IReadOnlyDictionary<string, Item?> value) => value;
			}
			""");
		Assert.Empty(generated.Result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		var strict = generated.Output.WithOptions(generated.Output.Options.WithGeneralDiagnosticOption(ReportDiagnostic.Error));
		Assert.Empty(strict.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
	}

	[Fact]
	public void SourceEventsGenerateTypedDelegatesAndNamedPayloadAdaptersWithoutReflection()
	{
		var generated = Generate("""
			using System;
			public record Data(int Value);
			public delegate void Pair(int count, string writer);
			public delegate void SenderPair(object sender, int count, string label);
			public class Events {
				[HybridEvent("event.empty")] public event Action? Empty;
				[HybridEvent("event.args")] public event EventHandler? Args;
				[HybridEvent("event.typed")] public event EventHandler<Data>? Typed;
				[HybridEvent("event.scalar")] public static event Action<int>? Scalar;
				[HybridEvent("event.pair")] public event Pair? Changed;
				[HybridEvent("event.senderPair")] public event SenderPair? Routed;
			}
			""");
		Assert.Empty(generated.Result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(generated.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		var code = string.Join("\n", generated.Result.GeneratedTrees);
		Assert.Contains("HybridEvents.Register", code, StringComparison.Ordinal);
		Assert.Contains("writer.WritePropertyName(\"count\")", code, StringComparison.Ordinal);
		Assert.Contains("writer.WritePropertyName(\"writer\")", code, StringComparison.Ordinal);
		Assert.Contains("writer.WriteNullValue()", code, StringComparison.Ordinal);
		Assert.DoesNotContain("GetEvent", code, StringComparison.Ordinal);
		Assert.DoesNotContain("DynamicInvoke", code, StringComparison.Ordinal);
		Assert.DoesNotContain("Expression.", code, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("public delegate int Invalid();")]
	[InlineData("public delegate void Invalid(ref int value);")]
	[InlineData("public delegate void Invalid(out int value);")]
	[InlineData("public delegate void Invalid(in int value);")]
	[InlineData("public delegate void Invalid(System.Span<int> value);")]
	public void UnsupportedNativeDelegateSignaturesAreDiagnosed(string declaration)
	{
		var result = Generate("using System; " + declaration +
			""" public class Events { [HybridEvent("event.bad")] public event Invalid? Changed; }""");
		Assert.Contains(result.Result.Diagnostics, d => d.Id == "HYBRID006");
		Assert.DoesNotContain(result.Result.Diagnostics, d => d.Id == "CS8785");
	}

	[Fact]
	public void EventsWithUnserializablePayloadsAndMethodAttributesWithoutLifecycleAreDiagnostics()
	{
		var payload = Generate("""
			using System;
			public class Events { [HybridEvent("event.bad")] public event Action<object>? Changed; }
			""");
		Assert.Contains(payload.Result.Diagnostics, d => d.Id == "HYBRID004");
		var method = Generate("""
			public class Events { [HybridEvent("event.bad")] public void Start() {} }
			""");
		Assert.Contains(method.Result.Diagnostics, d => d.Id == "HYBRID006");
	}

	[Fact]
	public void NativeOutputOnlyArgsUseStrictWritersAndCannotLeakIntoReadableInputContracts()
	{
		var output = Generate("""
			using System;
			public class NativeArgs { public string Text { get; } = "native"; }
			public class Events { [HybridEvent("event.native")] public event Action<NativeArgs[]>? Changed; }
			""");
		Assert.Empty(output.Result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(output.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		var input = Generate("""
			using System;
			public class NativeArgs { public string Text { get; } = "native"; }
			public class AEvents { [HybridEvent("event.native")] public event Action<NativeArgs[]>? Changed; }
			public class ZActions { [HybridAction("event.input")] public static void Call(NativeArgs[] args) {} }
			""");
		Assert.Contains(input.Result.Diagnostics, d => d.Id == "HYBRID004");
		Assert.DoesNotContain(input.Result.Diagnostics, d => d.Id == "CS8785");
	}

	[Fact]
	public void InvalidContractsCannotPoisonOtherProvidersInThePooledGraph()
	{
		var result = Generate("""
			public class BadDto { public object Value { get; set; } = new(); }
			public class AInvalid { [HybridAction("bad")] public BadDto Echo(BadDto value) => value; }
			public class ZValid { [HybridAction("good")] public int Echo(int value) => value; }
			""");
		Assert.Contains(result.Result.Diagnostics, d => d.Id == "HYBRID004");
		Assert.DoesNotContain(result.Result.Diagnostics, d => d.Id == "CS8785");
		Assert.Empty(result.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
	}

	[Fact]
	public void DeclaredEnumConvertersAreNotReplacedByTheDefaultNamedEnumHelper()
	{
		var result = Generate("""
			using System; using System.Text.Json; using System.Text.Json.Serialization;
			[JsonConverter(typeof(ModeConverter))] public enum Mode { Value }
			public class ModeConverter : JsonConverter<Mode> {
				public override Mode Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) =>
					reader.GetString() == "custom" ? Mode.Value : throw new JsonException();
				public override void Write(Utf8JsonWriter writer, Mode value, JsonSerializerOptions options) => writer.WriteStringValue("custom");
			}
			public class Actions { [HybridAction("custom.enum")] public static Mode Echo(Mode value) => value; }
			""");
		Assert.Empty(result.Result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(result.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		var code = string.Join("\n", result.Result.GeneratedTrees);
		Assert.Contains("ModeConverter CustomConverter", code, StringComparison.Ordinal);
		Assert.DoesNotContain("HybridJsonEnums.Create", code, StringComparison.Ordinal);
	}

	[Fact]
	public void PooledContractsAreIndependentOfActionCountAndActionFilesStayBounded()
	{
		var small = Generate(SizeFixture(6));
		var large = Generate(SizeFixture(24));
		Assert.Empty(small.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(large.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		static string[] Contracts(GeneratorDriverRunResult result) => result.Results.SelectMany(r => r.GeneratedSources)
			.Where(s => s.HintName.StartsWith("HybridContracts.", StringComparison.Ordinal)).Select(s => s.SourceText.ToString()).ToArray();
		Assert.Equal(Contracts(small.Result), Contracts(large.Result));
		Assert.All(large.Result.GeneratedTrees, tree => Assert.True(Encoding.UTF8.GetByteCount(tree.ToString()) < 32768, tree.FilePath));
		var text = string.Join("\n", small.Result.GeneratedTrees);
		Assert.DoesNotContain("json.GetInt32()", text, StringComparison.Ordinal);
		Assert.DoesNotContain("new NullConverter", text, StringComparison.Ordinal);
		Assert.DoesNotContain("sealed class Converter", text, StringComparison.Ordinal);
		Assert.Equal(small.Result.GeneratedTrees.Select(t => t.ToString()), Generate(SizeFixture(6)).Result.GeneratedTrees.Select(t => t.ToString()));
	}

	[Fact]
	public void GeneratedCodeSizeMetrics()
	{
		var result = Generate(SizeFixture(6));
		var files = result.Result.GeneratedTrees.Select(t => t.ToString()).ToArray();
		var bytes = files.Sum(Encoding.UTF8.GetByteCount);
		var lines = files.Sum(f => f.Count(c => c == '\n'));
		Console.WriteLine($"HYBRID_GENERATED_SIZE providers=8 actions=48 bytes={bytes} lines={lines} files={files.Length} maxBytes={files.Max(Encoding.UTF8.GetByteCount)}");
		Assert.Empty(result.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		// Measured with the original generator on this exact fixture and the same product namespace.
		const int baselineBytes = 131347, baselineLines = 2205;
		Assert.True(bytes < baselineBytes / 2, $"Generated {bytes} bytes; original generator emitted {baselineBytes}.");
		Assert.True(lines < baselineLines / 2, $"Generated {lines} lines; original generator emitted {baselineLines}.");
	}

	static string SizeFixture(int count)
	{
		var source = new StringBuilder("""
			using System.Collections.Generic;
			public enum Mode { First, Second, Third }
			public sealed record SharedDto(string Label, int[] Counts, Mode Mode, IReadOnlyDictionary<string,string?> Tags);
			""");
		for (int provider = 0; provider < 8; provider++)
		{
			source.Append("public class Provider").Append(provider).Append(" {");
			for (int action = 0; action < count; action++)
				source.Append("[HybridAction(\"provider").Append(provider).Append(".action").Append(action)
					.Append("\")] public static SharedDto Action").Append(action).Append("(SharedDto value) => value;");
			source.Append('}');
		}
		return source.ToString();
	}
}
