using System.Threading.Tasks;
using Android.Graphics;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class TextStyleHandlerTests<THandler, TStub>
	{
		[Theory(DisplayName = "Font Family Initializes Correctly")]
		[InlineData(null)]
		[InlineData("monospace")]
		[InlineData("Dokdo")]
		public async Task FontFamilyInitializesCorrectly(string family)
		{
			EnsureHandlerCreated(builder =>
			{
				builder
					.ConfigureFonts(fonts =>
					{
						fonts.AddFont("dokdo_regular.ttf", "Dokdo");
					});
			});

			var view = new TStub();

			view.GetType().GetProperty("Font").SetValue(view, Font.OfSize(family, 10));
			var handler = (await CreateHandlerAsync(view)) as ElementHandler;

			Typeface nativeTypeFace = GetNativeTextView(handler).Typeface;

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			var nativeFont = fontManager.GetTypeface(Font.OfSize(family, 0.0));

			Assert.Equal(nativeFont, nativeTypeFace);

			if (string.IsNullOrEmpty(family))
				Assert.Equal(fontManager.DefaultTypeface, nativeTypeFace);
			else
				Assert.NotEqual(fontManager.DefaultTypeface, nativeTypeFace);
		}

		protected double GetNativeUnscaledFontSize(THandler handler, bool autoScalingEnabled)
		{
			var textView = GetNativeTextView(handler);

			var scaleFactor = textView.Resources.DisplayMetrics.Density;

			if (autoScalingEnabled)
				scaleFactor = scaleFactor * textView.Context.Resources.Configuration.FontScale;

			double returnValue = textView.TextSize / scaleFactor;

			return returnValue;
		}

		protected TextView GetNativeTextView(THandler handler) =>
			GetNativeTextView(handler as ElementHandler);

		protected TextView GetNativeTextView(ElementHandler handler)
		{
			switch (handler.PlatformView)
			{
				case TextView tv:
					return tv;
				case SearchView sv:
					return sv.GetFirstChildOfType<EditText>();
				default:
					Assert.Fail($"I don't know how to get the TextView from here {handler.PlatformView}");
					return null;

			}
		}

		protected bool GetNativeIsBold(THandler handler) =>
			GetNativeTextView(handler).Typeface.GetFontWeight() == FontWeight.Bold;

		protected bool GetNativeIsItalic(THandler handler) =>
			GetNativeTextView(handler).Typeface.IsItalic;
	}
}