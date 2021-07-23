using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase<THandler, TStub>
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

			var expectedNativeFont = fontManager.GetFont(Font.OfSize(family, 0.0));

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

			if(autoScalingEnabled)
			{
				var scale = UIFontMetrics.DefaultMetrics.GetScaledValue(1);
				returnValue = returnValue / scale;
			}

			return returnValue;
		}

		protected UIFont GetNativeFont(THandler handler) =>
			GetNativeFont(handler as ElementHandler);
		
		protected UIFont GetNativeFont(ElementHandler handler)
		{
			switch (handler.NativeView)
			{
				case UITextField tf:
					return tf.Font;
				case UIButton button:
					return button.TitleLabel.Font;
				case UILabel label:
					return label.Font;
				case UISearchBar searchBar:
					return searchBar.FindDescendantView<UITextField>().Font;
				default:
					Assert.False(false, $"I don't know how to get the UIFont from here {handler}");
					return null;

			}
		}
	}
}