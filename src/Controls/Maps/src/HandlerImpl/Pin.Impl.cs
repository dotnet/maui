using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.Maps
{
	public partial class Pin : IMapPin
	{
		/// <summary>
		/// Explicit implementation of IMapPin.ImageSource to convert from ImageSource to IImageSource.
		/// </summary>
		IImageSource? IMapPin.ImageSource => ImageSource;
	}
}
