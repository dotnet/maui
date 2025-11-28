using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class BindingDiagnosticsTests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn);

	[Fact]
	public void BindingPropertyNotFound_ReportsCorrectDiagnostic()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:ViewModel">
	<Label Text="{Binding NonExistentProperty}" />
</ContentPage>
""";

		var csharp =
"""
namespace Test;

public partial class TestPage : Microsoft.Maui.Controls.ContentPage { }

public class ViewModel 
{
	public string Name { get; set; }
}
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(csharp));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml), assertNoCompilationErrors: false);

		// Should have a diagnostic for property not found
		var diagnostic = result.Diagnostics.FirstOrDefault(d => d.Id == "MAUIG2045");
		Assert.NotNull(diagnostic);
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
		
		var message = diagnostic.GetMessage();
		Assert.Contains("NonExistentProperty", message, System.StringComparison.Ordinal);
		Assert.Contains("ViewModel", message, System.StringComparison.Ordinal);
	}

	[Fact]
	public void BindingIndexerNotClosed_ReportsCorrectDiagnostic()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:ViewModel">
	<Label Text="{Binding Items[0}" />
</ContentPage>
""";

		var csharp =
"""
namespace Test;

public partial class TestPage : Microsoft.Maui.Controls.ContentPage { }

public class ViewModel 
{
	public string[] Items { get; set; }
}
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(csharp));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml), assertNoCompilationErrors: false);

		// Should have a diagnostic for indexer not closed
		var diagnostic = result.Diagnostics.FirstOrDefault(d => d.Id == "MAUIG2041");
		Assert.NotNull(diagnostic);
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
		
		var message = diagnostic.GetMessage();
		Assert.Contains("closing bracket", message, System.StringComparison.Ordinal);
	}

	[Fact]
	public void BindingIndexerEmpty_ReportsCorrectDiagnostic()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:ViewModel">
	<Label Text="{Binding Items[]}" />
</ContentPage>
""";

		var csharp =
"""
namespace Test;

public partial class TestPage : Microsoft.Maui.Controls.ContentPage { }

public class ViewModel 
{
	public string[] Items { get; set; }
}
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(csharp));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml), assertNoCompilationErrors: false);

		// Should have a diagnostic for indexer empty
		var diagnostic = result.Diagnostics.FirstOrDefault(d => d.Id == "MAUIG2042");
		Assert.NotNull(diagnostic);
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
		
		var message = diagnostic.GetMessage();
		Assert.Contains("did not contain arguments", message, System.StringComparison.Ordinal);
	}

	[Fact]
	public void DuplicateXName_ReportsCorrectDiagnostic()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<StackLayout>
		<Label x:Name="MyLabel" Text="First"/>
		<Label x:Name="MyLabel" Text="Second"/>
	</StackLayout>
</ContentPage>
""";

		var compilation = CreateMauiCompilation();
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml), assertNoCompilationErrors: false);

		// Should have a diagnostic for duplicate name
		var diagnostic = result.Diagnostics.FirstOrDefault(d => d.Id == "MAUIG2064");
		Assert.NotNull(diagnostic);
		Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
		
		var message = diagnostic.GetMessage();
		Assert.Contains("MyLabel", message, System.StringComparison.Ordinal);
		Assert.Contains("already exists", message, System.StringComparison.Ordinal);
	}

	[Fact]
	public void BindingWithXDataTypeFromOuterScope_ReportsWarning()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:ViewModel">
	<ContentPage.Resources>
		<DataTemplate x:Key="MyTemplate">
			<Label Text="{Binding Name}" />
		</DataTemplate>
	</ContentPage.Resources>
</ContentPage>
""";

		var csharp =
"""
namespace Test;

public partial class TestPage : Microsoft.Maui.Controls.ContentPage { }

public class ViewModel 
{
	public string Name { get; set; }
}
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(csharp));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml), assertNoCompilationErrors: false);

		// Should have a warning for x:DataType from outer scope
		var diagnostic = result.Diagnostics.FirstOrDefault(d => d.Id == "MAUIG2024");
		Assert.NotNull(diagnostic);
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
	}

	[Fact]
	public void BindingIndexerTypeUnsupported_ReportsCorrectDiagnostic()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:ViewModel">
	<Label Text="{Binding Name[0]}" />
</ContentPage>
""";

		var csharp =
"""
namespace Test;

public partial class TestPage : Microsoft.Maui.Controls.ContentPage { }

public class ViewModel 
{
	public string Name { get; set; }
}
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(csharp));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml), assertNoCompilationErrors: false);

		// Should have a diagnostic for unsupported indexer type
		var diagnostic = result.Diagnostics.FirstOrDefault(d => d.Id == "MAUIG2043");
		Assert.NotNull(diagnostic);
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
	}
}
