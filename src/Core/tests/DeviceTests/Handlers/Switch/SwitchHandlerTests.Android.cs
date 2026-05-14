using System;
using System.Threading.Tasks;
using Android.Graphics;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SwitchHandlerTests
	{
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

		[Fact(DisplayName = "Thumb Color Clears Correctly")]
		public async Task ThumbColorClearsCorrectly()
		{
			var switchStub = new SwitchStub
			{
				ThumbColor = Colors.Red
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<NullThumbTintSwitchHandler>(switchStub);
				var nativeSwitch = GetNativeSwitch(handler);

				Assert.NotNull(nativeSwitch.ThumbTintList);

				Assert.Equal(Colors.Red.ToPlatform().ToArgb(), GetThumbTintColor(nativeSwitch));

				switchStub.ThumbColor = null;
				handler.UpdateValue(nameof(ISwitch.ThumbColor));

				Assert.Null(nativeSwitch.ThumbTintList);
			});
		}

		class NullThumbTintSwitchHandler : SwitchHandler
		{
			protected override ASwitch CreatePlatformView()
			{
				var nativeSwitch = base.CreatePlatformView();
				nativeSwitch.ThumbTintList = null;
				return nativeSwitch;
			}
		}

		static int? GetThumbTintColor(ASwitch aSwitch)
		{
			var thumbTintList = aSwitch.ThumbTintList;

			return thumbTintList?.GetColorForState(aSwitch.GetDrawableState(), new global::Android.Graphics.Color(thumbTintList.DefaultColor));
		}
	}
}