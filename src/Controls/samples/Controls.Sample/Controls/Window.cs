using Microsoft.Maui;

namespace MauiControlsSample.Controls
{
	public class Window : IWindow
	{
		public IPage Page { get; set; }

		public IMauiContext MauiContext { get; set; }
	}
}
