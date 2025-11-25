using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CheckBoxHandlerTests
	{
		[Fact(DisplayName = "Accessibility Value Sets Correctly")]
		public async Task AccessibilityValueSetsCorrectly()
		{
			var checkboxStub = new CheckBoxStub()
			{
				IsChecked = true
			};

			var onValue = await GetValueAsync(checkboxStub, (handler) => GetNativeCheckBox(handler).AccessibilityValue);
			await SetValueAsync(checkboxStub, false, (handler, value) =>
			{
				handler.VirtualView.IsChecked = value;
				handler.UpdateValue(nameof(ICheckBox.IsChecked));
			});
			var offValue = await GetValueAsync(checkboxStub, (handler) => GetNativeCheckBox(handler).AccessibilityValue);

			Assert.Equal("1", onValue);
			Assert.Equal("0", offValue);
		}

		[Fact(DisplayName = "Accessibility Traits Init Correctly")]
		public async Task AccessibilityTraitsSetWithHeading()
		{
			var checkboxStub = new CheckBoxStub()
			{
				IsChecked = true,
				Semantics = new Semantics() { HeadingLevel = SemanticHeadingLevel.Level4 }
			};

			var traits = await GetValueAsync(checkboxStub, (handler) => GetNativeCheckBox(handler).AccessibilityTraits);
			var expectedTraits = await InvokeOnMainThreadAsync(() => new UISwitch().AccessibilityTraits);

			Assert.True(traits.HasFlag(expectedTraits));
			Assert.True(traits.HasFlag(UIAccessibilityTrait.Header));
		}

		[Fact(DisplayName = "Disabled CheckBox With Color Sets Disabled State Image")]
		public async Task DisabledCheckBoxWithColorSetsDisabledStateImage()
		{
			var checkboxStub = new CheckBoxStub()
			{
				Foreground = new SolidPaint(Colors.Purple),
				IsEnabled = false,
				IsChecked = true
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(checkboxStub);
				await Task.Yield();

				var nativeCheckBox = GetNativeCheckBox(handler);
				
				// Verify that the checkbox has an image set for the disabled state
				var disabledImage = nativeCheckBox.ImageForState(UIControlState.Disabled);
				Assert.NotNull(disabledImage);

				// Verify that the tint color is preserved
				Assert.NotNull(nativeCheckBox.CheckBoxTintColor);
				Assert.Equal(Colors.Purple, nativeCheckBox.CheckBoxTintColor);
			});
		}

		[Fact(DisplayName = "Enabled CheckBox Does Not Set Disabled State Image")]
		public async Task EnabledCheckBoxDoesNotSetDisabledStateImage()
		{
			var checkboxStub = new CheckBoxStub()
			{
				Foreground = new SolidPaint(Colors.Purple),
				IsEnabled = true,
				IsChecked = true
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(checkboxStub);
				await Task.Yield();

				var nativeCheckBox = GetNativeCheckBox(handler);
				
				// Verify that the enabled checkbox does not have an image set for the disabled state
				var disabledImage = nativeCheckBox.ImageForState(UIControlState.Disabled);
				Assert.Null(disabledImage);
			});
		}


		MauiCheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			(MauiCheckBox)checkBoxHandler.PlatformView;

		bool GetNativeIsChecked(CheckBoxHandler checkBoxHandler) =>
			GetNativeCheckBox(checkBoxHandler).IsChecked;

		async Task ValidateColor(ICheckBox checkBoxStub, Color color, Action action = null)
		{
			var expected = await GetValueAsync(checkBoxStub, handler =>
			{
				var native = GetNativeCheckBox(handler);
				action?.Invoke();
				return native.CheckBoxTintColor;
			});
			Assert.Equal(expected, color);
		}
	}
}