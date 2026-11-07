using System;
using System.Threading.Tasks;
using Android.Graphics;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SwitchHandlerTests
	{
		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(true, true)]
		public Task ClearingThumbColorRestoresNativeAppearance(bool isOn, bool useExplicitTintMode) =>
			InvokeOnMainThreadAsync(async () =>
			{
				var switchStub = new SwitchStub { IsOn = !isOn };
				var handler = CreateHandler(switchStub);
				var nativeSwitch = GetNativeSwitch(handler);
				if (useExplicitTintMode)
				{
					nativeSwitch.ThumbTintMode = PorterDuff.Mode.SrcIn;
				}

				var originalMode = nativeSwitch.ThumbTintMode;
				nativeSwitch.JumpDrawablesToCurrentState();
				using var oppositeState = await nativeSwitch.ToBitmap(MauiContext);

				switchStub.IsOn = isOn;
				handler.UpdateValue(nameof(ISwitch.IsOn));
				nativeSwitch.JumpDrawablesToCurrentState();
				using var before = await nativeSwitch.ToBitmap(MauiContext);

				foreach (var color in new[] { Colors.Red, Colors.Blue })
				{
					switchStub.ThumbColor = color;
					handler.UpdateValue(nameof(ISwitch.ThumbColor));
					nativeSwitch.JumpDrawablesToCurrentState();
					using var custom = await nativeSwitch.ToBitmap(MauiContext);
					await custom.AssertContainsColor(color);
					Assert.Equal(before.Width, custom.Width);
					Assert.Equal(before.Height, custom.Height);
					for (var y = 0; y < before.Height; y++)
					{
						for (var x = 0; x < before.Width; x++)
						{
							Assert.Equal(before.GetPixel(x, y) >> 24, custom.GetPixel(x, y) >> 24);
						}
					}
				}

				switchStub.ThumbColor = null;
				handler.UpdateValue(nameof(ISwitch.ThumbColor));
				handler.UpdateValue(nameof(ISwitch.ThumbColor));
				nativeSwitch.JumpDrawablesToCurrentState();
				Assert.Equal(originalMode, nativeSwitch.ThumbTintMode);
				using var restored = await nativeSwitch.ToBitmap(MauiContext);
				await before.AssertEqualAsync(restored);

				switchStub.IsOn = !isOn;
				handler.UpdateValue(nameof(ISwitch.IsOn));
				nativeSwitch.JumpDrawablesToCurrentState();
				using var toggled = await nativeSwitch.ToBitmap(MauiContext);
				await oppositeState.AssertEqualAsync(toggled);
			});

		void SetIsOn(SwitchHandler switchHandler, bool value) =>
			GetNativeSwitch(switchHandler).Checked = value;

		ASwitch GetNativeSwitch(SwitchHandler switchHandler) =>
			(ASwitch)switchHandler.PlatformView;

		bool GetNativeIsOn(SwitchHandler switchHandler) =>
			GetNativeSwitch(switchHandler).Checked;

		Task ValidateTrackColor(ISwitch switchStub, Color color, Action action = null, string updatePropertyValue = null) =>
			ValidateHasColor(switchStub, color, action, updatePropertyValue: updatePropertyValue);

		Task ValidateThumbColor(ISwitch switchStub, Color color, Action action = null, string updatePropertyValue = null) =>
			ValidateHasColor(switchStub, color, action, updatePropertyValue: updatePropertyValue);
	}
}