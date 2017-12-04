using System.Threading.Tasks;
using ElmSharp;
using Xamarin.Forms.Internals;
using EImage = ElmSharp.Image;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Extends the ElmSharp.Image class with functionality useful to renderer.
	/// </summary>
	public class Image : EImage, IMeasurable
	{
		Aspect _aspect;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xamarin.Forms.Platform.Tizen.Native.Image"/> class.
		/// </summary>
		/// <param name="parent">The parent EvasObject.</param>
		public Image(EvasObject parent) : base(parent)
		{
			IsScaling = true;
			CanScaleUp = true;
			CanScaleDown = true;

			ApplyAspect(Aspect.AspectFit);
		}

		/// <summary>
		/// Gets or sets the image aspect ratio preserving option.
		/// </summary>
		/// <value>The aspect option.</value>
		public Aspect Aspect
		{
			get
			{
				return _aspect;
			}

			set
			{
				if (_aspect != value)
				{
					ApplyAspect(value);
				}
			}
		}

		/// <summary>
		/// Loads image data from the given <see cref="Xamarin.Forms.ImageSource"/> asynchronously.
		/// </summary>
		/// <returns>A task which will be completed when image data is loaded.</returns>
		/// <param name="source">Image source specifying from where the image data has to be loaded.</param>
		public async Task<bool> LoadFromImageSourceAsync(ImageSource source)
		{
			IImageSourceHandler handler;
			bool isLoadComplate = false;
			if (source != null && (handler = Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				isLoadComplate = await handler.LoadImageAsync(this, source);
			}
			if (!isLoadComplate)
			{
				//If it fails, call the Load function to remove the previous image.
				Load(string.Empty);
			}

			return isLoadComplate;
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

		/// <summary>
		/// Sets the <c>IsFixedAspect</c> and <c>CanFillOutside</c> properties according to the given <paramref name="aspect"/>.
		/// </summary>
		/// <param name="aspect">The aspect setting to be applied to the image.</param>
		void ApplyAspect(Aspect aspect)
		{
			_aspect = aspect;

			switch (_aspect)
			{
				case Aspect.AspectFit:
					IsFixedAspect = true;
					CanFillOutside = false;
					break;

				case Aspect.AspectFill:
					IsFixedAspect = true;
					CanFillOutside = true;
					break;

				case Aspect.Fill:
					IsFixedAspect = false;
					CanFillOutside = false;
					break;

				default:
					Log.Warn("Invalid Aspect value: {0}", _aspect);
					break;
			}
		}
	}
}