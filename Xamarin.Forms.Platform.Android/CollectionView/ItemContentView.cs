using System;
using Android.Content;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	internal class ItemContentView : ViewGroup
	{
		protected IVisualElementRenderer Content;

		public ItemContentView(Context context) : base(context)
		{
		}

		internal void RealizeContent(View view)
		{
			Content = CreateRenderer(view, Context);
			AddView(Content.View);
		}

		internal void Recycle()
		{
			RemoveView(Content.View);
			Content = null;
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

			int pixelWidth = MeasureSpec.GetSize(widthMeasureSpec);
			int pixelHeight = MeasureSpec.GetSize(heightMeasureSpec);

			var width = Context.FromPixels(pixelWidth);
			var height = Context.FromPixels(pixelHeight);

			SizeRequest measure = Content.Element.Measure(width, height, MeasureFlags.IncludeMargins);

			if (pixelWidth == 0)
			{
				pixelWidth = (int)Context.ToPixels(Content.Element.Width > 0
					? Content.Element.Width
					: measure.Request.Width);
			}

			if (pixelHeight == 0)
			{
				pixelHeight = (int)Context.ToPixels(Content.Element.Height > 0
					? Content.Element.Height
					: measure.Request.Height);
			}

			SetMeasuredDimension(pixelWidth, pixelHeight);
		}

		static IVisualElementRenderer CreateRenderer(View view, Context context)
		{
			if (view == null)
			{
				throw new ArgumentNullException(nameof(view));
			}

			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			var renderer = Platform.CreateRenderer(view, context);
			Platform.SetRenderer(view, renderer);

			return renderer;
		}
	}
}