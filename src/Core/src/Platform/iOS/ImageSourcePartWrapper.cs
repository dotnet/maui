using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui
{
	internal partial class ImageSourcePartWrapper<T> : IImageSourcePart
			where T : ElementHandler
	{
		Action<UIImage?>? SetImage { get; }

		UIView? NativeView => Handler.NativeView as UIView;

		public ImageSourcePartWrapper(
			T handler,
			Func<T, IImageSource?> getSource,
			Func<T, bool>? getIsAnimationPlaying,
			Action<bool>? setIsLoading,
			Action<UIImage?> setImage)
		{
			GetSource = getSource;
			GetIsAnimationPlaying = getIsAnimationPlaying;
			SetIsLoading = setIsLoading;
			SetImage = setImage;
			Handler = handler;
		}
	}
}
