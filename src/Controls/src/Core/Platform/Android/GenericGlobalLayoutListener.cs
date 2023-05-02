using System;
using Android.Views;
using AView = Android.Views.View;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Controls.Platform
{
	internal class GenericGlobalLayoutListener : Object, ViewTreeObserver.IOnGlobalLayoutListener
	{
		Action<GenericGlobalLayoutListener, AView?>? _callback;
		WeakReference<AView>? _targetView;

		public GenericGlobalLayoutListener(Action<GenericGlobalLayoutListener, AView?> callback, AView? targetView = null)
		{
			_callback = callback;

			if (targetView?.ViewTreeObserver != null)
			{
				_targetView = new WeakReference<AView>(targetView);
				targetView.ViewTreeObserver.AddOnGlobalLayoutListener(this);
			}
		}

		public void OnGlobalLayout()
		{
			AView? targetView = null;
			_targetView?.TryGetTarget(out targetView);
			_callback?.Invoke(this, targetView);
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

			if (_targetView != null &&
				_targetView.TryGetTarget(out var targetView) &&
				targetView.IsAlive() &&
				targetView.ViewTreeObserver != null)
			{
				targetView.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
			}

			_targetView = null;
		}
	}
}