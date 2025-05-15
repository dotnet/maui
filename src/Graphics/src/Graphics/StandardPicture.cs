namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a method that performs drawing operations on a canvas.
	/// </summary>
	/// <param name="canvas">The canvas on which to perform drawing operations.</param>
	public delegate void DrawingCommand(ICanvas canvas);

	/// <summary>
	/// Provides a standard implementation of the <see cref="IPicture"/> interface using drawing commands.
	/// </summary>
	public class StandardPicture : IPicture
	{
		private readonly DrawingCommand[] _commands;

		/// <summary>
		/// Gets the x-coordinate of the picture's origin.
		/// </summary>
		public float X { get; }

		/// <summary>
		/// Gets the y-coordinate of the picture's origin.
		/// </summary>
		public float Y { get; }

		/// <summary>
		/// Gets the width of the picture.
		/// </summary>
		public float Width { get; }

		/// <summary>
		/// Gets the height of the picture.
		/// </summary>
		public float Height { get; }

		/// <summary>
		/// Gets or sets a hash value that uniquely identifies this picture's content.
		/// </summary>
		public string Hash { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardPicture"/> class with the specified location, size, and drawing commands.
		/// </summary>
		/// <param name="x">The x-coordinate of the picture's origin.</param>
		/// <param name="y">The y-coordinate of the picture's origin.</param>
		/// <param name="width">The width of the picture.</param>
		/// <param name="height">The height of the picture.</param>
		/// <param name="commands">The drawing commands that define the picture's content.</param>
		/// <param name="hash">An optional hash value that uniquely identifies this picture's content.</param>
		public StandardPicture(float x, float y, float width, float height, DrawingCommand[] commands, string hash = null)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
			_commands = commands;

			Hash = hash;
		}

		public void Draw(ICanvas canvas)
		{
			if (_commands != null)
				foreach (var command in _commands)
					command.Invoke(canvas);
		}
	}
}
