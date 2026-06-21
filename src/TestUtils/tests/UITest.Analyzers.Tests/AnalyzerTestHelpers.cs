using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UITest.Analyzers.Tests;

internal static class AnalyzerTestHelpers
{
	private static readonly CSharpParseOptions ParseOptions = new CSharpParseOptions(LanguageVersion.Latest);

	/// <summary>
	/// Runs the specified analyzer on the given source code and returns the diagnostics.
	/// </summary>
	internal static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync<TAnalyzer>(string source)
		where TAnalyzer : DiagnosticAnalyzer, new()
	{
		var compilation = CreateCompilation(source);
		var analyzer = new TAnalyzer();

		var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));
		var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

		return diagnostics;
	}

	/// <summary>
	/// Runs the specified analyzer on the given source code and returns diagnostics filtered by diagnostic ID.
	/// </summary>
	internal static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync<TAnalyzer>(string source, string diagnosticId)
		where TAnalyzer : DiagnosticAnalyzer, new()
	{
		var allDiagnostics = await GetDiagnosticsAsync<TAnalyzer>(source);
		return allDiagnostics.Where(d => d.Id == diagnosticId).ToImmutableArray();
	}

	private static CSharpCompilation CreateCompilation(string source)
	{
		var syntaxTree = CSharpSyntaxTree.ParseText(source, ParseOptions);

		// We need to reference NUnit's TestAttribute and CategoryAttribute
		var references = new List<MetadataReference>
		{
			MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
			MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
		};

		// Add NUnit reference - we'll create stub attributes in the test source instead
		// since we don't want to add a package reference to NUnit in the test project

		return CSharpCompilation.Create(
			"TestAssembly",
			syntaxTrees: new[] { syntaxTree },
			references: references,
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
	}

	/// <summary>
	/// NUnit attribute stubs for use in test source code.
	/// Include this in test source to simulate NUnit attributes without requiring the actual NUnit package.
	/// </summary>
	internal const string NUnitAttributeStubs = """
		namespace NUnit.Framework
		{
			[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
			public class TestAttribute : System.Attribute { }

			[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = true)]
			public class CategoryAttribute : System.Attribute
			{
				public CategoryAttribute(string name) { Name = name; }
				public string Name { get; }
			}
		}
		""";
}
