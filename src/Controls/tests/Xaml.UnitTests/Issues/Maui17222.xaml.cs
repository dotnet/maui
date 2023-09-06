using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17222 : ContentPage
{

	public Maui17222() => InitializeComponent();

	public Maui17222(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void GetsourceInfo([Values(false)] bool useCompiledXaml)
		{
			var app = new MockApplication();
			app.Resources.Add(new Maui17222BaseStyle(useCompiledXaml));
			app.Resources.Add(new Maui17222Style(useCompiledXaml));
			Application.SetCurrentApplication(app);

			var page = new Maui17222(useCompiledXaml);
			SourceInfo info = VisualDiagnostics.GetSourceInfo(page);
			Assert.AreEqual(new Uri("Issues/Maui17222.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.AreEqual(2, info.LineNumber);
			Assert.AreEqual(2, info.LinePosition);

			var button = page.button;
			info = VisualDiagnostics.GetSourceInfo(button);
			Assert.AreEqual(new Uri("Issues/Maui17222.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.AreEqual(6, info.LineNumber);
			Assert.AreEqual(6, info.LinePosition);

			Style style = button.Style;
			info = VisualDiagnostics.GetSourceInfo(style);
			Assert.AreEqual(new Uri("Issues/Maui17222Style.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.AreEqual(5, info.LineNumber);
			Assert.AreEqual(6, info.LinePosition);

			var setter = style.Setters[0];
			Assert.AreEqual(setter.Property, Button.TextColorProperty);
			Assert.AreEqual(setter.Value, Colors.Red);
			info = VisualDiagnostics.GetSourceInfo(setter);
			Assert.AreEqual(new Uri("Issues/Maui17222Style.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.AreEqual(6, info.LineNumber);
			Assert.AreEqual(10, info.LinePosition);


			style = style.BasedOn;
			info = VisualDiagnostics.GetSourceInfo(style);
			Assert.AreEqual(new Uri("Issues/Maui17222BaseStyle.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.AreEqual(5, info.LineNumber);
			Assert.AreEqual(6, info.LinePosition);

			//while (style != null)
			//{
			//	// Requirement #3 Getting SourceInfo for Style Object
			//	info = VisualDiagnostics.GetSourceInfo(style);
			//	Debug.WriteLine($"Style Name:{style.BaseResourceKey} SourceUri:{info.SourceUri}, Line:{info.LineNumber}, Column:{info.LinePosition}");
			//	// Output: Error!!!, Wrong Source URI Style Name: SourceUri:MainPage.xaml;assembly=MAUI, Line:18, Column:17

			//	IEnumerable<Setter> setters = style.Setters.Reverse();
			//	// Requirement #3 Walking the setters
			//	foreach (Setter setter in setters)
			//	{
			//		Debug.WriteLine($"Setter Property:{setter.Property.PropertyName}, Setter Value:{setter.Value.ToString()}");

			//		// Output: OK! Setter Property:TextColor, Setter Value:[Color: Red=1, Green=0, Blue=0, Alpha=1] (MainStyle.xaml)
			//		// Output: OK! Setter Property:WidthRequest, Setter Value:100 (BaseStyle.xaml)
			//		// Output: OK! Setter Property:HeightRequest, Setter Value:100 (BaseStyle.xaml)
			//		// Output: OK! Setter Property:TextColor, Setter Value:[Color: Red=0, Green=0, Blue=1, Alpha=1]
			//	}

			//	// Requirement #4, Walking the Style Parent Chain
			//	style = style.BasedOn;
		}
	}
}