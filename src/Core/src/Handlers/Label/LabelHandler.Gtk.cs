using System;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native.Gtk;
using Microsoft.Maui.Native;

namespace Microsoft.Maui.Handlers
{

	public partial class LabelHandler : ViewHandler<ILabel, LabelView>
	{

		private static Microsoft.Maui.Graphics.Native.Gtk.TextLayout? _textLayout;

		public Microsoft.Maui.Graphics.Native.Gtk.TextLayout SharedTextLayout => _textLayout ??= new Microsoft.Maui.Graphics.Native.Gtk.TextLayout(
			Microsoft.Maui.Graphics.Native.Gtk.NativeGraphicsService.Instance.SharedContext) { HeightForWidth = true };

		// https://docs.gtk.org/gtk3/class.Label.html
		protected override LabelView CreateNativeView()
		{
			return new()
			{
				LineWrap = true,
				Halign = Align.Fill,
				Xalign = 0,
			};
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (NativeView is not { } nativeView)
				return default;

			if (VirtualView is not { } virtualView)
				return default;

			int width = -1;
			int height = -1;

			var widthConstrained = !double.IsPositiveInfinity(widthConstraint);
			var heightConstrained = !double.IsPositiveInfinity(heightConstraint);

			var hMargin = nativeView.MarginStart + nativeView.MarginEnd;
			var vMargin = nativeView.MarginTop + nativeView.MarginBottom;

			// try use layout from Label: not working

			lock (SharedTextLayout)
			{
				SharedTextLayout.FontDescription = nativeView.GetPangoFontDescription();

				SharedTextLayout.TextFlow = TextFlow.ClipBounds;
				SharedTextLayout.HorizontalAlignment = virtualView.HorizontalTextAlignment.GetHorizontalAlignment();
				SharedTextLayout.VerticalAlignment = virtualView.VerticalTextAlignment.GetVerticalAlignment();

				SharedTextLayout.LineBreakMode = virtualView.LineBreakMode.GetLineBreakMode();

				var heightForWidth = !heightConstrained;

				var constraint = Math.Max(heightForWidth ? widthConstraint - hMargin : heightConstraint - vMargin,
					1);

				var lh = 0d;
				var layout = SharedTextLayout.GetLayout();
				layout.Height = -1;
				layout.Width = -1;
				layout.Ellipsize = nativeView.Ellipsize;
				layout.Spacing = nativeView.Layout.Spacing;

				layout.Attributes = nativeView.Attributes;

				if (virtualView.LineHeight > 1)
					layout.LineSpacing = (float)virtualView.LineHeight;
				else
				{
					layout.LineSpacing = 0;
				}

				layout.SetText(nativeView.Text);

				if (!heightConstrained)
				{
					if (nativeView.Lines > 0)
					{
						lh = layout.GetLineHeigth(nativeView.Lines, false);
						layout.Height = (int)lh;
					}
				}

				if (!heightForWidth && heightConstrained && widthConstrained)
				{
					layout.Width = Math.Max((widthConstraint - hMargin).ScaledToPango(), -1);
				}

				(width, height) = layout.GetPixelSize(nativeView.Text, constraint, heightForWidth);

				if (lh > 0)
				{
					height = Math.Min((int)lh.ScaledFromPango(), height);
				}

				layout.Attributes = null;

			}

			width += hMargin;
			height += vMargin;

			return new Size(width, height);

		}

		public static void MapText(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateText(label);
		}

		public static void MapTextColor(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateTextColor(label.TextColor);
		}

		public static void MapFont(LabelHandler handler, ILabel label)
		{
			handler.MapFont(label);
		}

		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateHorizontalTextAlignment(label);
		}

		public static void MapVerticalTextAlignment(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateVerticalTextAlignment(label);
		}

		public static void MapLineBreakMode(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateMaxLines(label);
		}

		public static void MapPadding(LabelHandler handler, ILabel label)
		{
			handler.NativeView.WithPadding(label.Padding);

		}

		public static void MapCharacterSpacing(LabelHandler handler, ILabel label)
		{
			if (handler.NativeView is not { } nativeView)
				return;

			nativeView.Attributes = nativeView.Attributes.AttrListFor(label.TextDecorations, label.CharacterSpacing);
		}

		public static void MapTextDecorations(LabelHandler handler, ILabel label)
		{
			if (handler.NativeView is not { } nativeView)
				return;

			nativeView.Attributes = nativeView.Attributes.AttrListFor(label.TextDecorations, label.CharacterSpacing);
		}

		public static void MapLineHeight(LabelHandler handler, ILabel label)
		{
			if (handler.NativeView is not { } nativeView)
				return;

			if (handler.VirtualView is not { } virtualView)
				return;

			if (label.LineHeight > 1)
			{
				// there is no LineHeight for label in gtk3:
				// https://gitlab.gnome.org/GNOME/gtk/-/issues/2379
				
				// try to set it over css: not working: exception thrown: 'line-height' is not a valid property name
				// nativeView.SetStyleValue($"{(int)label.LineHeight}","line-height");

				// try to set it over https://docs.gtk.org/Pango/method.Layout.set_line_spacing.html

				// no effect: https://docs.gtk.org/gtk3/method.Label.get_layout.html
				// The label is free to recreate its layout at any time, so it should be considered read-only
				// nativeView.Layout.LineSpacing = (float)label.LineHeight;

				// so we use LabelView, this sets it before OnDrawn:
				nativeView.LineHeight = (float)label.LineHeight;
			}

		}

	}

}