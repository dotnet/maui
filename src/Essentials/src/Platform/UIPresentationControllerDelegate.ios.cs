using System;
using UIKit;

namespace Microsoft.Maui.ApplicationModel
{
	class UIPresentationControllerDelegate : UIAdaptivePresentationControllerDelegate
	{
		Action dismissHandler;

		internal UIPresentationControllerDelegate(Action dismissHandler)
			=> this.dismissHandler = dismissHandler;

		public override void DidDismiss(UIPresentationController presentationController)
		{
			dismissHandler?.Invoke();
			dismissHandler = null;
		}

		protected override void Dispose(bool disposing)
		{
			dismissHandler?.Invoke();
			base.Dispose(disposing);
		}
	}
}
