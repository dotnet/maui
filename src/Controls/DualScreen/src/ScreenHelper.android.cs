using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.DualScreen
{	public class ScreenHelper
	{
		//HACK: public DisplayMask DisplayMask { get; private set; }
		public Rectangle FoldingFeatureBounds { get; set; } = Rectangle.Zero;
		public Rectangle WindowBounds { get; set; } = Rectangle.Zero;
		public bool IsSpanned { get; set; }
		public Activity Activity { get; set; }
		

		public ScreenHelper() {
			global::Android.Util.Log.Debug("JWM", "ScreenHelper.ctor - no Activity");
		}
		public ScreenHelper(IFoldableContext activity)
		{
			global::Android.Util.Log.Debug("JWM", "ScreenHelper.ctor - WITH Activity");
			Activity = activity as Activity;
		}

		//TODO: FOLDABLE remove this implementation
		[Obsolete("shouldnt care")]
		public static bool IsDualScreenDevice(IFoldableContext context)
		{
			return context.isSeparating;
			//return context.PackageManager.HasSystemFeature("com.microsoft.device.display.displaymask");
		}
		public bool Initialize(IFoldableContext activity)
		{
			//if (!IsDualScreenDevice(activity))
			//	return false;
			WindowBounds = activity.WindowBounds;

			try
			{
				Activity = activity as Activity;
				if (activity.isSeparating)
					FoldingFeatureBounds = activity.FoldingFeatureBounds;
				//HACK:DisplayMask = DisplayMask.FromResourcesRectApproximation(Activity);
				//HACK:if (DisplayMask == null)
				//    return false;
			}
			catch (Java.Lang.NoSuchMethodError ex)
			{
				ex.PrintStackTrace();
				return false;
			}
			catch (Java.Lang.RuntimeException ex)
			{
				ex.PrintStackTrace();
				return false;
			}
			catch (Java.Lang.NoClassDefFoundError ex)
			{
				ex.PrintStackTrace();
				return false;
			}

			return true;
		}

		public void OnConfigurationChanged(Configuration newConfig)
			=> Update();

		public void Update() {
			//HACK:=> DisplayMask = DisplayMask.FromResourcesRectApproximation(Activity);
//			FoldingFeatureBounds = (Activity as IFoldableContext).FoldingFeatureBounds;
//			WindowBounds = (Activity as IFoldableContext).WindowBounds;
		}


		public SurfaceOrientation GetRotation()
			=> GetRotation(Activity);

		Rect GetHinge(SurfaceOrientation rotation)
		{
			// Hinge's coordinates of its 4 edges in different mode
			// Double Landscape Rect(0, 1350 - 1800, 1434)
			// Double Portrait  Rect(1350, 0 - 1434, 1800)
			//HACK:var boundings = DisplayMask.GetBoundingRectsForRotation(rotation);
//			FoldingFeatureBounds = (Activity as IFoldableContext).FoldingFeatureBounds; // TODO get for rotation!!!!???

			//HACK:if (boundings.Count <= 0)
			if (FoldingFeatureBounds == Rectangle.Zero)
				return new Rect();

			return new Rect((int)FoldingFeatureBounds.Left, (int)FoldingFeatureBounds.Top, (int)FoldingFeatureBounds.Right, (int)FoldingFeatureBounds.Bottom);
			//HACK:[0];  used to expect multiple - TODO maybe reinstate this?
		}

		Rect GetHinge()
		{
			// Hinge's coordinates of its 4 edges in different mode
			// Double Landscape Rect(0, 1350 - 1800, 1434)
			// Double Portrait  Rect(1350, 0 - 1434, 1800)
//			FoldingFeatureBounds = (Activity as IFoldableContext).FoldingFeatureBounds; // TODO get for rotation!!!!???

			if (FoldingFeatureBounds == Rectangle.Zero)
				return new Rect();

			return new Rect((int)FoldingFeatureBounds.Left, (int)FoldingFeatureBounds.Top, (int)FoldingFeatureBounds.Right, (int)FoldingFeatureBounds.Bottom);
		}

		Rect GetWindowRect()
		{
			//HACK:FOLDABLE
			var windowRect = new Rect(0, 0, (int)WindowBounds.Width, (int)WindowBounds.Height);

			//HACK:Activity.WindowManager.DefaultDisplay.GetRectSize(windowRect); ;
			return windowRect;
		}

		public bool IsDualMode
		{
			get
			{
				//var rotation = GetRotation();
				var hinge = GetHinge(); //GetHinge(rotation);
				var windowRect = GetWindowRect();

				// Make sure hinge isn't null and window rect
				// Also make sure hinge has width OR height (not just Rect.Zero)
				// Finally make sure the window rect has width AND height

				if (hinge != null && windowRect != null
					&& (hinge.Width() > 0 || hinge.Height() > 0)
					&& windowRect.Width() > 0 && windowRect.Height() > 0)
				{
					// If the hinge intersects the window, dual mode
					return hinge.Intersect(windowRect);
				}

				return false;
			}
		}

		public Rect GetHingeBounds()
			=> GetHinge(GetRotation());

		public Rect GetHingeBoundsDip()
			=> RectPixelsToDip(GetHingeBounds());

		public static SurfaceOrientation GetRotation(Activity activity)
		{
			var wm = activity.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
			var rotation = SurfaceOrientation.Rotation0;
			if (wm != null)
				rotation = wm.DefaultDisplay.Rotation;
			return rotation;
		}

		double PixelsToDip(double px)
			=> px / Activity?.Resources?.DisplayMetrics?.Density ?? 1;

		Rect RectPixelsToDip(Rect rect)
			=> new Rect((int)PixelsToDip(rect.Left), (int)PixelsToDip(rect.Top), (int)PixelsToDip(rect.Right), (int)PixelsToDip(rect.Bottom));
	}
}