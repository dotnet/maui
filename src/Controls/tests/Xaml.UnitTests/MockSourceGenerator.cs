using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

#nullable enable

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class MockSourceGenerator
{
	public static Compilation CreateMauiCompilation(string name = $"{nameof(MockSourceGenerator)}.Generated" )
	{
		var references = GetMauiReferences();

		var compilation = CSharpCompilation.Create(name,
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
			references: references);

		return compilation;
	}

    public record AdditionalFile(AdditionalText Text, string Kind, string RelativePath, string? TargetPath, string? ManifestResourceName, string? TargetFramework);
    public record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null)
        : AdditionalFile(Text: ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework);

	public static AdditionalText ToAdditionalText(string path, string text) => CustomAdditionalText.From(path, text);

    public static GeneratorDriverRunResult RunMauiSourceGenerator(this Compilation compilation, Type xamlType, [CallerFilePath] string? codeFile = null)
    {
        var resourceId = XamlResourceIdAttribute.GetResourceIdForType(xamlType);
        var resourcePath = XamlResourceIdAttribute.GetPathForType(xamlType);
        var resourceStream = typeof(MockSourceGenerator).Assembly.GetManifestResourceStream(resourceId);
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(File.ReadAllText(codeFile!)));

        return RunMauiSourceGenerator(compilation, new AdditionalXamlFile(resourcePath, new StreamReader(resourceStream!).ReadToEnd()));
    }
    public static GeneratorDriverRunResult RunMauiSourceGenerator(this Compilation compilation, params AdditionalFile[] additionalFiles)
	{
        var current = System.IO.Directory.GetCurrentDirectory();
        var relative = "../../../../Controls.SourceGen/Debug/netstandard2.0/Microsoft.Maui.Controls.SourceGen.dll";
        var path = Path.GetFullPath(current + relative);
        var analyzerAssembly = Assembly.LoadFile(path);
        var generatorType = analyzerAssembly?.GetType("Microsoft.Maui.Controls.SourceGen.CodeBehindGenerator")!;
        var generator = (generatorType?.GetConstructor([])?.Invoke([]) as IIncrementalGenerator)?.AsSourceGenerator();

        if (generator is null)
            throw new InvalidOperationException("Failed to create generator instance");
        
		// Tell the driver to track all the incremental generator outputs
		var options = new GeneratorDriverOptions(
			disabledOutputs: IncrementalGeneratorOutputKind.None,
			trackIncrementalGeneratorSteps: true);

		GeneratorDriver driver = CSharpGeneratorDriver.Create([generator], driverOptions: options)
			.AddAdditionalTexts(additionalFiles.Select(f => f.Text).ToImmutableArray())
			.WithUpdatedAnalyzerConfigOptions(new CustomAnalyzerConfigOptionsProvider(additionalFiles));

		driver = driver.RunGenerators(compilation);

		GeneratorDriverRunResult runResult = driver.GetRunResult();
		return runResult;
	}

    public static string GeneratedCodeBehind(this GeneratorDriverRunResult result)
        =>	result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".sg.cs")).SourceText.ToString();

    public static string GeneratedInitializeComponent(this GeneratorDriverRunResult result)
        =>	result.Results.Single().GeneratedSources.Single(gs => gs.HintName.EndsWith(".xsg.cs")).SourceText.ToString();

    static MetadataReference[]? MauiReferences;
    static MetadataReference[] GetMauiReferences()
	{
		string dotNetAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

		MauiReferences ??=
			[
				MetadataReference.CreateFromFile(typeof(InternalsVisibleToAttribute).Assembly.Location),
				// .NET assemblies are finicky and need to be loaded in a special way.
				MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "mscorlib.dll")),
				MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.dll")),
				MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.Core.dll")),
				MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.Private.CoreLib.dll")),
				MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.Runtime.dll")),
				MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.ObjectModel.dll")),
				MetadataReference.CreateFromFile(typeof(Color).Assembly.Location),						//System.Drawings
                MetadataReference.CreateFromFile(typeof(Microsoft.Maui.Converters.ThicknessTypeConverter).Assembly.Location),			//Core
                MetadataReference.CreateFromFile(typeof(Microsoft.Maui.Graphics.Converters.RectTypeConverter).Assembly.Location),			//Graphics
				MetadataReference.CreateFromFile(typeof(Button).Assembly.Location),						//Controls
                MetadataReference.CreateFromFile(typeof(BindingExtension).Assembly.Location),			//Xaml

                
			];
		return MauiReferences;
	}

    class CustomAdditionalText : AdditionalText
	{
		private readonly SourceText _sourceText;

		public static AdditionalText From(string path, string content) => new CustomAdditionalText(path, content);

		private CustomAdditionalText(string path, string content)
		{
			Path = path;
			_sourceText = SourceText.From(content);
		}

		public override string Path { get; }

		public override SourceText GetText(CancellationToken cancellationToken = default) => _sourceText;
	}

    class CustomAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
	{
		private readonly IImmutableDictionary<string, AdditionalFile> _additionalFiles;

		public CustomAnalyzerConfigOptionsProvider(AdditionalFile[] additionalFiles) => _additionalFiles = additionalFiles.ToImmutableDictionary(f => f.Text.Path, f => f);

		public override AnalyzerConfigOptions GlobalOptions => throw new System.NotImplementedException();

		public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => throw new System.NotImplementedException();

		public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
		{
			return _additionalFiles.TryGetValue(textFile.Path, out var additionalFile)
				? (AnalyzerConfigOptions)new CustomAnalyzerConfigOptions(additionalFile)
				: CustomAnalyzerConfigOptions.Empty;
		}

		private class CustomAnalyzerConfigOptions : AnalyzerConfigOptions
		{
			readonly AdditionalFile? _additionalFile;

			public CustomAnalyzerConfigOptions(AdditionalFile? additionalFile)
			{
				_additionalFile = additionalFile;
			}

			public static AnalyzerConfigOptions Empty { get; } = new CustomAnalyzerConfigOptions(null);

			public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
			{
				if (_additionalFile == null)
				{
					value = null;
					return false;
				}

				value = key switch
				{
					"build_metadata.additionalfiles.GenKind" => _additionalFile.Kind,
					"build_metadata.additionalfiles.TargetPath" => _additionalFile.TargetPath,
					"build_metadata.additionalfiles.ManifestResourceName" => _additionalFile.ManifestResourceName,
					"build_metadata.additionalfiles.RelativePath" => _additionalFile.RelativePath,
					"build_property.targetframework" => _additionalFile.TargetFramework,
					_ => null
				};

				return value is not null;
			}
		}
	}
}
