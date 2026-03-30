using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;

public class XReferenceBindingTests : SourceGenTestsBase
{
	private record AdditionalXamlFile(string Path, string Content, string? RelativePath = null, string? TargetPath = null, string? ManifestResourceName = null, string? TargetFramework = null, string? NoWarn = null)
		: AdditionalFile(Text: SourceGeneratorDriver.ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: RelativePath ?? Path, TargetPath: TargetPath, ManifestResourceName: ManifestResourceName, TargetFramework: TargetFramework, NoWarn: NoWarn);

	[Fact]
	public void XReferenceDirectProperty_DoesNotFallBackSilently()
	{
		// Binding to a direct property on a referenced element (e.g., Slider.Value)
		// should attempt compilation. If the property exists on the element type,
		// no MAUIG2045 diagnostic should be emitted.
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<StackLayout>
		<Slider x:Name="MySlider" />
		<Label Text="{Binding Source={x:Reference MySlider}, Path=Value}" />
	</StackLayout>
</ContentPage>
""";

		var csharp =
"""
namespace Test;

public partial class TestPage : Microsoft.Maui.Controls.ContentPage { }
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(csharp));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml), assertNoCompilationErrors: false);

		// Value exists on Slider — should not report property not found
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIG2045");
	}

	[Fact]
	public void XReferenceInDataTemplate_DoesNotReportFalsePositive()
	{
		// x:Reference bindings inside DataTemplates should NOT produce MAUIG2045
		// for the DataTemplate's x:DataType
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:Name="PageRoot"
	x:DataType="test:ViewModel">
	<CollectionView ItemsSource="{Binding Items}">
		<CollectionView.ItemTemplate>
			<DataTemplate x:DataType="test:ItemModel">
				<Button Text="{Binding Name}"
						Command="{Binding Source={x:Reference PageRoot}, Path=BindingContext.SelectItemCommand}"
						CommandParameter="{Binding .}" />
			</DataTemplate>
		</CollectionView.ItemTemplate>
	</CollectionView>
</ContentPage>
""";

		var csharp =
"""
namespace Test;

public partial class TestPage : Microsoft.Maui.Controls.ContentPage { }

public class ViewModel
{
	public System.Collections.Generic.List<ItemModel> Items { get; set; }
	public Microsoft.Maui.Controls.Command SelectItemCommand { get; set; }
}

public class ItemModel
{
	public string Name { get; set; }
}
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(csharp));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml), assertNoCompilationErrors: false);

		// Should NOT report property not found on ItemModel for the x:Reference binding
		Assert.DoesNotContain(result.Diagnostics,
			d => d.Id == "MAUIG2045" && d.GetMessage().Contains("ItemModel", StringComparison.Ordinal));
	}

	[Fact]
	public void XReferenceNonExistentProperty_ReportsDiagnostic()
	{
		// A binding to a non-existent property on a referenced element should report MAUIG2045
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<StackLayout>
		<Slider x:Name="MySlider" />
		<Label Text="{Binding Source={x:Reference MySlider}, Path=NonExistentProperty}" />
	</StackLayout>
</ContentPage>
""";

		var csharp =
"""
namespace Test;

public partial class TestPage : Microsoft.Maui.Controls.ContentPage { }
""";

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(csharp));
		var result = RunGenerator<XamlGenerator>(compilation, new AdditionalXamlFile("Test.xaml", xaml), assertNoCompilationErrors: false);

		// Should report property not found on Slider
		var diagnostic = result.Diagnostics.FirstOrDefault(
			d => d.Id == "MAUIG2045" && d.GetMessage().Contains("NonExistentProperty", StringComparison.Ordinal));
		Assert.NotNull(diagnostic);
	}

	[Fact]
	public void XDataTypeExplicitNull_BlocksInheritance()
	{
		// x:DataType="{x:Null}" should block inheritance and prevent compiled bindings
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:ViewModel">
	<StackLayout x:DataType="{x:Null}">
		<Label Text="{Binding Name}" />
	</StackLayout>
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

		// x:DataType="{x:Null}" opts out of compilation — no MAUIG2045 should be reported
		// because the binding is not compiled at all (falls back to runtime)
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIG2045");
	}
}
