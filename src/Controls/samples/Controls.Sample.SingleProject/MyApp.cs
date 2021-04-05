using Microsoft.Maui;

namespace Maui.Controls.Sample.SingleProject
{
	public class MyApp : IApplication
	{
		public IWindow CreateWindow(IActivationState activationState)
		{
			System.Diagnostics.Debug.WriteLine("eiloneilon-myapp");
			Microsoft.Maui.Controls.Compatibility.Forms.Init(activationState);

			return new MainWindow();
		}
	}
}