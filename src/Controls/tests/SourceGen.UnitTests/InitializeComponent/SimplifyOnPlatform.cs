using System;
using System.Linq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.InitializeComponent;

public class SimplifyOnPlatform : SourceGenXamlInitializeComponentTestBase
{
	[Test]
	public void Test()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
    <ContentPage.Resources>
        <Style TargetType="Label" x:Key="style">
            <Setter Property="TextColor" Value="Pink" />
            <Setter Property="IsVisible" Value="{OnPlatform Android=True, iOS=False}" />
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

var expected =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
""";

		var (result, generated) = RunGenerator(xaml, code, targetFramework: "net10.0-android");
		Assert.IsFalse(result.Diagnostics.Any());

		Assert.AreEqual(expected, generated);

	}
}