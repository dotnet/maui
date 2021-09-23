using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Samples.iOS
{
	[Register(nameof(AppDelegate))]
	public partial class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}