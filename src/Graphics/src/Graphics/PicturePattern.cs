namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a pattern that draws a picture repeatedly.
	/// </summary>
	public class PicturePattern : AbstractPattern
	{
		private readonly IPicture _picture;

		/// <summary>
		/// Initializes a new instance of the <see cref="PicturePattern"/> class with the specified picture and step sizes.
		/// </summary>
		/// <param name="picture">The picture to use as a pattern.</param>
		/// <param name="stepX">The horizontal step size for repeating the pattern.</param>
		/// <param name="stepY">The vertical step size for repeating the pattern.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="picture"/> is null.</exception>
		public PicturePattern(IPicture picture, float stepX, float stepY) : base(picture.Width, picture.Height, stepX, stepY)
		{
			_picture = picture;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PicturePattern"/> class with the specified picture.
		/// Uses the picture's dimensions as step sizes.
		/// </summary>
		/// <param name="picture">The picture to use as a pattern.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="picture"/> is null.</exception>
		public PicturePattern(IPicture picture) : base(picture.Width, picture.Height)
		{
			_picture = picture;
		}

		/// <summary>
		/// Draws the picture pattern onto the specified canvas.
		/// </summary>
		/// <param name="canvas">The canvas to draw the pattern on.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="canvas"/> is null.</exception>
		public override void Draw(ICanvas canvas)
		{
			_picture.Draw(canvas);
		}
	}
}
