using Android.Content;
using Android.Views;
using Microsoft.Maui;

namespace Microsoft.Maui.Handlers
{
	public partial class AbstractViewHandler<TVirtualView, TNativeView> : INativeViewHandler
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
			if (TypedNativeView == null || VirtualView == null || Context == null)
			{
				return Size.Zero;
			}

			// Create a spec to handle the native measure
			var widthSpec = CreateMeasureSpec(widthConstraint, VirtualView.Width);
			var heightSpec = CreateMeasureSpec(heightConstraint, VirtualView.Height);

			TypedNativeView.Measure(widthSpec, heightSpec);

			// Convert back to xplat sizes for the return value
			return Context.FromPixels(TypedNativeView.MeasuredWidth, TypedNativeView.MeasuredHeight);
		}

		int CreateMeasureSpec(double constraint, double explicitSize)
		{
			var mode = MeasureSpecMode.AtMost;

			if (explicitSize >= 0)
			{
				// We have a set value (i.e., a Width or Height)
				mode = MeasureSpecMode.Exactly;
				constraint = explicitSize;
			}
			else if (double.IsInfinity(constraint))
			{
				// We've got infinite space; we'll leave the size up to the native control
				mode = MeasureSpecMode.Unspecified;
				constraint = 0;
			}

			// Convert to a native size to create the spec for measuring
			var deviceConstraint = (int)Context!.ToPixels(constraint);

			return mode.MakeMeasureSpec(deviceConstraint);
		}

		void SetupContainer()
		{

		}

		void RemoveContainer()
		{

		}
	}
}