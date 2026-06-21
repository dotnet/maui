using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui21774
{
	public Maui21774() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void AppThemeChangeOnUnparentedPage(XamlInflator inflator)
		{
			Application.Current.Resources.Add("labelColor", Colors.LimeGreen);
			Application.Current.UserAppTheme = AppTheme.Light;
			var page = new Maui21774(inflator);
			Application.Current.MainPage = page;

			Assert.Equal(Colors.LimeGreen, page.label0.TextColor);
			Assert.Equal(Colors.LimeGreen, page.label1.TextColor);

			//unparent the page, change the resource and the theme
			Application.Current.MainPage = null;
			Application.Current.Resources["labelColor"] = Colors.HotPink;
			Application.Current.UserAppTheme = AppTheme.Dark;
			//labels should not change
			Assert.Equal(Colors.LimeGreen, page.label0.TextColor);
			Assert.Equal(Colors.LimeGreen, page.label1.TextColor);

			//reparent the page
			Application.Current.MainPage = page;
			//labels should change
			Assert.Equal(Colors.HotPink, page.label0.TextColor);
			Assert.Equal(Colors.HotPink, page.label1.TextColor);
		}
	}
}
