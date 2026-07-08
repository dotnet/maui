using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using NLabel = Tizen.UIExtensions.NUI.Label;
using TFormattedString = Tizen.UIExtensions.Common.FormattedString;
using TSpan = Tizen.UIExtensions.Common.Span;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class LabelRenderer : ViewRenderer<Label, NLabel>
	{

		public LabelRenderer()
		{
			RegisterPropertyHandler(Label.TextProperty, UpdateText);
			RegisterPropertyHandler(Label.TextColorProperty, UpdateTextColor);
			RegisterPropertyHandler(Label.LineBreakModeProperty, UpdateLineBreakMode);
			RegisterPropertyHandler(Label.HorizontalTextAlignmentProperty, UpdateHorizontalTextAlignment);
			RegisterPropertyHandler(Label.VerticalTextAlignmentProperty, UpdateVerticalTextAlignment);
			RegisterPropertyHandler(Label.FormattedTextProperty, UpdateFormattedText);
			RegisterPropertyHandler(Label.LineHeightProperty, UpdateLineHeight);
			RegisterPropertyHandler(Label.TextDecorationsProperty, UpdateTextDecorations);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			if (Control == null)
			{
				base.SetNativeControl(new NLabel());
			}
			base.OnElementChanged(e);
		}

		TFormattedString ConvertFormattedText(FormattedString formattedString)
		{
			if (formattedString == null)
			{
				return null;
			}

			var nativeString = new TFormattedString();

			foreach (var span in formattedString.Spans)
			{
				var nativeSpan = new TSpan
				{
					Text = span.Text,
					FontAttributes = span.FontAttributes.ToPlatform(),
					FontFamily = span.FontFamily,
					FontSize = span.FontSize.ToPoint(),
					ForegroundColor = span.TextColor.ToPlatform(),
					BackgroundColor = span.BackgroundColor.ToPlatform(),
					TextDecorations = span.TextDecorations.ToPlatform(),
					LineHeight = span.LineHeight,
				};
				nativeString.Spans.Add(nativeSpan);
			}

			return nativeString;
		}

		void UpdateTextDecorations()
		{
			Control.TextDecorations = Element.TextDecorations.ToPlatform();
		}

		void UpdateFormattedText()
		{
			if (Element.FormattedText != null)
				Control.FormattedText = ConvertFormattedText(Element.FormattedText);
		}

		void UpdateText()
		{
			Control.Text = Element.Text ?? "";
		}

		void UpdateTextColor()
		{
			if (Element.TextColor.IsDefault())
				Control.TextColor = Colors.Black.ToPlatform();
			else
				Control.TextColor = Element.TextColor.ToPlatform();
		}

		void UpdateHorizontalTextAlignment()
		{
			Control.HorizontalTextAlignment = Element.HorizontalTextAlignment.ToPlatform();
		}

		void UpdateVerticalTextAlignment()
		{
			Control.VerticalTextAlignment = Element.VerticalTextAlignment.ToPlatform();
		}

		void UpdateFontProperties()
		{
			Control.FontSize = Element.FontSize.ToPoint();
			Control.FontAttributes = Element.FontAttributes.ToPlatform();
			Control.FontFamily = Element.FontFamily;
		}

		void UpdateLineBreakMode()
		{
			Control.LineBreakMode = Element.LineBreakMode.ToPlatform();
		}

		void UpdateLineHeight()
		{

		}
	}
}
