using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16538
{
	public Maui16538() => InitializeComponent();

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
		internal void VSMandAppTheme(XamlInflator inflator)
		{

			Application.Current.UserAppTheme = AppTheme.Dark;
			var page = new Maui16538(inflator);
			Application.Current.MainPage = page;
			Button button = page.button0;
			Assert.Equal(Color.FromHex("404040"), button.BackgroundColor);
			button.IsEnabled = true;
			Assert.Equal(Colors.White, button.BackgroundColor);
			Application.Current.UserAppTheme = AppTheme.Light;
			Assert.Equal(Color.FromHex("512BD4"), button.BackgroundColor);
		}
	}
}