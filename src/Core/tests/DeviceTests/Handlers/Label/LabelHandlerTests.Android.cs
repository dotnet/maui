using System.Threading.Tasks;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelHandlerTests
	{
		[Theory(DisplayName = "Font Family Initializes Correctly")]
		[InlineData(null)]
		[InlineData("monospace")]
		[InlineData("Dokdo")]
		public async Task FontFamilyInitializesCorrectly(string family)
		{
			var label = new LabelStub()
			{
				Text = "Test",
				FontFamily = family
			};

			var handler = await CreateHandlerAsync(label);
			var nativeLabel = GetNativeLabel(handler);

			var fontManager = App.Services.GetRequiredService<IFontManager>();

			var nativeFont = fontManager.GetTypeface(Font.OfSize(family, 0.0));

			Assert.Equal(nativeFont, nativeLabel.Typeface);

			if (string.IsNullOrEmpty(family))
				Assert.Equal(fontManager.DefaultTypeface, nativeLabel.Typeface);
			else
				Assert.NotEqual(fontManager.DefaultTypeface, nativeLabel.Typeface);
		}

		TextView GetNativeLabel(LabelHandler labelHandler) =>
			(TextView)labelHandler.View;

		string GetNativeText(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Text;

		Color GetNativeTextColor(LabelHandler labelHandler) =>
			((uint)GetNativeLabel(labelHandler).CurrentTextColor).ToColor();

		double GetNativeUnscaledFontSize(LabelHandler labelHandler)
		{
			var textView = GetNativeLabel(labelHandler);
			return textView.TextSize / textView.Resources.DisplayMetrics.Density;
		}

		bool GetNativeIsBold(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Typeface.IsBold;

		bool GetNativeIsItalic(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Typeface.IsItalic;

		Task ValidateNativeBackgroundColor(ILabel label, Color color)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeLabel(CreateHandler(label)).AssertContainsColor(color);
			});
		}
	}
}