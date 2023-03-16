using System;
using System.Runtime.CompilerServices;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;

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
					var pathSegments = nativeView.StyleContext.Path.ToString().Split(' ');
					mainNode = pathSegments[pathSegments.Length - 1].Split(':')[0];

					break;
			}

			return mainNode;
		}

		/// <summary>
		/// seems that CssParser doesn't support base64:
		/// https://github.com/GNOME/gtk/blob/gtk-3-22/gtk/gtkcssparser.c
		/// _gtk_css_parser_read_url
		/// </summary>
		static string CssImage(this Gdk.Pixbuf nativeImage)
		{
			var puf = nativeImage.SaveToBuffer(ImageFormat.Png.ToImageExtension());

			return $"url('data:image/png;base64,{Convert.ToBase64String(puf)}')";
		}

		[MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
		public static void Realize(Gtk.CssProvider p)
		{
			var l = p.ToString().Length;

			if (l != 0)
			{
				// ReSharper disable once EmptyStatement
				;
			}
		}

		public static void SetStyleValueNode(this Gtk.Widget widget, string value, string mainNode, string attr, string? subNode = null)
		{
			if (string.IsNullOrEmpty(value))
				return;

			using var p = new Gtk.CssProvider();

			subNode = subNode != null ? $" > {subNode} " : subNode;

			p.LoadFromData($"{mainNode}{subNode}{{{attr}:{value}}}");
			widget.StyleContext.AddProvider(p, Gtk.StyleProviderPriority.User);

			if (value.StartsWith("url"))
				Realize(p);
		}

		public static void SetStyleValue(this Gtk.Widget widget, string value, string attr, string? subNode = null)
			=> widget.SetStyleValueNode(value, widget.CssMainNode(), attr, subNode);

		public static string? CssState(this Gtk.StateFlags it) =>
			it switch
			{
				StateFlags.Normal => null,
				StateFlags.Active => "active",
				StateFlags.Prelight => "hover",
				StateFlags.Selected => "selected",
				StateFlags.Insensitive => "disabled",
				StateFlags.Inconsistent => "inconsistent",
				StateFlags.Focused => "focused",
				StateFlags.Backdrop => "backdrop",
				StateFlags.DirLtr => "dir(ltr)",
				StateFlags.DirRtl => "dir(rtl)",
				StateFlags.Link => "link",
				StateFlags.Visited => "visited",
				StateFlags.Checked => "checked",
				StateFlags.DropActive => "drop(active)",
				_ => throw new ArgumentOutOfRangeException(nameof(it), it, null)
			};

	}

}