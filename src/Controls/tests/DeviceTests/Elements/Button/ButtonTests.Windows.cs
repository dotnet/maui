#nullable enable
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using Controls = Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonTests
	{
		[Theory]
		[InlineData("#B2F9F9F9")]
		[InlineData("#B21A7F37")]
		public async Task InheritedStylePreservesTransparentBackgroundEndpoints(string hoverColorHex)
		{
			var hoverColor = Color.FromArgb(hoverColorHex);
			var pointerOver = new Controls.VisualState { Name = "PointerOver" };
			pointerOver.Setters.Add(new Controls.Setter
			{
				Property = Controls.VisualElement.BackgroundColorProperty,
				Value = hoverColor
			});
			var commonStates = new Controls.VisualStateGroup { Name = "CommonStates" };
			commonStates.States.Add(new Controls.VisualState { Name = "Normal" });
			commonStates.States.Add(pointerOver);
			var baseStyle = new Controls.Style(typeof(Controls.Button));
			baseStyle.Setters.Add(new Controls.Setter
			{
				Property = Controls.VisualElement.BackgroundColorProperty,
				Value = Color.FromArgb("#F9F9F9")
			});
			baseStyle.Setters.Add(new Controls.Setter
			{
				Property = Controls.VisualStateManager.VisualStateGroupsProperty,
				Value = new Controls.VisualStateGroupList { commonStates }
			});
			var transparentStyle = new Controls.Style(typeof(Controls.Button)) { BasedOn = baseStyle };
			transparentStyle.Setters.Add(new Controls.Setter
			{
				Property = Controls.VisualElement.BackgroundColorProperty,
				Value = Color.FromArgb("#00000000")
			});
			var button = new Controls.Button { Text = "Test", Style = transparentStyle };

			await AttachAndRun<ButtonHandler>(button, handler =>
			{
				var resources = handler.PlatformView.Resources;
				var expectedHoverColor = hoverColor.ToWindowsColor();
				var expectedTransparentColor = global::Windows.UI.Color.FromArgb(
					0, expectedHoverColor.R, expectedHoverColor.G, expectedHoverColor.B);
				Assert.True(Controls.VisualStateManager.GoToState(button, "Normal"));

				for (var transition = 0; transition < 3; transition++)
				{
					var transparentBrush = Assert.IsType<Microsoft.UI.Xaml.Media.SolidColorBrush>(resources["ButtonBackground"]);
					Assert.Equal((byte)0, transparentBrush.Color.A);
					Assert.True(Controls.VisualStateManager.GoToState(button, "PointerOver"));
					Assert.Equal(hoverColor, button.BackgroundColor);
					Assert.Equal(expectedTransparentColor, transparentBrush.Color);
					var hoverBrush = Assert.IsType<Microsoft.UI.Xaml.Media.SolidColorBrush>(resources["ButtonBackground"]);
					Assert.Equal(expectedHoverColor, hoverBrush.Color);

					Assert.True(Controls.VisualStateManager.GoToState(button, "Normal"));
					Assert.Equal(Color.FromArgb("#00000000"), button.BackgroundColor);
					Assert.Equal(expectedHoverColor, hoverBrush.Color);
					Assert.Equal(expectedTransparentColor,
						Assert.IsType<Microsoft.UI.Xaml.Media.SolidColorBrush>(resources["ButtonBackground"]).Color);
				}

				return Task.CompletedTask;
			});
		}

		Button GetPlatformButton(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;

		Task<string?> GetPlatformText(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformButton(buttonHandler).GetContent<TextBlock>()?.Text);
		}

		TextTrimming GetPlatformLineBreakMode(ButtonHandler buttonHandler) =>
			(GetPlatformButton(buttonHandler).Content as FrameworkElement)!.GetFirstDescendant<TextBlock>()!.TextTrimming;

		Task<float> GetPlatformOpacity(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformButton(buttonHandler);
				return (float)nativeView.Opacity;
			});
		}

		Task<bool> GetPlatformIsVisible(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformButton(buttonHandler);
				return nativeView.Visibility == Microsoft.UI.Xaml.Visibility.Visible;
			});
		}

		[Fact]
		[Description("The Opacity property of a Button should match with native Opacity")]
		public async Task VerifyButtonOpacityProperty()
		{
			var button = new Microsoft.Maui.Controls.Button
			{
				Opacity = 0.35f
			};
			var expectedValue = button.Opacity;

			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			await InvokeOnMainThreadAsync(async () =>
			{
				var nativeOpacityValue = await GetPlatformOpacity(handler);
				Assert.Equal(expectedValue, nativeOpacityValue);
			});
		}

	}
}
