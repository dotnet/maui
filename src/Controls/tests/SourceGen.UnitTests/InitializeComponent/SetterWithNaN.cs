using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class SetterWithNaN : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void SetterPropertyWithNaNValue()
	{
		// Issue: Setter with Value="NaN" generates incorrect code "Value = NaND" instead of "Value = double.NaN"
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
    <ContentPage.Resources>
        <Style TargetType="Label" x:Key="LabelStyle">
            <Setter Property="HeightRequest" Value="NaN" />
            <Setter Property="WidthRequest" Value="NaN" />
        </Style>
    </ContentPage.Resources>
</ContentPage>
""";

		var code =
"""
using System;
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

		// Verify that NaN is correctly generated as double.NaN, not NaND
		Assert.Contains("double.NaN", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("NaND", generated, StringComparison.Ordinal);
		
		// Verify both HeightRequest and WidthRequest setters use double.NaN
		Assert.Contains("HeightRequestProperty", generated, StringComparison.Ordinal);
		Assert.Contains("WidthRequestProperty", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void SetterPropertyWithNaNValueInObjectInitializer()
	{
		// Verify the generated Setter uses double.NaN in its object initializer
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
    <ContentPage.Resources>
        <Style TargetType="Button" x:Key="ButtonStyle">
            <Setter Property="HeightRequest" Value="NaN" />
        </Style>
    </ContentPage.Resources>
</ContentPage>
""";

		var code =
"""
using System;
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

		// Verify the Setter is created with Value = double.NaN in object initializer syntax
		Assert.Contains("new global::Microsoft.Maui.Controls.Setter", generated, StringComparison.Ordinal);
		Assert.Contains("Property = global::Microsoft.Maui.Controls.VisualElement.HeightRequestProperty", generated, StringComparison.Ordinal);
		Assert.Contains("Value = double.NaN", generated, StringComparison.Ordinal);
		
		// Ensure incorrect code is not generated
		Assert.DoesNotContain("Value = NaND", generated, StringComparison.Ordinal);
	}
}
