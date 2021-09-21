using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Views;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	public partial class ImageSourcePartLoader
	{
		Action<Drawable?>? SetImage { get; }

		View? NativeView => Handler.NativeView as View;

		public ImageSourcePartLoader(
			IElementHandler handler,
			Func<IImageSourcePart?> imageSourcePart,
			Action<Drawable?> setDrawable) : this(handler, imageSourcePart)
		{
			SetImage = setDrawable;
		}
	}
}
