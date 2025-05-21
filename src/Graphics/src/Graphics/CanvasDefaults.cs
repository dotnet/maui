namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides default values for canvas-related properties.
	/// </summary>
	public static class CanvasDefaults
	{
		/// <summary>
		/// The default shadow color, which is black with 50% opacity.
		/// </summary>
		public static readonly Color DefaultShadowColor = new Color(0f, 0f, 0f, .5f);

		/// <summary>
		/// The default shadow offset, which is 5 units in both x and y directions.
		/// </summary>
		public static readonly SizeF DefaultShadowOffset = new SizeF(5, 5);

		/// <summary>
		/// The default shadow blur value, which is 5.
		/// </summary>
		public const float DefaultShadowBlur = 5;

		/// <summary>
		/// The default miter limit value, which is 10.
		/// </summary>
		public const float DefaultMiterLimit = 10;
	}
}
