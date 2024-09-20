using System;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Xaml;
using System.Runtime.Loader;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System.CodeDom.Compiler;

using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

static class SourceGenHelpers
{
	private static readonly CSharpParseOptions ParseOptions = new CSharpParseOptions(LanguageVersion.Preview).WithFeatures(
				[new KeyValuePair<string, string>("InterceptorsPreviewNamespaces", "Microsoft.Maui.Controls.Generated")]);

	private static Compilation CreateCompilationFromSyntaxTrees(List<SyntaxTree> syntaxTrees)
	=> CSharpCompilation.Create("compilation",
		syntaxTrees,
		[
			MetadataReference.CreateFromFile(typeof(Microsoft.Maui.Controls.BindableObject).GetTypeInfo().Assembly.Location),
			MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
			MetadataReference.CreateFromFile(AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName("System.Runtime")).Location),
		],
		new CSharpCompilationOptions(OutputKind.ConsoleApplication)
		.WithNullableContextOptions(NullableContextOptions.Enable));


	internal static Compilation CreateCompilation(string source)
	{
		return CreateCompilationFromSyntaxTrees([CSharpSyntaxTree.ParseText(source, ParseOptions, path: @"Path\To\Program.cs")]);
	}

	internal static Compilation CreateCompilation(Dictionary<string, string> sources)
	{
		var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s.Value, ParseOptions, path: s.Key)).ToList();
		return CreateCompilationFromSyntaxTrees(syntaxTrees);
	}
}