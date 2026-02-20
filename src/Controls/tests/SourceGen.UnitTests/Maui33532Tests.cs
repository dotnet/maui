using System;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Tests for issue #33532: NaN value in XAML generates invalid code
/// When Padding="NaN" is used, the source generator was generating bare "NaN" instead of "double.NaN"
/// </summary>
public class Maui33532Tests : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void ThicknessWithSingleNaNValue()
	{
		// Issue #33532: Padding="NaN" generates invalid code with bare "NaN" instead of "double.NaN"
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Button x:Name="testButton" Padding="NaN" Text="Test"/>
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

		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0");
		Assert.False(result.Diagnostics.Any());

		// Verify that NaN is correctly generated as double.NaN
		Assert.Contains("double.NaN", generated, StringComparison.Ordinal);
		
		// Ensure incorrect bare NaN is not generated (would fail with CS0103)
		// The pattern "new global::Microsoft.Maui.Thickness(NaN)" would be incorrect
		Assert.DoesNotContain("Thickness(NaN)", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void ThicknessWithTwoNaNValues()
	{
		// Test Padding="NaN,NaN" (horizontal, vertical)
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Button x:Name="testButton" Padding="NaN,NaN" Text="Test"/>
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

		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0");
		Assert.False(result.Diagnostics.Any());

		// Verify that NaN is correctly generated as double.NaN
		Assert.Contains("double.NaN", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void ThicknessWithFourNaNValues()
	{
		// Test Padding="NaN,NaN,NaN,NaN" (left, top, right, bottom)
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Button x:Name="testButton" Padding="NaN,NaN,NaN,NaN" Text="Test"/>
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

		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0");
		Assert.False(result.Diagnostics.Any());

		// Verify that NaN is correctly generated as double.NaN (should appear 4 times)
		Assert.Contains("double.NaN", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void ThicknessWithMixedNaNAndRegularValues()
	{
		// Test mixing NaN with regular values: Padding="NaN,10,NaN,20"
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Button x:Name="testButton" Padding="NaN,10,NaN,20" Text="Test"/>
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

		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0");
		Assert.False(result.Diagnostics.Any());

		// Verify that both NaN and regular values are generated correctly
		Assert.Contains("double.NaN", generated, StringComparison.Ordinal);
		Assert.Contains("Thickness", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void MarginWithNaNValue()
	{
		// Test Margin (also uses ThicknessConverter) with NaN
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label x:Name="testLabel" Margin="NaN" Text="Test"/>
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

		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0");
		Assert.False(result.Diagnostics.Any());

		// Verify that NaN is correctly generated as double.NaN for Margin
		Assert.Contains("double.NaN", generated, StringComparison.Ordinal);
	}
}
