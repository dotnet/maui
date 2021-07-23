using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellContainerView : ViewGroup
	{
		View _view;
		ShellContentView _shellContentView;
		readonly IMauiContext _mauiContext;
		AView NativeView => _view?.Handler?.NativeView as AView;

		public ShellContainerView(Context context, View view, IMauiContext mauiContext) : base(context)
		{
			_mauiContext = mauiContext ?? throw new ArgumentNullException(nameof(mauiContext));
			View = view;
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
				_shellContentView?.TearDown();
				_view = null;
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (_shellContentView == null)
				return;

			_shellContentView.LayoutView(l, t, r, b);
			//var width = Context.FromPixels(r - l);
			//var height = Context.FromPixels(b - t);
			//LayoutView(0, 0, width, height);
		}

		//protected virtual void LayoutView(double x, double y, double width, double height)
		//{
		//	_shellContentView.LayoutView(x, y, width, height);
		//}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			//_shellContentView.Measure(widthMeasureSpec, heightMeasureSpec);

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

			//var width = GetSize(widthMeasureSpec);
			//var height = GetSize(heightMeasureSpec);

			//var measureWidth = width > 0 ? Context.FromPixels(width) : double.PositiveInfinity;
			//var measureHeight = height > 0 ? Context.FromPixels(height) : double.PositiveInfinity;

			//double? maxHeight = null;

			//if (MeasureHeight)
			//{
			//	maxHeight = measureHeight;
			//	measureHeight = double.PositiveInfinity;
			//}

			//_shellContentView.LayoutView(0, 0, measureWidth, measureHeight, null, maxHeight);

			//SetMeasuredDimension((MatchWidth && width != 0) ? width : (int)Context.ToPixels(View.Width),
			//					 (MatchHeight && height != 0) ? height : (int)Context.ToPixels(View.Height));

			var width = GetSize(widthMeasureSpec);
			var height = GetSize(heightMeasureSpec);

			var measureWidth = width > 0 ? width : MeasureSpecMode.Unspecified.MakeMeasureSpec(0);
			var measureHeight = height > 0 ? height : MeasureSpecMode.Unspecified.MakeMeasureSpec(0);

			double? maxHeight = null;

			if (MeasureHeight)
			{
				//maxHeight = measureHeight;
				measureHeight = MeasureSpecMode.Unspecified.MakeMeasureSpec(0);
			}
			else if(MatchWidth)
			{
				measureWidth = widthMeasureSpec;
			}
			else if(MatchHeight)
			{
				measureHeight = heightMeasureSpec;
				maxHeight = heightMeasureSpec.GetSize();
			}

			_shellContentView.Measure(measureWidth, measureHeight, null, (int?)maxHeight);
			//NativeView.Measure(measureWidth, measureHeight);
			SetMeasuredDimension(NativeView.MeasuredWidth, NativeView.MeasuredHeight);

			//_shellContentView.LayoutView(0, 0, measureWidth, measureHeight, null, maxHeight);

			//SetMeasuredDimension((MatchWidth && width != 0) ? width : (int)Context.ToPixels(View.Width),
			//					 (MatchHeight && height != 0) ? height : (int)Context.ToPixels(View.Height));

		}

		// TODO MAUI
		int GetSize(int measureSpec)
		{
			const int modeMask = 0x3 << 30;
			return measureSpec & ~modeMask;
		}

		int MakeMeasureSpec(MeasureSpecMode mode, int size)
		{
			return size + (int)mode;
		}

		protected virtual void OnViewSet(View view)
		{
			if (_shellContentView == null)
				_shellContentView = new ShellContentView(this.Context, view, _mauiContext);
			else
				_shellContentView.OnViewSet(view);

			if (_shellContentView.NativeView != null)
				AddView(_shellContentView.NativeView);
		}
	}
}
