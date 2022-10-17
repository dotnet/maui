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

		[Fact(DisplayName = "Rounded Maui Buttons Default Style")]
		public async Task RoundedCornersMauiButtons()
		{
			var flatCornerRadius = 0;
			var b = new Maui.Controls.Button();

			var button = new ButtonStub()
			{
				// assign ButtonStub the default CornerRadius value of a Maui.Controls.Button
				CornerRadius = b.CornerRadius
			};

			var values = await GetValueAsync(button, (handler) =>
			{
				return new
				{
					ViewValue = button.CornerRadius,
					platformResources = handler.PlatformView.Resources
				};
			});

			Assert.False(values.platformResources.ContainsKey("ControlCornerRadius"));
			Assert.NotEqual(flatCornerRadius, values.ViewValue);
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