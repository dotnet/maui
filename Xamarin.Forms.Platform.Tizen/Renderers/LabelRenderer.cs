using ElmSharp;
using Xamarin.Forms.Platform.Tizen.Native;
using EColor = ElmSharp.Color;
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
			if (TizenPlatformServices.AppDomain.IsTizenSpecificAvailable)
			{
				RegisterPropertyHandler("FontWeight", UpdateFontWeight);
			}
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
				Native.Span nativeSpan = new Native.Span();
				nativeSpan.Text = span.Text;
				nativeSpan.FontAttributes = span.FontAttributes;
				nativeSpan.FontFamily = span.FontFamily;
				nativeSpan.FontSize = span.FontSize;
				nativeSpan.ForegroundColor = span.ForegroundColor.ToNative();
				nativeSpan.BackgroundColor = span.BackgroundColor.ToNative();
				nativeString.Spans.Add(nativeSpan);
			}

			return nativeString;
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
			Control.FontFamily = Element.FontFamily;

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
