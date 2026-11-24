using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;

//https://github.com/dotnet/maui/issues/39763
[Collection("XamlHotReloadTests")]
public class Maui39763Tests
{
	const string PageSource = """
		[assembly: Microsoft.Maui.Controls.XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "Test39763.Routing")]
		[assembly: Microsoft.Maui.Controls.XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "Test39763.Other")]

		namespace Test39763
		{
			public partial class MainPage : global::Microsoft.Maui.Controls.ContentPage
			{
				public MainPage() => InitializeComponent();
			}
		}

		namespace Test39763.Other
		{
			public static class Maui39763Marker
			{
			}
		}
		""";

	const string RoutesSource = """
		namespace Test39763.Routing;

		public static class Routes
		{
			public const string LineItemsPage = "LineItemsPage";
		}
		""";

	static string PageXaml(string text) => $"""
		<ContentPage x:Class="Test39763.MainPage">
			<Label Text="{text}" />
		</ContentPage>
		""";

	[Fact]
	public void XStaticFallbackUsings_ReachUpdateComponent()
	{
		//Routes is not part of the compilation the generator sees (like the output of another
		//source generator), but its namespace is registered in the implicit (global) xmlns.
		//The using directives the x:Static fallback needs must reach the UpdateComponent file
		//too — imports in the InitializeComponent file don't apply to it.
		using var harness = new XamlHotReloadTestHarness(nameof(XStaticFallbackUsings_ReachUpdateComponent),
			"Test39763.MainPage", PageSource);
		var generation = harness.Generate(
			PageXaml("Plain"),
			PageXaml("{x:Static Routes.LineItemsPage}"));

		var updateComponent = generation[1].UpdateComponentSource;
		Assert.NotNull(updateComponent);
		Assert.Contains("Routes.LineItemsPage", updateComponent, StringComparison.Ordinal);
		Assert.Contains("using Test39763.Routing;", updateComponent, StringComparison.Ordinal);
		Assert.Contains("using Test39763.Other;", updateComponent, StringComparison.Ordinal);

		//the generated sources compile once Routes exists in the final compilation
		var finalCompilation = ((CSharpCompilation)SourceGeneratorDriver.CreateMauiCompilation(harness.AssemblyName))
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(PageSource))
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(RoutesSource))
			.AddSyntaxTrees(generation[1].GeneratorResult.Results
				.SelectMany(r => r.GeneratedSources)
				.Select(gs => CSharpSyntaxTree.ParseText(gs.SourceText.ToString()))
				.ToArray());
		Assert.DoesNotContain(finalCompilation.GetDiagnostics(), d => d.Severity == DiagnosticSeverity.Error);
	}
}
