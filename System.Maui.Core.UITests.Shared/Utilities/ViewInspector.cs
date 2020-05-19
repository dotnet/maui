using Xamarin.UITest;
using System.Linq;

namespace Xamarin.Forms.Core.UITests
{
	internal static class ViewInspector
	{
	    public static void LogPropertiesForView (this IApp app, string query, bool isOnParent = false)
	    {
#if __ANDROID__
			LogPropertiesForAndroidView (app, query, isOnParent);
#endif
#if __IOS__

			LogPropertiesForUIView(app, query, isOnParent);
			LogPropertiesForCALayer (app, query, isOnParent);
#endif
		}

		static void LogPropertiesForUIView (this IApp app, string query, bool isOnParent = false) {

            //Logger.LogLine ("--- UIView Properties ---");

			var properties = new [] {
				// just getters with no params, bools
				"alpha",
				"autoresizesSubviews",
				"autoresizingMask",
				"backgroundColor",
				"bounds",
				"center",
				"clearsContextBeforeDrawing",
				"clipsToBounds",
				"contentMode",
				"contentScaleFactor",
				"exclusiveTouch",
				"frame",
				"gestureRecognizers",
				"hidden",
				"layer",
				"motionEffects",
				"multipleTouchEnabled",
				"opaque",
				"restorationIdentifier",
				"subviews",
				"superview",
				"tag",
				"tintAdjustmentMode",
				"tintColor",
				"transform",
				"userInteractionEnabled",
				"window"
			};

			if (isOnParent)
				query = query + " parent * index:0";

			foreach (var property in properties) {
				object prop;
				bool found = 
					MaybeGetProperty<string> (app, query, property, out prop) || 
					MaybeGetProperty<int>    (app, query, property, out prop) || 
					MaybeGetProperty<float>  (app, query, property, out prop) ||
					MaybeGetProperty<bool>   (app, query, property, out prop);

                if (found)
                    continue;
				//	Logger.LogLine (string.Format ("{0,-30}: {1}", property, prop));
			}

            //Logger.LogLine();

		}

		static void LogPropertiesForCALayer(this IApp app, string query, bool isOnParent = false)
        {
           // Logger.LogLine ("--- UIView.Layer Properties ---");

            var properties = new[] {
				"actions", 
				"anchorPoint",
				"anchorPointZ",
				"backgroundColor",
				"backgroundFilters",
				"borderColor",
				"borderWidth",
				"bounds",
				"compositingFilter",
				"contents",
				"contentsCenter",
				"contentsGravity",
				"contentsRect",
				"contentsScale",
				"cornerRadius",
				"delegate",
				"doubleSided",
				"drawsAsynchronously",
				"edgeAntialiasingMask",
				"filters",
				"frame",
				"geometryFlipped",
				"hidden",
				"magnificationFilter",
				"mask",
				"masksToBounds",
				"minificationFilter",
				"minificationFilterBias",
				"name",
				"needsDisplayOnBoundsChange",
				"opacity",
				"opaque",
				"position",
				"rasterizationScale",
				"shadowColor",
				"shadowOffset",
				"shadowOpacity",
				"shadowPath",
				"shadowRadius",
				"shouldRasterize",
				"style",
				"sublayers",
				"sublayerTransform",
				"superlayer",
				"transform",
				"visibleRect",
				"zPosition"
			};

            if (isOnParent)
                query = query + " parent * index:0";

            foreach (var property in properties)
            {
                object prop;
                bool found =
                    MaybeGetLayerProperty<string>(app, query, property, out prop) ||
                    MaybeGetLayerProperty<int>(app, query, property, out prop) ||
                    MaybeGetLayerProperty<bool>(app, query, property, out prop);

                if (found)
                    continue;
				//if (found)
				//	Logger.LogLine(string.Format("{0,-30}: {1}", property, prop));
            }

            //Logger.LogLine();

        }

