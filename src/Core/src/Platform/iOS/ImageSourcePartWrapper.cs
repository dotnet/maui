using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui
{
	public partial class ImageSourcePartLoader
	{
		Action<UIImage?>? SetImage { get; }

		UIView? NativeView => Handler.NativeView as UIView;

		public ImageSourcePartLoader(
			IElementHandler handler,
			Func<IImageSourcePart?> imageSourcePart,
			Action<UIImage?> setImage)
			: this(handler, imageSourcePart)
		{
			SetImage = setImage;
		}
	}
}
