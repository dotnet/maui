#nullable enable
namespace Microsoft.Maui.Graphics
{
	public static class RectangleExtensions
	{
		public static Rectangle Inset(this Rectangle rectangle, double inset) 
		{
			if (inset == 0)
			{
				return rectangle;
			}

			return new Rectangle(rectangle.Left + inset, rectangle.Top + inset, 
				rectangle.Width - (2 * inset), rectangle.Height - (2 * inset));
		}
	}
}