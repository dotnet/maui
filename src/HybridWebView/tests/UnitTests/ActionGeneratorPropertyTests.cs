using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.HybridWebApps;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed partial class ActionGeneratorTests
{
	[Fact]
	public async Task AnnotatedInheritedAndSourceSettersPreserveNullabilityVoidAndDestinationErrors()
	{
		var generated = Generate("""
			using System;
			[assembly: HybridExport(typeof(ISelected))]
			public interface IBase {
				string? Name {
					[HybridAction("property.name")] get;
					[HybridAction("property.setName", MainThread = true)] set;
				}
			}
			public interface ISelected : IBase { new string? Name { get; set; } }
			public class Native : ISelected {
				string? native;
				string? IBase.Name {
					get => native;
					set { if (value == "throw") throw new ArgumentException("Native failure"); native = value; }
				}
				public string? Name {get;set;} = "shadow";
			}
			public class Local {
				public string Text {
					[HybridAction("local.get")] get;
					[HybridAction("local.set")] set;
				} = "";
			}
			""");
		Assert.Empty(generated.Result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(generated.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		var code = string.Join("\n", generated.Result.GeneratedTrees);
		Assert.Contains(".@Name = arg0", code, StringComparison.Ordinal);
		Assert.DoesNotContain("set_Name(", code, StringComparison.Ordinal);
		Assert.DoesNotContain("HybridNativePolicy", code, StringComparison.Ordinal);
		using var image = new MemoryStream();
		Assert.True(generated.Output.Emit(image).Success);
		var assembly = System.Reflection.Assembly.Load(image.ToArray());
		var services = TestComposition.Services();
		var native = Activator.CreateInstance(assembly.GetType("Native")!)!;
		services.AddSingleton(assembly.GetType("ISelected")!, native);
		assembly.GetType("Microsoft.Maui.Controls.HybridWebApps.Generated.ActionCompilationActionAssembly")!
			.GetMethod("AddActionCompilationActions")!.Invoke(null, [services]);
		using var host = new TestComposition.TestHost(services);
		using var scope = host.CreateScope();
		var registry = scope.ServiceProvider.GetRequiredService<CommandRegistry>();
		async Task<JsonElement> Call(string command, string args = "{}")
		{
			using var document = JsonDocument.Parse(args);
			return await registry.DispatchAsync(command, document.RootElement, new Operation("session", "request", default, (_, _) => { }));
		}
		Assert.Equal(JsonValueKind.Null, (await Call("property.setName", """{"value":"native"}""")).ValueKind);
		Assert.Equal("native", (await Call("property.name")).GetString());
		Assert.Equal("shadow", assembly.GetType("Native")!.GetProperty("Name")!.GetValue(native));
		await Call("property.setName", "[null]");
		Assert.Equal(JsonValueKind.Null, (await Call("property.name")).ValueKind);
		Assert.Equal("Native failure", (await Assert.ThrowsAsync<ArgumentException>(() =>
			Call("property.setName", """{"value":"throw"}"""))).Message);
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			Call("local.set", """{"value":null}"""))).Code);
		await Call("local.set", """["你好 👋"]""");
		Assert.Equal("你好 👋", (await Call("local.get")).GetString());
	}

	[Fact]
	public void ReadonlyNativeResultContractsNeedOnlyGettersButCannotSilentlyBecomeInputs()
	{
		const string native = """
			public readonly struct NativeValue {
				public NativeValue(double nativeValue) { Value = nativeValue; }
				public double Value {get;}
			}
			""";
		var output = Generate(native + """
			public class Actions {
				[HybridAction("native.read")] public NativeValue Read() => new(1.25);
			}
			""");
		Assert.Empty(output.Result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(output.Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		var input = Generate(native + """
			public class Actions {
				[HybridAction("native.read")] public NativeValue Read() => new(1.25);
				[HybridAction("native.write")] public void Write(NativeValue value) {}
			}
			""");
		Assert.Contains(input.Result.Diagnostics, d => d.Id == "HYBRID004");
		var opaque = Generate("""
			public readonly struct Opaque { }
			public class Actions { [HybridAction("native.opaque")] public Opaque Read() => default; }
			""");
		Assert.Contains(opaque.Result.Diagnostics, d => d.Id == "HYBRID004");
	}

	[Theory]
	[InlineData("""public string this[int index] { [HybridAction("bad.indexer")] get => ""; }""")]
	[InlineData("""public string Name { get; [HybridAction("bad.init")] init; } = "";""")]
	public void IndexersAndInitOnlyAccessorsProduceActionDiagnostics(string property)
	{
		var generated = Generate("public class Actions { " + property + " }");
		Assert.Contains(generated.Result.Diagnostics, d => d.Id == "HYBRID002");
		Assert.DoesNotContain(generated.Result.Diagnostics, d => d.Id == "CS8785");
	}
}
