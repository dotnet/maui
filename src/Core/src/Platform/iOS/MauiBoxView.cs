using Microsoft.Maui.Graphics.Native;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiBoxView : NativeGraphicsView
	{
		public MauiBoxView()
		{
			BackgroundColor = UIColor.Clear;
		}
	}
}