using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui13619 : ContentPage
{
	public Maui13619() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : BaseTestFixture
	{
		public Tests()
		{
			AppInfo.SetCurrent(new MockAppInfo());
			Application.Current = new MockApplication { UserAppTheme = AppTheme.Light };
		}

		[Theory]
		[XamlInflatorData]
		internal void AppThemeBindingAndDynamicResource(XamlInflator inflator)
		{
			var page = new Maui13619(inflator);
			Assert.Equal(Colors.HotPink, page.label0.TextColor);
			Assert.Equal(Colors.DarkGray, page.label0.BackgroundColor);

			page.Resources["Primary"] = Colors.SlateGray;
			Assert.Equal(Colors.SlateGray, page.label0.BackgroundColor);

		}
	}
}
