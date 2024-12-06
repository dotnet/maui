using System.Linq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;
public class DoesNotFail : SourceGenXamlInitializeComponentTestBase
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
	xmlns:local="clr-namespace:Test"
	x:Class="Test.TestPage" 
    Title="{Binding Title, Converter={local:TestConverter}}"/>
""";

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Collections.Generic;

namespace Test;

[AcceptEmptyServiceProvider]
public class TestConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotImplementedException();
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotImplementedException();
	}

	public object ProvideValue(IServiceProvider serviceProvider)
	{
		throw new NotImplementedException();
	}
}

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
		Assert.IsFalse(result.Diagnostics.Any());

		// Assert.AreEqual(expected, generated);
	}
}