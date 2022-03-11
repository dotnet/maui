using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase<THandler, TStub>
	{
		[Theory(DisplayName = "Font Size Initializes Correctly")]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(20)]
		[InlineData(100)]
		public async Task FontSizeInitializesCorrectly(int fontSize)
		{
			var view = new TStub();
			if (view is not ITextStyle textStyle)
				return;

			view.GetType().GetProperty("Font").SetValue(view, Font.OfSize("Arial", fontSize, enableScaling: false));
			await ValidatePropertyInitValue(view, () => textStyle.Font.Size, (h) => GetNativeUnscaledFontSize(h, false), textStyle.Font.Size);
		}

		[Theory(DisplayName = "Font Attributes Initialize Correctly")]
		[InlineData(FontWeight.Regular, false, false)]
		[InlineData(FontWeight.Bold, true, false)]
		[InlineData(FontWeight.Regular, false, true)]
		[InlineData(FontWeight.Bold, true, true)]
		public async Task FontAttributesInitializeCorrectly(FontWeight weight, bool isBold, bool isItalic)
		{
			var view = new TStub();
			if (view is not ITextStyle textStyle)
				return;

			view.GetType().GetProperty("Font").SetValue(view, Font.OfSize("Arial", 10, weight, isItalic ? FontSlant.Italic : FontSlant.Default));

			await ValidatePropertyInitValue(view, () => textStyle.Font.Weight == FontWeight.Bold, GetNativeIsBold, isBold);
			await ValidatePropertyInitValue(view, () => textStyle.Font.Slant == FontSlant.Italic, GetNativeIsItalic, isItalic);
		}

		[Theory(DisplayName = "Auto Scaling Enabled Initializes Correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task FontAutoScalingEnabledInitializesCorrectly(bool enableAutoScaling)
		{
			var view = new TStub();
			if (view is not ITextStyle textStyle)
				return;

			view.GetType().GetProperty("Font").SetValue(view, Font.OfSize(null, 10, enableScaling: enableAutoScaling));
			await ValidatePropertyInitValue(view, () => textStyle.Font.Size, (h) => GetNativeUnscaledFontSize(h, enableAutoScaling), textStyle.Font.Size);
		}
	}
}