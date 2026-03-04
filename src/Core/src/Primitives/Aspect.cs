namespace Microsoft.Maui
{
	/// <summary>
	/// Defines how an image is displayed.
	/// </summary>
	public enum Aspect
	{
		/// <summary>
		/// Scale the image to fit the view. Some parts may be left empty (letter boxing).
		/// </summary>
		AspectFit,
		/// <summary>
		/// Scale the image to fill the view. Some parts may be clipped in order to fill the view.
		/// </summary>
		AspectFill,
		/// <summary>
		/// Scale the image so it exactly fills the view. Scaling may not be uniform in X and Y.
		/// </summary>
		Fill,
		/// <summary>
		/// Center the image in the view without scaling.
		/// </summary>
		Center
	}
}
