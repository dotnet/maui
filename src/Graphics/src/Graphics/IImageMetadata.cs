#nullable enable

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents opaque metadata (such as EXIF) captured from an image when it is loaded with
	/// <see cref="ImageLoadOptions.PreserveMetadata"/> enabled. The metadata is carried through image
	/// transforms and can be re-embedded when the image is saved with
	/// <see cref="ImageSaveOptions.PreserveMetadata"/> enabled.
	/// </summary>
	/// <remarks>
	/// The contents are intentionally opaque and platform specific. Consumers should not depend on any
	/// particular representation; the metadata exists to be preserved across a load/transform/save
	/// round trip.
	/// </remarks>
	public interface IImageMetadata
	{
	}
}
