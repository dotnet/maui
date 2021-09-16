using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

#nullable enable

namespace Microsoft.Maui.TestUtils.SourceGen
{
	public partial class AssemblyGenerator
	{
		readonly List<MetadataReference> references = new();
		readonly List<SyntaxTree> sources = new();
		readonly Dictionary<string, string> globalOptions = new();
		readonly List<AdditionalText> additionalTexts = new();

		public void AddSource(string sourceText)
		{
			var source = SourceText.From(sourceText, Encoding.UTF8);
			var tree = CSharpSyntaxTree.ParseText(source);

			sources.Add(tree);
		}

		public void AddMSBuildProperty(string propertyName, string value)
			=> globalOptions[$"build_property.{propertyName}"] = value;

		public void AddMSBuildItems(params ITaskItem2[] items)
			=> additionalTexts.AddRange(
				items.Select(item => new MSBuildItemGroupAdditionalText(item)));
		
		public void AddReferences(params string[] assemblies)
		{
			foreach (var assembly in assemblies)
				AddReference(assembly);
		}

		public void AddReference(string assemblyName)
		{
			if (File.Exists(assemblyName))
			{
				references.Add(MetadataReference.CreateFromFile(assemblyName));
				return;
			}

			var assemblyNameWithoutDllExt = Path.GetFileNameWithoutExtension(assemblyName);

			if (assemblyNameWithoutDllExt != null)
			{
				var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyNameWithoutDllExt + ".dll");
				if (File.Exists(path))
				{
					references.Add(MetadataReference.CreateFromFile(path));
					return;
				}
			}

			var asm = AppDomain
				.CurrentDomain
				.GetAssemblies()
				.FirstOrDefault(x => x.GetName()?.Name?.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase) ?? false);

			if (asm == null)
				throw new DllNotFoundException($"Assembly '{assemblyName}' not found.");

			var reference = MetadataReference.CreateFromFile(asm.Location);
			references.Add(reference);
		}

		public CSharpCompilation Create(string assemblyName) => CSharpCompilation
			.Create(assemblyName)
			.WithReferences(references)
			.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.AddSyntaxTrees(sources);

		public Compilation Generate(string dllName, ISourceGenerator sourceGenerator)
		{
			var analyzerOptionsConfigProvider = new TestAnalyzerOptionsConfigProvider(globalOptions);

			GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { sourceGenerator }, optionsProvider: analyzerOptionsConfigProvider);

			driver = driver.AddAdditionalTexts(additionalTexts.ToImmutableArray());

			var inputCompilation = this.Create(dllName);

			driver = driver.RunGeneratorsAndUpdateCompilation(
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
	}
}
