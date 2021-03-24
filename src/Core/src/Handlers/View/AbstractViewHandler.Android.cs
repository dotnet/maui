using Android.Content;
using Android.Views;
using Microsoft.Maui;

namespace Microsoft.Maui.Handlers
{
	public partial class AbstractViewHandler<TVirtualView, TNativeView> : IAndroidViewHandler
	{
		public Context? Context => MauiContext?.Context;

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

			if (Context == null)
				return;

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

		public virtual Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (TypedNativeView == null)
			{
				return Size.Zero;
			}

			if (Context == null)
			{
				return new Size(widthConstraint, heightConstraint);
			}

			var deviceWidthConstraint = Context.ToPixels(widthConstraint);
			var deviceHeightConstraint = Context.ToPixels(heightConstraint);

			var widthSpec = MeasureSpecMode.AtMost.MakeMeasureSpec((int)deviceWidthConstraint);
			var heightSpec = MeasureSpecMode.AtMost.MakeMeasureSpec((int)deviceHeightConstraint);

			TypedNativeView.Measure(widthSpec, heightSpec);

			return Context.FromPixels(TypedNativeView.MeasuredWidth, TypedNativeView.MeasuredHeight);
		}

		void SetupContainer()
		{

		}

		void RemoveContainer()
		{

		}
	}
}