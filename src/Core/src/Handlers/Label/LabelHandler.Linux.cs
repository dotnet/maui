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

			var (baseWidth, baseHeight) = base.GetDesiredSize(widthConstraint, heightConstraint);
			int width = -1;
			int height = 1;

			var widthConstrained = !double.IsPositiveInfinity(widthConstraint);
			var heightConstrained = !double.IsPositiveInfinity(heightConstraint);

			// try use layout from Label: not working
			// var SharedTextLayout = new Microsoft.Maui.Graphics.Native.Gtk.TextLayout(NativeGraphicsService.Instance.SharedContext);
			// SharedTextLayout.SetLayout(nativeView.Layout);
			// ...

			lock (SharedTextLayout)
			{
				SharedTextLayout.FontFamily = virtualView.Font.FontFamily;
				SharedTextLayout.TextFlow = TextFlow.ClipBounds;
				SharedTextLayout.PangoFontSize = virtualView.Font.FontSize.ScaledToPango();
				SharedTextLayout.HorizontalAlignment = virtualView.HorizontalTextAlignment.GetHorizontalAlignment();
				SharedTextLayout.LineBreakMode = virtualView.LineBreakMode.GetLineBreakMode();

				SharedTextLayout.HeightForWidth = !heightConstrained;
				var constraint = SharedTextLayout.HeightForWidth ? widthConstraint : heightConstraint;
				(width, height) = SharedTextLayout.GetPixelSize(NativeView.Text, double.IsInfinity(constraint) ? -1 : constraint);

			}

			var inkRect = new Pango.Rectangle();
			var logicalRect = new Pango.Rectangle();
			nativeView.Layout.GetLineReadonly(0).GetExtents(ref inkRect, ref logicalRect);
			var lineHeigh = logicalRect.Height.ScaledFromPango();

			width += nativeView.MarginStart + nativeView.MarginEnd;
			height += nativeView.MarginTop + nativeView.MarginBottom;

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
		}

	}

}