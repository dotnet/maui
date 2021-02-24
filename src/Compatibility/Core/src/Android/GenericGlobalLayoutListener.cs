using System;
using Android.Views;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal class GenericGlobalLayoutListener : Object, ViewTreeObserver.IOnGlobalLayoutListener
	{
		Action _callback;

		public GenericGlobalLayoutListener(Action callback)
		{
			_callback = callback;
		}

		public void OnGlobalLayout()
		{
			_callback?.Invoke();
		}

		protected override void Dispose(bool disposing)
		{
			Invalidate();
			base.Dispose(disposing);
		}

		// I don't want our code to dispose of this class I'd rather just let the natural
		// process manage the life cycle so we don't dispose of this too early
		internal void Invalidate()
		{
			_callback = null;
		}
	}
}