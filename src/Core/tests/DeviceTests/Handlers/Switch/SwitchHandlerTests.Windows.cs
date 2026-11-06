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
					Assert.DoesNotContain(themeKey, toggleSwitch.Resources.ThemeDictionaries.Keys);
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

		[Theory]
		[InlineData("Default")]
		[InlineData("HighContrast")]
		[InlineData("Light")]
		[InlineData("Dark")]
		public async Task NullOnColorPreservesConfiguredHoverResource(string theme)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<ConfiguredHoverSwitchHandler>(new SwitchStub { IsOn = true });
				var resources = (ResourceDictionary)handler.PlatformView.Resources.ThemeDictionaries[theme];
				Assert.Same(handler.OriginalHoverBrush, resources["ToggleSwitchFillOnPointerOver"]);
			});
		}

		[Theory]
		[InlineData("Default")]
		[InlineData("HighContrast")]
		[InlineData("Light")]
		[InlineData("Dark")]
		public async Task CustomOnColorRestoresConfiguredHoverResource(string theme)
		{
			var switchStub = new SwitchStub { IsOn = true, TrackColor = Colors.Red };
			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<ConfiguredHoverSwitchHandler>(switchStub);
				var toggleSwitch = handler.PlatformView;
				var resources = (ResourceDictionary)toggleSwitch.Resources.ThemeDictionaries[theme];
				await toggleSwitch.AttachAndRun(() =>
				{
					toggleSwitch.RequestedTheme = theme == "Dark" ? ElementTheme.Dark : ElementTheme.Light;
					toggleSwitch.ApplyTemplate();
					for (int cycle = 0; cycle < 2; cycle++)
					{
						foreach (var color in new[] { Colors.Blue, Colors.Red })
						{
							switchStub.TrackColor = color;
							handler.UpdateValue(nameof(ISwitch.TrackColor));
							var brush = Assert.IsType<SolidColorBrush>(resources["ToggleSwitchFillOnPointerOver"]);
							Assert.Equal(color.ToWindowsColor(), brush.Color);
							Assert.Equal(theme == "HighContrast" ? 1 : 0.9, brush.Opacity, 2);

							if (theme != "HighContrast")
							{
								Assert.True(VisualStateManager.GoToState(toggleSwitch, "Normal", false));
								Assert.True(VisualStateManager.GoToState(toggleSwitch, "PointerOver", false));
								var track = toggleSwitch.GetDescendantByName<Rectangle>("SwitchKnobBounds");
								Assert.NotNull(track);
								Assert.Same(brush, track.Fill);
							}
						}

						switchStub.TrackColor = null;
						handler.UpdateValue(nameof(ISwitch.TrackColor));
						Assert.Same(handler.OriginalHoverBrush, resources["ToggleSwitchFillOnPointerOver"]);

						if (theme != "HighContrast")
						{
							Assert.True(VisualStateManager.GoToState(toggleSwitch, "Normal", false));
							Assert.True(VisualStateManager.GoToState(toggleSwitch, "PointerOver", false));
							var track = toggleSwitch.GetDescendantByName<Rectangle>("SwitchKnobBounds");
							Assert.NotNull(track);
							Assert.Same(handler.OriginalHoverBrush, track.Fill);
						}
					}
				}, MauiContext);
			});
		}

		[Theory]
		[InlineData("Default", true)]
		[InlineData("HighContrast", true)]
		[InlineData("Default", false)]
		[InlineData("HighContrast", false)]
		[InlineData("Light", true)]
		[InlineData("Dark", true)]
		[InlineData("Light", false)]
		[InlineData("Dark", false)]
		public async Task ClearingOnColorPreservesReplacedHoverResource(string theme, bool clearBeforeNextColor)
		{
			var switchStub = new SwitchStub { IsOn = true, TrackColor = Colors.Red };
			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<ConfiguredHoverSwitchHandler>(switchStub);
				var resources = (ResourceDictionary)handler.PlatformView.Resources.ThemeDictionaries[theme];
				var replacement = new SolidColorBrush(Colors.Purple.ToWindowsColor());
				resources["ToggleSwitchFillOnPointerOver"] = replacement;
				if (clearBeforeNextColor)
				{
					switchStub.TrackColor = null;
					handler.UpdateValue(nameof(ISwitch.TrackColor));
					Assert.Same(replacement, resources["ToggleSwitchFillOnPointerOver"]);
				}

				switchStub.TrackColor = Colors.Blue;
				handler.UpdateValue(nameof(ISwitch.TrackColor));
				switchStub.TrackColor = null;
				handler.UpdateValue(nameof(ISwitch.TrackColor));
				Assert.Same(replacement, resources["ToggleSwitchFillOnPointerOver"]);
			});
		}

		[Fact]
		public async Task ClearingOnColorRestoresDefaultOnlyThemeDictionaries()
		{
			var switchStub = new SwitchStub { IsOn = true };
			await AttachAndRun(switchStub, handler =>
			{
				var toggleSwitch = handler.PlatformView;
				var originalBrush = new SolidColorBrush(Colors.Green.ToWindowsColor());
				var originalDictionary = new ResourceDictionary
				{
					["ToggleSwitchFillOnPointerOver"] = originalBrush
				};
				toggleSwitch.Resources.ThemeDictionaries["Default"] = originalDictionary;
				Assert.DoesNotContain("HighContrast", toggleSwitch.Resources.ThemeDictionaries.Keys);

				for (int cycle = 0; cycle < 2; cycle++)
				{
					switchStub.TrackColor = Colors.Red;
					handler.UpdateValue(nameof(ISwitch.TrackColor));
					Assert.Contains("HighContrast", toggleSwitch.Resources.ThemeDictionaries.Keys);

					switchStub.TrackColor = null;
					handler.UpdateValue(nameof(ISwitch.TrackColor));
					Assert.DoesNotContain("HighContrast", toggleSwitch.Resources.ThemeDictionaries.Keys);
					Assert.Same(originalDictionary, toggleSwitch.Resources.ThemeDictionaries["Default"]);
					Assert.Same(originalBrush, originalDictionary["ToggleSwitchFillOnPointerOver"]);
				}
			});
		}

		[Theory]
		[InlineData("Default", false)]
		[InlineData("HighContrast", false)]
		[InlineData("Default", true)]
		[InlineData("HighContrast", true)]
		public async Task ClearingOnColorPreservesPopulatedCreatedThemeDictionary(string theme, bool useMergedDictionary)
		{
			var switchStub = new SwitchStub { IsOn = true, TrackColor = Colors.Red };
			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(switchStub);
				var resources = handler.PlatformView.Resources;
				var themeResources = (ResourceDictionary)resources.ThemeDictionaries[theme];
				var additionalResources = new ResourceDictionary { ["OtherResource"] = "value" };
				if (useMergedDictionary)
					themeResources.MergedDictionaries.Add(additionalResources);
				else
					themeResources["OtherResource"] = "value";

				switchStub.TrackColor = null;
				handler.UpdateValue(nameof(ISwitch.TrackColor));

				Assert.Same(themeResources, resources.ThemeDictionaries[theme]);
				Assert.DoesNotContain("ToggleSwitchFillOnPointerOver", themeResources.Keys);
				if (useMergedDictionary)
					Assert.Same(additionalResources, Assert.Single(themeResources.MergedDictionaries));
				else
					Assert.Equal("value", themeResources["OtherResource"]);
			});
		}

		class ConfiguredHoverSwitchHandler : SwitchHandler
		{
			public ConfiguredHoverSwitchHandler()
			{
			}

			public SolidColorBrush OriginalHoverBrush { get; } = new SolidColorBrush(Colors.Green.ToWindowsColor()) { Opacity = 0.65 };

			protected override ToggleSwitch CreatePlatformView()
			{
				var toggleSwitch = base.CreatePlatformView();
				foreach (var theme in new[] { "Default", "HighContrast", "Light", "Dark" })
				{
					toggleSwitch.Resources.ThemeDictionaries[theme] = new ResourceDictionary
					{
						["ToggleSwitchFillOnPointerOver"] = OriginalHoverBrush
					};
				}

				return toggleSwitch;
			}
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