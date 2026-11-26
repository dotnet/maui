using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class QueryPropertyConflictAnalyzerTests
{
	const string Usings = """
		using System;
		using System.Collections.Generic;
		using Microsoft.Maui.Controls;

		""";

	[Theory]
	[InlineData("public void ApplyQueryAttributes(IDictionary<string, object> query) { }")]
	[InlineData("void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query) { }")]
	public async Task WarnsForImplicitAndExplicitImplementations(string implementation)
	{
		var (result, compilation) = RunGenerator(CreateCompilation($$"""
			[QueryProperty(nameof(Value), "value")]
			public partial class Model : IQueryAttributable
			{
				public string Value { get; set; }
				{{implementation}}
				public void ApplyQueryProperties(IDictionary<string, object> query, IDictionary<string, object> previousQuery) { }
			}
			"""));

		Assert.Empty(result.GeneratedTrees);
		var diagnostic = Assert.Single(await GetDiagnostics(compilation));
		Assert.Equal("MAUI1201", diagnostic.Id);
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
		Assert.Equal("'Model' implements IQueryAttributable and uses [QueryProperty] attributes. The source generator skips this type; handle the properties manually or remove the interface implementation.", diagnostic.GetMessage());
		Assert.Equal("Model", diagnostic.Location.SourceTree!.GetText(cancellationToken: TestContext.Current.CancellationToken).ToString(diagnostic.Location.SourceSpan));
	}

	[Theory]
	[InlineData(true, false)]
	[InlineData(false, true)]
	[InlineData(false, false)]
	public async Task DoesNotWarnWithoutBothMechanisms(bool hasAttribute, bool hasInterface)
	{
		var (result, compilation) = RunGenerator(CreateCompilation($$"""
			{{(hasAttribute ? "[QueryProperty(nameof(Value), \"value\")]" : "")}}
			public partial class Model {{(hasInterface ? ": IQueryAttributable" : "")}}
			{
				public string Value { get; set; }
				public void ApplyQueryAttributes(IDictionary<string, object> query) { }
				public void ApplyQueryProperties(IDictionary<string, object> query, IDictionary<string, object> previousQuery) { }
			}
			"""));

		if (!hasAttribute)
		{
			Assert.Empty(result.GeneratedTrees);
			Assert.Empty(result.Diagnostics);
		}
		Assert.Empty(await GetDiagnostics(compilation));
	}

	[Fact]
	public async Task DoesNotWarnForGeneratedImplementation()
	{
		var (result, compilation) = RunGenerator(CreateCompilation("""
			[QueryProperty(nameof(Value), "value")]
			public partial class Model
			{
				public string Value { get; set; }
			}
			"""));

		Assert.Single(result.GeneratedTrees);
		Assert.Empty(await GetDiagnostics(compilation));
	}

	[Theory]
	[InlineData(true, false)]
	[InlineData(false, true)]
	[InlineData(true, true)]
	public async Task WarnsForInheritedMechanisms(bool inheritedAttribute, bool inheritedInterface)
	{
		var diagnostics = await Analyze($$"""
			{{(inheritedAttribute ? "[QueryProperty(nameof(Value), \"value\")]" : "")}}
			public class BaseModel {{(inheritedInterface ? ": IQueryAttributable" : "")}}
			{
				public string Value { get; set; }
				public void ApplyQueryAttributes(IDictionary<string, object> query) { }
				public void ApplyQueryProperties(IDictionary<string, object> query, IDictionary<string, object> previousQuery) { }
			}

			{{(!inheritedAttribute ? "[QueryProperty(nameof(Value), \"value\")]" : "")}}
			public class Model : BaseModel {{(!inheritedInterface ? ", IQueryAttributable" : "")}} { }
			""");

		var names = diagnostics.Select(d => d.Location.SourceTree!.GetText().ToString(d.Location.SourceSpan)).OrderBy(n => n).ToArray();
		Assert.Equal(inheritedAttribute && inheritedInterface ? new[] { "BaseModel", "Model" } : new[] { "Model" }, names);
		Assert.All(diagnostics, d => Assert.Equal("MAUI1201", d.Id));
	}

	[Fact]
	public async Task ReportsOnceForPartialClassWithMultipleAttributes()
	{
		var diagnostics = await Analyze("""
			[QueryProperty(nameof(Value), "value")]
			public partial class Model
			{
				public string Value { get; set; }
			}

			[QueryProperty(nameof(Value), "other")]
			public partial class Model : IQueryAttributable
			{
				public void ApplyQueryAttributes(IDictionary<string, object> query) { }
				public void ApplyQueryProperties(IDictionary<string, object> query, IDictionary<string, object> previousQuery) { }
			}
			""");

		Assert.Single(diagnostics);
	}

	[Theory]
	[InlineData(true, false, 1)]
	[InlineData(false, false, 0)]
	[InlineData(true, true, 1)]
	[InlineData(false, true, 0)]
	public async Task RespectsInheritanceOfDerivedAttributes(bool inherited, bool indirectAttribute, int expectedCount)
	{
		var diagnostics = await Analyze($$"""
			[AttributeUsage(AttributeTargets.Class, Inherited = {{(inherited ? "true" : "false")}})]
			public class CustomQueryAttribute : QueryPropertyAttribute
			{
				public CustomQueryAttribute() : base("Value", "value") { }
			}
			public class DerivedQueryAttribute : CustomQueryAttribute { }

			[{{(indirectAttribute ? "DerivedQuery" : "CustomQuery")}}]
			public class BaseModel
			{
				public string Value { get; set; }
			}

			public class Model : BaseModel, IQueryAttributable
			{
				public void ApplyQueryAttributes(IDictionary<string, object> query) { }
				public void ApplyQueryProperties(IDictionary<string, object> query, IDictionary<string, object> previousQuery) { }
			}
			""");

		Assert.Equal(expectedCount, diagnostics.Length);
	}

	[Theory]
	[InlineData("""
		public interface IQueryAttributable { }
		[Microsoft.Maui.Controls.QueryProperty("Value", "value")]
		public class Model : IQueryAttributable { }
		""")]
	[InlineData("""
		public class QueryPropertyAttribute : Attribute
		{
			public QueryPropertyAttribute(string name, string queryId) { }
		}
		[QueryProperty("Value", "value")]
		public class Model : Microsoft.Maui.Controls.IQueryAttributable
		{
			public void ApplyQueryAttributes(IDictionary<string, object> query) { }
			public void ApplyQueryProperties(IDictionary<string, object> query, IDictionary<string, object> previousQuery) { }
		}
		""")]
	public async Task IgnoresUnrelatedTypesWithSameNames(string source)
	{
		Assert.Empty(await Analyze(source));
	}

	[Theory]
	[InlineData("[QueryProperty(nameof(Value), \"value\")]", "", "public class Model : BaseModel, IQueryAttributable { }")]
	[InlineData("", ": IQueryAttributable", "[QueryProperty(nameof(Value), \"value\")] public class Model : BaseModel { }")]
	[InlineData("[QueryProperty(nameof(Value), \"value\")]", ": IQueryAttributable", "public class Model : BaseModel { }")]
	public async Task WarnsForMechanismsInheritedFromMetadata(string baseAttribute, string baseInterface, string source)
	{
		var baseCompilation = CreateCompilation($$"""
			{{baseAttribute}}
			public class BaseModel {{baseInterface}}
			{
				public string Value { get; set; }
				public void ApplyQueryAttributes(IDictionary<string, object> query) { }
				public void ApplyQueryProperties(IDictionary<string, object> query, IDictionary<string, object> previousQuery) { }
			}
			""");

		var diagnostics = await Analyze(source, EmitReference(baseCompilation));

		Assert.Single(diagnostics);
	}

	[Fact]
	public async Task WarnsForExplicitImplementationInheritedFromMetadata()
	{
		var baseCompilation = CreateCompilation("""
			public class BaseModel : IQueryAttributable
			{
				void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query) { }
				void IQueryAttributable.ApplyQueryProperties(IDictionary<string, object> query, IDictionary<string, object> previousQuery) { }
			}
			""");

		var diagnostics = await Analyze("""
			[QueryProperty(nameof(Value), "value")]
			public class Model : BaseModel
			{
				public string Value { get; set; }
			}
			""", EmitReference(baseCompilation));

		Assert.Single(diagnostics);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task DoesNotWarnForGeneratedImplementationInheritedFromMetadata(bool isPartial)
	{
		var (result, baseCompilation) = RunGenerator(CreateCompilation("""
			[QueryProperty(nameof(Value), "value")]
			public partial class BaseModel
			{
				public string Value { get; set; }
			}
			"""));

		Assert.Single(result.GeneratedTrees);
		var (derivedResult, compilation) = RunGenerator(CreateCompilation($$"""
			[QueryProperty(nameof(Other), "other")]
			public {{(isPartial ? "partial " : "")}}class Model : BaseModel
			{
				public string Other { get; set; }
			}
			""").AddReferences(EmitReference(baseCompilation)));

		Assert.Equal(isPartial ? 1 : 0, derivedResult.GeneratedTrees.Length);
		Assert.Empty(await GetDiagnostics(compilation));
	}

	[Fact]
	public async Task DoesNotWarnForGeneratedImplementationInheritedFromSource()
	{
		var (result, compilation) = RunGenerator(CreateCompilation("""
			[QueryProperty(nameof(Value), "value")]
			public partial class BaseModel
			{
				public string Value { get; set; }
			}

			[QueryProperty(nameof(Other), "other")]
			public class Model : BaseModel
			{
				public string Other { get; set; }
			}
			"""));

		Assert.Single(result.GeneratedTrees);
		Assert.Empty(await GetDiagnostics(compilation));
	}

	[Fact]
	public async Task WarnsWhenUserImplementationReplacesGeneratedBase()
	{
		var (result, compilation) = RunGenerator(CreateCompilation("""
			[QueryProperty(nameof(Value), "value")]
			public partial class BaseModel
			{
				public string Value { get; set; }
			}

			[QueryProperty(nameof(Other), "other")]
			public partial class Model : BaseModel, IQueryAttributable
			{
				public string Other { get; set; }
				void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query) { }
				void IQueryAttributable.ApplyQueryProperties(IDictionary<string, object> query, IDictionary<string, object> previousQuery) { }
			}
			"""));

		Assert.Single(result.GeneratedTrees);
		var diagnostic = Assert.Single(await GetDiagnostics(compilation));
		Assert.Equal("Model", diagnostic.Location.SourceTree!.GetText(cancellationToken: TestContext.Current.CancellationToken).ToString(diagnostic.Location.SourceSpan));
	}

	[Fact]
	public async Task WarnsForInheritedUserImplementationWhenGeneratorRuns()
	{
		var (result, compilation) = RunGenerator(CreateCompilation("""
			public class BaseModel : IQueryAttributable
			{
				public void ApplyQueryAttributes(IDictionary<string, object> query) { }
				public void ApplyQueryProperties(IDictionary<string, object> query, IDictionary<string, object> previousQuery) { }
			}

			[QueryProperty(nameof(Value), "value")]
			public partial class Model : BaseModel
			{
				public string Value { get; set; }
			}
			"""));

		Assert.Empty(result.GeneratedTrees);
		var diagnostic = Assert.Single(await GetDiagnostics(compilation));
		Assert.Equal("Model", diagnostic.Location.SourceTree!.GetText(cancellationToken: TestContext.Current.CancellationToken).ToString(diagnostic.Location.SourceSpan));
	}

	[Fact]
	public async Task WarnsForDerivedInterfaceAndAttributeWhenGeneratorRuns()
	{
		var (result, compilation) = RunGenerator(CreateCompilation("""
			public interface IQueryHandler : IQueryAttributable { }
			public class CustomQueryAttribute : QueryPropertyAttribute
			{
				public CustomQueryAttribute() : base("Value", "value") { }
			}

			[CustomQuery]
			public partial class Model : IQueryHandler
			{
				public string Value { get; set; }
				public void ApplyQueryAttributes(IDictionary<string, object> query) { }
				public void ApplyQueryProperties(IDictionary<string, object> query, IDictionary<string, object> previousQuery) { }
			}
			"""));

		Assert.Empty(result.GeneratedTrees);
		Assert.Single(await GetDiagnostics(compilation));
	}

	[Fact]
	public async Task WarnsForImplementationsMarkedAsGeneratedByOtherTools()
	{
		var diagnostics = await Analyze("""
			[QueryProperty(nameof(Value), "value")]
			public class Model : IQueryAttributable
			{
				public string Value { get; set; }
				[System.CodeDom.Compiler.GeneratedCode("OtherTool", "1.0")]
				public void ApplyQueryAttributes(IDictionary<string, object> query) { }
				public void ApplyQueryProperties(IDictionary<string, object> query, IDictionary<string, object> previousQuery) { }
			}
			""");

		Assert.Single(diagnostics);
	}

	[Fact]
	public async Task IgnoresGeneratedClasses()
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation()
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(Usings + """
				[QueryProperty("Value", "value")]
				public class Model : IQueryAttributable
				{
					public string Value { get; set; }
					public void ApplyQueryAttributes(IDictionary<string, object> query) { }
					public void ApplyQueryProperties(IDictionary<string, object> query, IDictionary<string, object> previousQuery) { }
				}
				""", path: "Model.g.cs", cancellationToken: TestContext.Current.CancellationToken));

		Assert.Empty(await GetDiagnostics(compilation));
	}

	static Compilation CreateCompilation(string source)
		=> SourceGeneratorDriver.CreateMauiCompilation($"QueryConflictTests{Guid.NewGuid():N}")
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(Usings + source));

	static (GeneratorDriverRunResult Result, Compilation Compilation) RunGenerator(Compilation compilation)
	{
		GeneratorDriver driver = CSharpGeneratorDriver.Create([new QueryPropertyGenerator().AsSourceGenerator()]);
		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out _);
		return (driver.GetRunResult(), updatedCompilation);
	}

	static MetadataReference EmitReference(Compilation compilation)
	{
		using var stream = new MemoryStream();
		var result = compilation.Emit(stream);
		Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
		return MetadataReference.CreateFromImage(stream.ToArray());
	}

	static Task<ImmutableArray<Diagnostic>> Analyze(string source, params MetadataReference[] references)
		=> GetDiagnostics(CreateCompilation(source).AddReferences(references));

	static async Task<ImmutableArray<Diagnostic>> GetDiagnostics(Compilation compilation)
	{
		Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		return await compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new QueryPropertyConflictAnalyzer()))
			.GetAnalyzerDiagnosticsAsync();
	}
}
