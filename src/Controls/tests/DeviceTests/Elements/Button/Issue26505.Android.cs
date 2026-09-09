using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public class Issue26505 : global::Microsoft.Maui.DeviceTests.ControlsHandlerTestBase
	{
		[Fact]
		[Category("Issue26505")]
		public async Task DisabledDefaultPaddingAllowsTextToFit()
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<global::Microsoft.Maui.Controls.Button, global::Microsoft.Maui.Handlers.ButtonHandler>()));
			var button = new Button { Text = "CI", WidthRequest = 64, HeightRequest = 64, CornerRadius = 32, BorderWidth = 0, BackgroundColor = global::Microsoft.Maui.Graphics.Colors.Red, TextColor = global::Microsoft.Maui.Graphics.Colors.White, HorizontalOptions = global::Microsoft.Maui.Controls.LayoutOptions.Center, VerticalOptions = global::Microsoft.Maui.Controls.LayoutOptions.Center };
			global::Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Button.SetUseDefaultPadding(button, false);
			var applyReportedTrigger = true;
			if (applyReportedTrigger)
			{
				button.FontSize = 36;
			}
			await CreateHandlerAndAddToWindow<global::Microsoft.Maui.Handlers.ButtonHandler>(new Window(new ContentPage { Content = button }), async handler =>
			{
				await AssertEventually(() => button.Handler != null && button.IsLoaded);
				Assert.True(button.Width >= 63 && button.Width <= 65 && button.Height >= 63 && button.Height <= 65);
				Assert.True(handler.PlatformView.Paint.MeasureText(handler.PlatformView.Text) <= handler.PlatformView.Width - handler.PlatformView.CompoundPaddingLeft - handler.PlatformView.CompoundPaddingRight);
			});
		}
	}
}

