using System;
using Gtk;
using Pango;
using WrapMode = Pango.WrapMode;

namespace Microsoft.Maui
{

	public static class LabelExtensions
	{

		public static void UpdateText(this Label nativeLabel, ILabel label)
		{
			nativeLabel.Text = label.Text;
		}

		public static void UpdateMaxLines(this Label nativeLabel, ILabel label)
		{
			nativeLabel.Lines = label.MaxLines;
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

		public static Microsoft.Maui.Graphics.Extras.LineBreakMode GetLineBreakMode(this LineBreakMode lineBreakMode) =>
			lineBreakMode switch
			{
				LineBreakMode.NoWrap => Graphics.Extras.LineBreakMode.None,
				LineBreakMode.WordWrap => Graphics.Extras.LineBreakMode.WordWrap,
				LineBreakMode.CharacterWrap => Graphics.Extras.LineBreakMode.CharacterWrap,
				LineBreakMode.HeadTruncation => Graphics.Extras.LineBreakMode.HeadTruncation,
				LineBreakMode.TailTruncation => Graphics.Extras.LineBreakMode.TailTruncation,
				LineBreakMode.MiddleTruncation => Graphics.Extras.LineBreakMode.MiddleTruncation,
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

		public static Microsoft.Maui.Graphics.Extras.LineBreakMode GetLineBreakMode(this Label nativeLabel)
		{
			var res = nativeLabel.Ellipsize switch
			{
				EllipsizeMode.None => Graphics.Extras.LineBreakMode.None,
				EllipsizeMode.Start => Graphics.Extras.LineBreakMode.Start | Graphics.Extras.LineBreakMode.Elipsis,
				EllipsizeMode.Middle => Graphics.Extras.LineBreakMode.Center | Graphics.Extras.LineBreakMode.Elipsis,
				EllipsizeMode.End => Graphics.Extras.LineBreakMode.End | Graphics.Extras.LineBreakMode.Elipsis,
				_ => throw new ArgumentOutOfRangeException()
			};

			var res1 = nativeLabel.LineWrapMode switch
			{
				WrapMode.Word => Graphics.Extras.LineBreakMode.Word,
				WrapMode.Char => Graphics.Extras.LineBreakMode.Character,
				WrapMode.WordChar => Graphics.Extras.LineBreakMode.Character | Graphics.Extras.LineBreakMode.Word,
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
			switch (label.LineBreakMode)
			{
				case LineBreakMode.NoWrap:
					nativeLabel.LineWrap = label.MaxLines > 0;
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