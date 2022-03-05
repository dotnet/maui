using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Google.Android.Material.Button;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = global::Android.Graphics.Color;

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
			Assert.Equal(expectedValue, values.PlatformViewValue, EmCoefficientPrecision);
		}

		AppCompatButton GetNativeButton(ButtonHandler buttonHandler) =>
			(AppCompatButton)buttonHandler.PlatformView;

		string GetNativeText(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).Text;

		Color GetNativeTextColor(ButtonHandler buttonHandler)
		{
			int currentTextColorInt = GetNativeButton(buttonHandler).CurrentTextColor;
			AColor currentTextColor = new AColor(currentTextColorInt);

			return currentTextColor.ToColor();
		}

		Thickness GetNativePadding(ButtonHandler buttonHandler)
		{
			var appCompatButton = GetNativeButton(buttonHandler);

			return new Thickness(
				appCompatButton.PaddingLeft,
				appCompatButton.PaddingTop,
				appCompatButton.PaddingRight,
				appCompatButton.PaddingBottom);
		}

		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeButton(CreateHandler(button)).PerformClick();
			});
		}

		double GetNativeCharacterSpacing(ButtonHandler buttonHandler)
		{
			var button = GetNativeButton(buttonHandler);

			if (button != null)
			{
				return button.LetterSpacing;
			}

			return -1;
		}

		bool ImageSourceLoaded(ButtonHandler buttonHandler)
		{
			var image = buttonHandler.PlatformView.Icon ??
						TextViewCompat.GetCompoundDrawablesRelative(buttonHandler.PlatformView)[3];

			return image != null;
		}
	}
}