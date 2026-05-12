using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SwitchHandlerTests
	{
		static readonly string[] ToggleSwitchThumbKeys =
		{
			"ToggleSwitchKnobFillOnPointerOver",
			"ToggleSwitchKnobFillOn",
			"ToggleSwitchKnobFillOnPressed",
			"ToggleSwitchKnobFillOnDisabled",
			"ToggleSwitchKnobFillOffPointerOver",
			"ToggleSwitchKnobFillOff",
			"ToggleSwitchKnobFillOffPressed",
			"ToggleSwitchKnobFillOffDisabled"
		};

		[Fact(DisplayName = "On and Off Content is null")]
		public async Task OnOffNullContent()
		{
			var switchStub = new SwitchStub()
			{
				IsOn = true
			};
			await AttachAndRun(switchStub, (handler) =>
			{
				var toggleSwitch = handler.PlatformView;
				Assert.NotNull(toggleSwitch);
				Assert.Null(toggleSwitch.OffContent);
				Assert.Null(toggleSwitch.OnContent);
			});
		}

		void SetIsOn(SwitchHandler switchHandler, bool value) =>
			GetNativeSwitch(switchHandler).IsOn = value;

		ToggleSwitch GetNativeSwitch(SwitchHandler switchHandler) =>
			(ToggleSwitch)switchHandler.PlatformView;

		bool GetNativeIsOn(SwitchHandler switchHandler) =>
			GetNativeSwitch(switchHandler).IsOn;

		Task ValidateTrackColor(ISwitch switchStub, Color color, Action action = null, string updatePropertyValue = null) =>
			ValidateHasColor(switchStub, color, action, updatePropertyValue: updatePropertyValue);

		Task ValidateThumbColor(ISwitch switchStub, Color color, Action action = null, string updatePropertyValue = null) =>
			ValidateHasColor(switchStub, color, action, updatePropertyValue: updatePropertyValue);

		[Fact(DisplayName = "Thumb Color Clears Correctly")]
		public async Task ThumbColorClearsCorrectly()
		{
			var switchStub = new SwitchStub();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(switchStub);
				var nativeSwitch = GetNativeSwitch(handler);

				switchStub.ThumbColor = Microsoft.Maui.Graphics.Colors.Red;
				handler.UpdateValue(nameof(ISwitch.ThumbColor));

				var thumbBrush = Assert.IsType<SolidColorBrush>(nativeSwitch.Resources["ToggleSwitchKnobFillOn"]);
				Assert.Equal(Microsoft.Maui.Graphics.Colors.Red.ToWindowsColor(), thumbBrush.Color);
				Assert.All(ToggleSwitchThumbKeys, key => Assert.True(nativeSwitch.Resources.ContainsKey(key)));

				switchStub.ThumbColor = null;
				handler.UpdateValue(nameof(ISwitch.ThumbColor));

				Assert.All(ToggleSwitchThumbKeys, key => Assert.False(nativeSwitch.Resources.ContainsKey(key)));
			});
		}
	}
}
