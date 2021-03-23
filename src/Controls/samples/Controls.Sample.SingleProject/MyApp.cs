using Microsoft.Maui;

namespace Maui.Controls.Sample.SingleProject
{
	public class MyApp : IApplication
	{
		public IWindow CreateWindow(IActivationState activationState)
		{
#if ANDROID || IOS
			// This will probably go into a compatibility app or window
			Microsoft.Maui.Controls.Compatibility.Forms.Init(activationState);
#endif

			return new MainWindow();
		}
	}
}