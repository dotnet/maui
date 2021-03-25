using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
#if WINDOWS_UWP
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using NativeSize = Windows.Foundation.Size;
#else
using System.Windows.Controls;
using System.Windows.Documents;
using NativeSize = System.Windows.Size;
#endif

#if WINDOWS_UWP
namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
#else
namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
#endif
{
	internal static class TextBlockExtensions
	{

		public static double FindDefaultLineHeight(this TextBlock control, Inline inline)
		{
			control.Inlines.Add(inline);

			control.Measure(new NativeSize(double.PositiveInfinity, double.PositiveInfinity));

			var height = control.DesiredSize.Height;

			control.Inlines.Remove(inline);
			control = null;

			return height;
		}

		public static void RecalculateSpanPositions(this TextBlock control, Label element, IList<double> inlineHeights)
		{
			if (element?.FormattedText?.Spans == null
				|| element.FormattedText.Spans.Count == 0)
				return;

			var labelWidth = control.ActualWidth;

			if (labelWidth <= 0 || control.Height <= 0)
				return;

			for (int i = 0; i < element.FormattedText.Spans.Count; i++)
			{
				var span = element.FormattedText.Spans[i];

				var inline = control.Inlines.ElementAt(i);
				var rect = inline.ContentStart.GetCharacterRect(LogicalDirection.Forward);
				var endRect = inline.ContentEnd.GetCharacterRect(LogicalDirection.Forward);

				var positions = new List<Rectangle>();


				var defaultLineHeight = inlineHeights[i];

				var yaxis = rect.Top;
				var lineHeights = new List<double>();
				while (yaxis < endRect.Bottom)
				{
					double lineHeight;
					if (yaxis == rect.Top) // First Line
					{
						lineHeight = rect.Bottom - rect.Top;
					}
					else if (yaxis != endRect.Top) // Middle Line(s)
					{
						lineHeight = defaultLineHeight;
					}
					else // Bottom Line
					{
						lineHeight = endRect.Bottom - endRect.Top;
					}
					lineHeights.Add(lineHeight);
					yaxis += lineHeight;
				}

				((ISpatialElement)span).Region = Region.FromLines(lineHeights.ToArray(), labelWidth, rect.X, endRect.X + endRect.Width, rect.Top).Inflate(10);

			}
		}

	}
}
