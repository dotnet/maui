using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using Microsoft.Maui.Graphics;
using ARect = Android.Graphics.Rect;
using Rectangle = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Platform
{
	public class LayoutViewGroup : ViewGroup, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable
	{
		readonly ARect _clipRect = new();
		readonly Context _context;
		SafeAreaPadding _safeArea = SafeAreaPadding.Empty;
		bool _safeAreaInvalidated = true;

		public bool InputTransparent { get; set; }

		public LayoutViewGroup(Context context) : base(context)
		{
			_context = context;
			SetupWindowInsetsHandling();
		}

		public LayoutViewGroup(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			var context = Context;
			ArgumentNullException.ThrowIfNull(context);
			_context = context;
			SetupWindowInsetsHandling();
		}

		public LayoutViewGroup(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			_context = context;
			SetupWindowInsetsHandling();
		}

		public LayoutViewGroup(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			_context = context;
			SetupWindowInsetsHandling();
		}

		public LayoutViewGroup(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
		{
			_context = context;
			SetupWindowInsetsHandling();
		}

		void SetupWindowInsetsHandling()
		{
			ViewCompat.SetOnApplyWindowInsetsListener(this, (view, insets) =>
			{
				_safeAreaInvalidated = true;
				RequestLayout();
				return ConsumeInsets(insets);
			});
		}

		WindowInsetsCompat ConsumeInsets(WindowInsetsCompat insets)
		{
			if (!RespondsToSafeArea())
				return insets;

			// Get the types of insets that we handle based on SafeAreaRegions
			var systemBarsToConsume = WindowInsetsCompat.Type.SystemBars();
			var displayCutoutToConsume = WindowInsetsCompat.Type.DisplayCutout();
			var imeToConsume = WindowInsetsCompat.Type.Ime();

			// Check which edges we're handling and should consume insets for
			var leftRegion = GetSafeAreaRegionForEdge(0);
			var topRegion = GetSafeAreaRegionForEdge(1);
			var rightRegion = GetSafeAreaRegionForEdge(2);
			var bottomRegion = GetSafeAreaRegionForEdge(3);

			// Only consume insets for edges that are NOT None (i.e., where we apply safe area)
			var shouldConsumeSystemBars = leftRegion != SafeAreaRegions.None || topRegion != SafeAreaRegions.None ||
										  rightRegion != SafeAreaRegions.None || bottomRegion != SafeAreaRegions.None;

			var shouldConsumeCutout = shouldConsumeSystemBars; // Cutouts typically go with system bars

			// Only consume IME insets if bottom edge has SoftInput region
			var shouldConsumeIme = SafeAreaEdges.IsSoftInput(bottomRegion);

			// Build the inset types to consume
			var typesToConsume = 0;
			if (shouldConsumeSystemBars)
				typesToConsume |= systemBarsToConsume;
			if (shouldConsumeCutout)
				typesToConsume |= displayCutoutToConsume;
			if (shouldConsumeIme)
				typesToConsume |= imeToConsume;

			// If we don't consume any insets, return original insets
			if (typesToConsume == 0)
				return insets;

			// Consume the insets we handle and return the remaining insets
			return new WindowInsetsCompat.Builder(insets)
				.SetInsets(typesToConsume, AndroidX.Core.Graphics.Insets.None)
				.Build();
		}

		public bool ClipsToBounds { get; set; }

		public ICrossPlatformLayout? CrossPlatformLayout
		{
			get; set;
		}

		Graphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Graphics.Size.Zero;
		}

		Graphics.Size CrossPlatformArrange(Graphics.Rect bounds)
		{
			return CrossPlatformLayout?.CrossPlatformArrange(bounds) ?? Graphics.Size.Zero;
		}

		bool RespondsToSafeArea()
		{
			return CrossPlatformLayout is ISafeAreaView2;
		}

		SafeAreaRegions GetSafeAreaRegionForEdge(int edge)
		{
			if (CrossPlatformLayout is ISafeAreaView2 safeAreaPage)
			{
				return safeAreaPage.GetSafeAreaRegionsForEdge(edge);
			}

			// Fallback to legacy ISafeAreaView behavior
			if (CrossPlatformLayout is ISafeAreaView sav)
			{
				return sav.IgnoreSafeArea ? SafeAreaRegions.None : SafeAreaRegions.Container;
			}

			return SafeAreaRegions.None;
		}

		static double GetSafeAreaForEdge(SafeAreaRegions safeAreaRegion, double originalSafeArea, double keyboardInset)
		{
			// Edge-to-edge content - no safe area padding
			if (safeAreaRegion == SafeAreaRegions.None)
				return 0;

			// SoftInput region - always pad for keyboard/soft input
			if (SafeAreaEdges.IsSoftInput(safeAreaRegion))
			{
				return Math.Max(originalSafeArea, keyboardInset);
			}

			// Container region - content flows under keyboard but stays out of bars/notch
			if (SafeAreaEdges.IsContainer(safeAreaRegion))
			{
				// For now, treat Container same as Default (can be enhanced later for keyboard-specific behavior)
				return originalSafeArea;
			}

			// All other regions respect safe area in some form
			// This includes:
			// - Default: Platform default behavior
			// - All: Obey all safe area insets  
			// - Any combination of the above flags
			return originalSafeArea;
		}

		Graphics.Rect AdjustForSafeArea(Graphics.Rect bounds)
		{
			ValidateSafeArea();
			
			if (_safeArea.IsEmpty)
				return bounds;

			return _safeArea.InsetRectF(bounds);
		}

		SafeAreaPadding GetAdjustedSafeAreaInsets()
		{
			// Get WindowInsets if available
			var rootView = RootView;
			if (rootView == null)
				return SafeAreaPadding.Empty;

			var windowInsets = ViewCompat.GetRootWindowInsets(rootView);
			if (windowInsets == null)
				return SafeAreaPadding.Empty;

			var baseSafeArea = windowInsets.ToSafeAreaInsets(_context);
			var keyboardInsets = windowInsets.GetKeyboardInsets(_context);

			// Apply safe area selectively per edge based on SafeAreaRegions
			if (CrossPlatformLayout is ISafeAreaView2)
			{
				var left = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(0), baseSafeArea.Left, keyboardInsets.Left);
				var top = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(1), baseSafeArea.Top, keyboardInsets.Top);
				var right = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(2), baseSafeArea.Right, keyboardInsets.Right);
				var bottom = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(3), baseSafeArea.Bottom, keyboardInsets.Bottom);

				return new SafeAreaPadding(left, right, top, bottom);
			}

			// Legacy ISafeAreaView handling
			if (CrossPlatformLayout is ISafeAreaView sav && sav.IgnoreSafeArea)
			{
				return SafeAreaPadding.Empty;
			}

			return baseSafeArea;
		}

		bool ValidateSafeArea()
		{
			if (!_safeAreaInvalidated)
				return true;

			_safeAreaInvalidated = false;

			var oldSafeArea = _safeArea;
			_safeArea = GetAdjustedSafeAreaInsets();

			return oldSafeArea == _safeArea;
		}

		// TODO: Possibly reconcile this code with ViewHandlerExtensions.MeasureVirtualView
		// If you make changes here please review if those changes should also
		// apply to ViewHandlerExtensions.MeasureVirtualView
		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (CrossPlatformMeasure == null)
			{
				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
				return;
			}

			var deviceIndependentWidth = widthMeasureSpec.ToDouble(_context);
			var deviceIndependentHeight = heightMeasureSpec.ToDouble(_context);

			var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
			var heightMode = MeasureSpec.GetMode(heightMeasureSpec);

			var measure = CrossPlatformMeasure(deviceIndependentWidth, deviceIndependentHeight);

			// If the measure spec was exact, we should return the explicit size value, even if the content
			// measure came out to a different size
			var width = widthMode == MeasureSpecMode.Exactly ? deviceIndependentWidth : measure.Width;
			var height = heightMode == MeasureSpecMode.Exactly ? deviceIndependentHeight : measure.Height;

			var platformWidth = _context.ToPixels(width);
			var platformHeight = _context.ToPixels(height);

			// Minimum values win over everything
			platformWidth = Math.Max(MinimumWidth, platformWidth);
			platformHeight = Math.Max(MinimumHeight, platformHeight);

			SetMeasuredDimension((int)platformWidth, (int)platformHeight);
		}

		// TODO: Possibly reconcile this code with ViewHandlerExtensions.MeasureVirtualView
		// If you make changes here please review if those changes should also
		// apply to ViewHandlerExtensions.MeasureVirtualView
		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (CrossPlatformArrange == null || _context == null)
			{
				return;
			}

			var destination = _context.ToCrossPlatformRectInReferenceFrame(l, t, r, b);

			// Apply safe area adjustments if needed
			if (RespondsToSafeArea())
			{
				destination = AdjustForSafeArea(destination);
			}

			CrossPlatformArrange(destination);

			if (ClipsToBounds)
			{
				_clipRect.Right = r - l;
				_clipRect.Bottom = b - t;
				ClipBounds = _clipRect;
			}
			else
			{
				ClipBounds = null;
			}
		}

		public override bool OnTouchEvent(MotionEvent? e)
		{
			if (InputTransparent)
			{
				return false;
			}

			return base.OnTouchEvent(e);
		}

		IVisualTreeElement? IVisualTreeElementProvidable.GetElement()
		{
			if (CrossPlatformLayout is IVisualTreeElement layoutElement &&
				layoutElement.IsThisMyPlatformView(this))
			{
				return layoutElement;
			}

			return null;
		}
	}
}
