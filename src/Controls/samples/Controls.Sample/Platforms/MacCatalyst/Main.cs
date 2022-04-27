using ObjCRuntime;
using UIKit;

namespace Sample.MacCatalyst
{
	public class Application
	{
		static void Main(string[] args) => UIApplication.Main(args, null, typeof(AppDelegate));
	}
}