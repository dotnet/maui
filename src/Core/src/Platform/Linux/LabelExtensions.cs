using System;
using Gtk;

namespace Microsoft.Maui
{
	public static class LabelExtensions
	{
		public static void UpdateText(this Label nativeLabel, ILabel label)
		{
			nativeLabel.Text = label.Text;
		}

		public static void UpdateLineBreakMode(this Label nativeLabel, ILabel label)
		{
			switch (label.LineBreakMode)
			{
				case LineBreakMode.NoWrap:
					nativeLabel.LineWrap = false;
					nativeLabel.Ellipsize = Pango.EllipsizeMode.None;
					break;
				case LineBreakMode.WordWrap:
					nativeLabel.LineWrap = true;
					nativeLabel.LineWrapMode = Pango.WrapMode.Word;
					nativeLabel.Ellipsize = Pango.EllipsizeMode.None;
					break;
				case LineBreakMode.CharacterWrap:
					nativeLabel.LineWrap = true;
					nativeLabel.LineWrapMode = Pango.WrapMode.Char;
					nativeLabel.Ellipsize = Pango.EllipsizeMode.None;
					break;
				case LineBreakMode.HeadTruncation:
					nativeLabel.LineWrap = false;
					nativeLabel.LineWrapMode = Pango.WrapMode.Word;
					nativeLabel.Ellipsize = Pango.EllipsizeMode.Start;
					break;
				case LineBreakMode.TailTruncation:
					nativeLabel.LineWrap = false;
					nativeLabel.LineWrapMode = Pango.WrapMode.Word;
					nativeLabel.Ellipsize = Pango.EllipsizeMode.End;
					break;
				case LineBreakMode.MiddleTruncation:
					nativeLabel.LineWrap = false;
					nativeLabel.LineWrapMode = Pango.WrapMode.Word;
					nativeLabel.Ellipsize = Pango.EllipsizeMode.Middle;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static void UpdateTextAlignment(this Label nativeLabel, ILabel label)
		{
			var hAlignmentValue = label.HorizontalTextAlignment.ToNative();

			nativeLabel.Halign = hAlignmentValue; 
		}
	}
}