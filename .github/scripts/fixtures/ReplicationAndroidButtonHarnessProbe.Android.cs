using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public class ReplicationAndroidButtonHarnessProbe : ControlsHandlerTestBase
	{
		[Fact]
		[Category("ReplicationAndroidButtonHarnessProbe")]
		public async Task RegisteredButtonAttachesToWindow()
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
				handlers.AddHandler<global::Microsoft.Maui.Controls.Button, global::Microsoft.Maui.Handlers.ButtonHandler>()));

			var button = new Button
			{
				Text = "CI",
				WidthRequest = 64,
				HeightRequest = 64,
				CornerRadius = 32,
				BorderWidth = 0,
				FontSize = 36,
				BackgroundColor = global::Microsoft.Maui.Graphics.Colors.Red,
				TextColor = global::Microsoft.Maui.Graphics.Colors.White,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			global::Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Button.SetUseDefaultPadding(button, false);

			await CreateHandlerAndAddToWindow<global::Microsoft.Maui.Handlers.ButtonHandler>(
				new Window(new ContentPage { Content = button }), async handler =>
				{
					await AssertEventually(() => button.Handler != null && button.IsLoaded);
					Assert.Same(button.Handler, handler);
					Assert.True(button.Width >= 63 && button.Width <= 65 && button.Height >= 63 && button.Height <= 65);
					Assert.True(handler.PlatformView.Width > 0 && handler.PlatformView.Height > 0);
				});
		}
	}
}
