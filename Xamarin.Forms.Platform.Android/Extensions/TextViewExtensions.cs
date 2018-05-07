using Android.Text;
using Android.Widget;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	internal static class TextViewExtensions
	{
		public static void SetLineBreakMode(this TextView textView, LineBreakMode lineBreakMode)
		{
			switch (lineBreakMode)
			{
				case LineBreakMode.NoWrap:
					textView.SetMaxLines(1);
					textView.SetSingleLine(true);
					textView.Ellipsize = null;
					break;
				case LineBreakMode.WordWrap:
					textView.Ellipsize = null;
					textView.SetMaxLines(100);
					textView.SetSingleLine(false);
					break;
				case LineBreakMode.CharacterWrap:
					textView.Ellipsize = null;
					textView.SetMaxLines(100);
					textView.SetSingleLine(false);
					break;
				case LineBreakMode.HeadTruncation:
					textView.SetMaxLines(1);
					textView.SetSingleLine(true);
					textView.Ellipsize = TextUtils.TruncateAt.Start;
					break;
				case LineBreakMode.TailTruncation:
					textView.SetMaxLines(1);
					textView.SetSingleLine(true);
					textView.Ellipsize = TextUtils.TruncateAt.End;
					break;
				case LineBreakMode.MiddleTruncation:
					textView.SetMaxLines(1);
					textView.SetSingleLine(true);
					textView.Ellipsize = TextUtils.TruncateAt.Middle;
					break;
			}
		}

		public static void RecalculateSpanPositions(this TextView textView, Label element, SpannableString spannableString, SizeRequest finalSize)
		{
			if (element?.FormattedText?.Spans == null
				|| element.FormattedText.Spans.Count == 0)
				return;

			var labelWidth = finalSize.Request.Width;

			if (labelWidth <= 0 || finalSize.Request.Height <= 0)
				return;

			var layout = textView.Layout;

			if (layout == null)
				return;

			var text = spannableString.ToString();

			int next = 0;
			int count = 0;
			IList<int> totalLineHeights = new List<int>();

			for (int i = 0; i < spannableString.Length(); i = next)
			{
				var type = Java.Lang.Class.FromType(typeof(Java.Lang.Object));

				var span = element.FormattedText.Spans[count];

				count++;

				if (string.IsNullOrEmpty(span.Text))
					continue;

				// find the next span
				next = spannableString.NextSpanTransition(i, spannableString.Length(), type);

				// get all spans in the range - Android can have overlapping spans				
				var spans = spannableString.GetSpans(i, next, type);

				var startSpan = spans[0];
				var endSpan = spans[spans.Length - 1];

				var startSpanOffset = spannableString.GetSpanStart(startSpan);
				var endSpanOffset = spannableString.GetSpanEnd(endSpan);

				var startX = layout.GetPrimaryHorizontal(startSpanOffset);
				var endX = layout.GetPrimaryHorizontal(endSpanOffset);

				var startLine = layout.GetLineForOffset(startSpanOffset);
				var endLine = layout.GetLineForOffset(endSpanOffset);

				double[] lineHeights = new double[endLine - startLine + 1];

				// calculate all the different line heights
				for (var lineCount = startLine; lineCount <= endLine; lineCount++)
				{
					var lineHeight = layout.GetLineBottom(lineCount) - layout.GetLineTop(lineCount);
					lineHeights[lineCount - startLine] = lineHeight;

					if (totalLineHeights.Count <= lineCount)
						totalLineHeights.Add(lineHeight);
				}

				var yaxis = 0.0;

				for (var line = startLine; line > 0; line--)
					yaxis += totalLineHeights[line];

				((ISpatialElement)span).Region = Region.FromLines(lineHeights, labelWidth, startX, endX, yaxis).Inflate(10);
			}
		}
	}
}
