using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Samples.Helpers
{
	public static class ViewHelpers
	{
		public static Rectangle GetAbsoluteBounds(this Microsoft.Maui.Controls.View element)
		{
			Element looper = element;

			var absoluteX = element.X + element.Margin.Top;
			var absoluteY = element.Y + element.Margin.Left;

			// TODO: add logic to handle titles, headers, or other non-view bars

			while (looper.Parent != null)
			{
				looper = looper.Parent;
				if (looper is Microsoft.Maui.Controls.View v)
				{
					absoluteX += v.X + v.Margin.Top;
					absoluteY += v.Y + v.Margin.Left;
				}
			}

			return new Rectangle(absoluteX, absoluteY, element.Width, element.Height);
		}
	}
}
