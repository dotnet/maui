using System;
using System.Threading.Tasks;
using Foundation;
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

		sealed class ThreeStateCheckBoxStub : CheckBoxStub, ICheckBox2
		{
			public new bool? IsChecked { get; set; }
			public bool IsThreeState => true;

			bool ICheckBox.IsChecked
			{
				get => IsChecked == true;
				set => IsChecked = value;
			}
		}

		[Fact(DisplayName = "Mixed Accessibility Value Updates Through Boolean States")]
		public Task MixedAccessibilityValueUpdatesThroughBooleanStates()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var checkBoxStub = new ThreeStateCheckBoxStub { IsChecked = null };
				var handler = CreateHandler(checkBoxStub);
				var native = GetNativeCheckBox(handler);
				var mixedValue = NSBundle.FromIdentifier("com.apple.UIKit")?.GetLocalizedString("Mixed") ?? "Mixed";

				Assert.Equal(mixedValue, native.AccessibilityValue);
				Assert.NotEqual("0", native.AccessibilityValue);
				Assert.NotEqual("1", native.AccessibilityValue);

				foreach (bool? state in new bool?[] { true, false, null })
				{
					((ICheckBox2)checkBoxStub).IsChecked = state;
					handler.UpdateValue(nameof(ICheckBox.IsChecked));

					Assert.Equal(state is null ? mixedValue : state == true ? "1" : "0", native.AccessibilityValue);
				}
			});
		}

		[Theory(DisplayName = "Indeterminate Image Is Distinct and Survives Tint Reset")]
		[InlineData(true)]
		[InlineData(false)]
		public Task IndeterminateImageIsDistinctAndSurvivesTintReset(bool isEnabled)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var checkBoxStub = new ThreeStateCheckBoxStub { IsChecked = null, IsEnabled = isEnabled };
				var handler = CreateHandler(checkBoxStub);
				var native = GetNativeCheckBox(handler);
				var defaultTint = native.TintColor;
				Assert.Equal(isEnabled, native.Enabled);

				UIImage GetImage(bool? state)
				{
					checkBoxStub.IsChecked = state;
					handler.UpdateValue(nameof(ICheckBox.IsChecked));
					var image = native.ImageForState(UIControlState.Normal);
					Assert.NotNull(image);
					return image;
				}

				byte[] GetImageBytes(bool? state)
				{
					using var data = GetImage(state).AsPNG();
					Assert.NotNull(data);
					return data.ToArray();
				}

				byte[] AssertMixedImage()
				{
					var image = GetImage(null);
					Assert.NotNull(image.CGImage);
					var x = (int)image.CGImage.Width / 2;
					var y = (int)image.CGImage.Height / 2;
					var fillY = (int)image.CGImage.Height / 3;

					// Allow edge antialiasing on 1x displays while requiring a mostly transparent dash.
					Assert.InRange(image.GetPixel(x, y)[3], (byte)0, (byte)127);
					Assert.Equal((byte)255, image.GetPixel(x, fillY)[3]);

					var mixed = GetImageBytes(null);
					Assert.NotEqual<byte>(mixed, GetImageBytes(false));
					Assert.NotEqual<byte>(mixed, GetImageBytes(true));
					GetImage(null);
					return mixed;
				}

				var defaultMixed = AssertMixedImage();
				byte[] previousTintedImage = null;
				foreach (var color in new[] { Colors.Red, Colors.Blue })
				{
					checkBoxStub.Foreground = new SolidPaint(color);
					handler.UpdateValue(nameof(ICheckBox.Foreground));
					var mixed = AssertMixedImage();

					if (isEnabled)
					{
						Assert.Equal(color.ToPlatform(), native.ImageView.TintColor);
					}
					else
					{
						var image = GetImage(null);
						Assert.Equal(UIImageRenderingMode.AlwaysOriginal, image.RenderingMode);
						image.AssertColorAtPoint(color.ToPlatform(), (int)image.CGImage.Width / 2, (int)image.CGImage.Height / 3);
						if (previousTintedImage is not null)
							Assert.NotEqual<byte>(previousTintedImage, mixed);
					}

					previousTintedImage = mixed;
				}

				checkBoxStub.Foreground = null;
				handler.UpdateValue(nameof(ICheckBox.Foreground));
				Assert.Null(native.CheckBoxTintColor);
				Assert.Equal(defaultTint, native.TintColor);
				Assert.Equal(UIImageRenderingMode.AlwaysTemplate, GetImage(null).RenderingMode);
				Assert.Equal<byte>(defaultMixed, AssertMixedImage());
			});
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

		bool GetNativeIsChecked(CheckBoxHandler checkBoxHandler)
		{
			var value = GetNativeCheckBox(checkBoxHandler).IsChecked;
			Assert.True(value.HasValue);
			return value.Value;
		}

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