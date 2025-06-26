using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16960
{

	public Maui16960() => InitializeComponent();

	public Maui16960(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	class Test
	{
		// Constructor
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// IDisposable public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
		public void VSMandAppTheme([Theory]
		[InlineData(false)]
		[InlineData(true)] bool useCompiledXaml)
		{

			Application.Current.UserAppTheme = AppTheme.Light;

			var page = new Maui16960(useCompiledXaml);
			Button button = page.button;
			Assert.Null(button.BackgroundColor);

			VisualStateManager.GoToState(button, "PointerOver");
			Assert.Equal(Colors.Red, button.BackgroundColor);

			VisualStateManager.GoToState(button, "Pressed");
			Assert.Equal(Colors.Yellow, button.BackgroundColor);

			VisualStateManager.GoToState(button, "Normal");
			Assert.Null(button.BackgroundColor);

			VisualStateManager.GoToState(button, "PointerOver");
			Assert.Equal(Colors.Red, button.BackgroundColor);

		}
	}
}