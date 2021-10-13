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

namespace Microsoft.Maui.Controls.DualScreen
{	public class ScreenHelper
	{
		//HACK: public DisplayMask DisplayMask { get; private set; }

		public Activity Activity { get; private set; }

		public static bool IsDualScreenDevice(Context context)
			=> context.PackageManager.HasSystemFeature("com.microsoft.device.display.displaymask");

		public bool Initialize(Activity activity)
		{
			if (!IsDualScreenDevice(activity))
				return false;

			try
			{
				Activity = activity;
				//HACK:DisplayMask = DisplayMask.FromResourcesRectApproximation(Activity);
				//HACK:if (DisplayMask == null)
				return false;
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

			//HACK:return true;
		}

		public void OnConfigurationChanged(Configuration newConfig)
			=> Update();

		public void Update() { }
			//HACK:=> DisplayMask = DisplayMask.FromResourcesRectApproximation(Activity);

		public SurfaceOrientation GetRotation()
			=> GetRotation(Activity);

		Rect GetHinge(SurfaceOrientation rotation)
		{
			// Hinge's coordinates of its 4 edges in different mode
			// Double Landscape Rect(0, 1350 - 1800, 1434)
			// Double Portrait  Rect(1350, 0 - 1434, 1800)
			//HACK:var boundings = DisplayMask.GetBoundingRectsForRotation(rotation);

			//HACK:if (boundings.Count <= 0)
			return new Rect();

			//HACK:return boundings[0];
		}

		Rect GetWindowRect()
		{
			var windowRect = new Rect();
			//HACK:Activity.WindowManager.DefaultDisplay.GetRectSize(windowRect); ;
			return windowRect;
		}

		public bool IsDualMode
		{
			get
			{
				var rotation = GetRotation();
				var hinge = GetHinge(rotation);
				var windowRect = GetWindowRect();

				// Make sure hinge isn't null and window rect
				// Also make sure hinge has width OR height (not just Rect.Zero)
				// Finally make sure the window rect has width AND height

				//HACK:
				//if (hinge != null && windowRect != null
				//	&& (hinge.Width() > 0 || hinge.Height() > 0)
				//	&& windowRect.Width() > 0 && windowRect.Height() > 0)
				//{
				//	// If the hinge intersects the window, dual mode
				//	return hinge.Intersect(windowRect);
				//}

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