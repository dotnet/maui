using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class Maui40045Tests : SourceGenTestsBase
{
	[Fact]
	public async Task ImplicitXmlnsOptInDoesNotRequirePreviewFeaturesAssemblyAttribute()
	{
		// Mimics an app targeting a non-latest TFM: EnablePreviewFeatures does not add [assembly: RequiresPreviewFeatures].
		var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols("MauiAllowImplicitXmlnsDeclaration");
		var compilation = SourceGeneratorDriver.CreateMauiCompilation()
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText("class C {}", parseOptions));

		CSharpGeneratorDriver.Create([new XamlGenerator().AsSourceGenerator()], parseOptions: parseOptions)
			.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out _);

		var globalXmlns = updatedCompilation.SyntaxTrees.Single(t => t.FilePath.EndsWith("GlobalXmlns.g.cs", System.StringComparison.Ordinal));
		Assert.Contains("AllowImplicitXmlnsDeclaration]", globalXmlns.ToString(), System.StringComparison.Ordinal);

		var diagnostics = await updatedCompilation
			.WithAnalyzers(GetPreviewFeaturesAnalyzers())
			.GetAnalyzerDiagnosticsAsync();

		Assert.DoesNotContain(diagnostics, d => d.Id == "CA2252");
	}

	static ImmutableArray<DiagnosticAnalyzer> GetPreviewFeaturesAnalyzers()
	{
		var netAnalyzersPath = typeof(Maui40045Tests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
			.First(a => a.Key == "NetAnalyzersPath").Value!;
		var loader = new AnalyzerLoader();
		var analyzers = new[] { netAnalyzersPath, Path.Combine(Path.GetDirectoryName(netAnalyzersPath)!, "Microsoft.CodeAnalysis.CSharp.NetAnalyzers.dll") }
			.SelectMany(path =>
			{
				var reference = new AnalyzerFileReference(path, loader);
				reference.AnalyzerLoadFailed += (_, e) => throw e.Exception ?? new System.InvalidOperationException(e.Message);
				return reference.GetAnalyzers(LanguageNames.CSharp);
			})
			.Where(a => a.SupportedDiagnostics.Any(d => d.Id == "CA2252"))
			.ToImmutableArray();

		Assert.NotEmpty(analyzers);
		return analyzers;
	}

	class AnalyzerLoader : IAnalyzerAssemblyLoader
	{
		public void AddDependencyLocation(string fullPath) { }
		public Assembly LoadFromPath(string fullPath) => Assembly.LoadFrom(fullPath);
	}
}
