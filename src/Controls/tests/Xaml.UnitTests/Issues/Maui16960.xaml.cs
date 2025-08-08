using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16960
{
	public Maui16960() => InitializeComponent();

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

			Application.Current.UserAppTheme = AppTheme.Light;

			var page = new Maui16960(inflator);
			Button button = page.button;
			Assert.That(button.BackgroundColor, Is.Null);

			VisualStateManager.GoToState(button, "PointerOver");
			Assert.That(button.BackgroundColor, Is.EqualTo(Colors.Red));

			VisualStateManager.GoToState(button, "Pressed");
			Assert.That(button.BackgroundColor, Is.EqualTo(Colors.Yellow));

			VisualStateManager.GoToState(button, "Normal");
			Assert.That(button.BackgroundColor, Is.Null);

			VisualStateManager.GoToState(button, "PointerOver");
			Assert.That(button.BackgroundColor, Is.EqualTo(Colors.Red));


		}
	}
}