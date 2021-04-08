using System;

namespace Microsoft.Maui
{
	internal class AdornerService : IAdornerService
	{
		Action<Point>? _onTouchDown;
		public void HandlePoint(Action<Point> onTouchDown)
		{
			_onTouchDown = onTouchDown;
		}

#if __ANDROID__
		global::Android.App.Activity? _activity;
		public void SetActivity(global::Android.App.Activity activity)
		{
			_activity = activity;
		}

		public global::Android.Views.ViewOverlay? GetAdornerLayer()
		{
			//Maybe we should add a layer and return that layer
			var parent = _activity?.FindViewById(global::Android.Resource.Id.Content) as global::Android.Views.ViewGroup;

			if (parent == null)
				throw new InvalidOperationException("Couldn0t find root view");

			var overlay = parent.Overlay;
			return overlay;
		}
#endif
		public Rectangle GetRectForView(IView view) => view.Frame;

		//This is Forms pixels
		//Hack because the base ScrollVIew is not a Maui.Core.ILayout
		//This should take a iWindow
		public IView? GetViewAtPoint(ILayout layout, Point point) => 
			FindIViewAt(layout, (int) point.X, (int) point.Y);

		public IView? GetViewAtPoint(IWindow window, Point point)
		{
			if (window.Page.View is not ILayout layout)
				return null;
			return GetViewAtPoint(layout, point);
		}

		internal void ExecuteTouchEventDelegate(Point point)
		{
			_onTouchDown?.Invoke(point);
		}

		IView? FindIViewAt(IView? view, int x, int y)
		{
			if (view == null)
				return null;

			if (view is ILayout layout)
			{
				for (int i = 0; i < layout.Children.Count; i++)
				{
					IView? foundView = FindIViewAt(layout.Children[i], x, y);

					if (foundView != null)
					{
						return foundView;	
					}
				}
			}

			if (view.Frame.Contains(x, y))
				return view;
		
			return null;
		}


	}
}
