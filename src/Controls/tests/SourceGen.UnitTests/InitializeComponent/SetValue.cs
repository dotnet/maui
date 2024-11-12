using System.Linq;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;
public class SetValue : SourceGenXamlInitializeComponentTestBase
{
	[Test]
	public void TestInitializeComponentGenerator_BasicXaml()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
		<Button x:Name="MyButton" Text="Hello MAUI!" />
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

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a .NET MAUI source generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable

namespace Test;

[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Maui.Controls.SourceGen, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "1.0.0.0")]
public partial class TestPage
{
	private partial void InitializeComponentSourceGen()
	{
		global::Microsoft.Maui.Controls.Button button0 = new global::Microsoft.Maui.Controls.Button();
		global::Test.TestPage __root = this;
		this.MyButton = button0;
		button0.SetValue(global::Microsoft.Maui.Controls.Button.TextProperty, "Hello MAUI!");
		__root.SetValue(global::Microsoft.Maui.Controls.ContentPage.ContentProperty, button0);
	}
}

""";

		var (result, generated) = RunGenerator(xaml, code);
		Assert.IsFalse(result.Diagnostics.Any());

		Assert.AreEqual(expected, generated);
	}
}