using Xamarin.Forms.Core;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Tizen.Native;
using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific.Label;

namespace Xamarin.Forms.Platform.Tizen
{

	public class LabelRenderer : ViewRenderer<Label, Native.Label>
	{

		public LabelRenderer()
		{
			RegisterPropertyHandler(Label.TextProperty, UpdateText);
			RegisterPropertyHandler(Label.TextColorProperty, UpdateTextColor);
			// FontProperty change is called also for FontSizeProperty, FontFamilyProperty and FontAttributesProperty change
			RegisterPropertyHandler(Label.FontProperty, UpdateFontProperties);
			RegisterPropertyHandler(Label.LineBreakModeProperty, UpdateLineBreakMode);
			RegisterPropertyHandler(Label.HorizontalTextAlignmentProperty, UpdateHorizontalTextAlignment);
			RegisterPropertyHandler(Label.VerticalTextAlignmentProperty, UpdateVerticalTextAlignment);
			RegisterPropertyHandler(Label.FormattedTextProperty, UpdateFormattedText);
			RegisterPropertyHandler(Label.LineHeightProperty, UpdateLineHeight);
			RegisterPropertyHandler(Specific.FontWeightProperty, UpdateFontWeight);
			RegisterPropertyHandler(Label.TextDecorationsProperty, UpdateTextDecorations);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			if (Control == null)
			{
				base.SetNativeControl(new Native.Label(Forms.NativeParent));
			}
			base.OnElementChanged(e);
		}

		protected override Size MinimumSize()
		{
			return Control.Measure(Control.MinimumWidth, Control.MinimumHeight).ToDP();
		}

		Native.FormattedString ConvertFormattedText(FormattedString formattedString)
		{
			if (formattedString == null)
			{
				return null;
			}

			Native.FormattedString nativeString = new Native.FormattedString();

			foreach (var span in formattedString.Spans)
			{
				var textDecorations = span.TextDecorations;

				Native.Span nativeSpan = new Native.Span();
				nativeSpan.Text = span.Text;
				nativeSpan.FontAttributes = span.FontAttributes;
				nativeSpan.FontFamily = span.FontFamily;
				nativeSpan.FontSize = span.FontSize;
				nativeSpan.ForegroundColor = span.TextColor.ToNative();
				nativeSpan.BackgroundColor = span.BackgroundColor.ToNative();
				nativeSpan.Underline = (textDecorations & TextDecorations.Underline) != 0;
				nativeSpan.Strikethrough = (textDecorations & TextDecorations.Strikethrough) != 0;
				nativeSpan.LineHeight = span.LineHeight;
				nativeString.Spans.Add(nativeSpan);
			}

			return nativeString;
		}

		void UpdateTextDecorations()
		{
			Control.BatchBegin();
			var textDecorations = Element.TextDecorations;
			Control.Strikethrough = (textDecorations & TextDecorations.Strikethrough) != 0;
			Control.Underline = (textDecorations & TextDecorations.Underline) != 0;
			Control.BatchCommit();
		}

		void UpdateFormattedText()
		{
			if (Element.FormattedText != null)
				Control.FormattedText = ConvertFormattedText(Element.FormattedText);
		}

		void UpdateText()
		{
			Control.Text = Element.Text;
		}

		void UpdateTextColor()
		{
			Control.TextColor = Element.TextColor.ToNative();
		}

		void UpdateHorizontalTextAlignment()
		{
			Control.HorizontalTextAlignment = Element.HorizontalTextAlignment.ToNative();
		}

		void UpdateVerticalTextAlignment()
		{
			Control.VerticalTextAlignment = Element.VerticalTextAlignment.ToNative();
		}

		void UpdateFontProperties()
		{
			Control.BatchBegin();

			Control.FontSize = Element.FontSize;
			Control.FontAttributes = Element.FontAttributes;
			Control.FontFamily = Element.FontFamily.ToNativeFontFamily();

			Control.BatchCommit();
		}

		void UpdateLineBreakMode()
		{
			Control.LineBreakMode = ConvertToNativeLineBreakMode(Element.LineBreakMode);
		}

		void UpdateFontWeight()
		{
			Control.FontWeight = Specific.GetFontWeight(Element);
		}

		void UpdateLineHeight()
		{
			Control.LineHeight = Element.LineHeight;
		}

		Native.LineBreakMode ConvertToNativeLineBreakMode(LineBreakMode mode)
		{
			switch (mode)
			{
				case LineBreakMode.CharacterWrap:
					return Native.LineBreakMode.CharacterWrap;
				case LineBreakMode.HeadTruncation:
					return Native.LineBreakMode.HeadTruncation;
				case LineBreakMode.MiddleTruncation:
					return Native.LineBreakMode.MiddleTruncation;
				case LineBreakMode.NoWrap:
					return Native.LineBreakMode.NoWrap;
				case LineBreakMode.TailTruncation:
					return Native.LineBreakMode.TailTruncation;
				case LineBreakMode.WordWrap:
				default:
					return Native.LineBreakMode.WordWrap;
			}
		}
	}
}
