using System;

namespace Microsoft.Maui
{
	internal class AdornerService : IAdornerService
	{
		Action<Point>? _onTouchDown;
		public void HandlePoint(Action<Point> onTouchDown) => _onTouchDown = onTouchDown;

#if __ANDROID__
		global::Android.App.Activity? _activity;
		global::Android.Views.ViewGroupOverlay? _layer;

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

			_layer = parent.Overlay as global::Android.Views.ViewGroupOverlay;
			return _layer;
		}
#elif __IOS__
		UIKit.UIWindow? _window;
		CoreAnimation.CALayer? _layer;

		public void SetUIWindow(UIKit.UIWindow window)
		{
			_window = window ?? throw new ArgumentNullException(nameof(window));
			_layer = null;

			CreateLayer(_window);
		}

		public CoreAnimation.CALayer? GetAdornerLayer() => _layer;

		void CreateLayer(UIKit.UIWindow window)
		{
			if (window.Layer.Sublayers != null)
			{
				foreach (var sublayer in window.Layer.Sublayers)
				{
					if (sublayer is OverlayLayer)
					{
						_layer = sublayer;
					}
				}
			}

			if (_layer == null)
			{
				_layer = new OverlayLayer
				{
					Frame = window.Frame,
					ZPosition = 10000
				};
				window.Layer.AddSublayer(_layer);
			}
		}

		class OverlayLayer : CoreAnimation.CALayer { }
#endif

		public Rectangle GetRectForView(IView view) => view.Frame;

		//This is Forms pixels
		//Hack because the base ScrollVIew is not a Maui.Core.ILayout
		public IView? GetViewAtPoint(ILayout layout, Point point) => FindIViewAt(layout, (int)point.X, (int)point.Y);

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

		public void ClearAdorners()
		{
#if __ANDROID__
			_layer?.Clear();
#elif __IOS__
			if (_layer?.Sublayers == null)
				return;

			foreach (var subView in _layer.Sublayers)
			{
				subView.RemoveFromSuperLayer();
			}
#endif
		}
	}
}
