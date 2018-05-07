using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Xamarin.Forms.Platform.WPF
{
	public static class FormattedStringExtensions
	{
		public static IEnumerable<Inline> ToInlines(this FormattedString formattedString)
		{
			foreach (Span span in formattedString.Spans)
				yield return span.ToRun();
		}

		public static Run ToRun(this Span span)
		{
			var run = new Run { Text = span.Text ?? string.Empty };

			if (span.TextColor != Color.Default)
				run.Foreground = span.TextColor.ToBrush();
			
			if (span.BackgroundColor != Color.Default)
				run.Background = span.BackgroundColor.ToBrush();

			if (!span.IsDefault())
#pragma warning disable 618
				run.ApplyFont(span.Font);
#pragma warning restore 618

			return run;
		}
	}

}
