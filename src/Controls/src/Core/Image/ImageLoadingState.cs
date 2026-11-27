namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Describes the loading state of the <see cref="Image.Source"/> of an <see cref="Image"/>.
	/// </summary>
	public enum ImageLoadingState
	{
		/// <summary>No image has been loaded, for example because <see cref="Image.Source"/> is <see langword="null"/>.</summary>
		Idle,

		/// <summary>The image is loading.</summary>
		Loading,

		/// <summary>The image loaded and is displayed.</summary>
		Loaded,

		/// <summary>The image failed to load.</summary>
		Failed,
	}
}
