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
			Content.Element.MeasureInvalidated += ElementMeasureInvalidated;
		}

		void ElementMeasureInvalidated(object sender, System.EventArgs e)
		{
			RequestLayout();
		}

		internal void Recycle()
		{
			if (Content?.Element != null)
			{
				Content.Element.MeasureInvalidated -= ElementMeasureInvalidated;
			}
			
			if(Content?.View != null)
			{
				RemoveView(Content.View);
			}
			
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

			// When we implement ItemSizingStrategy.MeasureFirstItem for Android, these next two clauses will need to
			// be updated to use the static width/height

			if (pixelWidth == 0)
			{
				pixelWidth = (int)Context.ToPixels(measure.Request.Width);
			}

			if (pixelHeight == 0)
			{
				pixelHeight = (int)Context.ToPixels(measure.Request.Height);
			}

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
