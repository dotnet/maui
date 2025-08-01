using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui23201
{
	public Maui23201() => InitializeComponent();

	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void ToolBarItemAppThemeBinding([Values] XamlInflator inflator)
		{
			Application.Current.Resources.Add("Black", Colors.DarkGray);
			Application.Current.Resources.Add("White", Colors.LightGray);

			Application.Current.UserAppTheme = AppTheme.Light;
			var page = new Maui23201(inflator);
			Application.Current.MainPage = page;

			Assert.That(((FontImageSource)(page.ToolbarItems[0].IconImageSource)).Color, Is.EqualTo(Colors.DarkGray));
			Assert.That(((FontImageSource)(page.ToolbarItems[1].IconImageSource)).Color, Is.EqualTo(Colors.Black));

			Application.Current.UserAppTheme = AppTheme.Dark;
			Assert.That(((FontImageSource)(page.ToolbarItems[0].IconImageSource)).Color, Is.EqualTo(Colors.LightGray));
			Assert.That(((FontImageSource)(page.ToolbarItems[1].IconImageSource)).Color, Is.EqualTo(Colors.White));

		}
	}
}