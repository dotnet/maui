using Microsoft.Maui.Graphics.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiBoxView : PlatformGraphicsView
	{
		public MauiBoxView()
		{
			BackgroundColor = UIColor.Clear;
		}
	}
}