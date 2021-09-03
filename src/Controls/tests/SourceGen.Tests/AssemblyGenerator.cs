using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

#nullable enable

namespace Microsoft.Maui.Controls.SourceGen.Tests
{
	public class AssemblyGenerator
	{
		readonly List<MetadataReference> references = new List<MetadataReference>();
		readonly List<SyntaxTree> sources = new List<SyntaxTree>();
		readonly Dictionary<string, string> globalOptions = new Dictionary<string, string>();
		readonly List<AdditionalText> additionalTexts = new List<AdditionalText>();

		public void AddSource(string sourceText)
		{
			var source = SourceText.From(sourceText, Encoding.UTF8);
			var tree = CSharpSyntaxTree.ParseText(source);

			this.sources.Add(tree);
		}

		public void AddMSBuildProperty(string propertyName, string value)
			=> globalOptions[$"build_property.{propertyName}"] = value;

		public void AddMSBuildItems(params ITaskItem2[] items)
			=> additionalTexts.AddRange(
				items.Select(item => new MSBuildItemGroupAdditionalText(item)));
		
		public void AddReferences(params string[] assemblies)
		{
			foreach (var assembly in assemblies)
				this.AddReference(assembly);
		}

		public void AddReference(string assemblyName)
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName);
			var asm = AppDomain
				.CurrentDomain
				.GetAssemblies()
				.FirstOrDefault(x => x.GetName()?.Name?.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase) ?? false);

			if (asm == null)
				throw new FileNotFoundException($"Assembly '{assemblyName}' not found.", path);

			var reference = MetadataReference.CreateFromFile(asm.Location);
			this.references.Add(reference);
		}

		public CSharpCompilation Create(string assemblyName) => CSharpCompilation
			.Create(assemblyName)
			.WithReferences(this.references)
			.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.AddSyntaxTrees(this.sources);

		public Compilation Generate(string dllName, ISourceGenerator sourceGenerator)
		{
			var analyzerOptionsConfigProvider = new TestAnalyzerOptionsConfigProvider(globalOptions);

			var driver = CSharpGeneratorDriver.Create(new[] { sourceGenerator }, optionsProvider: analyzerOptionsConfigProvider);
			var inputCompilation = this.Create(dllName);

			driver.AddAdditionalTexts(additionalTexts.ToImmutableArray());

			driver.RunGeneratorsAndUpdateCompilation(
				inputCompilation,
				out var outputCompilation,
				out var diags
			);
			foreach (var diag in diags)
			{
				if (diag.Severity == DiagnosticSeverity.Error)
					throw new ArgumentException(diag.GetMessage());
			}
			return outputCompilation;
		}

		class TestAnalyzerOptionsConfigProvider : AnalyzerConfigOptionsProvider
		{
			public TestAnalyzerOptionsConfigProvider(Dictionary<string, string> options)
				=> GlobalOptions = new TestAnalyzerConfigOptions(options.ToImmutableDictionary());

			public override AnalyzerConfigOptions GlobalOptions { get; }

			public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
				=> new TestAnalyzerConfigOptions();

			public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
			{
				if (textFile is MSBuildItemGroupAdditionalText msBuildItemAdditionalText)
					return new TestAnalyzerConfigOptions(msBuildItemAdditionalText.Metadata);

				return new TestAnalyzerConfigOptions();
			}

			sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
			{
				ImmutableDictionary<string, string> backing;

				public TestAnalyzerConfigOptions()
					=> backing = new Dictionary<string, string>().ToImmutableDictionary();

				public TestAnalyzerConfigOptions(ImmutableDictionary<string, string> options)
					=> backing = options;

				public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
					=> backing.TryGetValue(key, out value);
			}
		}

		class MSBuildItemGroupAdditionalText : AdditionalText
		{
			public MSBuildItemGroupAdditionalText(ITaskItem2 taskItem)
			{
				Path = taskItem.ItemSpec;

				var metadata = new Dictionary<string, string>();

				foreach (string name in taskItem.MetadataNames)
					metadata.Add($"build_metadata.AdditionalFiles.{name}", taskItem.GetMetadata(name));

				Metadata = metadata.ToImmutableDictionary();
			}

			public MSBuildItemGroupAdditionalText(string path, ImmutableDictionary<string, string> metadata)
			{
				Path = path;
				Metadata = metadata;
			}

			public override string Path { get; }

			public ImmutableDictionary<string, string> Metadata { get; }

			public override SourceText GetText(CancellationToken cancellationToken = default)
				=> SourceText.From(JsonConvert.SerializeObject(Metadata));
		}
	}
}
