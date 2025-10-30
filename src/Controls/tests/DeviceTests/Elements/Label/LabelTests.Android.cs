using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Text;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelTests
	{
		[Fact(DisplayName = "Html Text Initializes Correctly")]
		public async Task HtmlTextInitializesCorrectly()
		{
			var expected = "Html";

			var label = new Label()
			{
				Text = $"&lt;b&gt;{expected}&lt;/b&gt;",
				TextType = TextType.Html
			};

			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var platformText = await InvokeOnMainThreadAsync(() => TextForHandler(handler));
			Assert.Equal(expected, platformText);
		}

		// This test will only run if the Android Manifest of the Controls.DeviceTests project is edited to have android:supportsRtl="false"
		[Fact(DisplayName = "Horizontal text aligned when RTL is not supported")]
		public async Task HorizontalTextAlignedWhenRtlIsFalse()
		{
			if (Rtl.IsSupported)
				return;

			var label = new Label { Text = "Foo", HorizontalTextAlignment = TextAlignment.Center };

			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var platformLabel = GetPlatformLabel(handler);

			Assert.False(platformLabel.Gravity.HasFlag(GravityFlags.Start), "Label should not have the Start flag.");
			Assert.False(platformLabel.Gravity.HasFlag(GravityFlags.End), "Label should not have the End flag.");
			Assert.True(platformLabel.Gravity.HasFlag(GravityFlags.CenterHorizontal), "Label should have the CenterHorizontal flag.");
		}

		// This test will only run if the Android Manifest of the Controls.DeviceTests project is edited to have android:supportsRtl="false"
		[Fact(DisplayName = "Vertical text aligned when RTL is not supported")]
		public async Task VerticalTextAlignedWhenRtlIsFalse()
		{
			if (Rtl.IsSupported)
				return;

			var label = new Label { Text = "Foo", VerticalTextAlignment = TextAlignment.Center };

			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var platformLabel = GetPlatformLabel(handler);

			Assert.False(platformLabel.Gravity.HasFlag(GravityFlags.Top), "Label should not have the Top flag.");
			Assert.False(platformLabel.Gravity.HasFlag(GravityFlags.Bottom), "Label should not have the Bottom flag.");
			Assert.True(platformLabel.Gravity.HasFlag(GravityFlags.CenterVertical), "Label should only have the CenterVertical flag.");
		}

		// https://github.com/dotnet/maui/issues/18059
		[Fact(DisplayName = "Using TailTruncation LineBreakMode with 2 MaxLines")]
		public async Task UsingTailTruncationWith2MaxLines()
		{
			var label = new Label()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				LineBreakMode = LineBreakMode.TailTruncation,
				MaxLines = 2
			};

			var handler = await CreateHandlerAsync<LabelHandler>(label);

			var platformLabel = GetPlatformLabel(handler);

			await InvokeOnMainThreadAsync((System.Action)(() =>
			{
				Assert.Equal(2, GetPlatformMaxLines(handler));
				Assert.Equal(LineBreakMode.TailTruncation.ToPlatform(), GetPlatformLineBreakMode(handler));
			}));
		}

		[Fact]
		[Description("The ScaleX property of a Label should match with native ScaleX")]
		public async Task ScaleXConsistent()
		{
			var label = new Label() { ScaleX = 0.45f };
			var expected = label.ScaleX;
			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var platformLabel = GetPlatformLabel(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => platformLabel.ScaleX);
			Assert.Equal(expected, platformScaleX);
		}

		[Fact]
		[Description("The ScaleY property of a Label should match with native ScaleY")]
		public async Task ScaleYConsistent()
		{
			var label = new Label() { ScaleY = 1.23f };
			var expected = label.ScaleY;
			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var platformLabel = GetPlatformLabel(handler);
			var platformScaleY = await InvokeOnMainThreadAsync(() => platformLabel.ScaleY);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The Scale property of a Label should match with native Scale")]
		public async Task ScaleConsistent()
		{
			var label = new Label() { Scale = 2.0f };
			var expected = label.Scale;
			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var platformLabel = GetPlatformLabel(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => platformLabel.ScaleX);
			var platformScaleY = await InvokeOnMainThreadAsync(() => platformLabel.ScaleY);
			Assert.Equal(expected, platformScaleX);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The RotationX property of a Label should match with native RotationX")]
		public async Task RotationXConsistent()
		{
			var label = new Label() { RotationX = 33.0 };
			var expected = label.RotationX;
			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var platformLabel = GetPlatformLabel(handler);
			var platformRotationX = await InvokeOnMainThreadAsync(() => platformLabel.RotationX);
			Assert.Equal(expected, platformRotationX);
		}

		[Fact]
		[Description("The RotationY property of a Label should match with native RotationY")]
		public async Task RotationYConsistent()
		{
			var label = new Label() { RotationY = 87.0 };
			var expected = label.RotationY;
			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var platformLabel = GetPlatformLabel(handler);
			var platformRotationY = await InvokeOnMainThreadAsync(() => platformLabel.RotationY);
			Assert.Equal(expected, platformRotationY);
		}

		[Fact]
		[Description("The Rotation property of a Label should match with native Rotation")]
		public async Task RotationConsistent()
		{
			var label = new Label() { Rotation = 23.0 };
			var expected = label.Rotation;
			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var platformLabel = GetPlatformLabel(handler);
			var platformRotation = await InvokeOnMainThreadAsync(() => platformLabel.Rotation);
			Assert.Equal(expected, platformRotation);
		}

		[Fact]
		[Description("The IsEnabled property of a Label should match with native IsEnabled")]
		public async Task VerifyLabelIsEnabledProperty()
		{
			var label = new Label
			{
				IsEnabled = false
			};
			var expectedValue = label.IsEnabled;

			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var nativeView = GetPlatformLabel(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var isEnabled = nativeView.Enabled;
				Assert.Equal(expectedValue, isEnabled);
			});
		}

		//src/Compatibility/Core/tests/Android/TranslationTests.cs
		[Fact]
		[Description("The Translation property of a Label should match with native Translation")]
		public async Task LabelTranslationConsistent()
		{
			var label = new Label()
			{
				Text = "Label Test",
				TranslationX = 50,
				TranslationY = -20
			};

			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var nativeView = GetPlatformLabel(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				AssertTranslationMatches(nativeView, label.TranslationX, label.TranslationY);
			});
		}

		TextView GetPlatformLabel(LabelHandler labelHandler) =>
			labelHandler.PlatformView;

		TextUtils.TruncateAt GetPlatformLineBreakMode(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).Ellipsize;

		int GetPlatformMaxLines(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).MaxLines;

		Task<float> GetPlatformOpacity(LabelHandler labelHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformLabel(labelHandler);
				return (float)nativeView.Alpha;
			});
		}

		Task<bool> GetPlatformIsVisible(LabelHandler labelHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformLabel(labelHandler);
				return nativeView.Visibility == global::Android.Views.ViewStates.Visible;
			});
		}
	}
}