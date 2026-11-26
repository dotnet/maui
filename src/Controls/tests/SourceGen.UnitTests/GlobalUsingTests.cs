using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class GlobalUsingTests
{
	const string GlobalXmlns = "http://schemas.microsoft.com/dotnet/maui/global";

	[Fact]
	public void GeneratesMappingsAndResolvesTypesInTheSamePass()
	{
		var compilation = CreateMauiCompilation().AddSyntaxTrees(
			CSharpSyntaxTree.ParseText(
				"""
				global using global::Demo.Controls;

				namespace Demo.Controls
				{
					public class LocalControl : global::Microsoft.Maui.Controls.ContentView { }
				}

				namespace @class
				{
					public class EscapedControl : global::Microsoft.Maui.Controls.ContentView { }
				}

				namespace Test
				{
					public partial class TestPage : global::Microsoft.Maui.Controls.ContentPage
					{
						public TestPage() => InitializeComponent();
					}
				}
				"""),
			CSharpSyntaxTree.ParseText(
				"""
				global using Demo.Controls;
				global using @class;
				""", path: "obj/Debug/Project.GlobalUsings.g.cs"));
		var xaml = """
			<ContentPage x:Class="Test.TestPage">
				<VerticalStackLayout>
					<LocalControl x:Name="local" />
					<EscapedControl x:Name="escaped" />
				</VerticalStackLayout>
			</ContentPage>
			""";

		var run = Run(CreateDriver(xaml), compilation);

		AssertNoErrors(run);
		Assert.Single(GetMappings(run.Output, "Demo.Controls"));
		Assert.Single(GetMappings(run.Output, "class"));
		Assert.Equal("Demo.Controls.LocalControl", GetFieldType(run.Output, "local"));
		Assert.Equal("class.EscapedControl", GetFieldType(run.Output, "escaped").Replace("@", "", StringComparison.Ordinal));
		Assert.Contains(run.Result.Results.Single().GeneratedSources, source => source.HintName.EndsWith(".xsg.cs", StringComparison.Ordinal));
	}

	[Fact]
	public void IgnoresNonGlobalAliasesStaticImportsAndInactiveDirectives()
	{
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(
			"""
			global using Included;
			global using NamespaceAlias = AliasNamespace;
			global using TypeAlias = AliasNamespace.AliasType;
			global using static StaticNamespace.Helper;
			#if NEVER
			global using InactiveNamespace;
			#endif
			using OrdinaryNamespace;

			namespace Included { public class Marker { } }
			namespace AliasNamespace { public class AliasType { } }
			namespace StaticNamespace { public static class Helper { } }
			namespace InactiveNamespace { public class Marker { } }
			namespace OrdinaryNamespace { public class Marker { } }
			"""));

		var run = Run(CreateDriver(), compilation);

		AssertNoErrors(run);
		Assert.Single(GetMappings(run.Output, "Included"));
		Assert.Empty(GetMappings(run.Output, "AliasNamespace"));
		Assert.Empty(GetMappings(run.Output, "StaticNamespace"));
		Assert.Empty(GetMappings(run.Output, "InactiveNamespace"));
		Assert.Empty(GetMappings(run.Output, "OrdinaryNamespace"));
	}

	[Theory]
	[InlineData("LocalControl", false, true)]
	[InlineData("LocalControl", true, true)]
	[InlineData("ExternalControl", false, false)]
	[InlineData("ExternalControl", true, true)]
	public void ImportedNamespaceOnlyAddsCurrentAssemblyTypes(string typeName, bool explicitMapping, bool shouldResolve)
	{
		var external = CreateMauiCompilation("External.Assembly").AddSyntaxTrees(CSharpSyntaxTree.ParseText(
			"""
			namespace Shared.Controls;
			public class ExternalControl : global::Microsoft.Maui.Controls.ContentView { }
			"""));
		var mapping = explicitMapping
			? """[assembly: global::Microsoft.Maui.Controls.XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "Shared.Controls", AssemblyName = "External.Assembly")]"""
			: "";
		var compilation = CreateMauiCompilation()
			.AddReferences(external.ToMetadataReference())
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
				$$"""
				global using Shared.Controls;
				{{mapping}}

				namespace Shared.Controls
				{
					public class LocalControl : global::Microsoft.Maui.Controls.ContentView { }
				}

				namespace Test
				{
					public partial class TestPage : global::Microsoft.Maui.Controls.ContentPage
					{
						public TestPage() => InitializeComponent();
					}
				}
				"""));
		var xaml = $$"""
			<ContentPage x:Class="Test.TestPage">
				<{{typeName}} x:Name="control" />
			</ContentPage>
			""";

		var run = Run(CreateDriver(xaml), compilation);

		var generatedMapping = Assert.Single(GetMappings(run.Output, "Shared.Controls"),
			attribute => attribute.NamedArguments.IsEmpty);
		Assert.Empty(generatedMapping.NamedArguments);
		if (shouldResolve)
		{
			AssertNoErrors(run);
			Assert.Equal($"Shared.Controls.{typeName}", GetFieldType(run.Output, "control"));
		}
		else
		{
			Assert.Contains(run.Result.Diagnostics, diagnostic => diagnostic.Id == "MAUIX2000");
		}
	}

	[Fact]
	public void ExternAliasDoesNotImportSameNamedLocalNamespace()
	{
		var external = CreateMauiCompilation("External.Assembly").AddSyntaxTrees(CSharpSyntaxTree.ParseText(
			"namespace Shared.Controls { public class ExternalControl { } }"));
		var compilation = CreateMauiCompilation()
			.AddReferences(external.ToMetadataReference(ImmutableArray.Create("external")))
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
				"""
				extern alias external;
				global using external::Shared.Controls;
				namespace Shared.Controls { public class LocalControl { } }
				"""));

		var run = Run(CreateDriver(), compilation);

		AssertNoErrors(run);
		Assert.Empty(GetMappings(run.Output, "Shared.Controls"));
	}

	[Fact]
	public void DoesNotImportUsingsGeneratedByAnotherGeneratorInTheSamePass()
	{
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(
			"namespace Generated.Controls { public class Example { } }"));
		var driver = CSharpGeneratorDriver.Create(
			[new XamlGenerator().AsSourceGenerator(), new OtherUsingGenerator().AsSourceGenerator()]);

		var run = Run(driver, compilation);

		AssertNoErrors(run);
		Assert.Contains(run.Result.Results, result => result.GeneratedSources.Any(source => source.HintName == "Other.GlobalUsings.g.cs"));
		Assert.Empty(GetMappings(run.Output, "Generated.Controls"));
	}

	[Fact]
	public void ExplicitMauiDefaultDoesNotIncludeGlobalUsings()
	{
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(
			"""
			global using Demo.Controls;
			namespace Demo.Controls { public class LocalControl : global::Microsoft.Maui.Controls.ContentView { } }
			namespace Test { public partial class TestPage : global::Microsoft.Maui.Controls.ContentPage { } }
			"""));
		var xaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="Test.TestPage">
				<LocalControl x:Name="control" />
			</ContentPage>
			""";

		var run = Run(CreateDriver(xaml), compilation);

		Assert.Contains(run.Result.Diagnostics, diagnostic => diagnostic.Id == "MAUIX2000");
	}

	[Fact]
	public void ReportsAmbiguousTypesFromGlobalUsings()
	{
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(
			"""
			global using First.Controls;
			global using Second.Controls;
			namespace First.Controls { public class Duplicate : global::Microsoft.Maui.Controls.ContentView { } }
			namespace Second.Controls { public class Duplicate : global::Microsoft.Maui.Controls.ContentView { } }
			namespace Test { public partial class TestPage : global::Microsoft.Maui.Controls.ContentPage { } }
			"""));
		var xaml = """
			<ContentPage x:Class="Test.TestPage">
				<Duplicate x:Name="control" />
			</ContentPage>
			""";

		var run = Run(CreateDriver(xaml), compilation);

		Assert.Contains(run.Result.Diagnostics, diagnostic => diagnostic.Id == "MAUIX2001");
	}

	[Fact]
	public void GlobalUsingEditsInvalidateMappingsAndTypeResolution()
	{
		var declarations = CSharpSyntaxTree.ParseText(
			"""
			namespace First.Controls { public class Example : global::Microsoft.Maui.Controls.ContentView { } }
			namespace Second.Controls { public class Example : global::Microsoft.Maui.Controls.ContentView { } }
			namespace Test { public partial class TestPage : global::Microsoft.Maui.Controls.ContentPage { } }
			""");
		var usings = CSharpSyntaxTree.ParseText("", path: "GlobalUsings.cs");
		var compilation = CreateMauiCompilation().AddSyntaxTrees(declarations, usings);
		var xaml = """
			<ContentPage x:Class="Test.TestPage">
				<Example x:Name="example" />
			</ContentPage>
			""";
		var run = Run(CreateDriver(xaml), compilation);
		Assert.Contains(run.Result.Diagnostics, diagnostic => diagnostic.Id == "MAUIX2000");

		(compilation, usings) = ReplaceUsings(compilation, usings, "global using First.Controls;");
		run = Run(run.Driver, compilation);
		AssertNoErrors(run);
		Assert.Single(GetMappings(run.Output, "First.Controls"));
		Assert.Equal("First.Controls.Example", GetFieldType(run.Output, "example"));

		(compilation, usings) = ReplaceUsings(compilation, usings, "global using Second.Controls;");
		run = Run(run.Driver, compilation);
		AssertNoErrors(run);
		Assert.Empty(GetMappings(run.Output, "First.Controls"));
		Assert.Single(GetMappings(run.Output, "Second.Controls"));
		Assert.Equal("Second.Controls.Example", GetFieldType(run.Output, "example"));

		(compilation, usings) = ReplaceUsings(compilation, usings, "");
		run = Run(run.Driver, compilation);
		Assert.Empty(GetMappings(run.Output, "Second.Controls"));
		Assert.Contains(run.Result.Diagnostics, diagnostic => diagnostic.Id == "MAUIX2000");
	}

	[Fact]
	public void RebindsAnUnchangedUsingWhenItsNamespaceIsAddedOrRemoved()
	{
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(
			"""
			global using Pending.Controls;
			namespace Test { public partial class TestPage : global::Microsoft.Maui.Controls.ContentPage { } }
			"""));
		var xaml = """
			<ContentPage x:Class="Test.TestPage">
				<Example x:Name="example" />
			</ContentPage>
			""";
		var run = Run(CreateDriver(xaml), compilation);
		Assert.Empty(GetMappings(run.Output, "Pending.Controls"));

		var declarations = CSharpSyntaxTree.ParseText(
			"namespace Pending.Controls { public class Example : global::Microsoft.Maui.Controls.ContentView { } }");
		compilation = compilation.AddSyntaxTrees(declarations);
		run = Run(run.Driver, compilation);
		AssertNoErrors(run);
		Assert.Single(GetMappings(run.Output, "Pending.Controls"));
		Assert.Equal("Pending.Controls.Example", GetFieldType(run.Output, "example"));

		compilation = compilation.RemoveSyntaxTrees(declarations);
		run = Run(run.Driver, compilation);
		Assert.Empty(GetMappings(run.Output, "Pending.Controls"));
		Assert.Contains(run.Result.Diagnostics, diagnostic => diagnostic.Id == "MAUIX2000");
	}

	[Fact]
	public void ReorderingAndDuplicatingUsingsKeepsGeneratedMappingsStable()
	{
		var declarations = CSharpSyntaxTree.ParseText(
			"namespace First { public class Example { } } namespace Second { public class Example { } }");
		var usings = CSharpSyntaxTree.ParseText("global using Second; global using First;", path: "GlobalUsings.cs");
		var compilation = CreateMauiCompilation().AddSyntaxTrees(declarations, usings);
		var initial = Run(CreateDriver(), compilation);

		(compilation, usings) = ReplaceUsings(compilation, usings, "global using First; global using Second; global using First;");
		var reordered = Run(initial.Driver, compilation);

		AssertNoErrors(initial);
		AssertNoErrors(reordered);
		var initialSource = initial.Result.Results.Single().GeneratedSources.Single(source => source.HintName == "Global.Usings.Xmlns.cs").SourceText.ToString();
		var reorderedSource = reordered.Result.Results.Single().GeneratedSources.Single(source => source.HintName == "Global.Usings.Xmlns.cs").SourceText.ToString();
		Assert.Equal(initialSource, reorderedSource);
		Assert.True(initialSource.IndexOf("\"First\"", StringComparison.Ordinal) < initialSource.IndexOf("\"Second\"", StringComparison.Ordinal));
		Assert.Equal(IncrementalStepRunReason.Unchanged,
			reordered.Result.Results.Single().TrackedSteps[TrackingNames.CompilationProvider].Single().Outputs.Single().Reason);
	}

	sealed class OtherUsingGenerator : IIncrementalGenerator
	{
		public void Initialize(IncrementalGeneratorInitializationContext context)
			=> context.RegisterSourceOutput(context.CompilationProvider,
				static (output, _) => output.AddSource("Other.GlobalUsings.g.cs", "global using Generated.Controls;"));
	}

	static (Compilation Compilation, SyntaxTree Usings) ReplaceUsings(Compilation compilation, SyntaxTree oldUsings, string source)
	{
		var newUsings = CSharpSyntaxTree.ParseText(source, path: oldUsings.FilePath);
		return (compilation.ReplaceSyntaxTree(oldUsings, newUsings), newUsings);
	}

	static GeneratorDriver CreateDriver(string? xaml = null)
	{
		AdditionalFile[] additionalFiles = xaml is null
			? []
			: [new AdditionalFile(ToAdditionalText("Test.xaml", xaml), "Xaml", "Test.xaml", "Test.xaml", "Test.TestPage.xaml", "net10.0", null)];
		return CSharpGeneratorDriver.Create(
				[new XamlGenerator().AsSourceGenerator()],
				driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true))
			.AddAdditionalTexts([.. additionalFiles.Select(file => file.Text)])
			.WithUpdatedAnalyzerConfigOptions(CreateAnalyzerConfigOptionsProvider(additionalFiles));
	}

	static (GeneratorDriver Driver, Compilation Output, GeneratorDriverRunResult Result) Run(GeneratorDriver driver, Compilation compilation)
	{
		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);
		return (driver, output, driver.GetRunResult());
	}

	static AttributeData[] GetMappings(Compilation compilation, string target)
	{
		var attributeType = compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.XmlnsDefinitionAttribute");
		return compilation.Assembly.GetAttributes()
			.Where(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType)
				&& attribute.ConstructorArguments.Length == 2
				&& Equals(attribute.ConstructorArguments[0].Value, GlobalXmlns)
				&& Equals(attribute.ConstructorArguments[1].Value, target))
			.ToArray();
	}

	static string GetFieldType(Compilation compilation, string name)
		=> Assert.IsAssignableFrom<IFieldSymbol>(Assert.Single(compilation.GetTypeByMetadataName("Test.TestPage")!.GetMembers(name))).Type.ToDisplayString();

	static void AssertNoErrors((GeneratorDriver Driver, Compilation Output, GeneratorDriverRunResult Result) run)
	{
		Assert.Empty(run.Result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
		Assert.Empty(run.Output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
	}
}
