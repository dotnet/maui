using System;
using Gtk;
using Pango;
using WrapMode = Pango.WrapMode;

namespace Microsoft.Maui
{

	public static class LabelExtensions
	{

		public static void UpdateText(this Label nativeLabel, ILabel label, TextType type = TextType.Text)
		{
			// https://docs.gtk.org/gtk3/method.Label.set_use_markup.html

			if (type == TextType.Html)
			{
				nativeLabel.Markup = HtmlToPangoMarkup(label.Text);
			}
			else
			{
				nativeLabel.UseMarkup = false;
				nativeLabel.Text = label.Text;
			}

		}

		// https://docs.gtk.org/Pango/pango_markup.html
		[MissingMapper]
		public static string HtmlToPangoMarkup(string text)
		{
			return text;
		}

		public static void UpdateMaxLines(this Label nativeLabel, ILabel label)
		{
			// nativeLabel.Lines = label.MaxLines;
			nativeLabel.AdjustMaxLines();

		}

		public static void AdjustMaxLines(this Label nativeLabel)
		{
			if (nativeLabel.Lines > 0)
			{
				nativeLabel.LineWrap = true;

				if (nativeLabel.Ellipsize == EllipsizeMode.None)
					nativeLabel.Ellipsize = EllipsizeMode.End;
			}
		}

		public static Graphics.Platform.Gtk.LineBreakMode GetLineBreakMode(this LineBreakMode lineBreakMode) =>
			lineBreakMode switch
			{
				LineBreakMode.NoWrap => Graphics.Platform.Gtk.LineBreakMode.None,
				LineBreakMode.WordWrap => Graphics.Platform.Gtk.LineBreakMode.WordWrap,
				LineBreakMode.CharacterWrap => Graphics.Platform.Gtk.LineBreakMode.CharacterWrap,
				LineBreakMode.HeadTruncation => Graphics.Platform.Gtk.LineBreakMode.HeadTruncation,
				LineBreakMode.TailTruncation => Graphics.Platform.Gtk.LineBreakMode.TailTruncation,
				LineBreakMode.MiddleTruncation => Graphics.Platform.Gtk.LineBreakMode.MiddleTruncation,
				_ => throw new ArgumentOutOfRangeException()
			};

		public static Maui.Graphics.HorizontalAlignment GetHorizontalAlignment(this TextAlignment alignment) =>
			alignment switch
			{

				TextAlignment.Start => Graphics.HorizontalAlignment.Left,
				TextAlignment.Center => Graphics.HorizontalAlignment.Center,
				TextAlignment.End => Graphics.HorizontalAlignment.Right,
				_ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null)
			};

		public static Maui.Graphics.VerticalAlignment GetVerticalAlignment(this TextAlignment alignment) =>
			alignment switch
			{

				TextAlignment.Start => Graphics.VerticalAlignment.Top,
				TextAlignment.Center => Graphics.VerticalAlignment.Center,
				TextAlignment.End => Graphics.VerticalAlignment.Bottom,
				_ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null)
			};

		public static Microsoft.Maui.Graphics.Platform.Gtk.LineBreakMode GetLineBreakMode(this Label nativeLabel)
		{
			var res = nativeLabel.Ellipsize switch
			{
				EllipsizeMode.None => Graphics.Platform.Gtk.LineBreakMode.None,
				EllipsizeMode.Start => Graphics.Platform.Gtk.LineBreakMode.Head | Graphics.Platform.Gtk.LineBreakMode.Ellipsis,
				EllipsizeMode.Middle => Graphics.Platform.Gtk.LineBreakMode.Middle | Graphics.Platform.Gtk.LineBreakMode.Ellipsis,
				EllipsizeMode.End => Graphics.Platform.Gtk.LineBreakMode.Tail | Graphics.Platform.Gtk.LineBreakMode.Ellipsis,
				_ => throw new ArgumentOutOfRangeException()
			};

			var res1 = nativeLabel.LineWrapMode switch
			{
				WrapMode.Word => Graphics.Platform.Gtk.LineBreakMode.Word,
				WrapMode.Char => Graphics.Platform.Gtk.LineBreakMode.Character,
				WrapMode.WordChar => Graphics.Platform.Gtk.LineBreakMode.Character | Graphics.Platform.Gtk.LineBreakMode.Word,
				_ => throw new ArgumentOutOfRangeException()
			};

			if (nativeLabel.LineWrap || nativeLabel.Wrap)
			{
				res |= res1;
			}

			return res;
		}

		public static void UpdateLineBreakMode(this Label nativeLabel, ILabel label)
		{
			var labelLineBreakMode = LineBreakMode.CharacterWrap;
			var labelMaxLines = 0;
			
			switch (labelLineBreakMode)
			{
				case LineBreakMode.NoWrap:
					nativeLabel.LineWrap = labelMaxLines > 0;
					nativeLabel.Ellipsize = Pango.EllipsizeMode.None;

					break;
				case LineBreakMode.WordWrap:
					nativeLabel.LineWrap = true;
					nativeLabel.Wrap = true;

					nativeLabel.LineWrapMode = Pango.WrapMode.Word;
					nativeLabel.Ellipsize = Pango.EllipsizeMode.None;

					break;
				case LineBreakMode.CharacterWrap:
					nativeLabel.LineWrap = true;
					nativeLabel.Wrap = true;
					nativeLabel.LineWrapMode = Pango.WrapMode.Char;
					nativeLabel.Ellipsize = Pango.EllipsizeMode.None;

					break;
				case LineBreakMode.HeadTruncation:
					nativeLabel.LineWrap = true;
					nativeLabel.Wrap = true;
					nativeLabel.LineWrapMode = Pango.WrapMode.Word;
					nativeLabel.Ellipsize = Pango.EllipsizeMode.Start;

					break;
				case LineBreakMode.TailTruncation:
					nativeLabel.LineWrap = true;
					nativeLabel.LineWrapMode = Pango.WrapMode.Word;
					nativeLabel.Wrap = true;
					nativeLabel.Ellipsize = Pango.EllipsizeMode.End;

					break;
				case LineBreakMode.MiddleTruncation:
					nativeLabel.LineWrap = true;
					nativeLabel.Wrap = true;
					nativeLabel.LineWrapMode = Pango.WrapMode.Word;
					nativeLabel.Ellipsize = Pango.EllipsizeMode.Middle;

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			nativeLabel.AdjustMaxLines();
		}

		public static void UpdateHorizontalTextAlignment(this Label nativeLabel, ILabel label)
		{
			nativeLabel.Justify = label.HorizontalTextAlignment.ToJustification();
			nativeLabel.Xalign = label.HorizontalTextAlignment.ToXyAlign();

		}

		public static void UpdateVerticalTextAlignment(this Label nativeLabel, ILabel label)
		{
			nativeLabel.Yalign = label.VerticalTextAlignment.ToXyAlign();
		}

	}

}