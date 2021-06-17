using System;
using Gtk;
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
				case ProgressBar:

				case ComboBox box:

				default:
					mainNode = nativeView.StyleContext.Path.ToString().Split(':')[0];

					break;
			}

			return mainNode;
		}

		public static string CssImage(this Gdk.Pixbuf nativeImage)
		{
			var puf = nativeImage.SaveToBuffer(Graphics.ImageFormat.Png.ToImageExtension());

			return $"url('data:image/png;base64,{Convert.ToBase64String(puf)}')";
		}
		
		public static void SetStyleImage(this Gtk.Widget widget, string cssImage, string mainNode, string attr, string? subNode = null)
		{
			using var p = new Gtk.CssProvider();

			subNode = subNode != null ? $" > {subNode} " : subNode;

			p.LoadFromData($"{mainNode}{subNode}{{{attr}:{cssImage}}}");
			widget.StyleContext.AddProvider(p, Gtk.StyleProviderPriority.User);
		}

	}

}