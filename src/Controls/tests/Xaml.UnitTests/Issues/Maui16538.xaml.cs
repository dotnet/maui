using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16538
{
	public Maui16538() => InitializeComponent();

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
		public void VSMandAppTheme([Values] XamlInflator inflator)
		{

			Application.Current.UserAppTheme = AppTheme.Dark;
			var page = new Maui16538(inflator);
			Application.Current.MainPage = page;
			Button button = page.button0;
			Assert.That(button.BackgroundColor, Is.EqualTo(Color.FromHex("404040")));
			button.IsEnabled = true;
			Assert.That(button.BackgroundColor, Is.EqualTo(Colors.White));
			Application.Current.UserAppTheme = AppTheme.Light;
			Assert.That(button.BackgroundColor, Is.EqualTo(Color.FromHex("512BD4")));
		}
	}
}