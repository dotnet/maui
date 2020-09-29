using System;
using Android.Content;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class ItemContentView : ViewGroup
	{
		protected IVisualElementRenderer Content;
		internal Element Element => Content?.Element;
		Size? _size;
		Action<Size> _reportMeasure;

		public ItemContentView(Context context) : base(context)
		{
		}

		internal void ClickOn()
		{
			CallOnClick();
		}

		internal void RealizeContent(View view)
		{
			Content = CreateRenderer(view, Context);
			AddView(Content.View);
			Content.Element.MeasureInvalidated += ElementMeasureInvalidated;
		}

		void ElementMeasureInvalidated(object sender, System.EventArgs e)
		{
			if (this.IsAlive())
			{
				RequestLayout();
			}
			else if (sender is VisualElement ve)
			{
				ve.MeasureInvalidated -= ElementMeasureInvalidated;
			}
		}

		internal IVisualElementRenderer VisualElementRenderer => Content;

		internal void Recycle()
		{
			if (Content?.Element != null)
			{
				Content.Element.MeasureInvalidated -= ElementMeasureInvalidated;
			}

			if (Content?.View != null)
			{
				RemoveView(Content.View);
			}

			Content = null;
			_size = null;
		}

		internal void HandleItemSizingStrategy(Action<Size> reportMeasure, Size? size)
		{
			_reportMeasure = reportMeasure;
			_size = size;
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (Content == null)
			{
				return;
			}

			var size = Context.FromPixels(r - l, b - t);

			Content.Element.Layout(new Rectangle(Point.Zero, size));

			Content.UpdateLayout();
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (Content == null)
			{
				SetMeasuredDimension(0, 0);
				return;
			}

			if (_size != null)
			{
				// If we're using ItemSizingStrategy.MeasureFirstItem and now we have a set size, use that
				SetMeasuredDimension((int)_size.Value.Width, (int)_size.Value.Height);
				return;
			}

			int pixelWidth = MeasureSpec.GetSize(widthMeasureSpec);
			int pixelHeight = MeasureSpec.GetSize(heightMeasureSpec);

			var width = MeasureSpec.GetMode(widthMeasureSpec) == MeasureSpecMode.Unspecified
				? double.PositiveInfinity
				: Context.FromPixels(pixelWidth);

			var height = MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpecMode.Unspecified
				? double.PositiveInfinity
				: Context.FromPixels(pixelHeight);

			SizeRequest measure = Content.Element.Measure(width, height, MeasureFlags.IncludeMargins);

			if (pixelWidth == 0)
			{
				pixelWidth = (int)Context.ToPixels(measure.Request.Width);
			}

			if (pixelHeight == 0)
			{
				pixelHeight = (int)Context.ToPixels(measure.Request.Height);
			}

			_reportMeasure?.Invoke(new Size(pixelWidth, pixelHeight));
			_reportMeasure = null; // Make sure we only report back the measure once

			SetMeasuredDimension(pixelWidth, pixelHeight);
		}

		static IVisualElementRenderer CreateRenderer(View view, Context context)
		{
			var renderer = Platform.CreateRenderer(view, context);
			Platform.SetRenderer(view, renderer);

			return renderer;
		}
	}
}