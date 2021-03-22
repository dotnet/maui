using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS
{
	public class Application
	{
		static void Main(string[] args)
		{
			UIApplication.Main(args, typeof(CustomApplication), typeof(AppDelegate));
		}
	}
}
