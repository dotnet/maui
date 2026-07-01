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
		/// Provides high-fidelity scaling when preserving smooth detail is critical.
		/// Auto preserves the existing mipmapped downscale behavior.
		/// </summary>
		Best = 1,

		/// <summary>
		/// Fastest processing with nearest-neighbor interpolation.
		/// May appear pixelated when scaling. Ideal for pixel art
		/// or when build speed matters more than visual fidelity.
		/// </summary>
		Fastest = 2,
	}
}
