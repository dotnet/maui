using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16960
{

	public Maui16960() => InitializeComponent();

	public Maui16960(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
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
		public void VSMandAppTheme([Values(false, true)] bool useCompiledXaml)
		{

			Application.Current.UserAppTheme = AppTheme.Light;

			var page = new Maui16960(useCompiledXaml);
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