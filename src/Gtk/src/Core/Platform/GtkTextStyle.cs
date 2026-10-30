using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Gtk.Fonts;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// Text styling shared by the text-carrying handlers (Label, Button, Entry, Editor...), expressed as CSS declarations
	/// on the widget's <see cref="GtkStyle"/>: GTK's labels and text widgets pick colour, font, letter spacing and text
	/// decorations up from their CSS node.
	/// </summary>
	public static class GtkTextStyle
	{
		/// <summary>Sets the text colour (CSS <c>color</c>), or clears it when <paramref name="color"/> is <see langword="null"/>.</summary>
		/// <param name="widget">The widget.</param>
		/// <param name="color">The colour.</param>
		/// <param name="selector">A CSS sub-selector (for example <c>" placeholder"</c>).</param>
		public static void SetColor(global::Gtk.Widget widget, Color? color, string selector = "") =>
			GtkStyle.For(widget).Set("color", color?.ToCss(), selector);

		/// <summary>Applies <paramref name="font"/> as CSS font declarations.</summary>
		/// <param name="widget">The widget.</param>
		/// <param name="font">The MAUI font.</param>
		/// <param name="fontManager">The font manager; a <see cref="GtkFontManager"/> resolves registered font aliases.</param>
		/// <param name="selector">A CSS sub-selector.</param>
		public static void SetFont(global::Gtk.Widget widget, Font font, IFontManager? fontManager, string selector = "")
		{
			var manager = fontManager as GtkFontManager ?? new GtkFontManager(null);
			var style = GtkStyle.For(widget);

			style.ClearPrefix("font-", selector);
			foreach (var declaration in manager.GetCssFont(font).Split(';', StringSplitOptions.RemoveEmptyEntries))
			{
				var separator = declaration.IndexOf(':', StringComparison.Ordinal);
				style.Set(declaration.Substring(0, separator).Trim(), declaration.Substring(separator + 1).Trim(), selector);
			}
		}

		/// <summary>Sets the letter spacing in pixels.</summary>
		/// <param name="widget">The widget.</param>
		/// <param name="characterSpacing">The spacing (0 clears it).</param>
		/// <param name="selector">A CSS sub-selector.</param>
		public static void SetCharacterSpacing(global::Gtk.Widget widget, double characterSpacing, string selector = "") =>
			GtkStyle.For(widget).Set("letter-spacing", characterSpacing == 0 ? null : characterSpacing.ToString("0.##", CultureInfo.InvariantCulture) + "px", selector);

		/// <summary>Sets the padding (CSS <c>padding</c>).</summary>
		/// <param name="widget">The widget.</param>
		/// <param name="padding">The thickness.</param>
		/// <param name="selector">A CSS sub-selector.</param>
		public static void SetPadding(global::Gtk.Widget widget, Thickness padding, string selector = "") =>
			GtkStyle.For(widget).Set("padding", FormattableString.Invariant($"{padding.Top:0.##}px {padding.Right:0.##}px {padding.Bottom:0.##}px {padding.Left:0.##}px"), selector);

		/// <summary>Sets underline/strikethrough text decorations.</summary>
		/// <param name="widget">The widget.</param>
		/// <param name="decorations">The decorations.</param>
		public static void SetTextDecorations(global::Gtk.Widget widget, TextDecorations decorations)
		{
			string? value = null;
			if (decorations.HasFlag(TextDecorations.Underline) && decorations.HasFlag(TextDecorations.Strikethrough))
				value = "underline line-through";
			else if (decorations.HasFlag(TextDecorations.Underline))
				value = "underline";
			else if (decorations.HasFlag(TextDecorations.Strikethrough))
				value = "line-through";

			GtkStyle.For(widget).Set("text-decoration-line", value);
		}

		/// <summary>Resolves the <see cref="IFontManager"/> of a handler.</summary>
		/// <param name="handler">The handler.</param>
		public static IFontManager? GetFontManager(IElementHandler handler) =>
			handler.MauiContext?.Services.GetService<IFontManager>();

		/// <summary>Converts a horizontal <see cref="TextAlignment"/> to a GTK alignment fraction (0, 0.5, 1).</summary>
		/// <param name="alignment">The MAUI alignment.</param>
		/// <param name="flowDirection">The flow direction (start/end swap under right-to-left).</param>
		public static float ToAlignment(TextAlignment alignment, FlowDirection flowDirection = FlowDirection.LeftToRight)
		{
			bool rtl = flowDirection == FlowDirection.RightToLeft;
			return alignment switch
			{
				TextAlignment.Center => 0.5f,
				TextAlignment.End => rtl ? 0f : 1f,
				_ => rtl ? 1f : 0f,
			};
		}
	}
}
