using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Foundation;

namespace Maui.Controls.Sample.Mac
{
	[Register(nameof(AppDelegate))]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}
