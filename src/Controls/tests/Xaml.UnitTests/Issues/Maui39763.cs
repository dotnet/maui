using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

//https://github.com/dotnet/maui/issues/39763
[Collection("Xaml Inflation")]
public class Maui39763
{
	const string Xaml =
"""
<ContentPage x:Class="Test.Maui39763Page">
	<Label Text="{x:Static Routes.LineItemsPage}" />
</ContentPage>
""";

	const string PageSource =
"""
[assembly: Microsoft.Maui.Controls.XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "Test.Routing")]

namespace Test;

public partial class Maui39763Page : global::Microsoft.Maui.Controls.ContentPage
{
	public Maui39763Page() => InitializeComponent();
}
""";

	const string RoutesSource =
"""
namespace Test.Routing;

public static class Routes
{
	public const string LineItemsPage = "LineItemsPage";
}
""";

	[Fact]
	internal void XStaticWithGeneratedTypeInImplicitXmlns()
	{
		//Routes is not part of the compilation the generator sees (like the output of another
		//source generator), but its namespace is registered in the implicit (global) xmlns.
		//SourceGen should defer to the C# compiler instead of failing with MAUIG1001, like it
		//already does for explicit clr-namespace prefixes.
		var compilation = CreateMauiCompilation().WithAdditionalSource(PageSource);
		var result = compilation.RunMauiSourceGenerator(new AdditionalXamlFile("Maui39763Page.xaml", Xaml));

		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);

		//a single candidate namespace is emitted fully qualified, immune to name collisions
		Assert.Contains("global::Test.Routing.Routes.LineItemsPage", result.GeneratedInitializeComponent(), StringComparison.Ordinal);

		//the generated code compiles once Routes exists in the final compilation
		var finalCompilation = compilation
			.WithAdditionalSource(RoutesSource, "Routes.cs")
			.AddSyntaxTrees(result.Results.Single().GeneratedSources.Select(gs => CSharpSyntaxTree.ParseText(gs.SourceText.ToString())).ToArray());
		Assert.DoesNotContain(finalCompilation.GetDiagnostics(), d => d.Severity == DiagnosticSeverity.Error);
	}

	[Fact]
	internal void XStaticWithGeneratedTypeInAggregatedXmlns()
	{
		//the global xmlns aggregates another xmlns URI; that URI is not a CLR namespace and
		//must not be emitted as a using directive
		const string pageSource =
"""
[assembly: Microsoft.Maui.Controls.XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "urn:test39763")]
[assembly: Microsoft.Maui.Controls.XmlnsDefinition("urn:test39763", "Test.Routing")]

namespace Test;

public partial class Maui39763Page : global::Microsoft.Maui.Controls.ContentPage
{
	public Maui39763Page() => InitializeComponent();
}
""";
		var compilation = CreateMauiCompilation().WithAdditionalSource(pageSource);
		var result = compilation.RunMauiSourceGenerator(new AdditionalXamlFile("Maui39763Page.xaml", Xaml));

		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		Assert.DoesNotContain("using urn:test39763;", result.GeneratedInitializeComponent(), StringComparison.Ordinal);

		var finalCompilation = compilation
			.WithAdditionalSource(RoutesSource, "Routes.cs")
			.AddSyntaxTrees(result.Results.Single().GeneratedSources.Select(gs => CSharpSyntaxTree.ParseText(gs.SourceText.ToString())).ToArray());
		Assert.DoesNotContain(finalCompilation.GetDiagnostics(), d => d.Severity == DiagnosticSeverity.Error);
	}

	[Fact]
	internal void XStaticWithGeneratedTypeInImplicitXmlnsMultipleNamespaces()
	{
		//with multiple candidate namespaces the generator can't guess which one holds the type,
		//so it emits using directives and lets the C# compiler resolve the unqualified name
		const string pageSource =
"""
[assembly: Microsoft.Maui.Controls.XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "Test.Routing")]
[assembly: Microsoft.Maui.Controls.XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "Test.Other")]

namespace Test
{
	public partial class Maui39763Page : global::Microsoft.Maui.Controls.ContentPage
	{
		public Maui39763Page() => InitializeComponent();
	}
}

namespace Test.Other
{
	public static class Maui39763Marker
	{
	}
}
""";
		var compilation = CreateMauiCompilation().WithAdditionalSource(pageSource);
		var result = compilation.RunMauiSourceGenerator(new AdditionalXamlFile("Maui39763Page.xaml", Xaml));

		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);

		var generated = result.GeneratedInitializeComponent();
		Assert.Contains("using Test.Routing;", generated, StringComparison.Ordinal);
		Assert.Contains("using Test.Other;", generated, StringComparison.Ordinal);
		Assert.Contains("= Routes.LineItemsPage;", generated, StringComparison.Ordinal);

		var finalCompilation = compilation
			.WithAdditionalSource(RoutesSource, "Routes.cs")
			.AddSyntaxTrees(result.Results.Single().GeneratedSources.Select(gs => CSharpSyntaxTree.ParseText(gs.SourceText.ToString())).ToArray());
		Assert.DoesNotContain(finalCompilation.GetDiagnostics(), d => d.Severity == DiagnosticSeverity.Error);
	}

	[Fact]
	internal void XStaticWithResolvableTypeInImplicitXmlns()
	{
		//control: when the type is part of the compilation, it resolves through the implicit xmlns
		var compilation = CreateMauiCompilation()
			.WithAdditionalSource(PageSource)
			.WithAdditionalSource(RoutesSource, "Routes.cs");
		var result = compilation.RunMauiSourceGenerator(new AdditionalXamlFile("Maui39763Page.xaml", Xaml));

		Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
		Assert.Contains("global::Test.Routing.Routes.LineItemsPage", result.GeneratedInitializeComponent(), StringComparison.Ordinal);
	}
}
