using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = global::Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonHandlerTests
	{
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

			var fontManager = App.Services.GetRequiredService<IFontManager>();

			var nativeFont = fontManager.GetTypeface(Font.OfSize(family, 0.0));

			Assert.Equal(nativeFont, nativeButton.Typeface);

			if (string.IsNullOrEmpty(family))
				Assert.Equal(fontManager.DefaultTypeface, nativeButton.Typeface);
			else
				Assert.NotEqual(fontManager.DefaultTypeface, nativeButton.Typeface);
		}

		AppCompatButton GetNativeButton(ButtonHandler buttonHandler) =>
			(AppCompatButton)buttonHandler.View;

		string GetNativeText(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).Text;

		Color GetNativeTextColor(ButtonHandler buttonHandler)
		{
			int currentTextColorInt = GetNativeButton(buttonHandler).CurrentTextColor;
			AColor currentTextColor = new AColor(currentTextColorInt);

			return currentTextColor.ToColor();
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
			GetNativeButton(buttonHandler).Typeface.IsBold;

		bool GetNativeIsItalic(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).Typeface.IsItalic;
	}
}