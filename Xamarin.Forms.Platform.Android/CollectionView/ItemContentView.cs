using Android.Content;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	internal class ItemContentView : ViewGroup
	{
		protected readonly IVisualElementRenderer Content;

		public ItemContentView(IVisualElementRenderer content, Context context) : base(context)
		{
			Content = content;
			AddContent();
		}

		void AddContent()
		{
			AddView(Content.View);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			var size = Context.FromPixels(r - l, b - t);

			Content.Element.Layout(new Rectangle(Point.Zero, size));

			Content.UpdateLayout();
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
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
	}
}