using ElmSharp;
using System.Collections.Generic;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ImageCellRenderer : TextCellRenderer
	{
		const int _defaultHeight = 55;
		Dictionary<EvasObject, Native.Image> _realizedViews = new Dictionary<EvasObject, Native.Image>();

		public ImageCellRenderer() : this("default")
		{
			ImagePart = "elm.swallow.icon";
		}
		protected ImageCellRenderer(string style) : base(style) { }

		protected string ImagePart { get; set; }

		protected override EvasObject OnGetContent(Cell cell, string part)
		{
			if (part == ImagePart)
			{
				var imgCell = cell as ImageCell;
				int pixelSize = Forms.ConvertToScaledPixel(imgCell.RenderHeight);
				if (pixelSize <= 0)
				{
					pixelSize = Forms.ConvertToPixel(_defaultHeight);
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
