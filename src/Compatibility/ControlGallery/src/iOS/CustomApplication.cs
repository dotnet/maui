using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.ControlGallery.iOS
{
	public class CustomApplication : UIApplication
	{
		public CustomApplication()
		{
			ApplicationSupportsShakeToEdit = true;
		}

		public override void MotionEnded(UIEventSubtype motion, UIEvent evt)
		{
			if (motion == UIEventSubtype.MotionShake)
			{
				(Delegate as AppDelegate)?.Reset(string.Empty);
			}
			base.MotionEnded(motion, evt);
		}
	}
}
