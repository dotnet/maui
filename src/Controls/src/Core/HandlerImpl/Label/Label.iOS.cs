#nullable disable
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		MauiLabel _mauiLabel;

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			if (Handler is not null)
			{
				if (Handler is LabelHandler labelHandler && labelHandler.PlatformView is MauiLabel mauiLabel)
				{
					_mauiLabel = mauiLabel;
					_mauiLabel.LayoutSubviewsChanged += OnLayoutSubviewsChanged;
				}
			}
			else
			{
				if (_mauiLabel is not null)
				{
					_mauiLabel.LayoutSubviewsChanged -= OnLayoutSubviewsChanged;
					_mauiLabel = null;
				}
			}
		}

		public static void MapTextType(LabelHandler handler, Label label) => MapText((ILabelHandler)handler, label);
		public static void MapText(LabelHandler handler, Label label) => MapText((ILabelHandler)handler, label);
		public static void MapCharacterSpacing(LabelHandler handler, Label label) => MapCharacterSpacing((ILabelHandler)handler, label);
		public static void MapTextDecorations(LabelHandler handler, Label label) => MapTextDecorations((ILabelHandler)handler, label);
		public static void MapLineHeight(LabelHandler handler, Label label) => MapLineHeight((ILabelHandler)handler, label);
		public static void MapFont(LabelHandler handler, Label label) => MapFont((ILabelHandler)handler, label);
		public static void MapTextColor(LabelHandler handler, Label label) => MapTextColor((ILabelHandler)handler, label);


		public static void MapTextType(ILabelHandler handler, Label label)
		{
			Platform.LabelExtensions.UpdateText(handler.PlatformView, label);
		}

		public static void MapText(ILabelHandler handler, Label label)
		{
			Platform.LabelExtensions.UpdateText(handler.PlatformView, label);
		}

		public static void MapTextDecorations(ILabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapTextDecorations(handler, label);
		}

		public static void MapCharacterSpacing(ILabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapCharacterSpacing(handler, label);
		}

		public static void MapLineHeight(ILabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html)
			{
				return;
			}

			LabelHandler.MapLineHeight(handler, label);
		}

		public static void MapFont(ILabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html && FontIsDefault(label))
			{
				// If no explicit font has been specified and we're displaying HTML, 
				// let the HTML determine the font
				return;
			}

			LabelHandler.MapFont(handler, label);
		}

		public static void MapTextColor(ILabelHandler handler, Label label)
		{
			if (label?.HasFormattedTextSpans ?? false)
				return;

			if (label?.TextType == TextType.Html && label.GetValue(TextColorProperty) == null)
			{
				// If no explicit text color has been specified and we're displaying HTML, 
				// let the HTML determine the colors
				return;
			}

			LabelHandler.MapTextColor(handler, label);
		}

		public static void MapLineBreakMode(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateMaxLines(label);
		}

		static bool FontIsDefault(Label label)
		{
			if (label.IsSet(Label.FontAttributesProperty))
			{
				return false;
			}

			if (label.IsSet(Label.FontFamilyProperty))
			{
				return false;
			}

			if (label.IsSet(Label.FontSizeProperty))
			{
				return false;
			}

			return true;
		}

		void OnLayoutSubviewsChanged(object sender, System.EventArgs e)
		{
			if (Handler is LabelHandler labelHandler)
			{
				if (labelHandler.PlatformView is not UILabel platformView || labelHandler.VirtualView is not Label virtualView)
					return;

				platformView.RecalculateSpanPositions(virtualView);
			}
		}
	}
}
