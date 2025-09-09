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
	
	xmlns:cmp="clr-namespace:Microsoft.Maui.Controls.Compatibility;assembly=Microsoft.Maui.Controls"
	x:Class="Test.TestPage" >
	<ContentPage.Resources>
		<ResourceDictionary>
			<local:ReverseConverter x:Key="reverseConverter"/>
			<DataTemplate x:Key="SimpleMessageTemplate">
				<ViewCell>
					<cmp:StackLayout >
						<Label Text="{Binding Converter={StaticResource reverseConverter}}" x:DataType="x:String" />
					</cmp:StackLayout>
				</ViewCell>
			</DataTemplate>
			<ListView x:Key="listview"
					  ItemTemplate="{StaticResource SimpleMessageTemplate}" />
		</ResourceDictionary>
	</ContentPage.Resources>
</ContentPage>
""";

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Collections.Generic;

namespace Test;

public class ReverseConverter : IValueConverter
{
	public static ReverseConverter Instance = new ReverseConverter();

	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		var s = value as string;
		if (s == null)
			return value;
		return new string(s.Reverse().ToArray());
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		var s = value as string;
		if (s == null)
			return value;
		return new string(s.Reverse().ToArray());
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
		Assert.False(result.Diagnostics.Any());

		// Assert.AreEqual(expected, generated);
	}
}