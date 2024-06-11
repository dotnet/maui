using ObjCRuntime;
using UIKit;

namespace Maui.Controls.Sample.Platform
{
	public class Application
	{
		static void Main(string[] args) => UIApplication.Main(args, null, typeof(AppDelegate));
	}
}