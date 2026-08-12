using AView = Android.Views.View;

namespace Maui.Controls.Sample.Platform.Android;

static class SandboxFirstFrameProbe
{
	static FirstDrawListener? s_listener;

	public static void Schedule(Action callback)
	{
		if (s_listener is not null)
			return;

		var decorView = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window?.DecorView;
		var viewTreeObserver = decorView?.ViewTreeObserver;
		if (decorView is null || viewTreeObserver is null || !viewTreeObserver.IsAlive)
			return;

		s_listener = new FirstDrawListener(decorView, callback);
		viewTreeObserver.AddOnDrawListener(s_listener);
	}

	sealed class FirstDrawListener : Java.Lang.Object, global::Android.Views.ViewTreeObserver.IOnDrawListener
	{
		readonly AView _view;
		readonly Action _callback;
		int _reported;

		public FirstDrawListener(AView view, Action callback)
		{
			_view = view;
			_callback = callback;
		}

		public void OnDraw()
		{
			if (Interlocked.Exchange(ref _reported, 1) != 0)
				return;

			_view.Post(() =>
			{
				var observer = _view.ViewTreeObserver;
				if (observer?.IsAlive == true)
					observer.RemoveOnDrawListener(this);

				s_listener = null;
				_callback();
				Dispose();
			});
		}
	}
}
