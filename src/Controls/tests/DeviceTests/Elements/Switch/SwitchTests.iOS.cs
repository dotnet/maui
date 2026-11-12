using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Switch)]
	public class SwitchTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Switch, SwitchHandler>();
				});
			});
		}

		[Fact(DisplayName = "OnColor is set while the Switch is off")]
		public async Task OnColorIsSetWhileTheSwitchIsOff()
		{
			// https://github.com/dotnet/maui/issues/34560
			SetupBuilder();
			var switchView = new Switch
			{
				IsToggled = false,
				OnColor = Colors.Purple,
			};

			var handler = await CreateHandlerAsync<SwitchHandler>(switchView);

			await InvokeOnMainThreadAsync(() =>
			{
				// A tap that turns the switch on then doesn't set the color while the switch animates,
				// which shows a solid track over the thumb with Liquid Glass
				Assert.Equal(Colors.Purple, handler.PlatformView.OnTintColor?.ToColor());

				switchView.OnColor = Colors.Orange;
				Assert.Equal(Colors.Orange, handler.PlatformView.OnTintColor?.ToColor());
			});
		}

		[Fact(DisplayName = "Turning the Switch on natively doesn't set its colors again")]
		public async Task TurningOnNativelyDoesNotSetTheColorsAgain()
		{
			// https://github.com/dotnet/maui/issues/34560
			SetupBuilder();
			var switchView = new Switch
			{
				IsToggled = false,
				OnColor = Colors.Purple,
			};

			var handler = await CreateHandlerAsync<SwitchHandler>(switchView);

			await InvokeOnMainThreadAsync(() =>
			{
				var uiSwitch = handler.PlatformView;
				var onTintColor = uiSwitch.OnTintColor;
				var trackColor = uiSwitch.GetTrackColor();
				Assert.NotNull(trackColor);

				// A tap turns the switch on and starts animating it before UIKit sends ValueChanged
				uiSwitch.SetState(true, true);
				uiSwitch.SendActionForControlEvents(UIControlEvent.ValueChanged);

				Assert.True(switchView.IsToggled);

				// With Liquid Glass, setting the colors while the switch animates shows a solid track over the thumb
				Assert.Equal(onTintColor?.Handle, uiSwitch.OnTintColor?.Handle);
				Assert.Equal(Colors.Purple, uiSwitch.OnTintColor?.ToColor());

				if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
				{
					Assert.Equal(trackColor.Handle, uiSwitch.GetTrackColor()?.Handle);
				}
			});
		}
	}
}
