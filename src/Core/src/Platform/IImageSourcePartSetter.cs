#if __IOS__ || MACCATALYST
using PlatformImage = UIKit.UIImage;
#elif MONOANDROID
using PlatformImage = Android.Graphics.Drawables.Drawable;
#elif WINDOWS
using PlatformImage = Microsoft.UI.Xaml.Media.ImageSource;
#elif TIZEN
using PlatformImage = Microsoft.Maui.Platform.MauiImageSource;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using System;
using PlatformImage = System.Object;
#endif

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// This represents a object that knows what the desired image is and how to apply a loaded version
	/// of the image to a platform view.
	/// </summary>
	/// <remarks>
	/// If a handler has multiple image parts, then multiple <see cref="IImageSourcePartSetter"/>
	/// instances can be used for each image part.
	/// 
	/// The handler should not implement this interface itself as is breaks re-use of
	/// mappers and/or handlers.
	/// </remarks>
	public interface IImageSourcePartSetter
	{
		/// <summary>
		/// Gets the <see cref="IElementHandler"/> which is doing the image loading.
		/// </summary>
		IElementHandler? Handler { get; }

		/// <summary>
		/// Gets the <see cref="IImageSourcePart"/> that is being loaded.
		/// </summary>
		IImageSourcePart? ImageSourcePart { get; }

		/// <summary>
		/// The platform-specific implementation that knows how to set the loaded image into a platform view.
		/// </summary>
		/// <param name="platformImage">The platform image to set.</param>
		void SetImageSource(PlatformImage? platformImage);
	}
}
