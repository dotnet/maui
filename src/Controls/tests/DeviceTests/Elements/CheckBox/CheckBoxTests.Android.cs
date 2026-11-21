using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.CheckBox;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	public partial class CheckBoxTests
	{
		public class Material3CheckBoxHandler : CheckBoxHandler
		{
			protected override AppCompatCheckBox CreatePlatformView() =>
				new MauiMaterialCheckBox(MauiMaterialContextThemeWrapper.Create(Context, useMaterial3: true));
		}

		bool? GetNativeCheckedState(CheckBoxHandler handler) =>
			Assert.IsAssignableFrom<MaterialCheckBox>(handler.PlatformView).CheckedState switch
			{
				MaterialCheckBox.StateChecked => true,
				MaterialCheckBox.StateUnchecked => false,
				MaterialCheckBox.StateIndeterminate => null,
				var state => throw new InvalidOperationException($"Unexpected checked state: {state}")
			};

		void ActivateNativeCheckBox(CheckBoxHandler handler) =>
			handler.PlatformView.PerformClick();

		[Theory]
		[InlineData(false, false, true)]
		[InlineData(false, true, false)]
		[InlineData(true, false, true)]
		[InlineData(true, true, null)]
		[InlineData(true, null, false)]
		public Task ActivationDispatchesNativeClick(bool threeState, bool? initial, bool? expected)
		{
			SetupBuilder();
			return InvokeOnMainThreadAsync(() =>
			{
				var checkBox = new CheckBox { IsThreeState = threeState, IsChecked = initial };
				var handler = CreateThreeStateHandler(checkBox);
				var clicks = 0;
				EventHandler onClick = (_, _) =>
				{
					Assert.Equal(expected, checkBox.IsChecked);
					Assert.Equal(checkBox.IsChecked, GetNativeCheckedState(handler));
					clicks++;
				};
				handler.PlatformView.Click += onClick;
				try
				{
					Assert.True(handler.PlatformView.PerformClick());
					Assert.Equal(1, clicks);
				}
				finally
				{
					handler.PlatformView.Click -= onClick;
				}
			});
		}

		AppCompatCheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			checkBoxHandler.PlatformView;

		Task<float> GetPlatformOpacity(CheckBoxHandler CheckBoxHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeCheckBox(CheckBoxHandler);
				return nativeView.Alpha;
			});
		}

		[Fact]
		[Description("The ScaleX property of a CheckBox should match with native ScaleX")]
		public async Task ScaleXConsistent()
		{
			var checkBox = new CheckBox() { ScaleX = 0.45f };
			var expected = checkBox.ScaleX;
			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			var PlatformCheckBox = GetNativeCheckBox(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => PlatformCheckBox.ScaleX);
			Assert.Equal(expected, platformScaleX);
		}

		[Fact]
		[Description("The ScaleY property of a CheckBox should match with native ScaleY")]
		public async Task ScaleYConsistent()
		{
			var checkBox = new CheckBox() { ScaleY = 1.23f };
			var expected = checkBox.ScaleY;
			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			var PlatformCheckBox = GetNativeCheckBox(handler);
			var platformScaleY = await InvokeOnMainThreadAsync(() => PlatformCheckBox.ScaleY);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The Scale property of a CheckBox should match with native Scale")]
		public async Task ScaleConsistent()
		{
			var checkBox = new CheckBox() { Scale = 2.0f };
			var expected = checkBox.Scale;
			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			var PlatformCheckBox = GetNativeCheckBox(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => PlatformCheckBox.ScaleX);
			var platformScaleY = await InvokeOnMainThreadAsync(() => PlatformCheckBox.ScaleY);
			Assert.Equal(expected, platformScaleX);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The RotationX property of a CheckBox should match with native RotationX")]
		public async Task RotationXConsistent()
		{
			var checkBox = new CheckBox() { RotationX = 33.0 };
			var expected = checkBox.RotationX;
			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			var PlatformCheckBox = GetNativeCheckBox(handler);
			var platformRotationX = await InvokeOnMainThreadAsync(() => PlatformCheckBox.RotationX);
			Assert.Equal(expected, platformRotationX);
		}

		[Fact]
		[Description("The RotationY property of a CheckBox should match with native RotationY")]
		public async Task RotationYConsistent()
		{
			var checkBox = new CheckBox() { RotationY = 87.0 };
			var expected = checkBox.RotationY;
			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			var PlatformCheckBox = GetNativeCheckBox(handler);
			var platformRotationY = await InvokeOnMainThreadAsync(() => PlatformCheckBox.RotationY);
			Assert.Equal(expected, platformRotationY);
		}

		[Fact]
		[Description("The Rotation property of a CheckBox should match with native Rotation")]
		public async Task RotationConsistent()
		{
			var checkBox = new CheckBox() { Rotation = 23.0 };
			var expected = checkBox.Rotation;
			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			var PlatformCheckBox = GetNativeCheckBox(handler);
			var platformRotation = await InvokeOnMainThreadAsync(() => PlatformCheckBox.Rotation);
			Assert.Equal(expected, platformRotation);
		}

		[Fact("The IsEnabled of a CheckBox should match with native IsEnabled")]
		public async Task CheckBoxIsEnabled()
		{
			var checkBox = new CheckBox();
			checkBox.IsEnabled = false;
			var expectedValue = checkBox.IsEnabled;

			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			var nativeView = GetNativeCheckBox(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var isEnabled = nativeView.Enabled;
				Assert.Equal(expectedValue, isEnabled);
			});
		}

		Task<bool> GetPlatformIsVisible(CheckBoxHandler checkBoxHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeCheckBox(checkBoxHandler);
				return nativeView.Visibility == global::Android.Views.ViewStates.Visible;
			});
		}

		//src/Compatibility/Core/tests/Android/TranslationTests.cs
		[Fact]
		[Description("The Translation property of a CheckBox should match with native Translation")]
		public async Task CheckBoxTranslationConsistent()
		{
			var checkBox = new CheckBox()
			{
				TranslationX = 50,
				TranslationY = -20
			};

			var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
			var nativeView = GetNativeCheckBox(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				AssertTranslationMatches(nativeView, checkBox.TranslationX, checkBox.TranslationY);
			});
		}
	}
}