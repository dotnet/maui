using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class DoesNotFail : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void Test()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:local="clr-namespace:Test"
	x:Class="Test.TestPage" >
    <ContentPage.Resources>
        <x:Array x:Key="myArray" Type="{x:Type x:Object}">
            <x:String>Some string</x:String>
            <x:Int32>69</x:Int32>
            <x:Int32>32</x:Int32>
        </x:Array>
    </ContentPage.Resources>
	<Label Text="{Binding Path=., Converter={StaticResource reverseConverter}}" x:DataType="x:String"/>
</ContentPage>
""";

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Collections.Generic;

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

		var (result, generated) = RunGenerator(xaml, code);
		Assert.False(result.Diagnostics.Any());

		// Assert.AreEqual(expected, generated);
	}
}