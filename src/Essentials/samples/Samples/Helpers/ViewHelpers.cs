using Xamarin.Forms;

namespace Samples.Helpers
{
	public static class ViewHelpers
	{
		public static Rectangle GetAbsoluteBounds(this Xamarin.Forms.View element)
		{
			Element looper = element;

			var absoluteX = element.X + element.Margin.Top;
			var absoluteY = element.Y + element.Margin.Left;

			// TODO: add logic to handle titles, headers, or other non-view bars

			while (looper.Parent != null)
			{
				looper = looper.Parent;
				if (looper is Xamarin.Forms.View v)
				{
					absoluteX += v.X + v.Margin.Top;
					absoluteY += v.Y + v.Margin.Left;
				}
			}

			return new Rectangle(absoluteX, absoluteY, element.Width, element.Height);
		}

		public static System.Drawing.Rectangle ToSystemRectangle(this Rectangle rect) =>
			new System.Drawing.Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
	}
}
