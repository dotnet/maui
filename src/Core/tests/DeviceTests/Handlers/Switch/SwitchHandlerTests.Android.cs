using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using AColor = global::Android.Graphics.Color;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;
using Color = Microsoft.Maui.Graphics.Color;
using MaterialSwitch = Google.Android.Material.MaterialSwitch.MaterialSwitch;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SwitchHandlerTests
	{
		[Fact]
		public Task MaterialSwitchKeepsCustomOnTintWhenTurningOffWithoutOffColor() =>
			InvokeOnMainThreadAsync(() =>
			{
				var context = MauiContext.Context!;
				using var platformView = new MaterialSwitch(MauiMaterialContextThemeWrapper.Create(context, useMaterial3: true));
				const int enabled = global::Android.Resource.Attribute.StateEnabled;
				const int checkedState = global::Android.Resource.Attribute.StateChecked;
				var themeTint = platformView.TrackTintList;
				Assert.NotNull(themeTint);
				var defaultOffColor = themeTint.GetColorForState([enabled], AColor.Transparent);
				var defaultOnColor = themeTint.GetColorForState([enabled, checkedState], AColor.Transparent);
				var customOnColor = Color.FromArgb(defaultOnColor == unchecked((int)0xFFDFD8F7) ? "#FF0000" : "#DFD8F7");

				var view = new SwitchStub { IsOn = true, TrackColor = customOnColor };
				platformView.UpdateIsOn(view);
				platformView.UpdateTrackColor(view);

				view.IsOn = false;
				view.TrackColor = null;
				platformView.UpdateIsOn(view);
				platformView.UpdateTrackColor(view);

				var transitionTint = platformView.TrackTintList;
				Assert.NotNull(transitionTint);
				Assert.Equal(customOnColor.ToPlatform().ToArgb(), transitionTint.GetColorForState([enabled, checkedState], AColor.Transparent));
				Assert.Equal(defaultOffColor, transitionTint.GetColorForState([enabled], AColor.Transparent));
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