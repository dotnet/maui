using ElmSharp;
using SkiaSharp.Views.Tizen;

namespace Xamarin.Forms.Platform.Tizen.SkiaSharp
{
	public interface ICanvasRenderer
	{
		public EvasObject RealNativeView { get; }
	}
}