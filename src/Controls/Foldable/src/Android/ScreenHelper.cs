using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Views;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Foldable
{
	class ScreenHelper
	{
		/// <summary>
		/// Whether the app is spanned across a hinge or fold
		/// </summary>
		public bool IsSpanned => _foldableContext.IsSeparating;

		IFoldableContext _foldableContext;

		WeakReference<Activity> _activity;

		public ScreenHelper(IFoldableContext foldableContext, Activity activity)
		{
			_activity = new WeakReference<Activity>(activity);
			_foldableContext = foldableContext;
		}

		/// <summary>
		/// NOTE: we don't use rotation any more, instead checks window height and width
		/// since rotation assumed Surface Duo orientations and didn't always work
		/// for other foldable devices
		/// </summary>
		public SurfaceOrientation GetRotation()
		{
			if (_activity.TryGetTarget(out var activity))
			{
				return GetRotation(activity);
			}

			return SurfaceOrientation.Rotation0;
		}

		/// <summary>
		/// Passed from UseDualScreen HostBuilderExtension
		/// </summary>
		Rect GetHinge() =>
			_foldableContext.FoldingFeatureBounds;

		/// <summary>
		/// Passed from UseDualScreen HostBuilderExtension
		/// </summary>
		Rect GetWindowRect() =>
			_foldableContext.WindowBounds;

		public bool IsDualMode
		{
			get
			{
				var hinge = GetHinge();
				var windowRect = GetWindowRect();

				// Make sure hinge isn't null and window rect
				// Also make sure hinge has width OR height (not just Rect.Zero)
				// Finally make sure the window rect has width AND height

				if (hinge != Rect.Zero && windowRect != Rect.Zero
					&& (hinge.Width > 0 || hinge.Height > 0)
					&& windowRect.Width > 0 && windowRect.Height > 0)
				{
					// If the hinge intersects the window, dual mode is true and fold location is fixed
					return hinge.IntersectsWith(windowRect);
				}

				return false;
			}
		}

		public Rect GetHingeBounds()
			=> GetHinge();

		public Rect GetHingeBoundsDip()
			=> RectPixelsToDip(GetHingeBounds());

		public static SurfaceOrientation GetRotation(Activity activity)
		{
			var wm = activity.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
			var rotation = SurfaceOrientation.Rotation0;
			if (wm != null)
			{
				rotation = wm.DefaultDisplay.Rotation;
			}

			return rotation;
		}

		/// <summary>
		/// Convert pixel values to dp using the density/scaling factor
		/// from UseDualScreen HostBuilderExtension
		/// </summary>
		/// <remarks>
		/// Previously accessed the Activity context directly but we no 
		/// longer pass the activity context to this class
		/// //var density = Activity?.Resources?.DisplayMetrics?.Density ?? 1;
		/// </remarks>
		double PixelsToDip(double px)
		{
			if (_activity.TryGetTarget(out var activity))
			{
				return activity.FromPixels(px);
			}

			return px;
		}

		Rect RectPixelsToDip(Rect rect)
			=> new Rect((int)PixelsToDip(rect.Left), (int)PixelsToDip(rect.Top), (int)PixelsToDip(rect.Width), (int)PixelsToDip(rect.Height));
	}
}