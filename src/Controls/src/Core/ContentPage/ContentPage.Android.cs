using Android.Views;
using Android.Widget;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;
using System;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	public partial class ContentPage
	{
		internal event EventHandler<MotionEvent?>? DispatchTouchEvent;

		void OnDispatchTouch(object? sender, MotionEvent? e)
		{
			DispatchTouchEvent?.Invoke(this, e);
		}

		internal IDisposable? SetupTapIntoNothingness(AView? aView)
		{
			if (aView is null)
				return null;

			var tracker = new TapPageTracker(aView, this);
			return new ActionDisposable(() =>
			{
				tracker.Disconnect();
				tracker = null;
			});
		}

		private protected override void AddedToPlatformVisualTree()
		{
			base.AddedToPlatformVisualTree();
			Window.DispatchTouchEvent += OnDispatchTouch;
		}

		private protected override void RemovedFromPlatformVisualTree(IWindow? oldWindow)
		{
			base.RemovedFromPlatformVisualTree(oldWindow);
			if (oldWindow is Window window)
				window.DispatchTouchEvent -= OnDispatchTouch;
		}
	}
}
