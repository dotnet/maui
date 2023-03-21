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