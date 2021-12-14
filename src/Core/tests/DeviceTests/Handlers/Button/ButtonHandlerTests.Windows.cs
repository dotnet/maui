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
					NativeViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue);
		}

		[Fact(DisplayName = "Button Padding Initializing")]
		public async Task PaddingInitializesCorrectly()
		{
			var expected = new Thickness(5, 10, 15, 20);

			var button = new ButtonStub()
			{
				Text = "Test",
				Padding = expected
			};

			var actual = await GetValueAsync(button, GetNativePadding);

			Assert.Equal(expected.Left, actual.Left);
			Assert.Equal(expected.Top, actual.Top);
			Assert.Equal(expected.Right, actual.Right);
			Assert.Equal(expected.Bottom, actual.Bottom);
		}

		UI.Xaml.Controls.Button GetNativeButton(ButtonHandler buttonHandler) =>
			buttonHandler.NativeView;

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
				var nativeButton = GetNativeButton(CreateHandler(button));
				var ap = new ButtonAutomationPeer(nativeButton);
				var ip = ap.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
				ip?.Invoke();
			});
		}

		double GetNativeCharacterSpacing(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).GetContent<TextBlock>()?.CharacterSpacing ?? 0;

		bool ImageSourceLoaded(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).GetContent<Image>()?.Source != null;
	}
}