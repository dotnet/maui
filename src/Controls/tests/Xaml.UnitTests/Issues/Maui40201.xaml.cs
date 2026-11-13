using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui40201 : ContentPage
{
	public Maui40201() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		readonly MockAppInfo _appInfo;
		readonly MockApplication _app;

		public Tests()
		{
			AppInfo.SetCurrent(_appInfo = new MockAppInfo());
			Application.Current = _app = new MockApplication();
		}

		public void Dispose()
		{
			Application.Current = null;
			AppInfo.SetCurrent(null);
		}

		[Theory]
		[XamlInflatorData]
		internal void AppThemeBindingInStyleSetterConvertsValues(XamlInflator inflator)
		{
			_appInfo.RequestedTheme = AppTheme.Light;
			_app.NotifyThemeChanged();
			var page = new Maui40201(inflator);
			var brush = Assert.IsType<SolidColorBrush>(page.border.Background);
			Assert.Equal(Color.FromArgb("#e9f9ff"), brush.Color);

			_appInfo.RequestedTheme = AppTheme.Dark;
			_app.NotifyThemeChanged();
			page = new Maui40201(inflator);
			brush = Assert.IsType<SolidColorBrush>(page.border.Background);
			Assert.Equal(Color.FromArgb("#616161"), brush.Color);
		}

		[Theory]
		[XamlInflatorData]
		internal void OtherStyleSettersStillApply(XamlInflator inflator)
		{
			_appInfo.RequestedTheme = AppTheme.Light;
			_app.NotifyThemeChanged();
			var page = new Maui40201(inflator);
			Assert.Equal(0d, page.border.StrokeThickness);
			var roundRectangle = Assert.IsType<RoundRectangle>(page.border.StrokeShape);
			Assert.Equal(new CornerRadius(20), roundRectangle.CornerRadius);
		}
	}
}
