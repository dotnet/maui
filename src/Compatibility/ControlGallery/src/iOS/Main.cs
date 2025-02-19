using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.ControlGallery.iOS
{
	public class Application
	{
		static void Main(string[] args)
		{
			UIApplication.Main(args, typeof(CustomApplication), typeof(AppDelegate));
		}
	}
}
