using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.SingleProject
{
	public class MainWindow : ControlsWindow
	{
		static bool useBlazor = false;
		public MainWindow() : base(useBlazor ? new BlazorPage() : new MainPage())
		{
		}
	}
}