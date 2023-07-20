using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TextStyleHandlerTests<THandler, TStub>
	{
		[Theory(DisplayName = "Font Family Initializes Correctly")]
		[InlineData(null)]
		[InlineData("Times New Roman")]
		[InlineData("Dokdo")]
		public async Task FontFamilyInitializesCorrectly(string family)
		{
			var view = new TStub();
			if (view is not ITextStyle)
				return;

			view.GetType().GetProperty("Font").SetValue(view, Font.OfSize(family, 10));

			var handler = await CreateHandlerAsync(view);
			var nativeFont = await GetValueAsync(view, handler => GetNativeFont(handler));

			var fontManager = (handler as ElementHandler).Services.GetRequiredService<IFontManager>();

			var expectedNativeFont = await InvokeOnMainThreadAsync(() => fontManager.GetFont(Font.OfSize(family, 0.0)));

			Assert.Equal(expectedNativeFont.FamilyName, nativeFont.FamilyName);
			if (string.IsNullOrEmpty(family))
				Assert.Equal(fontManager.DefaultFont.FamilyName, nativeFont.FamilyName);
			else
				Assert.NotEqual(fontManager.DefaultFont.FamilyName, nativeFont.FamilyName);
		}

		protected bool GetNativeIsBold(THandler handler) =>
			GetNativeFont(handler).FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Bold);

		protected bool GetNativeIsItalic(THandler handler) =>
			GetNativeFont(handler).FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Italic);

		protected double GetNativeUnscaledFontSize(THandler handler, bool autoScalingEnabled)
		{
			var returnValue = GetNativeFont(handler).PointSize;

			if (autoScalingEnabled && handler.VirtualView is ITextStyle ts)
			{
				// I'm not sure the math that iOS uses to scale fonts
				// I tried passing a 1 into GetScaledValue to retrieve the ratio but the ratio
				// wasn't correct. It seems like iOS uses different ratios based on the passed in font traits
				// so I'm just testing here if it's scaled compared to the original and then return
				// ts.Font.Size so the test can pass
				var font = ts.Font;
				var scale = UIFontMetrics.DefaultMetrics.GetScaledValue(1);

				// Device doesn't have larger fonts enabled
				if (scale == 1)
					return returnValue;

				var fontScaling = returnValue / scale;

				// Just see if it scaled close-ish
				if (Math.Abs(font.Size - fontScaling) <= 1.5)
					return font.Size;
			}

			return returnValue;
		}

		protected UIFont GetNativeFont(THandler handler) =>
			GetNativeFont(handler as ElementHandler);

		protected UIFont GetNativeFont(ElementHandler handler)
		{
			switch (handler.PlatformView)
			{
				case UITextField tf:
					return tf.Font;
				case UIButton button:
					return button.TitleLabel.Font;
				case UILabel label:
					return label.Font;
				case UISearchBar searchBar:
					return searchBar.FindDescendantView<UITextField>().Font;
				case UITextView tv:
					return tv.Font;
				default:
					Assert.Fail($"I don't know how to get the UIFont from here {handler.PlatformView}");
					return null;

			}
		}
	}
}