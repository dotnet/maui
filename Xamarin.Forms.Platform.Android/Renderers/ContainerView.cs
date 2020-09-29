using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class ContainerView : ViewGroup
	{
		Context _context;
		IVisualElementRenderer _renderer;
		View _view;

		public ContainerView(Context context, View view) : base(context)
		{
			_context = context;
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

		public bool MatchWidth { get; set; }

		public View View
		{
			get { return _view; }
			set
			{
				_view = value;
				OnViewSet(value);
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				_renderer?.Dispose();
				_renderer = null;
				_view = null;
				_context = null;
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (_renderer == null)
				return;

			var width = _context.FromPixels(r - l);
			var height = _context.FromPixels(b - t);

			LayoutView(0, 0, width, height);
			_renderer.UpdateLayout();
		}

		protected virtual void LayoutView(double x, double y, double width, double height)
		{
			View?.Layout(new Rectangle(x, y, width, height));
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (View == null)
			{
				SetMeasuredDimension(0, 0);
				return;
			}

			// chain on down
			_renderer.View.Measure(widthMeasureSpec, heightMeasureSpec);

			var width = MeasureSpecFactory.GetSize(widthMeasureSpec);
			var height = MeasureSpecFactory.GetSize(heightMeasureSpec);

			var measureWidth = width > 0 ? _context.FromPixels(width) : double.PositiveInfinity;
			var measureHeight = height > 0 ? _context.FromPixels(height) : double.PositiveInfinity;

			var sizeReq = View.Measure(measureWidth, measureHeight);

			SetMeasuredDimension((MatchWidth && width != 0) ? width : (int)_context.ToPixels(sizeReq.Request.Width),
								 (MatchHeight && height != 0) ? height : (int)_context.ToPixels(sizeReq.Request.Height));
		}

		protected virtual void OnViewSet(View view)
		{
			if (_renderer != null)
			{
				_renderer.View.RemoveFromParent();
				_renderer.Dispose();
				_renderer = null;
			}

			if (view != null)
			{
				_renderer = Platform.CreateRenderer(view, Context);
				Platform.SetRenderer(view, _renderer);

				AddView(_renderer.View);
			}
		}
	}
}