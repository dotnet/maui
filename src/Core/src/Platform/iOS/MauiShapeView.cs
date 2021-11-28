using Microsoft.Maui.Graphics.Native;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiShapeView : NativeGraphicsView
	{
		public MauiShapeView()
		{
			BackgroundColor = UIColor.Clear;
		}
	}
}