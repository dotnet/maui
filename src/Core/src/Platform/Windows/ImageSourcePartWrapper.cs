using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui
{
	public partial class ImageSourcePartLoader
	{
		Action<ImageSource?>? SetImage { get; }

		FrameworkElement? NativeView => Handler.NativeView as FrameworkElement;

		public ImageSourcePartLoader(
			IElementHandler handler,
			Func<IImageSource?> getSource,
			Func<bool>? getIsAnimationPlaying,
			Action<bool>? setIsLoading,
			Action<ImageSource?> setImage)
			: this(handler, getSource, getIsAnimationPlaying, setIsLoading)
		{
			SetImage = setImage;
		}
	}
}
