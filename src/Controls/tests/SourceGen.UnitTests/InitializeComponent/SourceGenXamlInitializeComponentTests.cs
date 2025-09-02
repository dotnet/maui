using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class SourceGenXamlInitializeComponentTestBase : SourceGenTestsBase
{
	protected record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null)
		: AdditionalFile(Text: ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn);

	protected (GeneratorDriverRunResult result, string? text) RunGenerator(string xaml, string code, string noWarn = "", string targetFramework = "")
	{
		var compilation = CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));
		var result = RunGenerator<CodeBehindGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml, TargetFramework: targetFramework, NoWarn: noWarn));
		var generated = result.Results.SingleOrDefault().GeneratedSources.SingleOrDefault(gs => gs.HintName.EndsWith(".xsg.cs")).SourceText?.ToString();

		return (result, generated);
	}
}
