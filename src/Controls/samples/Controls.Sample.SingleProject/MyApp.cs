using Microsoft.Maui;

namespace Maui.Controls.Sample.SingleProject
{
	public class MyApp : IApplication
	{
		public IWindow CreateWindow(IActivationState activationState)
		{
			return new MainWindow();
		}
	}
}