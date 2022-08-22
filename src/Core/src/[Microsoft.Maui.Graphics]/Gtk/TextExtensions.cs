using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Graphics.Native.Gtk
{

	public static class TextExtensions
	{

		public static double GetLineHeigth(this Pango.Layout layout, int numLines, bool scaled = true)
		{
			var inkRect = new Pango.Rectangle();
			var logicalRect = new Pango.Rectangle();
			var numLines1 = numLines > 0 ? Math.Min(numLines, layout.LineCount) : layout.LineCount;
			var lineHeigh = 0d;

			var metrics = layout.Context.GetMetrics(layout.FontDescription, Pango.Language.Default);
			var baseline = metrics.Ascent / (double)(metrics.Ascent + metrics.Descent);
			layout.GetLineReadonly(0).GetExtents(ref inkRect, ref logicalRect);
			lineHeigh += (scaled ? logicalRect.Height.ScaledFromPango() : logicalRect.Height);
			var fact = 1f;

			if (layout.LineSpacing > 0)
			{
				fact = layout.LineSpacing;
			}

			return lineHeigh * baseline + lineHeigh + (lineHeigh * fact * (numLines - 1));
		}

		public static (int width, int height) GetPixelSize(this Pango.Layout layout, string text, double desiredSize = -1d, bool heightForWidth = true)
		{
			desiredSize = double.IsInfinity(desiredSize) ? -1 : desiredSize;

			if (desiredSize > 0)
			{
				if (heightForWidth)
				{
					layout.Width = desiredSize.ScaledToPango();
				}
				else
				{
					layout.Height = desiredSize.ScaledToPango();
				}
			}

			layout.SetText(text);
			layout.GetPixelSize(out var textWidth, out var textHeight);

			return (textWidth, textHeight);
		}

		public static double ScaledFromPango(this double it)
			=> Math.Ceiling(it / Pango.Scale.PangoScale);

		public static Pango.AttrList? AddAttrFor(this Pango.AttrList? l, TextDecorations it)
		{
			if (l == null)
				return l;

			if (it.HasFlag(TextDecorations.Underline))
			{

				l.Insert(new Pango.AttrUnderline(Pango.Underline.Single));
			}

			if (it.HasFlag(TextDecorations.Strikethrough))
			{
				l.Insert(new Pango.AttrStrikethrough(true));

			}

			return l;
		}

		public static Pango.AttrList? AddAttrFor(this Pango.AttrList? list, int spacing)
		{
			if (spacing <= 1.ScaledToPango())
				return list;

			list?.Insert(new Pango.AttrLetterSpacing(spacing));

			return list;
		}

		public static Pango.AttrList? AttrListFor(this Pango.AttrList? list, TextDecorations decorations, double letterspacing)
		{
			var spacing = letterspacing.ScaledToPango();

			if (decorations == TextDecorations.None && letterspacing <= 1)
				return null;

			var l = new Pango.AttrList();

			if (decorations != TextDecorations.None)
				l.AddAttrFor(decorations);

			if (letterspacing > 1)
				l.AddAttrFor(spacing);

			return l;

		}

		/// <summary>
		/// Use this only if there are no other attributes to set
		/// </summary>
		/// <param name="list"></param>
		/// <param name="letterspacing"></param>
		/// <returns></returns>
		public static Pango.AttrList? AttrListFor(this Pango.AttrList? list, double letterspacing)
		{
			var spacing = letterspacing.ScaledToPango();

			if (letterspacing <= 1)
				return null;

			var l = new Pango.AttrList();

			l.AddAttrFor(spacing);

			return l;

		}

		static Dictionary<TextDecorations, Pango.AttrList>? _attrLists;

		[Obsolete("not working with spacing")]
		static Pango.AttrList? DefaultAttrListFor(this TextDecorations decorations)
		{
			_attrLists ??= new();

			if (TextDecorations.None == decorations)
			{
				return null;
			}

			if (_attrLists.TryGetValue(decorations, out var l))
				return l;

			l = new Pango.AttrList();

			l.AddAttrFor(decorations);

			_attrLists[decorations] = l;

			return l;
		}

		[Obsolete("not working together with letterspacing")]
		static Pango.AttrList? AttrListFor(this Pango.AttrList? list, TextDecorations decorations)
		{

			// something wrong with Filter; iterater is null then
			var l = list?.Filter(f => f.Type != Pango.AttrType.Underline || f.Type != Pango.AttrType.Strikethrough);

			if (decorations == TextDecorations.None)
				return list;

			list ??= new Pango.AttrList();

			list.AddAttrFor(decorations);

			return list;

		}

	}

}