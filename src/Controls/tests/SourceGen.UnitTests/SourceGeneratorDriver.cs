using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime;
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

	public record AdditionalFile(AdditionalText Text, string Kind, string RelativePath, string? TargetPath, string? ManifestResourceName, string? TargetFramework, string? NoWarn, string LineInfo="enable");

	public static GeneratorDriverRunResult RunGenerator<T>(Compilation compilation, params AdditionalFile[] additionalFiles)
		where T : IIncrementalGenerator, new()
	{
		return RunGenerator<T>(compilation, assertNoCompilationErrors: false, additionalFiles);
	}

	public static GeneratorDriverRunResult RunGenerator<T>(Compilation compilation, bool assertNoCompilationErrors, params AdditionalFile[] additionalFiles)
		where T : IIncrementalGenerator, new()
	{
		ISourceGenerator generator = new T().AsSourceGenerator();

		// Tell the driver to track all the incremental generator outputs
		var options = new GeneratorDriverOptions(
			disabledOutputs: IncrementalGeneratorOutputKind.None,
			trackIncrementalGeneratorSteps: true);

		GeneratorDriver driver = CSharpGeneratorDriver.Create([generator], driverOptions: options)
			.AddAdditionalTexts([.. additionalFiles.Select(f => f.Text)])
			.WithUpdatedAnalyzerConfigOptions(new CustomAnalyzerConfigOptionsProvider(additionalFiles));

		// Use RunGeneratorsAndUpdateCompilation to validate the generated code
		// This will catch C# compilation errors in the generated code
		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var diagnostics);

		// Assert that the generated code compiles without errors (if requested)
		if (assertNoCompilationErrors)
		{
			var compilationErrors = updatedCompilation.GetDiagnostics()
				.Where(d => d.Severity == DiagnosticSeverity.Error)
				.Where(d => d.Location.SourceTree?.FilePath?.EndsWith(".xsg.cs", StringComparison.OrdinalIgnoreCase) == true)
				.ToArray();

			if (compilationErrors.Length > 0)
			{
				var errorMessages = string.Join(Environment.NewLine, compilationErrors.Select(e => $"{e.Id}: {e.GetMessage()} at {e.Location}"));
				throw new InvalidOperationException($"Generated code has compilation errors:{Environment.NewLine}{errorMessages}");
			}
		}

		var runResult = driver.GetRunResult();
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

	public static Compilation CreateMauiCompilation(string name = $"{nameof(SourceGeneratorDriver)}.Generated")
	{
		var references = GetMauiReferences();

		var compilation = CSharpCompilation.Create(name,
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
			references: references);

		return compilation;
	}

	public static AdditionalText ToAdditionalText(string path, string text) => CustomAdditionalText.From(path, text);

	private static MetadataReference[] GetMauiReferences()
	{
		string dotNetAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

		if (MauiReferences == null)
		{
			MauiReferences = new[]
			{
				MetadataReference.CreateFromFile(typeof(InternalsVisibleToAttribute).Assembly.Location),
				// .NET assemblies are finicky and need to be loaded in a special way.
				MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "mscorlib.dll")),
				MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.dll")),
				MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.Core.dll")),
				MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.Private.CoreLib.dll")),
				MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.Runtime.dll")),
				MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.ObjectModel.dll")),
				MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location),						//System.Private.Uri
				MetadataReference.CreateFromFile(typeof(Color).Assembly.Location),						//Graphics
				MetadataReference.CreateFromFile(typeof(Button).Assembly.Location),						//Controls
				MetadataReference.CreateFromFile(typeof(BindingExtension).Assembly.Location),			//Xaml
				MetadataReference.CreateFromFile(typeof(Thickness).Assembly.Location),					//Core
				MetadataReference.CreateFromFile(typeof(Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebView).Assembly.Location), //Xaml.dll
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
					"build_metadata.additionalfiles.Inflator" => "SourceGen",
					"build_property.targetFramework" => _additionalFile.TargetFramework,
#if RELEASE
					"build_property.Configuration" => "Release",
#else
					"build_property.Configuration" => "Debug",
#endif
					"build_property.EnableMauiXamlDiagnostics" => "true",
					"build_property.MauiXamlLineInfo" => _additionalFile.LineInfo != "default" ?  _additionalFile.LineInfo : null,
					"build_property.MauiXamlNoWarn" => _additionalFile.NoWarn,

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
