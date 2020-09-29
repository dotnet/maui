using System.Collections.Generic;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ImageCellRenderer : TextCellRenderer
	{
		public ImageCellRenderer() : this(ThemeManager.GetImageCellRendererStyle())
		{
			ImagePart = this.GetImagePart();
		}

		protected ImageCellRenderer(string style) : base(style)
		{
		}

		protected string ImagePart { get; set; }

		protected override EvasObject OnGetContent(Cell cell, string part)
		{
			if (part == ImagePart)
			{
				var imgCell = cell as ImageCell;
				int pixelSize = Forms.ConvertToScaledPixel(imgCell.RenderHeight);
				if (pixelSize <= 0)
				{
					pixelSize = this.GetDefaultHeightPixel();
				}

				var image = new Native.Image(Forms.NativeParent)
				{
					MinimumWidth = pixelSize,
					MinimumHeight = pixelSize
				};
				image.SetAlignment(-1.0, -1.0); // fill
				image.SetWeight(1.0, 1.0); // expand

				var task = image.LoadFromImageSourceAsync(imgCell.ImageSource);
				return image;
			}
			else
			{
				return null;
			}
		}

		protected override bool OnCellPropertyChanged(Cell cell, string property, Dictionary<string, EvasObject> realizedView)
		{
			if (property == ImageCell.ImageSourceProperty.PropertyName)
			{
				EvasObject image;
				realizedView.TryGetValue(ImagePart, out image);
				(image as Native.Image)?.LoadFromImageSourceAsync((cell as ImageCell)?.ImageSource);
				return false;
			}
			return base.OnCellPropertyChanged(cell, property, realizedView);
		}
	}
}
