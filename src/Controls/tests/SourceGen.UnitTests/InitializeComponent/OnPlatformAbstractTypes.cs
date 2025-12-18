using System;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Tests for OnPlatform with abstract types or types with protected constructors.
/// Issues:
/// 1. "Cannot create an instance of the abstract type or interface 'Brush'"
/// 2. "error CS0122: 'View.View()' is inaccessible due to its protection level"
/// </summary>
public class OnPlatformAbstractTypes : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void OnPlatformWithAbstractBrushTypeUsesDefault()
	{
		// When the target platform is Android and OnPlatform has WinUI + Default,
		// the Default (SolidColorBrush) should be used instead of trying to instantiate abstract Brush
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<OnPlatform x:Key="TestBrush" x:TypeArguments="Brush">
			<On Platform="WinUI">
				<LinearGradientBrush StartPoint="0.5,0" EndPoint="0.5,1">
					<LinearGradientBrush.GradientStops>
						<GradientStop Offset="0.00" Color="Red" />
						<GradientStop Offset="1.00" Color="Blue" />
					</LinearGradientBrush.GradientStops>
				</LinearGradientBrush>
			</On>
			<OnPlatform.Default>
				<SolidColorBrush Color="Red" />
			</OnPlatform.Default>
		</OnPlatform>
	</ContentPage.Resources>
</ContentPage>
""";

		var code =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
""";

		// Target Android - WinUI platform is not matched, so Default should be used
		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0-android");
		
		// Should not have any errors
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error),
			$"SourceGen should not fail for OnPlatform<Brush> with Default. Errors: {string.Join(", ", result.Diagnostics)}");
		
		// Should contain SolidColorBrush (from Default), not try to instantiate abstract Brush
		Assert.Contains("SolidColorBrush", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("new global::Microsoft.Maui.Controls.Brush()", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void OnPlatformWithAbstractBrushTypeMatchesPlatform()
	{
		// When the target platform is WinUI, the WinUI value (LinearGradientBrush) should be used
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<OnPlatform x:Key="TestBrush" x:TypeArguments="Brush">
			<On Platform="WinUI">
				<LinearGradientBrush StartPoint="0.5,0" EndPoint="0.5,1">
					<LinearGradientBrush.GradientStops>
						<GradientStop Offset="0.00" Color="Red" />
						<GradientStop Offset="1.00" Color="Blue" />
					</LinearGradientBrush.GradientStops>
				</LinearGradientBrush>
			</On>
			<OnPlatform.Default>
				<SolidColorBrush Color="Red" />
			</OnPlatform.Default>
		</OnPlatform>
	</ContentPage.Resources>
</ContentPage>
""";

		var code =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
""";

		// Target Windows - WinUI platform is matched
		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0-windows");
		
		// Should not have any errors
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error),
			$"SourceGen should not fail for OnPlatform<Brush>. Errors: {string.Join(", ", result.Diagnostics)}");
		
		// Should contain LinearGradientBrush (from WinUI platform)
		Assert.Contains("LinearGradientBrush", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void OnPlatformWithViewTypeUsesDefault()
	{
		// When the target platform is Android and OnPlatform has WinUI + Default,
		// the Default (Label) should be used instead of trying to instantiate View with protected ctor
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<StackLayout>
		<OnPlatform x:TypeArguments="View">
			<On Platform="WinUI">
				<Border BackgroundColor="Blue" />
			</On>
			<OnPlatform.Default>
				<Label Text="Default Content" />
			</OnPlatform.Default>
		</OnPlatform>
	</StackLayout>
</ContentPage>
""";

		var code =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
""";

		// Target Android - WinUI platform is not matched, so Default should be used
		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0-android");
		
		// Should not have any errors
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error),
			$"SourceGen should not fail for OnPlatform<View> with Default. Errors: {string.Join(", ", result.Diagnostics)}");
		
		// Should contain Label (from Default), not try to instantiate View
		Assert.Contains("Label", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("new global::Microsoft.Maui.Controls.View()", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void OnPlatformWithViewTypeMatchesPlatform()
	{
		// When the target platform is WinUI, the WinUI value (Border) should be used
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<StackLayout>
		<OnPlatform x:TypeArguments="View">
			<On Platform="WinUI">
				<Border BackgroundColor="Blue" />
			</On>
			<OnPlatform.Default>
				<Label Text="Default Content" />
			</OnPlatform.Default>
		</OnPlatform>
	</StackLayout>
</ContentPage>
""";

		var code =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
""";

		// Target Windows - WinUI platform is matched
		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0-windows");
		
		// Should not have any errors
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error),
			$"SourceGen should not fail for OnPlatform<View>. Errors: {string.Join(", ", result.Diagnostics)}");
		
		// Should contain Border (from WinUI platform)
		Assert.Contains("Border", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void OnPlatformWithAbstractTypeNoDefaultNoMatchingPlatform()
	{
		// When there's no Default and no matching platform, and the type is abstract,
		// SourceGen should NOT try to create an instance of the abstract type.
		// Instead, it should either skip the resource or keep the OnPlatform runtime behavior.
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<ContentPage.Resources>
		<OnPlatform x:Key="TestBrush" x:TypeArguments="Brush">
			<On Platform="WinUI">
				<LinearGradientBrush StartPoint="0.5,0" EndPoint="0.5,1">
					<LinearGradientBrush.GradientStops>
						<GradientStop Offset="0.00" Color="Red" />
						<GradientStop Offset="1.00" Color="Blue" />
					</LinearGradientBrush.GradientStops>
				</LinearGradientBrush>
			</On>
		</OnPlatform>
	</ContentPage.Resources>
</ContentPage>
""";

		var code =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
""";

		// Target Android - WinUI platform is not matched and there's no Default
		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0-android");
		
		// Should NOT contain "new Brush()" - that would be a compiler error
		Assert.DoesNotContain("new global::Microsoft.Maui.Controls.Brush()", generated, StringComparison.Ordinal);
		
		// The OnPlatform should either:
		// 1. Be kept as-is (OnPlatform element) for runtime resolution, or
		// 2. Not be simplified at all for this platform
		// Either way, there should be no compilation errors
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error),
			$"SourceGen should not fail for OnPlatform<Brush> without Default. Errors: {string.Join(", ", result.Diagnostics)}");
	}

	[Fact]
	public void OnPlatformWithProtectedCtorTypeNoDefaultNoMatchingPlatform()
	{
		// When there's no Default and no matching platform, and the type has protected ctor,
		// SourceGen should NOT try to create an instance of that type.
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<StackLayout>
		<OnPlatform x:TypeArguments="View">
			<On Platform="WinUI">
				<Border BackgroundColor="Blue" />
			</On>
		</OnPlatform>
	</StackLayout>
</ContentPage>
""";

		var code =
"""
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
""";

		// Target Android - WinUI platform is not matched and there's no Default
		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0-android");
		
		// Should NOT contain "new View()" - that would be a compiler error due to protected ctor
		Assert.DoesNotContain("new global::Microsoft.Maui.Controls.View()", generated, StringComparison.Ordinal);
		
		// Either the OnPlatform is kept for runtime, or no element is added
		// Either way, there should be no compilation errors
		Assert.False(result.Diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error),
			$"SourceGen should not fail for OnPlatform<View> without Default. Errors: {string.Join(", ", result.Diagnostics)}");
	}
}
