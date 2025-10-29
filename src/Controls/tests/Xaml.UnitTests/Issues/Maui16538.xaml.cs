using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16538
{

	public Maui16538() => InitializeComponent();

	public Maui16538(bool useCompiledXaml)
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

			Application.Current.UserAppTheme = AppTheme.Dark;
			var page = new Maui16538(useCompiledXaml);
			Application.Current.MainPage = page;
			Button button = page.button0;
			Assert.Equal(Color.FromHex("404040", button.BackgroundColor));
			button.IsEnabled = true;
			Assert.Equal(Colors.White, button.BackgroundColor);
			Application.Current.UserAppTheme = AppTheme.Light;
			Assert.Equal(Color.FromHex("512BD4", button.BackgroundColor));
		}
	}
}