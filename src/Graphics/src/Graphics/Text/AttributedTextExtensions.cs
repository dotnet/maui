using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.Graphics.Text;

namespace Microsoft.Maui.Graphics.Text
{
	public static class AttributedTextExtensions
	{
		public static IAttributedText Optimize(this IAttributedText attributedText)
		{
			if (attributedText?.Text == null)
				return null;

			if (attributedText is AbstractAttributedText abstractAttributedText && abstractAttributedText.Optimal)
				return attributedText;

			var start = 0;
			var attributeIndex = 0;
			var text = attributedText.Text;
			var length = text.Length;
			var runs = new List<IAttributedTextRun>();
			attributedText.CreateParagraphRun(start, length, runs, attributeIndex);
			return new AttributedText(text, runs, true);
		}

		internal static List<IAttributedTextRun> OptimizeRuns(this IAttributedText attributedText)
		{
			if (attributedText?.Text == null)
				return null;

			if (attributedText is AbstractAttributedText abstractAttributedText && abstractAttributedText.Optimal)
			{
				if (attributedText.Runs == null)
					return null;

				if (attributedText.Runs is List<IAttributedTextRun> list)
					return list;

				return attributedText.Runs.ToList();
			}

			var start = 0;
			var attributeIndex = 0;
			var text = attributedText.Text;
			var length = text.Length;
			var runs = new List<IAttributedTextRun>();
			attributedText.CreateParagraphRun(start, length, runs, attributeIndex);
			return runs;
		}

		public static IReadOnlyList<IAttributedText> CreateParagraphs(this IAttributedText attributedText)
		{
			if (attributedText?.Text == null)
				return null;

			List<IAttributedText> paragraphs = new List<IAttributedText>();

			int start = 0;
			int attributeIndex = 0;

			using (var sr = new StringReader(attributedText.Text))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					var length = line.Length;

					var runs = new List<IAttributedTextRun>();
					attributeIndex = attributedText.CreateParagraphRun(start, length, runs, attributeIndex);

					var paragraph = new AttributedText(line, runs);
					paragraphs.Add(paragraph);

					start += length + 1;
				}
			}

			return paragraphs;
		}

		public static int CreateParagraphRun(
			this IAttributedText text,
			int start,
			int length,
			IList<IAttributedTextRun> runs,
			int startIndexForSearch = 0)
		{
			// If the text doesn't have any runs, then we can simply return
			if (text.Runs == null || text.Runs.Count == 0)
				return 0;

			// If we've already reached the end of the runs, we can simply return
			if (!(startIndexForSearch < text.Runs.Count))
				return startIndexForSearch;

			var end = start + length;
			var index = startIndexForSearch;

			do
			{
				var run = text.Runs[index];

				// If the run is after the end index, then we can go ahead and return
				if (end < run.Start)
					return index;

				if (run.Intersects(start, length))
				{
					if (start == run.Start)
					{
						var paragraphStart = run.Start - start;
						var paragraphLength = Math.Min(run.Length, length);
						runs.Add(new AttributedTextRun(paragraphStart, paragraphLength, run.Attributes));

						// If the length of the run is the same as the paragraph, then we know
						// that the next run (if any) will apply to to the next paragraph.
						if (run.Length == length)
							return index + 1;

						// If the run is longer than the line, then we know that the attributes from this run
						// will also apply to the next paragraph.
						if (run.Length > length)
							return index;

						// If the run length is less than the length of the line, then the next run may apply
						// to this line, so continue
					}
					else if (end == run.GetEnd())
					{
						var paragraphStart = Math.Max(run.Start - start, 0);
						var paragraphLength = Math.Min(run.Length, end - paragraphStart);
						runs.Add(new AttributedTextRun(paragraphStart, paragraphLength, run.Attributes));

						// Since the run and the line have the same end, we know that the next run (if any) will
						// apply to to the next paragraph.
						return index + 1;
					}
					else
					{
						var paragraphStart = Math.Max(run.Start - start, 0);
						var paragraphLength = Math.Min(run.Length, length - paragraphStart);
						runs.Add(new AttributedTextRun(paragraphStart, paragraphLength, run.Attributes));
					}
				}

				index++;
			} while (index < text.Runs.Count);

			return index;
		}

		public static IList<AttributedTextBlock> CreateBlocks(this IAttributedText text)
		{
			if (text?.Text == null)
				return null;

			var blocks = new List<AttributedTextBlock>();

			var start = 0;
			var end = text.Text.Length;

			if (text.Runs?.Count > 0)
			{
				foreach (var run in text.Runs)
				{
					if (start < run.Start)
					{
						var noAttrLength = run.Start - start;
						var noAttrValue = text.Text.Substring(start, noAttrLength);
						blocks.Add(new AttributedTextBlock(noAttrValue, null));
						start = run.Start;
					}

					var length = run.Length;
					if (length > 0)
					{
						var value = text.Text.Substring(start, length);
						blocks.Add(new AttributedTextBlock(value, run.Attributes));
						start = run.GetEnd();
					}
#if DEBUG
					else
						System.Diagnostics.Debug.WriteLine("Length should not be less then 0");
#endif
				}
			}

			if (start < end)
			{
				var value = text.Text.Substring(start);
				blocks.Add(new AttributedTextBlock(value, null));
			}

			return blocks;
		}
	}
}
