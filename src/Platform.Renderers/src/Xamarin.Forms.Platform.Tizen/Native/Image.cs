using System.Threading.Tasks;
using ElmSharp;
using EImage = ElmSharp.Image;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Extends the ElmSharp.Image class with functionality useful to renderer.
	/// </summary>
	public class Image : EImage, IMeasurable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xamarin.Forms.Platform.Tizen.Native.Image"/> class.
		/// </summary>
		/// <param name="parent">The parent EvasObject.</param>
		public Image(EvasObject parent) : base(parent)
		{
		}

		/// <summary>
		/// Implements the <see cref="Xamarin.Forms.Platform.Tizen.Native.IMeasurable"/> interface.
		/// </summary>
		/// <param name="availableWidth">Available width.</param>
		/// <param name="availableHeight">Available height.</param>
		public ESize Measure(int availableWidth, int availableHeight)
		{
			var imageSize = ObjectSize;
			var size = new ESize()
			{
				Width = imageSize.Width,
				Height = imageSize.Height,
			};

			if (0 != availableWidth && 0 != availableHeight
				&& (imageSize.Width > availableWidth || imageSize.Height > availableHeight))
			{
				// when available size is limited and insufficient for the image ...
				double imageRatio = (double)imageSize.Width / (double)imageSize.Height;
				double availableRatio = (double)availableWidth / (double)availableHeight;
				// depending on the relation between availableRatio and imageRatio, copy the availableWidth or availableHeight
				// and calculate the size which preserves the image ratio, but does not exceed the available size
				size.Width = availableRatio > imageRatio ? imageSize.Width * availableHeight / imageSize.Height : availableWidth;
				size.Height = availableRatio > imageRatio ? availableHeight : imageSize.Height * availableWidth / imageSize.Width;
			}

			return size;
		}
	}
}