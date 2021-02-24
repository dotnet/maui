using ElmSharp;
using SkiaSharp.Views.Tizen;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.SkiaSharp
{
	public interface ICanvasRenderer
	{
		public EvasObject RealNativeView { get; }
	}
}