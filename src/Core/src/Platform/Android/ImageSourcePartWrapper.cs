using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;
using System.Threading.Tasks;
using Android.Views;
using Android.Graphics.Drawables;

namespace Microsoft.Maui
{
	internal partial class ImageSourcePartWrapper<T> : IImageSourcePart
			where T : ElementHandler
	{
		Action<Drawable?>? SetImage { get; }

		View? NativeView => Handler.NativeView as View;

		public ImageSourcePartWrapper(
			T handler,
			Func<T, IImageSource?> getSource,
			Func<T, bool>? getIsAnimationPlaying,
			Action<bool>? setIsLoading,
			Action<Drawable?> setDrawable)
		{
			GetSource = getSource;
			GetIsAnimationPlaying = getIsAnimationPlaying;
			SetIsLoading = setIsLoading;
			SetImage = setDrawable;
			Handler = handler;
		}
	}
}
