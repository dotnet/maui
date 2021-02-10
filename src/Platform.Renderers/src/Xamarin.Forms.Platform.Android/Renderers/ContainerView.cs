using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class ContainerView : ViewGroup
	{
		View _view;
		ShellViewRenderer _shellViewRenderer;
		public ContainerView(Context context, View view) : base(context)
		{
			View = view;
		}

		public ContainerView(Context context, IAttributeSet attribs) : base(context, attribs)
		{
		}

		public ContainerView(Context context, IAttributeSet attribs, int defStyleAttr) : base(context, attribs, defStyleAttr)
		{
		}

		protected ContainerView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public bool MatchHeight { get; set; }

		internal bool MeasureHeight { get; set; }

		public bool MatchWidth { get; set; }

		public View View
		{
			get { return _view; }
			set
			{
				if (_view == value)
					return;

				_view = value;
				OnViewSet(value);
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				_shellViewRenderer?.TearDown();
				_view = null;
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (_shellViewRenderer == null)
				return;

			var width = Context.FromPixels(r - l);
			var height = Context.FromPixels(b - t);
			LayoutView(0, 0, width, height);
		}

		protected virtual void LayoutView(double x, double y, double width, double height)
		{
			_shellViewRenderer.LayoutView(x, y, width, height);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (View == null)
			{
				SetMeasuredDimension(0, 0);
				return;
			}
			if (!View.IsVisible)
			{
				View.Measure(0, 0);
				SetMeasuredDimension(0, 0);
				return;
			}

			var width = MeasureSpecFactory.GetSize(widthMeasureSpec);
			var height = MeasureSpecFactory.GetSize(heightMeasureSpec);

			var measureWidth = width > 0 ? Context.FromPixels(width) : double.PositiveInfinity;
			var measureHeight = height > 0 ? Context.FromPixels(height) : double.PositiveInfinity;

			double? maxHeight = null;

			if(MeasureHeight)
			{
				maxHeight = measureHeight;
				measureHeight = double.PositiveInfinity;
			}

			_shellViewRenderer.LayoutView(0, 0, measureWidth, measureHeight, null, maxHeight);

			SetMeasuredDimension((MatchWidth && width != 0) ? width : (int)Context.ToPixels(View.Width),
								 (MatchHeight && height != 0) ? height : (int)Context.ToPixels(View.Height));
		}

		protected virtual void OnViewSet(View view)
		{
			if (_shellViewRenderer == null)
				_shellViewRenderer = new ShellViewRenderer(this.Context, view);
			else
				_shellViewRenderer.OnViewSet(view);

			if (_shellViewRenderer.NativeView != null)
				AddView(_shellViewRenderer.NativeView);
		}
	}
}
