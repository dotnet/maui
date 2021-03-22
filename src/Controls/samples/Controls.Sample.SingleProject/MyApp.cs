using Microsoft.Maui;

namespace Maui.Controls.Sample.SingleProject
{
	public class MyApp : MauiApp
	{
		public override IWindow CreateWindow(IActivationState state)
		{
#if ANDROID || IOS
			// This will probably go into a compatibility app or window
			Microsoft.Maui.Controls.Compatibility.Forms.Init(state);
#endif

			return new MainWindow();
		}
	}
}