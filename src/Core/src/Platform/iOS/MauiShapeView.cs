using Microsoft.Maui.Graphics.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiShapeView : PlatformGraphicsView
	{
		public MauiShapeView()
		{
			BackgroundColor = UIColor.Clear;
		}
	}
}