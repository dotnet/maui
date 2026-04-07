using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class MultipleChildrenWarningTests : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void BorderWithMultipleChildren_EmitsWarning()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Border>
		<Label Text="First" />
		<Label Text="Second" />
	</Border>
</ContentPage>
"""
;

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
"""
;

		var (result, generated) = RunGenerator(xaml, code);
		
		// Verify that a warning diagnostic was emitted
		var warnings = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ToArray();
		Assert.True(warnings.Length > 0, "Expected at least one warning diagnostic");
		
		var multipleChildrenWarning = warnings.FirstOrDefault(d => d.Id == "MAUIX2015");
		Assert.NotNull(multipleChildrenWarning);
		Assert.Contains("Border.Content", multipleChildrenWarning.GetMessage(), StringComparison.Ordinal);
		Assert.Contains("multiple times", multipleChildrenWarning.GetMessage(), StringComparison.Ordinal);
	}

	[Fact]
	public void ContentPageWithMultipleChildren_EmitsWarning()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="First" />
	<Label Text="Second" />
</ContentPage>
"""
;

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
"""
;

		var (result, generated) = RunGenerator(xaml, code);
		
		// Verify that a warning diagnostic was emitted
		var warnings = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ToArray();
		Assert.True(warnings.Length > 0, "Expected at least one warning diagnostic");
		
		var multipleChildrenWarning = warnings.FirstOrDefault(d => d.Id == "MAUIX2015");
		Assert.NotNull(multipleChildrenWarning);
		Assert.Contains("ContentPage.Content", multipleChildrenWarning.GetMessage(), StringComparison.Ordinal);
	}

	[Fact]
	public void BorderWithSingleChild_NoWarning()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Border>
		<Label Text="Only Child" />
	</Border>
</ContentPage>
"""
;

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
"""
;

		var (result, generated) = RunGenerator(xaml, code);
		
		// Verify that NO warning diagnostic was emitted for single child
		var multipleChildrenWarning = result.Diagnostics.FirstOrDefault(d => d.Id == "MAUIX2015");
		Assert.Null(multipleChildrenWarning);
	}

	[Fact]
	public void StackLayoutWithMultipleChildren_NoWarning()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<VerticalStackLayout>
		<Label Text="First" />
		<Label Text="Second" />
		<Label Text="Third" />
	</VerticalStackLayout>
</ContentPage>
"""
;

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
"""
;

		var (result, generated) = RunGenerator(xaml, code);
		
		// VerticalStackLayout is a collection type, so multiple children are valid
		var multipleChildrenWarning = result.Diagnostics.FirstOrDefault(d => d.Id == "MAUIX2015");
		Assert.Null(multipleChildrenWarning);
	}
}
