using Android.Content;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public class MauiShapeView : PlatformGraphicsView
	{
		public MauiShapeView(Context? context) : base(context)
		{
			ClipToOutline = true;
		}
	}
}