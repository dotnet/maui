#if PLATFORM && !TIZEN
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TextStyleHandlerTests<THandler, TStub> : HandlerTestBase<THandler, TStub>
		where THandler : class, IViewHandler, new()
		where TStub : IStubBase, IView, ITextStyle, new()
	{
#if !WINDOWS
		[Theory(DisplayName = "Font Size Initializes Correctly")]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(20)]
		[InlineData(100)]
		public async Task FontSizeInitializesCorrectly(int fontSize)
		{
			var view = new TStub();
			SetFont(view, Font.OfSize("Arial", fontSize, enableScaling: false));
			await ValidatePropertyInitValue(view, () => view.Font.Size, (h) => GetNativeUnscaledFontSize(h, false), view.Font.Size);
		}

		[Theory(DisplayName = "Font Attributes Initialize Correctly")]
		[InlineData(FontWeight.Regular, false, false)]
		[InlineData(FontWeight.Bold, true, false)]
		[InlineData(FontWeight.Regular, false, true)]
		[InlineData(FontWeight.Bold, true, true)]
		public async Task FontAttributesInitializeCorrectly(FontWeight weight, bool isBold, bool isItalic)
		{
			var view = new TStub();
			SetFont(view, Font.OfSize("Arial", 10, weight, isItalic ? FontSlant.Italic : FontSlant.Default));

			await ValidatePropertyInitValue(view, () => view.Font.Weight == FontWeight.Bold, GetNativeIsBold, isBold);
			await ValidatePropertyInitValue(view, () => view.Font.Slant == FontSlant.Italic, GetNativeIsItalic, isItalic);
		}

		[Theory(DisplayName = "Auto Scaling Enabled Initializes Correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task FontAutoScalingEnabledInitializesCorrectly(bool enableAutoScaling)
		{
			var view = new TStub();
			SetFont(view, Font.OfSize(null, 10, enableScaling: enableAutoScaling));
			await ValidatePropertyInitValue(view, () => view.Font.Size, (h) => GetNativeUnscaledFontSize(h, enableAutoScaling), view.Font.Size);
		}

		[Theory(DisplayName = "Font Family and Weight Initializes Correctly")]
		[InlineData(null, FontWeight.Regular, FontSlant.Default)]
		[InlineData(null, FontWeight.Regular, FontSlant.Italic)]
		[InlineData(null, FontWeight.Bold, FontSlant.Default)]
		[InlineData(null, FontWeight.Bold, FontSlant.Italic)]
		[InlineData("Lobster Two", FontWeight.Regular, FontSlant.Default)]
		[InlineData("Lobster Two", FontWeight.Regular, FontSlant.Italic)]
		[InlineData("Lobster Two", FontWeight.Bold, FontSlant.Default)]
		[InlineData("Lobster Two", FontWeight.Bold, FontSlant.Italic)]
#if !__IOS__
		// iOS cannot force a font to be bold like all other OS
		[InlineData("Dokdo", FontWeight.Regular, FontSlant.Default)]
		[InlineData("Dokdo", FontWeight.Regular, FontSlant.Italic)]
		[InlineData("Dokdo", FontWeight.Bold, FontSlant.Default)]
		[InlineData("Dokdo", FontWeight.Bold, FontSlant.Italic)]
#endif
#if __ANDROID__
		// "monospace" is a special font name on Android
		[InlineData("monospace", FontWeight.Regular, FontSlant.Default)]
		[InlineData("monospace", FontWeight.Regular, FontSlant.Italic)]
		[InlineData("monospace", FontWeight.Bold, FontSlant.Default)]
		[InlineData("monospace", FontWeight.Bold, FontSlant.Italic)]
#endif
		public async Task FontFamilyAndAttributesInitializesCorrectly(string family, FontWeight weight, FontSlant slant)
		{
			EnsureHandlerCreated(builder =>
			{
				builder
					.ConfigureFonts(fonts =>
					{
						fonts.AddFont("dokdo_regular.ttf", "Dokdo");
					});
			});

			var label = new TStub();

			SetFont(label, Font.OfSize(family, 30, weight, slant));
			SetText(label);

			var (isBold, isItalic) = await GetValueAsync(label, (handler) =>
			{
				var isBold = GetNativeIsBold(handler);
				var isItalic = GetNativeIsItalic(handler);

				return (isBold, isItalic);
			});

			Assert.Equal(weight == FontWeight.Bold, isBold);
			Assert.Equal(slant == FontSlant.Italic, isItalic);
		}
#endif
		protected virtual void SetFont(TStub stub, Font font)
		{
			stub.GetType().GetProperty("Font").SetValue(stub, font);
		}

		protected virtual void SetText(TStub stub)
		{
			string text = "Test";
			if (stub is ITextInput ti)
				ti.Text = text;
			else
				stub.GetType().GetProperty("Text").SetValue(stub, text);
		}
	}
}
#endif