		static void LogPropertiesForAndroidView (this IApp app, string query, bool isOnParent = false)
		{
           // Logger.LogLine( "--- Android View Properties ---");

			var properties = new [] {
				// just getters with no params, bools
				//"getAccessibilityLiveRegion",
				//"getAccessibilbityNodeProvider",
				//"getAlpha",
				//"getAnimation",
				//"getApplicationWindowToken",
				//"getBackground",
				//"getBaseline",
				//"getBottom",
				//"getCameraDistance",
				//"getClipBounds",
				//"getContentDescription",
				//"getContext",
				//"getDefaultSize",
				//"getDisplay",
				//"getDrawableState",
				//"getDrawingCache",
				//"getDrawingCacheBackgroundColor",
				//"getDrawingRect",
				//"getDrawingTime",
				//"getFilterTouchesWhenObscurred",
				//"getFitsSystemWindows",
				//"getFocusables",
				//"getHandler",
				//"getHeight",
				//"getHitRect",
				//"getHorizontalFadingEdgeLength",
				//"getId",
				//"getImportantForAccessibility",
				//"getKeepScreenOn",
				//"getKeyDispatcherState",
				//"getLabelFor",
				//"getLayerType",
				//"getLayoutDirection",
				//"getLayourParams",
				//"getLeft",
				"getMatrix",
				//"getMeasuredHeight",
				//"getMeasuredHeightAndState",
				//"getMeasuredState",
				//"getMeasuredWidth",
				//"getMeasuredWidthAndState",
				//"getMinimumHeight",
				//"getMinimumWidth",
				//"getNextFocusDownId",
				//"getNextFocusForwardId",
				//"getNextFocusLeftId",
				//"getNextFocusRightId",
				//"getNextFocusUpId",
				//"getOnFocusChangedListener",
				//"getOverScrollMethod",
				//"getOverlay",
				//"getPaddingBottom",
				//"getPaddingEnd",
				//"getPaddingLeft",
				//"getPaddingRight",
				//"getPaddingStart",
				//"getPaddingTop",
				//"getParent",
				//"getParentForAccessibility",
				//"getPivotX",
				//"getPivotY",
				//"getResources",
				//"getRight",
				//"getRootView",
				//"getRotation",
				//"getRotationX",
				//"getRotationY",
				"getScaleX",
				"getScaleY",
				//"getScrollBarDefaultDelayBeforeFade",
				//"getScrollBarFadeDuration",
				//"getScrollBarSize",
				//"getScrollBarStyle",
				//"getScrollX",
				//"getScrollY",
				//"getSolidColor",
				//"getSystemUiVisibility",
				//"getTag",
				//"getTextAlignment",
				//"getTextDirection",
				//"getTop",
				//"getTouchDelegate",
				//"getTouchables",
				//"getTranslationX",
				//"getTranslationY",
				//"getVerticalFadingEdgeLength",
				//"getVerticalScrollbarPosition",
				//"getVerticalScrollbarWidth",
				//"getViewTreeObserver",
				//"getVisibility",
				//"getWidth",
				//"getWindowId",
				//"getWindowSystemUiVisbility",
				//"getWindowToken",
				//"getWindowVisibility",
				//"getX",
				//"getY",
				//"hasFocus",
				//"hasFocusable",
				//"hasOnClickListener",
				//"hasOverlappingRendering",
				//"hasTransientState",
				//"hasWindowFocus",
				//"isActivated",
				//"isAttachedToWindow",
				//"isClickable",
				//"isDirty",
				//"isDrawingCacheEnabled",
				//"isDuplicateParentStateEnabled",
				//"isEnabled",
				//"isFocusable",
				//"isFocusableInTouchWindow",
				//"isFocused",
				//"isHapticFeedbackEnabled",
				//"isHardwareAccelerated",
				//"isHorizontalFadingEdgeEnabled",
				//"isHovered",
				//"idInEditMode",
				//"isInLayout",
				//"isInTouchMode",
				//"isLaidOut",
				//"isLayoutDirectionResolved",
				//"isLayoutRequested",
				//"isLongClickable",
				//"isOpaque",
				//"isPaddingRelative",
				//"isPressed",
				//"isSaveEnabled",
				//"isSaveFromParentEnabled",
				//"isScrollContainer",
				//"isScrollBarFadingEnabled",
				//"isSelected",
				//"isShown",
				//"isSoundEffectsEnabled",
				//"isTextAlignmentResolved",
				//"isTextDirectionResolved",
				//"isVerticalFadingEdgeEnabled",
				//"isVerticalScrollBarEnabled"
			};

			if (isOnParent)
				query = query + " parent * index:0";

			foreach (var property in properties) {
				object prop;
				bool found = 
					MaybeGetProperty<string> (app, query, property, out prop) || 
					//MaybeGetProperty<int>    (app, query, property, out prop) || 
					MaybeGetProperty<float>  (app, query, property, out prop) ||
					MaybeGetProperty<bool>   (app, query, property, out prop);

                if (found)
                    continue;
				//if (found)
				//	Logger.LogLine (string.Format ("{0,-30}: {1}", property, prop));
			}

            //Logger.LogLine();

		}

		static bool MaybeGetLayerProperty<T> (IApp app, string query, string property, out object result)
		{

			try {
				result = app.Query (q => q.Raw (query).Invoke ("layer").Invoke (property).Value<T> ()).First ();
			} catch {
				result = null;
				return false;
			}

			return true;
		}

		static bool MaybeGetProperty<T> (IApp app, string query, string property, out object result)
		{

			try {
				result = app.Query (q => q.Raw (query).Invoke (property).Value<T> ()).First ();
			} catch {
				result = null;
				return false;
			}

			return true;
		}
	}
}

