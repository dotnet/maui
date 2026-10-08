#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonHandlerTests
	{
		[Theory]
		[InlineData("#B2F9F9F9")]
		[InlineData("#B21A7F37")]
		public async Task TransparentBackgroundEndpointsPreserveTransitionRgb(string hoverColorHex)
		{
			var button = new ButtonStub
			{
				Text = "Test",
				Background = new SolidPaintStub(Color.FromArgb("#00FF0000"))
			};

			await AttachAndRun(button, handler =>
			{
				var resources = handler.PlatformView.Resources;
				var resourceKeys = new[]
				{
					"ButtonBackground",
					"ButtonBackgroundPointerOver",
					"ButtonBackgroundPressed",
					"ButtonBackgroundDisabled"
				};

				for (var transition = 0; transition < 3; transition++)
				{
					var transparentBrush = Assert.IsType<UI.Xaml.Media.SolidColorBrush>(resources["ButtonBackground"]);
					Assert.Equal((byte)0, transparentBrush.Color.A);

					button.Background = new SolidPaintStub(Color.FromArgb(hoverColorHex));
					handler.UpdateValue(nameof(IView.Background));

					var hoverBrush = Assert.IsType<UI.Xaml.Media.SolidColorBrush>(resources["ButtonBackground"]);
					var hoverColor = hoverBrush.Color;
					Assert.Equal((byte)0xB2, hoverColor.A);
					Assert.Equal(global::Windows.UI.Color.FromArgb(0, hoverColor.R, hoverColor.G, hoverColor.B), transparentBrush.Color);
					foreach (var key in resourceKeys)
						Assert.Equal(hoverColor, Assert.IsType<UI.Xaml.Media.SolidColorBrush>(resources[key]).Color);

					button.Background = new SolidPaintStub(Color.FromArgb("#00FF0000"));
					handler.UpdateValue(nameof(IView.Background));

					var expectedTransparentColor = global::Windows.UI.Color.FromArgb(0, hoverColor.R, hoverColor.G, hoverColor.B);
					Assert.Equal(hoverColor, hoverBrush.Color);
					foreach (var key in resourceKeys)
						Assert.Equal(expectedTransparentColor, Assert.IsType<UI.Xaml.Media.SolidColorBrush>(resources[key]).Color);
				}

				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var xplatCharacterSpacing = 4;

			var button = new ButtonStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = "Test"
			};

			float expectedValue = button.CharacterSpacing.ToEm();

			var values = await GetValueAsync(button, (handler) =>
			{
				return new
				{
					ViewValue = button.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		[Fact(DisplayName = "Corner radius is rounded by default")]
		public async Task RoundedCornersDefault()
		{
			var flatCornerRadius = 0;
			var button = new ButtonStub()
			{
				// assign the default value
				CornerRadius = -1
			};

			var values = await GetValueAsync(button, (handler) =>
			{
				return new
				{
					ViewValue = button.CornerRadius,
					ContainsResource = handler.PlatformView.Resources.Keys.Contains("ControlCornerRadius")
				};
			});

			Assert.False(values.ContainsResource);
			Assert.NotEqual(flatCornerRadius, values.ViewValue);
		}

		[Fact(DisplayName = "Corner Radius Set Correctly")]
		public async Task CornerRadiusSetCorrectly()
		{
			var cornerRadius = 8;
			var button = new ButtonStub()
			{
				CornerRadius = cornerRadius
			};

			var values = await GetValueAsync(button, (handler) =>
			{
				var ret = new
				{
					ViewValue = button.CornerRadius,
					ContainsResource = handler.PlatformView.Resources.Keys.Contains("ControlCornerRadius")
				};
				return ret;
			});

			Assert.True(values.ContainsResource);
			Assert.Equal(cornerRadius, values.ViewValue);
		}

		UI.Xaml.Controls.Button GetNativeButton(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;

		string? GetNativeText(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).GetContent<TextBlock>()?.Text;

		Color GetNativeTextColor(ButtonHandler buttonHandler) =>
			((UI.Xaml.Media.SolidColorBrush)GetNativeButton(buttonHandler).Foreground).Color.ToColor();

		UI.Xaml.Thickness GetNativePadding(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).Padding;

		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var platformButton = GetNativeButton(CreateHandler(button));
				var ap = new ButtonAutomationPeer(platformButton);
				var ip = ap.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
				ip?.Invoke();
			});
		}

		double GetNativeCharacterSpacing(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).GetContent<TextBlock>()?.CharacterSpacing ?? 0;

		bool ImageSourceLoaded(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).GetContent<Image>()?.Source != null;

		UI.Xaml.TextTrimming GetNativeLineBreakMode(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).GetContent<TextBlock>()?.TextTrimming ?? UI.Xaml.TextTrimming.None;
	}
}