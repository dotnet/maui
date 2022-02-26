using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.DualScreen
{
	public class ScreenHelper
	{
		/// <summary>
		///                         Surface Duo             Surface Duo2
		/// Dual-portrait (wide)    1350, 0 - 1434, 1800    1344, 0 - 1410, 1892
		/// Dual-landscape (tall)   0, 1350 - 1800, 1434    0, 1344 - 1892, 1410
		/// </summary>
		public Rectangle FoldingFeatureBounds { get; set; } = Rectangle.Zero;
		/// <summary>
		///                         Surface Duo          Surface Duo2
		/// Dual-portrait (wide)    0, 0 - 2784, 1800    0, 0 - 2754, 1892
		/// Dual-landscape (tall)   0, 0 - 1800, 2784    0, 0 - 1892, 2754
		/// </summary>
		public Rectangle WindowBounds { get; set; } = Rectangle.Zero;
		/// <summary>
		/// Whether the app is spanned across a hinge or fold
		/// </summary>
		public bool IsSpanned { get; set; }
		/// <summary>
		/// Removed public access for this property, we no longer expose the underlying activity
		/// </summary>
		Activity Activity { get; set; }
		/// <summary>
		/// Need to pass the density up from the UseDualScreen HostBuilderExtension
		/// so we can calculate dp from pixels for layout measuring
		/// </summary>
		float screenDensity = 1f;
		

		public ScreenHelper() {
			global::Android.Util.Log.Debug("JWM", "ScreenHelper.ctor - no IFoldableContext");
		}
		public ScreenHelper(IFoldableContext activity)
		{
			global::Android.Util.Log.Debug("JWM", "ScreenHelper.ctor - WITH IFoldableContext");
			Activity = activity as Activity;
			screenDensity = activity.ScreenDensity;
		}

		public bool Initialize(IFoldableContext activity)
		{
			Activity = activity as Activity;
			screenDensity = activity.ScreenDensity;
			return true;
		}

		public void OnConfigurationChanged(Configuration newConfig)
			=> Update();

		public void Update() 
		{
			FoldingFeatureBounds = (Activity as IFoldableContext).FoldingFeatureBounds;
			WindowBounds = (Activity as IFoldableContext).WindowBounds;
			screenDensity = (Activity as IFoldableContext).ScreenDensity;
		}

		/// <summary>
		/// NOTE: we don't use rotation any more, instead checks window height and width
		/// since rotation assumed Surface Duo orientations and didn't always work
		/// for other foldable devices
		/// </summary>
		public SurfaceOrientation GetRotation()
			=> GetRotation(Activity);

		[Obsolete("No longer used with rotation parameter")]
		Rect GetHinge(SurfaceOrientation rotation)
		{
			// Hinge's coordinates of its 4 edges in different mode (Surface Duo 1 & 2 sizes)
			// Double Portrait  Rect(1350, 0 - 1434, 1800)     Rect(1344, 0 - 1410, 1892)
			// Double Landscape Rect(0, 1350 - 1800, 1434)     Rect(0, 1344 - 1892, 1410)

			if (FoldingFeatureBounds == Rectangle.Zero)
				return new Rect();

			return new Rect((int)FoldingFeatureBounds.Left, (int)FoldingFeatureBounds.Top, (int)FoldingFeatureBounds.Right, (int)FoldingFeatureBounds.Bottom);
		}

		/// <summary>
		/// Passed from UseDualScreen HostBuilderExtension
		/// </summary>
		Rect GetHinge()
		{
			// Hinge's coordinates of its 4 edges in different mode on Surface Duo 1
			// Double Portrait  Rect(1350, 0 - 1434, 1800)     Rect(1344, 0 - 1410, 1892)
			// Double Landscape Rect(0, 1350 - 1800, 1434)     Rect(0, 1344 - 1892, 1410)

			if (FoldingFeatureBounds == Rectangle.Zero)
				return new Rect();

			return new Rect((int)FoldingFeatureBounds.Left, (int)FoldingFeatureBounds.Top, (int)FoldingFeatureBounds.Right, (int)FoldingFeatureBounds.Bottom);
		}

		/// <summary>
		/// Passed from UseDualScreen HostBuilderExtension
		/// </summary>
		Rect GetWindowRect()
		{
			var windowRect = new Rect(0, 0, (int)WindowBounds.Width, (int)WindowBounds.Height);
			return windowRect;
		}

		public bool IsDualMode
		{
			get
			{
				var hinge = GetHinge();
				var windowRect = GetWindowRect();

				// Make sure hinge isn't null and window rect
				// Also make sure hinge has width OR height (not just Rect.Zero)
				// Finally make sure the window rect has width AND height

				if (hinge != null && windowRect != null
					&& (hinge.Width() > 0 || hinge.Height() > 0)
					&& windowRect.Width() > 0 && windowRect.Height() > 0)
				{
					// If the hinge intersects the window, dual mode is true and fold location is fixed
					return hinge.Intersect(windowRect);
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
				rotation = wm.DefaultDisplay.Rotation;
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
			return px / screenDensity;
		}

		Rect RectPixelsToDip(Rect rect)
			=> new Rect((int)PixelsToDip(rect.Left), (int)PixelsToDip(rect.Top), (int)PixelsToDip(rect.Right), (int)PixelsToDip(rect.Bottom));
	}
}