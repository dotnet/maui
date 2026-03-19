namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Controls the quality of image resampling during resize operations.
	/// </summary>
	internal enum ResizeQuality
	{
		/// <summary>
		/// Default behavior, preserving existing image output.
		/// Uses bilinear interpolation with mipmaps.
		/// </summary>
		Auto = 0,

		/// <summary>
		/// Highest quality output using Mitchell cubic resampler.
		/// Best for upscaling or when visual fidelity is critical.
		/// </summary>
		Best = 1,

		/// <summary>
		/// Fastest processing with nearest-neighbor interpolation.
		/// Produces smaller file sizes but may appear pixelated when scaling.
		/// Ideal for pixel art or when file size matters more than quality.
		/// </summary>
		Fastest = 2,
	}
}
