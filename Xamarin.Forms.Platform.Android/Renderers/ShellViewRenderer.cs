using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Google.Android.Material.AppBar;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Xamarin.Forms.Platform.Android
{
	// This is used to monitor an xplat View and apply layout changes
	internal class ShellViewRenderer
	{
		IVisualElementRenderer _renderer;
		View _view;
		WeakReference<Context> _context;
		private double _width;
		private double _height;

		public ShellViewRenderer(Context context, View view)
		{
			_context = new WeakReference<Context>(context);
			View = view;
		}

		public View View
		{
			get { return _view; }
			set
			{
				OnViewSet(value);
			}
		}

		public void TearDown()
		{
			View = null;
			_renderer?.Dispose();
			_renderer = null;
			_view = null;
			_context = null;
		}

		public void LayoutView(double width, double height)
		{
			_width = width;
			_height = height;
			Context context;

			if (_renderer == null || !(_context.TryGetTarget(out context)) || !_renderer.View.IsAlive())
				return;

			if (View == null)
			{
				var empty = MeasureSpecFactory.GetSize(0);
				_renderer.View.Measure(empty, empty);
				return;
			}

			var request = View.Measure(width, height, MeasureFlags.None);

			var layoutParams = NativeView.LayoutParameters;
			if (height == -1)
				height = request.Request.Height;

			if (width == -1)
				width = request.Request.Width;

			if (layoutParams.Width != LP.MatchParent)
				layoutParams.Width = (int)context.ToPixels(width);

			if (layoutParams.Height != LP.MatchParent)
				layoutParams.Height = (int)context.ToPixels(height);

			NativeView.LayoutParameters = layoutParams;
			View.Layout(new Rectangle(0, 0, width, height));
			_renderer.UpdateLayout();
		}

		public void OnViewSet(View view)
		{
			if (View != null)
				View.SizeChanged -= OnViewSizeChanged;

			if (View is VisualElement oldView)
				oldView.MeasureInvalidated -= OnViewSizeChanged;

			if (_renderer != null)
			{
				_renderer.View.RemoveFromParent();
				_renderer.Dispose();
				_renderer = null;
			}

			_view = view;
			if (view != null)
			{
				Context context;

				if (!(_context.TryGetTarget(out context)))
					return;

				_renderer = Platform.CreateRenderer(view, context);
				Platform.SetRenderer(view, _renderer);
				NativeView = _renderer.View;

				if (View is VisualElement ve)
					ve.MeasureInvalidated += OnViewSizeChanged;
				else
					View.SizeChanged += OnViewSizeChanged;
			}
		}

		void OnViewSizeChanged(object sender, EventArgs e) =>
			LayoutView(_width, _height);

		public AView NativeView
		{
			get;
			private set;
		}
	}
}