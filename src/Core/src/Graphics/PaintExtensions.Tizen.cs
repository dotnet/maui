namespace Microsoft.Maui.Graphics
{
	public static partial class PaintExtensions
	{
		public static IDrawable ToDrawable(this Paint paint, PathF path)
		{
			return new BackgroundDrawable(paint, path);
		}

		public static IDrawable ToDrawable(this Paint paint, IBorder border)
		{
			return new BorderDrawable(paint, border);
		}
	}
}
