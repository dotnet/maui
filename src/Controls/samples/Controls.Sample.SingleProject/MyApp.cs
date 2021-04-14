using Microsoft.Maui;

namespace Maui.Controls.Sample.SingleProject
{
	public class MyApp : IApplication
	{
		public IWindow CreateWindow(IActivationState activationState)
		{
			Microsoft.Maui.Controls.Compatibility.Forms.Init(activationState);

			return new MainWindow();
		}
	}
}