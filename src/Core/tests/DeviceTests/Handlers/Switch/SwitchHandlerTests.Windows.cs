using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Xunit;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SwitchHandlerTests
	{
		[Theory]
		[InlineData(ElementTheme.Light, 1f)]
		[InlineData(ElementTheme.Dark, 1f)]
		[InlineData(ElementTheme.Light, 0.5f)]
		[InlineData(ElementTheme.Dark, 0.5f)]
		public async Task CustomOnColorHasPointerOverFeedback(ElementTheme theme, float alpha)
		{
			var switchStub = new SwitchStub
			{
				IsOn = true,
				TrackColor = Colors.Red.WithAlpha(alpha)
			};

			await AttachAndRun(switchStub, handler =>
			{
				var toggleSwitch = handler.PlatformView;
				toggleSwitch.RequestedTheme = theme;
				toggleSwitch.ApplyTemplate();
				var track = toggleSwitch.GetDescendantByName<Rectangle>("SwitchKnobBounds");
				Assert.NotNull(track);

				Assert.True(VisualStateManager.GoToState(toggleSwitch, "Normal", false));
				var normal = Assert.IsType<SolidColorBrush>(track.Fill);
				Assert.Equal(switchStub.TrackColor.ToWindowsColor(), normal.Color);
				Assert.Equal(1, normal.Opacity);

				Assert.True(VisualStateManager.GoToState(toggleSwitch, "PointerOver", false));
				var pointerOver = Assert.IsType<SolidColorBrush>(track.Fill);
				Assert.Equal(normal.Color, pointerOver.Color);
				Assert.Equal(0.9, pointerOver.Opacity, 2);

				Assert.True(VisualStateManager.GoToState(toggleSwitch, "Pressed", false));
				var pressed = Assert.IsType<SolidColorBrush>(track.Fill);
				Assert.Equal(normal.Color, pressed.Color);
				Assert.Equal(normal.Opacity, pressed.Opacity);

				Assert.True(VisualStateManager.GoToState(toggleSwitch, "Normal", false));
				Assert.Same(normal, track.Fill);

				var highContrast = Assert.IsType<ResourceDictionary>(toggleSwitch.Resources.ThemeDictionaries["HighContrast"]);
				var highContrastHover = Assert.IsType<SolidColorBrush>(highContrast["ToggleSwitchFillOnPointerOver"]);
				Assert.Equal(normal.Color, highContrastHover.Color);
				Assert.Equal(normal.Opacity, highContrastHover.Opacity);

				toggleSwitch.IsEnabled = false;
				Assert.True(VisualStateManager.GoToState(toggleSwitch, "Disabled", false));
				var disabled = Assert.IsType<SolidColorBrush>(track.Fill);
				Assert.Equal(normal.Color, disabled.Color);
				Assert.Equal(normal.Opacity, disabled.Opacity);
			});
		}

		[Theory]
		[InlineData(ElementTheme.Light)]
		[InlineData(ElementTheme.Dark)]
		public async Task CustomOnColorUpdatesAndResetsPointerOver(ElementTheme theme)
		{
			var switchStub = new SwitchStub { IsOn = true };

			await AttachAndRun(switchStub, handler =>
			{
				var toggleSwitch = handler.PlatformView;
				toggleSwitch.RequestedTheme = theme;
				toggleSwitch.ApplyTemplate();
				var track = toggleSwitch.GetDescendantByName<Rectangle>("SwitchKnobBounds");
				Assert.NotNull(track);

				Assert.True(VisualStateManager.GoToState(toggleSwitch, "PointerOver", false));
				var original = Assert.IsType<SolidColorBrush>(track.Fill);
				var originalColor = original.Color;
				var originalOpacity = original.Opacity;

				foreach (var color in new[] { Colors.Blue, Colors.Red })
				{
					switchStub.TrackColor = color;
					handler.UpdateValue(nameof(ISwitch.TrackColor));
					Assert.True(VisualStateManager.GoToState(toggleSwitch, "Normal", false));
					Assert.True(VisualStateManager.GoToState(toggleSwitch, "PointerOver", false));
					var pointerOver = Assert.IsType<SolidColorBrush>(track.Fill);
					Assert.Equal(color.ToWindowsColor(), pointerOver.Color);
					Assert.Equal(0.9, pointerOver.Opacity, 2);
				}

				switchStub.TrackColor = null;
				handler.UpdateValue(nameof(ISwitch.TrackColor));
				Assert.True(VisualStateManager.GoToState(toggleSwitch, "Normal", false));
				Assert.True(VisualStateManager.GoToState(toggleSwitch, "PointerOver", false));
				var restored = Assert.IsType<SolidColorBrush>(track.Fill);
				Assert.Equal(originalColor, restored.Color);
				Assert.Equal(originalOpacity, restored.Opacity);

				foreach (var themeKey in new[] { "Default", "HighContrast" })
				{
					var resources = Assert.IsType<ResourceDictionary>(toggleSwitch.Resources.ThemeDictionaries[themeKey]);
					Assert.DoesNotContain("ToggleSwitchFillOnPointerOver", resources.Keys);
				}
			});
		}

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
	}
}