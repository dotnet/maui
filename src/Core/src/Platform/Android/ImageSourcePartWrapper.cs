using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Views;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	public partial class ImageSourcePartLoader : IImageSourcePart
	{
		Action<Drawable?>? SetImage { get; }

		View? NativeView => Handler.NativeView as View;

		public ImageSourcePartLoader(
			IElementHandler handler,
			Func<IImageSource?> getSource,
			Func<bool>? getIsAnimationPlaying,
			Action<bool>? setIsLoading,
			Action<Drawable?> setDrawable) : this(handler, getSource, getIsAnimationPlaying, setIsLoading)
		{
			SetImage = setDrawable;
		}
	}
}
