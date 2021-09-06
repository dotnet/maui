using Foundation;
using Microsoft.Maui;

namespace Samples.iOS
{
	[Register(nameof(AppDelegate))]
	public partial class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}