using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

static class SourceGenHelpers
{
	static readonly CSharpParseOptions ParseOptions = new CSharpParseOptions(LanguageVersion.Preview).WithFeatures(
				[new KeyValuePair<string, string>("InterceptorsPreviewNamespaces", "Microsoft.Maui.Controls.Generated")]);

	static Compilation CreateCompilationFromSyntaxTrees(List<SyntaxTree> syntaxTrees)
	=> CSharpCompilation.Create("compilation",
		syntaxTrees,
		[
			MetadataReference.CreateFromFile(typeof(Microsoft.Maui.Controls.BindableObject).GetTypeInfo().Assembly.Location),
			MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
			MetadataReference.CreateFromFile(AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName("System.Runtime")).Location),
		],
		new CSharpCompilationOptions(OutputKind.ConsoleApplication)
		.WithNullableContextOptions(NullableContextOptions.Enable));


	public static Compilation CreateCompilation(string source)
	{
		return CreateCompilationFromSyntaxTrees([CSharpSyntaxTree.ParseText(source, ParseOptions, path: @"Path\To\Program.cs")]);
	}

	public static Compilation CreateCompilation(Dictionary<string, string> sources)
	{
		var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s.Value, ParseOptions, path: s.Key)).ToList();
		return CreateCompilationFromSyntaxTrees(syntaxTrees);
	}

	public static Compilation CreateXamlUnitTestCompilation()
	{
		return CSharpCompilation.Create(
			assemblyName: "Microsoft.Maui.Controls.Xaml.UnitTests",
			references:
			[
				MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
				MetadataReference.CreateFromFile(typeof(Microsoft.Maui.Controls.Xaml.UnitTests.SourceGenHelpers).GetTypeInfo().Assembly.Location),
				MetadataReference.CreateFromFile(typeof(Microsoft.Maui.Controls.Label).GetTypeInfo().Assembly.Location),
			],
			options: new CSharpCompilationOptions(OutputKind.ConsoleApplication).WithNullableContextOptions(NullableContextOptions.Enable)
		);
	}
}