using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.UWP
{
	internal class WindowsTicker : Ticker
	{
		protected override void DisableTimer()
		{
			CompositionTarget.Rendering -= RenderingFrameEventHandler;
		}

		protected override void EnableTimer()
		{
			CompositionTarget.Rendering += RenderingFrameEventHandler;
		}

		void RenderingFrameEventHandler(object sender, object args)
		{
			SendSignals();
		}
	}
}