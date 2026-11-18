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
		var diagnostics = await Analyze($$"""
			[QueryProperty(nameof(Value), "value")]
			public class Model : IQueryAttributable
			{
				public string Value { get; set; }
				{{implementation}}
			}
			""");

		var diagnostic = Assert.Single(diagnostics);
		Assert.Equal("MAUI1201", diagnostic.Id);
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
		Assert.Equal("'Model' implements IQueryAttributable and uses [QueryProperty] attributes. Remove either the IQueryAttributable implementation or the [QueryProperty] attributes.", diagnostic.GetMessage());
		Assert.Equal("Model", diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan));
	}

	[Theory]
	[InlineData(true, false)]
	[InlineData(false, true)]
	[InlineData(false, false)]
	public async Task DoesNotWarnWithoutBothMechanisms(bool hasAttribute, bool hasInterface)
	{
		var diagnostics = await Analyze($$"""
			{{(hasAttribute ? "[QueryProperty(nameof(Value), \"value\")]" : "")}}
			public class Model {{(hasInterface ? ": IQueryAttributable" : "")}}
			{
				public string Value { get; set; }
				public void ApplyQueryAttributes(IDictionary<string, object> query) { }
			}
			""");

		Assert.Empty(diagnostics);
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
			}

			{{(!inheritedAttribute ? "[QueryProperty(nameof(Value), \"value\")]" : "")}}
			public class Model : BaseModel {{(!inheritedInterface ? ", IQueryAttributable" : "")}} { }
			""");

		var names = diagnostics.Select(d => d.Location.SourceTree!.GetText().ToString(d.Location.SourceSpan)).OrderBy(n => n).ToArray();
		// When both mechanisms come from the base class, only the base class is reported.
		Assert.Equal(inheritedAttribute && inheritedInterface ? new[] { "BaseModel" } : new[] { "Model" }, names);
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
			}
			""");

		Assert.Equal(expectedCount, diagnostics.Length);
	}

	[Fact]
	public async Task WarnsForDerivedInterfaceAndAttribute()
	{
		var diagnostics = await Analyze("""
			public interface IQueryHandler : IQueryAttributable { }
			public class CustomQueryAttribute : QueryPropertyAttribute
			{
				public CustomQueryAttribute() : base("Value", "value") { }
			}

			[CustomQuery]
			public class Model : IQueryHandler
			{
				public string Value { get; set; }
				public void ApplyQueryAttributes(IDictionary<string, object> query) { }
			}
			""");

		Assert.Single(diagnostics);
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
		}
		""")]
	public async Task IgnoresUnrelatedTypesWithSameNames(string source)
	{
		Assert.Empty(await Analyze(source));
	}

	[Theory]
	[InlineData("[QueryProperty(nameof(Value), \"value\")]", "", "public class Model : BaseModel, IQueryAttributable { public void ApplyQueryAttributes(IDictionary<string, object> query) { } }", 1)]
	[InlineData("", ": IQueryAttributable", "[QueryProperty(nameof(Value), \"value\")] public class Model : BaseModel { }", 1)]
	[InlineData("[QueryProperty(nameof(Value), \"value\")]", ": IQueryAttributable", "public class Model : BaseModel { }", 0)]
	public async Task HandlesMechanismsInheritedFromMetadata(string baseAttribute, string baseInterface, string source, int expectedCount)
	{
		var baseCompilation = CreateCompilation($$"""
			{{baseAttribute}}
			public class BaseModel {{baseInterface}}
			{
				public string Value { get; set; }
				public void ApplyQueryAttributes(IDictionary<string, object> query) { }
			}
			""");
		using var stream = new MemoryStream();
		var result = baseCompilation.Emit(stream);
		Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));

		var diagnostics = await Analyze(source, MetadataReference.CreateFromImage(stream.ToArray()));

		Assert.Equal(expectedCount, diagnostics.Length);
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
				}
				""", path: "Model.g.cs"));

		Assert.Empty(await GetDiagnostics(compilation));
	}

	static Compilation CreateCompilation(string source)
		=> SourceGeneratorDriver.CreateMauiCompilation($"QueryConflictTests{Guid.NewGuid():N}")
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(Usings + source));

	static Task<ImmutableArray<Diagnostic>> Analyze(string source, params MetadataReference[] references)
		=> GetDiagnostics(CreateCompilation(source).AddReferences(references));

	static async Task<ImmutableArray<Diagnostic>> GetDiagnostics(Compilation compilation)
	{
		Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
		return await compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new QueryPropertyConflictAnalyzer()))
			.GetAnalyzerDiagnosticsAsync();
	}
}
