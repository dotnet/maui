using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
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
					NativeViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue, EmCoefficientPrecision);
		}

		[Theory(DisplayName = "Font Family Initializes Correctly")]
		[InlineData(null)]
		[InlineData("monospace")]
		[InlineData("Dokdo")]
		public async Task FontFamilyInitializesCorrectly(string family)
		{
			var button = new ButtonStub
			{
				Text = "Test",
				Font = Font.OfSize(family, 10)
			};

			var handler = await CreateHandlerAsync(button);
			var nativeButton = GetNativeButton(handler);

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			var nativeFont = fontManager.GetTypeface(Font.OfSize(family, 0.0));

			Assert.Equal(nativeFont, nativeButton.Typeface);

			if (string.IsNullOrEmpty(family))
				Assert.Equal(fontManager.DefaultTypeface, nativeButton.Typeface);
			else
				Assert.NotEqual(fontManager.DefaultTypeface, nativeButton.Typeface);
		}

		[Fact(DisplayName = "Button Padding Initializing")]
		public async Task PaddingInitializesCorrectly()
		{
			var button = new ButtonStub()
			{
				Text = "Test",
				Padding = new Thickness(5, 10, 15, 20)
			};

			var handler = await CreateHandlerAsync(button);
			var appCompatButton = (AppCompatButton)handler.NativeView;
			var (left, top, right, bottom) = (appCompatButton.PaddingLeft, appCompatButton.PaddingTop, appCompatButton.PaddingRight, appCompatButton.PaddingBottom);

			var context = handler.NativeView.Context;

			var expectedLeft = context.ToPixels(5);
			var expectedTop = context.ToPixels(10);
			var expectedRight = context.ToPixels(15);
			var expectedBottom = context.ToPixels(20);

			Assert.Equal(expectedLeft, left);
			Assert.Equal(expectedTop, top);
			Assert.Equal(expectedRight, right);
			Assert.Equal(expectedBottom, bottom);
		}

		AppCompatButton GetNativeButton(ButtonHandler buttonHandler) =>
			(AppCompatButton)buttonHandler.NativeView;

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
			return ToThicknees(appCompatButton);

			static Thickness ToThicknees(AppCompatButton appCompatButton)
			{
				var onePx = appCompatButton.Context.ToPixels(1);


				return new Thickness(appCompatButton.PaddingLeft,
					appCompatButton.PaddingTop, appCompatButton.PaddingRight, appCompatButton.PaddingBottom);
			}
		}

		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeButton(CreateHandler(button)).PerformClick();
			});
		}

		double GetNativeUnscaledFontSize(ButtonHandler buttonHandler)
		{
			var textView = GetNativeButton(buttonHandler);
			return textView.TextSize / textView.Resources.DisplayMetrics.Density;
		}

		bool GetNativeIsBold(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).Typeface.GetFontWeight() == FontWeight.Bold;

		bool GetNativeIsItalic(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).Typeface.IsItalic;

		double GetNativeCharacterSpacing(ButtonHandler buttonHandler)
		{
			var button = GetNativeButton(buttonHandler);

			if (button != null)
			{
				return button.LetterSpacing;
			}

			return -1;
		}
	}
}