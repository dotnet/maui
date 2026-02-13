using System;
using Android.Widget;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, ProgressBar>
	{
		protected override ProgressBar CreatePlatformView() => new ProgressBar(Context) { Indeterminate = true };

		public static partial void MapIsRunning(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateIsRunning(activityIndicator);
		}

		public static partial void MapColor(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateColor(activityIndicator);
		}
	}

	// TODO: material3 - make it public in .net 11
	internal class ActivityIndicatorHandler2 : ActivityIndicatorHandler
	{
		protected override MaterialActivityIndicator CreatePlatformView()
		{
			return new MaterialActivityIndicator(Context)
			{
				Indeterminate = true
			};
		}

		public override void PlatformArrange(Rect frame)
		{
			if (Context == null || PlatformView == null)
			{
				return;
			}

			// Get the child's desired size (what it measured at)
			var desiredWidth = VirtualView?.DesiredSize.Width ?? frame.Width;
			var desiredHeight = VirtualView?.DesiredSize.Height ?? frame.Height;

			// Constrain to desired size (don't let parent stretch us)
			var constrainedWidth = Math.Min(frame.Width, desiredWidth);
			var constrainedHeight = Math.Min(frame.Height, desiredHeight);

			// Create new frame with constrained size, centered if necessary
			var arrangeFrame = new Rect(
				frame.X + (frame.Width - constrainedWidth) / 2,
				frame.Y + (frame.Height - constrainedHeight) / 2,
				constrainedWidth,
				constrainedHeight);

			base.PlatformArrange(arrangeFrame);
		}
	}
}