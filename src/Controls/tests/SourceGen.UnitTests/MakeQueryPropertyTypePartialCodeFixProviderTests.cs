using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen.CodeFixes;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class MakeQueryPropertyTypePartialCodeFixProviderTests
{
	const string Usings = """
		using Microsoft.Maui.Controls;

		""";

	[Fact]
	public async Task MakesTypePartial()
	{
		await VerifyFix(
			"""
			[QueryProperty(nameof(Value), "value")]
			public sealed class Model
			{
				public string Value { get; set; }
			}
			""",
			"""
			[QueryProperty(nameof(Value), "value")]
			public sealed partial class Model
			{
				public string Value { get; set; }
			}
			""");
	}

	[Fact]
	public async Task MakesRecordPartial()
	{
		await VerifyFix(
			"""
			[QueryProperty(nameof(Value), "value")]
			record Model
			{
				public string Value { get; set; }
			}
			""",
			"""
			[QueryProperty(nameof(Value), "value")]
			partial record Model
			{
				public string Value { get; set; }
			}
			""");
	}

	[Fact]
	public async Task MakesContainingTypesPartial()
	{
		await VerifyFix(
			"""
			public class Outer
			{
				public partial class Middle
				{
					[QueryProperty(nameof(Value), "value")]
					public class Model
					{
						public string Value { get; set; }
					}
				}
			}
			""",
			"""
			public partial class Outer
			{
				public partial class Middle
				{
					[QueryProperty(nameof(Value), "value")]
					public partial class Model
					{
						public string Value { get; set; }
					}
				}
			}
			""");
	}

	[Fact]
	public async Task MakesDerivedTypePartial()
	{
		await VerifyFix(
			"""
			[QueryProperty(nameof(Value), "value")]
			public partial class Base
			{
				public string Value { get; set; }
			}

			public class Derived : Base
			{
			}
			""",
			"""
			[QueryProperty(nameof(Value), "value")]
			public partial class Base
			{
				public string Value { get; set; }
			}

			public partial class Derived : Base
			{
			}
			""");
	}

	[Fact]
	public async Task FixAllMakesSharedContainingTypePartialOnce()
	{
		await VerifyFix(
			"""
			public class Outer
			{
				[QueryProperty(nameof(Value), "value")]
				public class First
				{
					public string Value { get; set; }
				}

				[QueryProperty(nameof(Value), "value")]
				public class Second
				{
					public string Value { get; set; }
				}
			}
			""",
			"""
			public partial class Outer
			{
				[QueryProperty(nameof(Value), "value")]
				public partial class First
				{
					public string Value { get; set; }
				}

				[QueryProperty(nameof(Value), "value")]
				public partial class Second
				{
					public string Value { get; set; }
				}
			}
			""",
			fixAll: true);
	}

	[Fact]
	public async Task DoesNotOfferFixForOtherUnsupportedReasons()
	{
		var (document, diagnostics) = await GetDiagnostics(
			"""
			[QueryProperty(nameof(Value), "value")]
			public partial class Model
			{
				public string Value { get; init; }
			}
			""");

		var diagnostic = Assert.Single(diagnostics);
		Assert.Equal("MAUI1200", diagnostic.Id);
		Assert.Empty(await GetCodeActions(document, diagnostic));
	}

	static async Task VerifyFix(string source, string expected, bool fixAll = false)
	{
		var (document, diagnostics) = await GetDiagnostics(source);
		Assert.NotEmpty(diagnostics);
		Assert.All(diagnostics, d => Assert.Equal("MAUI1200", d.Id));

		Document fixedDocument;
		if (fixAll)
		{
			Assert.True(diagnostics.Length > 1);
			var provider = new MakeQueryPropertyTypePartialCodeFixProvider();
			var context = new FixAllContext(
				document,
				provider,
				FixAllScope.Document,
				nameof(MakeQueryPropertyTypePartialCodeFixProvider),
				provider.FixableDiagnosticIds,
				new FixedDiagnosticProvider(diagnostics),
				CancellationToken.None);
			var action = await provider.GetFixAllProvider().GetFixAsync(context);
			fixedDocument = await ApplyAsync(document, action!);
		}
		else
		{
			var action = Assert.Single(await GetCodeActions(document, Assert.Single(diagnostics)));
			Assert.Equal("Make type partial", action.Title);
			fixedDocument = await ApplyAsync(document, action);
		}

		Assert.Equal(Usings + expected, (await fixedDocument.GetTextAsync()).ToString());

		var (_, remaining) = await GetDiagnostics(fixedDocument);
		Assert.Empty(remaining);
	}

	static async Task<(Document Document, ImmutableArray<Diagnostic> Diagnostics)> GetDiagnostics(string source)
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation($"QueryPartialFixTests{Guid.NewGuid():N}");
		var project = new AdhocWorkspace()
			.AddProject(compilation.AssemblyName!, LanguageNames.CSharp)
			.WithCompilationOptions(compilation.Options)
			.WithParseOptions(new CSharpParseOptions(LanguageVersion.Preview))
			.WithMetadataReferences(compilation.References);
		return await GetDiagnostics(project.AddDocument("Model.cs", Usings + source));
	}

	static async Task<(Document Document, ImmutableArray<Diagnostic> Diagnostics)> GetDiagnostics(Document document)
	{
		var compilation = await document.Project.GetCompilationAsync();
		GeneratorDriver driver = CSharpGeneratorDriver.Create([new QueryPropertyGenerator().AsSourceGenerator()]);
		var diagnostics = driver.RunGenerators(compilation!).GetRunResult().Diagnostics;
		return (document, diagnostics);
	}

	static async Task<List<CodeAction>> GetCodeActions(Document document, Diagnostic diagnostic)
	{
		var actions = new List<CodeAction>();
		var context = new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None);
		await new MakeQueryPropertyTypePartialCodeFixProvider().RegisterCodeFixesAsync(context);
		return actions;
	}

	static async Task<Document> ApplyAsync(Document document, CodeAction action)
	{
		var operation = Assert.Single((await action.GetOperationsAsync(CancellationToken.None)).OfType<ApplyChangesOperation>());
		return operation.ChangedSolution.GetDocument(document.Id)!;
	}

	sealed class FixedDiagnosticProvider(ImmutableArray<Diagnostic> diagnostics) : FixAllContext.DiagnosticProvider
	{
		public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken) => Task.FromResult<IEnumerable<Diagnostic>>(diagnostics);
		public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken) => Task.FromResult<IEnumerable<Diagnostic>>(diagnostics);
		public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken) => Task.FromResult<IEnumerable<Diagnostic>>([]);
	}
}
