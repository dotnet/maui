using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Numerics;

#if ANDROID
using Android.Runtime;
using Android.Locations;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.CoordinatorLayout.Widget;
using Microsoft.Maui.Platform;
using AccessibilityCheck.Droid;
using AccessibilityCheck.Droid.Checks;
using AccessibilityCheck.Droid.UIElement;
using AndroidX.Fragment.App.StrictMode;
using Com.Google.Android.Apps.Common.Testing.Accessibility.Framework.Replacements;
using Microsoft.Maui.Graphics;
using Point = Microsoft.Maui.Graphics.Point;
using ARect = Android.Graphics.Rect;
using ACRect = Com.Google.Android.Apps.Common.Testing.Accessibility.Framework.Replacements.Rect;
#endif

namespace Microsoft.Maui
{
#pragma warning disable RS0016
#if ANDROID
	[System.Runtime.Versioning.SupportedOSPlatform("android23.0")]
#endif
	public class AccessibilityDiagnosticsOverlay : WindowOverlay 
	{
		public AccessibilityDiagnosticsOverlay(IWindow window) : base(window)
		{
		}

		public override bool Initialize()
		{
			var initialized = base.Initialize();

			if (initialized)
			{
#if ANDROID
				FindRootView();
#endif
			}

			Tapped += OverlayOnTapped;

			return initialized;
		}

#if ANDROID
		ViewGroup? _nativeLayer;

		public Maui.Graphics.Point Offset { get; internal set; }

		public override void HandleUIChange()
		{
			base.HandleUIChange();

			UpdateOffset();

			//if (!_displayed)
			{
				// Just doing this once, when the page is displayed, for the PoC. 
				// In reality, this should probably clear any overlay elements and 
				// reassess whenever the UI changes

				Assess();
			}

			//_displayed = true;
		}

		void UpdateOffset() 
		{
			if (GraphicsView != null)
				Offset = GenerateAdornerOffset(GraphicsView);
		}

		Point GenerateAdornerOffset(View graphicsView)
		{
			if (graphicsView == null || graphicsView.Context?.GetActivity() is not Activity nativeActivity)
				return new Maui.Graphics.Point();

			if (nativeActivity.Resources == null || nativeActivity.Resources.DisplayMetrics == null)
				return new Maui.Graphics.Point();

			var decorView = nativeActivity.Window?.DecorView;
			var rectangle = new Android.Graphics.Rect();

			if (decorView is not null)
			{
				decorView.GetWindowVisibleDisplayFrame(rectangle);
			}

			float dpi = nativeActivity.Resources.DisplayMetrics.Density;
			return new Point(0, -(rectangle.Top / dpi));
		}

		void FindRootView()
		{
			var platformWindow = Window?.Content?.ToPlatform();

			if (platformWindow == null)
				return;

			var handler = Window?.Handler as WindowHandler;
			if (handler?.MauiContext == null)
				return;

			var rootManager = handler.MauiContext.GetNavigationRootManager();
			if (rootManager == null)
				return;

			_nativeLayer = rootManager.RootView as ViewGroup;
			_nativeLayer = _nativeLayer?.GetFirstChildOfType<CoordinatorLayout>() ?? _nativeLayer;

			if (_nativeLayer is ContainerView containerView)
				_nativeLayer = containerView.MainView as ViewGroup;
		}

		void Assess() 
		{
			RemoveWindowElements();

			if (_nativeLayer == null)
			{
				return;
			}

			var errors = GetErrors(AssessAccessibility(_nativeLayer));
			var displayErrors = MergeErrors(errors);

			foreach (var e in displayErrors)
			{
				var bounds = e.Rect;
				if (bounds != null)
				{
					var rect = new ARect(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
					AddAccessibilityIssueHighlight(rect, e.Message);
				}
			}
		}

		List<ErrorInfo> MergeErrors(List<AccessibilityHierarchyCheckResult> errors) 
		{
			var errorInfos = errors.Select(e => new ErrorInfo(e.Element.CondensedUniqueId, e.Message, e.Element.BoundsInScreen));

			var groupedErrors = errorInfos.GroupBy(e => e.Id);

			List<ErrorInfo> displayErrors = new List<ErrorInfo>();

			foreach (var group in groupedErrors)
			{
				var mergedError = new ErrorInfo(group.Key, string.Empty, ACRect.Empty);

				foreach (var errorInfo in group)
				{
					if (mergedError.Message.Length > 0)
					{
						mergedError.Message += "\r\n";
					}

					mergedError.Message += errorInfo.Message;
					mergedError.Rect = errorInfo.Rect;
				}

				displayErrors.Add(mergedError);
			}

			return displayErrors;
		}

		class ErrorInfo 
		{
			public string Message { get; set; }
			public long Id { get; }
			public ACRect Rect { get; set; }

			public ErrorInfo(long id, string message, ACRect rect) 
			{
				Id = id;
				Message = message;
				Rect = rect;
			}
		}

		static List<AccessibilityHierarchyCheckResult> AssessAccessibility(Android.Views.View root) 
		{
			List<AccessibilityHierarchyCheck> checks = new List<AccessibilityHierarchyCheck>()
					{
						new ClassNameCheck(),
						new DuplicateClickableBoundsCheck(),
						new DuplicateSpeakableTextCheck(),
						new EditableContentDescCheck(),
						new RedundantDescriptionCheck(),
						new SpeakableTextPresentCheck(),
						new TouchTargetSizeCheck(),
					};

			var hierarchy = AccessibilityHierarchyAndroid.NewBuilder(root).Build();

			var results = RunChecks(hierarchy, checks) ?? new List<AccessibilityHierarchyCheckResult>();

			foreach (var x in results)
			{
				System.Diagnostics.Debug.WriteLine($"{x.Type} {x.Message}");
			}

			return results;
		}

		static List<AccessibilityHierarchyCheckResult> RunChecks(AccessibilityHierarchy hierarchy,
			List<AccessibilityHierarchyCheck> checks)
		{
			List<AccessibilityHierarchyCheckResult> results = new();

			for (int c = 0; c < checks.Count; c++)
			{
				var checkResult = checks[c].RunCheckOnHierarchy(hierarchy);

				for (int r = 0; r < checkResult.Count; r++)
				{
					results.Add(checkResult[r]);
				}
			}

			return results;
		}

		static List<AccessibilityHierarchyCheckResult> GetErrors(List<AccessibilityHierarchyCheckResult> results) 
		{
			var errors = new List<AccessibilityHierarchyCheckResult>();

			for (int n = 0; n < results.Count; n++)
			{
				var result = results[n];

				if (result.Type == AccessibilityCheckResult.AccessibilityCheckResultType.Error)
				{
					errors.Add(result);
				}
			}

			return errors;
		}

		bool AddAccessibilityIssueHighlight(Android.Graphics.Rect rect, string message) 
		{
			var context = _nativeLayer?.Context;

			if (context == null)
			{
				return false;
			}

			var destRect = context.FromPixels(rect);
			return base.AddWindowElement(new AccessibilityOverlayElement(destRect, message, offset: Offset));
		}


#endif
		void OverlayOnTapped(object? sender, WindowOverlayTappedEventArgs e)
		{
			foreach (var element in WindowElements)
			{
				if (element is AccessibilityOverlayElement a11yElement)
				{
					a11yElement.HideInfo();
				}
			}

			foreach (var element in WindowElements)
			{
				if (element.Contains(e.Point) && element is AccessibilityOverlayElement a11yElement)
				{
					a11yElement.ShowInfo();
				}
			}

			Invalidate();
		}
	}

	

#pragma warning restore RS0016
}