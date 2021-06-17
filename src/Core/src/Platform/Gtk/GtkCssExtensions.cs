using System;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native.Gtk;
using Pango;

namespace Microsoft.Maui
{

	public static class GtkCssExtensions
	{

		public static string CssMainNode(this Gtk.Widget nativeView)
		{
			var mainNode = string.Empty;

			switch (nativeView)
			{
				case Gtk.ProgressBar:

				case Gtk.ComboBox box:

				default:
					mainNode = nativeView.StyleContext.Path.ToString().Split(':')[0];

					break;
			}

			return mainNode;
		}

		public static string CssImage(this Gdk.Pixbuf nativeImage)
		{
			var puf = nativeImage.SaveToBuffer(ImageFormat.Png.ToImageExtension());

			return $"url('data:image/png;base64,{Convert.ToBase64String(puf)}')";
		}

		[PortHandler("implement drawing of other paints than GradientPaint")]
		public static string? CssImage(this Paint? paint)
		{
			if (paint.IsNullOrEmpty())
				return null;

			string Stops(GradientStop[] sorted)
			{
				var max = sorted[^1].Offset;
				max = 100 / (max == 0 ? 1 : max);
				var stops = string.Join(",", sorted.Select(s => $"{s.Color.ToGdkRgba().ToString()} {s.Offset * max}%"));

				return stops;
			}

			switch (paint)
			{
				case LinearGradientPaint lg:
				{
					var stops = Stops(lg.GetSortedStops());
					var css = $"linear-gradient( to right, {stops})";

					return css;
				}
				case RadialGradientPaint rg:
				{
					var stops = Stops(rg.GetSortedStops());
					var css = $"radial-gradient({stops})";

					return css;
				}

				default:
					break;
			}

			return null;

		}

		public static void SetStyleImageNode(this Gtk.Widget widget, string cssImage, string mainNode, string attr, string? subNode = null)
		{
			using var p = new Gtk.CssProvider();

			subNode = subNode != null ? $" > {subNode} " : subNode;

			p.LoadFromData($"{mainNode}{subNode}{{{attr}:{cssImage}}}");
			widget.StyleContext.AddProvider(p, Gtk.StyleProviderPriority.User);
		}

		public static void SetStyleImage(this Gtk.Widget widget, string cssImage, string attr, string? subNode = null)
			=> widget.SetStyleImageNode(cssImage, widget.CssMainNode(), attr, subNode);

	}

}