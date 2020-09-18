using Android.Content;
using Android.Views;
using Xamarin.Forms;

namespace Xamarin.Platform.Handlers
{
	public partial class AbstractViewHandler<TVirtualView, TNativeView> : IAndroidViewHandler
	{
		public void SetContext(Context context) => Context = context;

		public Context Context { get; private set; }

		public void SetFrame(Rectangle frame)
		{
			var nativeView = View;

			if (nativeView == null)
				return;

			if (frame.Width < 0 || frame.Height < 0)
			{
				// This is just some initial Forms value nonsense, nothing is actually laying out yet
				return;
			}

			var left = Context.ToPixels(frame.Left);
			var top = Context.ToPixels(frame.Top);
			var bottom = Context.ToPixels(frame.Bottom);
			var right = Context.ToPixels(frame.Right);
			var width = Context.ToPixels(frame.Width);
			var height = Context.ToPixels(frame.Height);

			if (nativeView.LayoutParameters == null)
			{
				nativeView.LayoutParameters = new ViewGroup.LayoutParams((int)width, (int)height);
			}
			else
			{
				nativeView.LayoutParameters.Width = (int)width;
				nativeView.LayoutParameters.Height = (int)height;
			}

			nativeView.Layout((int)left, (int)top, (int)right, (int)bottom);
		}

		public virtual SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (TypedNativeView == null)
			{
				return new SizeRequest(Size.Zero);
			}

			var deviceWidthConstraint = Context.ToPixels(widthConstraint);
			var deviceHeightConstraint = Context.ToPixels(heightConstraint);

			var widthSpec = MeasureSpecMode.AtMost.MakeMeasureSpec((int)deviceWidthConstraint);
			var heightSpec = MeasureSpecMode.AtMost.MakeMeasureSpec((int)deviceHeightConstraint);

			TypedNativeView.Measure(widthSpec, heightSpec);

			var deviceIndependentSize = Context.FromPixels(TypedNativeView.MeasuredWidth, TypedNativeView.MeasuredHeight);

			return new SizeRequest(deviceIndependentSize);
		}

		void SetupContainer()
		{

		}

		void RemoveContainer()
		{

		}
	}
}