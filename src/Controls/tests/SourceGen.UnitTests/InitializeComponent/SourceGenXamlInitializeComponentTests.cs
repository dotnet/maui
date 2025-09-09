using System;
using System.Collections.Generic;
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
	protected record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null, string Lineinfo = "enable", bool TreeOrder = false, bool Dry = false)
		: AdditionalFile(Text: ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn, LineInfo: Lineinfo, TreeOrder: TreeOrder, Dry: Dry);

	protected (GeneratorDriverRunResult result, string? text) RunGenerator(string xaml, string code, string noWarn = "", string targetFramework = "", string? path = null, string lineinfo = "enable", bool assertNoCompilationErrors = true, bool treeOrder = false, bool dry = false)
	{
		var compilation = CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));
		var workingDirectory = Environment.CurrentDirectory;
		var xamlFile = new AdditionalXamlFile(Path.Combine(workingDirectory, path ?? "Test.xaml"), xaml, RelativePath: path ?? "Test.xaml", TargetFramework: targetFramework, NoWarn: noWarn, ManifestResourceName: $"{compilation.AssemblyName}.Test.xaml", Lineinfo: lineinfo, TreeOrder: treeOrder, Dry: dry);
		var result = RunGenerator<XamlGenerator>(compilation, xamlFile, assertNoCompilationErrors);
		var generated = result.Results.SingleOrDefault().GeneratedSources.SingleOrDefault(gs => gs.HintName.EndsWith(".xsg.cs")).SourceText?.ToString();

		return (result, generated);
	}
}
