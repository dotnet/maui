using System;
using System.Runtime.InteropServices.ComTypes;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native.Gtk;
using Pango;

namespace Microsoft.Maui.Handlers
{

	public partial class LabelHandler : ViewHandler<ILabel, Label>
	{

		private static Microsoft.Maui.Graphics.Native.Gtk.TextLayout? _textLayout;

		public Microsoft.Maui.Graphics.Native.Gtk.TextLayout SharedTextLayout => _textLayout ??= new Microsoft.Maui.Graphics.Native.Gtk.TextLayout(
			Microsoft.Maui.Graphics.Native.Gtk.NativeGraphicsService.Instance.SharedContext) { HeightForWidth = true };

		// https://developer.gnome.org/gtk3/stable/GtkLabel.html
		protected override Label CreateNativeView()
		{
			return new Label()
			{
				LineWrap = true,
				Halign = Align.Fill,
				Xalign = 0,
				MaxWidthChars = 1
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
			handler.NativeView?.UpdateTextAlignment(label);
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

		[MissingMapper]
		public static void MapCharacterSpacing(LabelHandler handler, ILabel label)
		{ }

		[MissingMapper]
		public static void MapTextDecorations(LabelHandler handler, ILabel label)
		{ }

		[MissingMapper]
		public static void MapLineHeight(LabelHandler handler, ILabel label)
		{
			// there is no LineHeight for label in gtk3:
			// https://gitlab.gnome.org/GNOME/gtk/-/issues/2379

			if (handler.NativeView is not { } nativeView)
				return;

			if (handler.VirtualView is not { } virtualView)
				return;

			if (label.LineHeight > 1)
				// should be: https://developer.gnome.org/pango/1.46/pango-Layout-Objects.html#pango-layout-set-line-spacing
				// see: https://github.com/GtkSharp/GtkSharp/issues/258
				nativeView.Layout.Spacing = (int)label.LineHeight.ScaledToPango();

		}

	}

}