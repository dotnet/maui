namespace Microsoft.Maui.Graphics
{
	public static class PictureExtensions
	{
		public static RectF GetBounds(this IPicture target)
		{
			if (target == null) return default;
			return new RectF(target.X, target.Y, target.Width, target.Height);
		}
	}
}
