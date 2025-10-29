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
	public class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}


		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Method(bool useCompiledXaml)
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