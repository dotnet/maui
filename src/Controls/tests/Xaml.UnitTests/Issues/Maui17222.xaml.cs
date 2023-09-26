using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
#if DEBUG
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
			Assert.AreEqual(new Uri($"Issues{System.IO.Path.DirectorySeparatorChar}Maui17222.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.AreEqual(2, info.LineNumber);
			Assert.AreEqual(2, info.LinePosition);

			var button = page.button;
			info = VisualDiagnostics.GetSourceInfo(button);
			Assert.AreEqual(new Uri($"Issues{System.IO.Path.DirectorySeparatorChar}Maui17222.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.AreEqual(6, info.LineNumber);
			Assert.AreEqual(6, info.LinePosition);

			Style style = button.Style;
			info = VisualDiagnostics.GetSourceInfo(style);
			Assert.AreEqual(new Uri($"Issues{System.IO.Path.DirectorySeparatorChar}Maui17222Style.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.AreEqual(5, info.LineNumber);
			Assert.AreEqual(6, info.LinePosition);

			var setter = style.Setters[0];
			Assert.AreEqual(setter.Property, Button.TextColorProperty);
			Assert.AreEqual(setter.Value, Colors.Red);
			info = VisualDiagnostics.GetSourceInfo(setter);
			Assert.AreEqual(new Uri($"Issues{System.IO.Path.DirectorySeparatorChar}Maui17222Style.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.AreEqual(6, info.LineNumber);
			Assert.AreEqual(10, info.LinePosition);


			style = style.BasedOn;
			info = VisualDiagnostics.GetSourceInfo(style);
			Assert.AreEqual(new Uri($"Issues{System.IO.Path.DirectorySeparatorChar}Maui17222BaseStyle.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.AreEqual(5, info.LineNumber);
			Assert.AreEqual(6, info.LinePosition);
		}
#endif
	}
}
