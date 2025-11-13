using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class SourceGenXamlInitializeComponentTestBase : SourceGenTestsBase
{
	protected record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null, string Lineinfo = "enable")
		: AdditionalFile(Text: ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn, LineInfo: Lineinfo);

	protected (TestResult result, string? text) RunGenerator(string xaml, string code, string noWarn = "", string targetFramework = "", string? path = null, string lineinfo = "enable")
	{
		var compilation = CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));
		var workingDirectory = Environment.CurrentDirectory;
		var xamlFile = new AdditionalXamlFile(Path.Combine(workingDirectory, path ?? "Test.xaml"), xaml, RelativePath: path ?? "Test.xaml", TargetFramework: targetFramework, NoWarn: noWarn, ManifestResourceName: $"{compilation.AssemblyName}.Test.xaml", Lineinfo: lineinfo);
		var generatorResult = RunGenerator<XamlGenerator>(compilation, out var updatedCompilation, xamlFile);
		var generated = generatorResult.Results.SingleOrDefault().GeneratedSources.SingleOrDefault(gs => gs.HintName.EndsWith(".xsg.cs")).SourceText?.ToString();

		// Include compilation diagnostics so tests can check for C# errors in generated code
		// Only include diagnostics from generated XAML code, not from the test scaffolding code
		var compilationDiagnostics = updatedCompilation.GetDiagnostics()
			.Where(d => d.Severity >= DiagnosticSeverity.Error)  // Only errors, not warnings
			.Where(d => 
			{
				// Only include diagnostics from generated (.xsg.cs) files
				var filePath = d.Location.SourceTree?.FilePath ?? string.Empty;
				if (!filePath.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase))
					return false;
					
				// Exclude errors about XamlProcessing/XamlInflator which are test scaffolding
				if (d.Id == "CS0246" && (d.GetMessage().Contains("XamlProcessing", StringComparison.Ordinal) || d.GetMessage().Contains("XamlInflator", StringComparison.Ordinal)))
					return false;
				// Exclude accessibility errors for XamlInflator
				if (d.Id == "CS0122" && d.GetMessage().Contains("XamlInflator", StringComparison.Ordinal))
					return false;
				return true;
			});
		
		// Merge generator diagnostics with compilation diagnostics
		var allDiagnostics = generatorResult.Diagnostics.Concat(compilationDiagnostics);
		
		var result = new TestResult(generatorResult, allDiagnostics, updatedCompilation);

		return (result, generated);
	}

	protected record TestResult(GeneratorDriverRunResult GeneratorResult, IEnumerable<Diagnostic> Diagnostics, Compilation UpdatedCompilation)
	{
		// Helper properties to match the old API
		public ImmutableArray<GeneratorRunResult> Results => GeneratorResult.Results;
		public ImmutableArray<SyntaxTree> GeneratedTrees => GeneratorResult.GeneratedTrees;
	}
}
