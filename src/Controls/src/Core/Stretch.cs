namespace Microsoft.Maui.Controls
{
	/// <summary>Specifies how content is scaled to fill its allocated space.</summary>
	public enum Stretch
	{
		/// <summary>Content preserves its original size.</summary>
		None,
		/// <summary>Content is resized to fill the destination, aspect ratio is not preserved.</summary>
		Fill,
		/// <summary>Content is resized to fit the destination while preserving aspect ratio.</summary>
		Uniform,
		/// <summary>Content is resized to fill the destination while preserving aspect ratio, clipping if necessary.</summary>
		UniformToFill
	}
}