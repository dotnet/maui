using Xamarin.Platform;

namespace Maui.Controls.Sample.Controls
{
	public class Window : IWindow
	{
		public IPage Page { get; set; }
		public IMauiContext MauiContext { get; set; }
	}
}
