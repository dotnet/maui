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

		[Fact(DisplayName = "Foreground Resets to Default When Set to Null")]
		public async Task ForegroundResetsToDefaultWhenSetToNull()
		{
			var checkBoxStub = new CheckBoxStub
			{
				Foreground = new SolidPaint(Colors.Red),
				IsChecked = true
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(checkBoxStub);
				var native = GetNativeCheckBox(handler);

				// Confirm that the red tint was applied initially
				Assert.NotNull(native.CheckBoxTintColor);

				// Simulate a dynamic Color = null (Foreground reset to default)
				checkBoxStub.Foreground = null;
				handler.UpdateValue(nameof(ICheckBox.Foreground));

				// After reset, the native tint must be null so iOS reverts to its inherited default
				Assert.Null(native.CheckBoxTintColor);
			});
		}
	}
}