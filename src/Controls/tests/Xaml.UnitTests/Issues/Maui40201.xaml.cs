using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui40201 : ContentPage
{
	public Maui40201() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		readonly MockApplication _app;

		public Tests()
		{
			AppInfo.SetCurrent(new MockAppInfo());
			Application.Current = _app = new MockApplication();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Application.Current = null;
				AppInfo.SetCurrent(null);
			}

			base.Dispose(disposing);
		}

		[Theory]
		[XamlInflatorData]
		internal void StyledBorderBackgroundTracksAppTheme(XamlInflator inflator)
		{
			_app.UserAppTheme = AppTheme.Light;
			var page = new Maui40201(inflator);
			_app.LoadPage(page);

			AssertBackgrounds(Color.FromArgb("#e9f9ff"));

			_app.UserAppTheme = AppTheme.Dark;
			AssertBackgrounds(Color.FromArgb("#616161"));

			_app.UserAppTheme = AppTheme.Light;
			AssertBackgrounds(Color.FromArgb("#e9f9ff"));

			void AssertBackgrounds(Color expected)
			{
				Assert.Equal(expected, Assert.IsAssignableFrom<SolidColorBrush>(page.zeroStrokeBorder.Background).Color);
				Assert.Equal(expected, Assert.IsAssignableFrom<SolidColorBrush>(page.positiveStrokeBorder.Background).Color);
				Assert.Equal(expected, Assert.IsAssignableFrom<SolidColorBrush>(page.defaultBackgroundBorder.Background).Color);
			}
		}
	}
}
