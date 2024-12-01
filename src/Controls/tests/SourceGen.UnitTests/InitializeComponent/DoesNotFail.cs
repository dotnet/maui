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
	x:Class="Test.TestPage">
	<local:Bz36422Control x:Name="control">
		<local:Bz36422Control.Views>
			<x:Array Type="{x:Type ContentView}">
				<ContentView>
					<Label Text="Page 1"/>
				</ContentView>
				<ContentView>
					<Label Text="Page 2"/>
				</ContentView>
				<ContentView>
					<Label Text="Page 3"/>
				</ContentView>
			</x:Array>
		</local:Bz36422Control.Views>
	</local:Bz36422Control>
</ContentPage>
""";

		var code =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Collections.Generic;

namespace Test;

public class Bz36422Control : ContentView
{
	public IList<ContentView> Views { get; set; }
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