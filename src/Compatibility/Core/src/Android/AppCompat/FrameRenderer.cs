using Android.Content;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat
{
	// This version of FrameRenderer is here for backward compatibility with anyone referencing 
	// FrameRenderer from this namespace
	public class FrameRenderer : FastRenderers.FrameRenderer
	{
		public FrameRenderer(Context context) : base(context)
		{

		}
	}
}