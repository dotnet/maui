using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public static class SourceGeneratorDriver
{
	private static MetadataReference[]? MauiReferences;

	public record AdditionalFile(AdditionalText Text, string Kind, string RelativePath, string? TargetPath, string? ManifestResourceName, string? TargetFramework);

	public static GeneratorDriverRunResult RunGenerator<T>(Compilation compilation, params AdditionalFile[] additionalFiles)
		where T : IIncrementalGenerator, new()
	{
		ISourceGenerator generator = new T().AsSourceGenerator();

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

	public static (GeneratorDriverRunResult result1, GeneratorDriverRunResult result2) RunGeneratorWithChanges<T>(Compilation compilation,
		Func<GeneratorDriver, Compilation, (GeneratorDriver driver, Compilation compilation)> applyChanges, params AdditionalFile[] additionalFiles)
		where T : IIncrementalGenerator, new()
	{
		ISourceGenerator generator = new T().AsSourceGenerator();

		// Tell the driver to track all the incremental generator outputs
		var options = new GeneratorDriverOptions(
			disabledOutputs: IncrementalGeneratorOutputKind.None,
			trackIncrementalGeneratorSteps: true);

		GeneratorDriver driver = CSharpGeneratorDriver.Create([generator], driverOptions: options)
			.AddAdditionalTexts(additionalFiles.Select(f => f.Text).ToImmutableArray())
			.WithUpdatedAnalyzerConfigOptions(new CustomAnalyzerConfigOptionsProvider(additionalFiles));

		driver = driver.RunGenerators(compilation);
		GeneratorDriverRunResult runResult1 = driver.GetRunResult();

		var change = applyChanges(driver, compilation);
		var driver2 = change.driver.RunGenerators(change.compilation);
		GeneratorDriverRunResult runResult2 = driver2.GetRunResult();

		return (runResult1, runResult2);
	}

	public static Compilation CreateMauiCompilation()
	{
		var name = $"{nameof(SourceGeneratorDriver)}.Generated";
		var references = GetMauiReferences();

		var compilation = CSharpCompilation.Create(name,
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
			references: references);

		return compilation;
	}

	public static AdditionalText ToAdditionalText(string path, string text) => CustomAdditionalText.From(path, text);

	private static MetadataReference[] GetMauiReferences()
	{
		if (MauiReferences == null)
		{
			MauiReferences = new[]
			{
				MetadataReference.CreateFromFile(typeof(InternalsVisibleToAttribute).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(Color).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(Button).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(BindingExtension).Assembly.Location),
			};
		}
		return MauiReferences;
	}

	private class CustomAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
	{
		private readonly IImmutableDictionary<string, AdditionalFile> _additionalFiles;

		public CustomAnalyzerConfigOptionsProvider(AdditionalFile[] additionalFiles)
		{
			_additionalFiles = additionalFiles.ToImmutableDictionary(f => f.Text.Path, f => f);
		}

		public override AnalyzerConfigOptions GlobalOptions => throw new System.NotImplementedException();

		public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
		{
			throw new System.NotImplementedException();
		}

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

	private class CustomAdditionalText : AdditionalText
	{
		private readonly SourceText _sourceText;

		public static AdditionalText From(string path, string content)
		{
			return new CustomAdditionalText(path, content);
		}

		private CustomAdditionalText(string path, string content)
		{
			Path = path;
			_sourceText = SourceText.From(content);
		}

		public override string Path { get; }

		public override SourceText GetText(CancellationToken cancellationToken = default)
		{
			return _sourceText;
		}
	}
}
