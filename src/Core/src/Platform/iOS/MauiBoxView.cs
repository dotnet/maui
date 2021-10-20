using Microsoft.Maui.Graphics.Native;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiBoxView : NativeGraphicsView
	{
		public MauiBoxView()
		{
			BackgroundColor = UIColor.Clear;
		}
	}
}