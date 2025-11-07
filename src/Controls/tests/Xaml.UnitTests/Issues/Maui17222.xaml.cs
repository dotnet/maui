using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17222 : ContentPage
{

	public Maui17222() => InitializeComponent();


	public class Test : IDisposable
	{
#if DEBUG
		bool enableDiagnosticsInitialState;

		public Test()
		{
			AppInfo.SetCurrent(new MockAppInfo());
			enableDiagnosticsInitialState = RuntimeFeature.EnableDiagnostics;
			RuntimeFeature.EnableMauiDiagnostics = true;
		}

		public void Dispose()
		{
			RuntimeFeature.EnableMauiDiagnostics = enableDiagnosticsInitialState;
			AppInfo.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void GetsourceInfo(XamlInflator inflator)
		{
			var app = new MockApplication();
			app.Resources.Add(new Maui17222BaseStyle(inflator));
			app.Resources.Add(new Maui17222Style(inflator));
			Application.SetCurrentApplication(app);

			var page = new Maui17222(inflator);
			SourceInfo info = VisualDiagnostics.GetSourceInfo(page);
			Assert.Equal(new Uri($"Issues{System.IO.Path.DirectorySeparatorChar}Maui17222.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.Equal(2, info.LineNumber);
			Assert.Equal(2, info.LinePosition);

			var button = page.button;
			info = VisualDiagnostics.GetSourceInfo(button);
			Assert.Equal(new Uri($"Issues{System.IO.Path.DirectorySeparatorChar}Maui17222.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.Equal(6, info.LineNumber);
			Assert.Equal(6, info.LinePosition);

			Style style = button.Style;
			info = VisualDiagnostics.GetSourceInfo(style);
			Assert.Equal(new Uri($"Issues{System.IO.Path.DirectorySeparatorChar}Maui17222Style.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.Equal(5, info.LineNumber);
			Assert.Equal(6, info.LinePosition);

			var setter = style.Setters[0];
			Assert.Equal(setter.Property, Button.TextColorProperty);
			Assert.Equal(setter.Value, Colors.Red);
			info = VisualDiagnostics.GetSourceInfo(setter);
			Assert.Equal(new Uri($"Issues{System.IO.Path.DirectorySeparatorChar}Maui17222Style.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.Equal(6, info.LineNumber);
			Assert.Equal(10, info.LinePosition);

			style = style.BasedOn;
			info = VisualDiagnostics.GetSourceInfo(style);
			Assert.Equal(new Uri($"Issues{System.IO.Path.DirectorySeparatorChar}Maui17222BaseStyle.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), info.SourceUri);
			Assert.Equal(5, info.LineNumber);
			Assert.Equal(6, info.LinePosition);
		}
#endif
	}
}
