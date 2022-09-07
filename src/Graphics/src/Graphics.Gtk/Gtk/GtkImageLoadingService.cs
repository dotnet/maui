using System.IO;

namespace Microsoft.Maui.Graphics.Platform.Gtk;

	public class GtkImageLoadingService : IImageLoadingService
	{
		public IImage FromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			return GtkImage.FromStream(stream, formatHint);
		}
	}

