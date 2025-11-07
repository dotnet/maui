using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui23201
{
	public Maui23201() => InitializeComponent();

	public class Test
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void ToolBarItemAppThemeBinding(XamlInflator inflator)
		{
			Application.Current.Resources.Add("Black", Colors.DarkGray);
			Application.Current.Resources.Add("White", Colors.LightGray);

			Application.Current.UserAppTheme = AppTheme.Light;
			var page = new Maui23201(inflator);
			Application.Current.MainPage = page;

			Assert.Equal(Colors.DarkGray, ((FontImageSource)(page.ToolbarItems[0].IconImageSource)).Color);
			Assert.Equal(Colors.Black, ((FontImageSource)(page.ToolbarItems[1].IconImageSource)).Color);

			Application.Current.UserAppTheme = AppTheme.Dark;
			Assert.Equal(Colors.LightGray, ((FontImageSource)(page.ToolbarItems[0].IconImageSource)).Color);
			Assert.Equal(Colors.White, ((FontImageSource)(page.ToolbarItems[1].IconImageSource)).Color);

		}
	}
}
