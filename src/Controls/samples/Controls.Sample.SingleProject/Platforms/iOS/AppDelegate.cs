using Foundation;
using Microsoft.Maui;

namespace Maui.Controls.Sample.SingleProject
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiAppBuilder CreateAppBuilder() => MauiProgram.CreateAppBuilder();
	}
}