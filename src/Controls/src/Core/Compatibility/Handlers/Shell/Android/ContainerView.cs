using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ContainerView : ViewGroup
	{
		View _view;
		ShellViewRenderer _shellContentView;
		readonly IMauiContext _mauiContext;
		AView PlatformView => _view?.Handler?.PlatformView as AView;

		public ContainerView(Context context, View view, IMauiContext mauiContext) : base(context)
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

			var width = GetSize(widthMeasureSpec);
			var height = GetSize(heightMeasureSpec);

			var measureWidth = width > 0 ? width : MeasureSpecMode.Unspecified.MakeMeasureSpec(0);
			var measureHeight = height > 0 ? height : MeasureSpecMode.Unspecified.MakeMeasureSpec(0);

			double? maxHeight = null;

			if (MeasureHeight)
			{
				measureHeight = MeasureSpecMode.Unspecified.MakeMeasureSpec(0);
			}
			else if (MatchWidth)
			{
				measureWidth = widthMeasureSpec;
			}
			else if (MatchHeight)
			{
				measureHeight = heightMeasureSpec;
				maxHeight = heightMeasureSpec.GetSize();
			}

			_shellContentView.Measure(measureWidth, measureHeight, null, (int?)maxHeight);
			SetMeasuredDimension(PlatformView.MeasuredWidth, PlatformView.MeasuredHeight);

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
				_shellContentView = new ShellViewRenderer(this.Context, view, _mauiContext);
			else
				_shellContentView.OnViewSet(view);

			if (_shellContentView.PlatformView != null)
				AddView(_shellContentView.PlatformView);
		}
	}
}
