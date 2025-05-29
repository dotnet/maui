namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides extension methods for the <see cref="IPicture"/> interface.
	/// </summary>
	public static class PictureExtensions
	{
		/// <summary>
		/// Gets the bounds of a picture as a rectangle.
		/// </summary>
		/// <param name="target">The picture to get the bounds of.</param>
		/// <returns>A <see cref="RectF"/> representing the position and size of the picture, or default if the picture is null.</returns>
		public static RectF GetBounds(this IPicture target)
		{
			if (target == null)
				return default;
			return new RectF(target.X, target.Y, target.Width, target.Height);
		}
	}
}
