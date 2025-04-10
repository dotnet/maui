using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using NUnit.Framework;
using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class SourceGenXmlnsGlobal : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework);

	[Test]
	[Ignore("This test is ignored because it is not relevant to the current context.")]
	public void TestXmlns_aggregation()
	{
		var code =
"""
using Microsoft.Maui.Controls;
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "http://schemas.microsoft.com/dotnet/2021/maui")]
""";
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));

		var result = SourceGeneratorDriver.RunGenerator<CodeBehindGenerator>(compilation);

		Assert.IsFalse(result.Diagnostics.Any());

		//var generated = result.Results.Single().GeneratedSources.Single().SourceText.ToString();
	}
}
