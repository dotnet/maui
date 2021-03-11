using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform.iOS;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelHandlerTests
	{
		[Theory(DisplayName = "Font Family Initializes Correctly")]
		[InlineData(null)]
		[InlineData("Times New Roman")]
		[InlineData("Dokdo")]
		public async Task FontFamilyInitializesCorrectly(string family)
		{
			var label = new LabelStub()
			{
				Text = "Test",
				Font = Font.OfSize(family, 10)
			};

			var nativeFont = await GetValueAsync(label, handler => GetNativeLabel(handler).Font);

			var fontManager = App.Services.GetRequiredService<IFontManager>();

			var expectedNativeFont = fontManager.GetFont(Font.OfSize(family, 0.0));

			Assert.Equal(expectedNativeFont.FamilyName, nativeFont.FamilyName);
			if (string.IsNullOrEmpty(family))
				Assert.Equal(fontManager.DefaultFont.FamilyName, nativeFont.FamilyName);
			else
				Assert.NotEqual(fontManager.DefaultFont.FamilyName, nativeFont.FamilyName);
		}

		[Fact(DisplayName = "Negative MaxLines value with wrap is correct")]
		public async Task NegativeMaxValueWithWrapIsCorrect()
		{
			var label = new LabelStub()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				MaxLines = -1,
				LineBreakMode = LineBreakMode.WordWrap,
			};

			var nativeValue = await GetValueAsync(label, GetNativeMaxLines);

			Assert.Equal(0, nativeValue);
		}

		[Fact(DisplayName = "Padding Initializes Correctly")]
		public async Task PaddingInitializesCorrectly()
		{
			var label = new LabelStub()
			{
				Text = "Test",
				Padding = new Thickness(5, 10, 15, 20)
			};

			var handler = await CreateHandlerAsync(label);
			var insets = ((MauiLabel)handler.NativeView).TextInsets;

			Assert.Equal(5, insets.Left);
			Assert.Equal(10, insets.Top);
			Assert.Equal(15, insets.Right);
			Assert.Equal(20, insets.Bottom);
		}

		UILabel GetNativeLabel(LabelHandler labelHandler) =>
			(UILabel)labelHandler.View;

		string GetNativeText(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Text;

		Color GetNativeTextColor(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).TextColor.ToColor();

		double GetNativeUnscaledFontSize(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Font.PointSize;

		bool GetNativeIsBold(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Bold);

		bool GetNativeIsItalic(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Italic);

		double GetNativeCharacterSpacing(LabelHandler labelHandler)
		{
			var nativeLabel = GetNativeLabel(labelHandler);
			var text = nativeLabel.AttributedText;
			if (text == null)
				return 0;

			var value = text.GetAttribute(UIStringAttributeKey.KerningAdjustment, 0, out var range);
			if (value == null)
				return 0;

			Assert.Equal(0, range.Location);
			Assert.Equal(text.Length, range.Length);

			var kerning = Assert.IsType<NSNumber>(value);
			return kerning.DoubleValue;
		}

		Task ValidateNativeBackgroundColor(ILabel label, Color color)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				return GetNativeLabel(CreateHandler(label)).AssertContainsColor(color);
			});
		}

		int GetNativeMaxLines(LabelHandler labelHandler) =>
			(int)GetNativeLabel(labelHandler).Lines;

		UILineBreakMode GetNativeLineBreakMode(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).LineBreakMode;
	}
}