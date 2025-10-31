using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using AndroidX.Core.Graphics;
using AndroidX.Core.View;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.AppBar;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Registry entry for tracking view instances and their associated listeners.
	/// Uses WeakReference to avoid memory leaks when views are disposed.
	/// </summary>
	internal record ViewEntry(WeakReference<object> View, GlobalWindowInsetListener Listener);

	/// <summary>
	/// Manages window insets and safe area handling for Android views.
	/// This class can be used as a global listener (one per activity) or as local listeners
	/// attached to specific views for better isolation in complex navigation scenarios.
	///
	/// Thread Safety: All public methods should be called on the UI thread.
	/// Android view operations are not thread-safe and must execute on the main thread.
	/// </summary>
	internal class GlobalWindowInsetListener : WindowInsetsAnimationCompat.Callback, IOnApplyWindowInsetsListener
	{
		readonly HashSet<AView> _trackedViews = [];
		bool IsImeAnimating { get; set; }

		AView? _pendingView;

		// Static tracking for views that have local inset listeners.
		// This registry allows child views to find their appropriate listener without
		// relying on a global activity-level listener.
		// Thread Safety: All access must be on UI thread (enforced by Android's threading model).
		static readonly List<ViewEntry> _registeredViews = new();

		/// <summary>
		/// Registers a view to use this local listener instead of the global one.
		/// This enables per-view inset management for better isolation in complex scenarios.
		/// Must be called on UI thread.
		/// </summary>
		/// <param name="view">The view to register</param>
		internal void RegisterView(AView view)
		{
			// Clean up dead references and check for existing registration
			for (int i = _registeredViews.Count - 1; i >= 0; i--)
			{
				var entry = _registeredViews[i];
				if (!entry.View.TryGetTarget(out var existingView))
				{
					_registeredViews.RemoveAt(i);
				}
				else if (existingView == view)
				{
					// Already registered, no need to add again
					return;
				}
			}

			// Add this view to the registry
			_registeredViews.Add(new ViewEntry(new WeakReference<object>(view), this));
		}

		/// <summary>
		/// Unregisters a view from using this local listener.
		/// Must be called on UI thread.
		/// </summary>
		/// <param name="view">The view to unregister</param>
		internal static GlobalWindowInsetListener? UnregisterView(AView view)
		{
			for (int i = _registeredViews.Count - 1; i >= 0; i--)
			{
				if (_registeredViews[i].View.TryGetTarget(out var registeredView) && registeredView == view)
				{
					var listener = _registeredViews[i].Listener;
					_registeredViews.RemoveAt(i);
					return listener;
				}
			}
			return null;
		}

		/// <summary>
		/// Finds the appropriate GlobalWindowInsetListener for a given view by walking
		/// up the view hierarchy until a registered view is found.
		/// Must be called on UI thread.
		/// </summary>
		/// <param name="view">The view to find a listener for</param>
		/// <returns>The local listener if view is in a registered view hierarchy, null otherwise</returns>
		internal static GlobalWindowInsetListener? FindListenerForView(AView view)
		{
			// Walk up the view hierarchy looking for a registered view
			var parent = view.Parent;
			while (parent is not null)
			{
				// Skip setting listener on views inside nested scroll containers or AppBarLayout (except MaterialToolbar)
				// We want the layout listener logic to get applied to the MaterialToolbar itself
				// But we don't want any layout listeners to get applied to the children of MaterialToolbar (like the TitleView)
				if (view is not MaterialToolbar &&
					(parent is AppBarLayout || parent is MauiScrollView || parent is IMauiRecyclerView))
				{
					return null;
				}

				if (parent is AView parentView)
				{
					// Check if this parent view is registered
					// Clean up dead references while searching
					for (int i = _registeredViews.Count - 1; i >= 0; i--)
					{
						var entry = _registeredViews[i];
						if (!entry.View.TryGetTarget(out var registeredView))
						{
							_registeredViews.RemoveAt(i);
						}
						else if (ReferenceEquals(registeredView, parentView))
						{
							return entry.Listener;
						}
					}
				}

				parent = parent.Parent;
			}

			return null;
		}

		/// <summary>
		/// Sets up a view to use this listener for inset handling.
		/// This method registers the view and attaches the listener.
		/// Must be called on UI thread.
		/// </summary>
		/// <param name="view">The view to set up</param>
		/// <returns>The same view for method chaining</returns>
		internal static AView SetupViewWithLocalListener(AView view, GlobalWindowInsetListener? listener = null)
		{
			listener ??= new GlobalWindowInsetListener();
			ViewCompat.SetOnApplyWindowInsetsListener(view, listener);
			ViewCompat.SetWindowInsetsAnimationCallback(view, listener);

			listener.RegisterView(view);

			return view;
		}

		/// <summary>
		/// Removes the local listener from a view and properly cleans up.
		/// This resets all tracked views and unregisters the view.
		/// Must be called on UI thread.
		/// </summary>
		/// <param name="view">The view to clean up</param>
		internal static void RemoveViewWithLocalListener(AView view)
		{
			// Remove the listener from the view
			ViewCompat.SetOnApplyWindowInsetsListener(view, null);
			ViewCompat.SetWindowInsetsAnimationCallback(view, null);

			// Reset any tracked views within this view
			UnregisterView(view)?.ResetAppliedSafeAreas(view);
		}

		public GlobalWindowInsetListener() : base(DispatchModeStop)
		{
		}

		public virtual WindowInsetsCompat? OnApplyWindowInsets(AView? v, WindowInsetsCompat? insets)
		{
			if (insets is null || !insets.HasInsets || v is null || IsImeAnimating)
			{
				if (IsImeAnimating)
				{
					_pendingView = v;
				}

				return insets;
			}

			_pendingView = null;

			// Handle custom inset views first
			if (v is IHandleWindowInsets customHandler)
			{
				return customHandler.HandleWindowInsets(v, insets);
			}

			// Apply default window insets for standard views
			return ApplyDefaultWindowInsets(v, insets);
		}

		static WindowInsetsCompat? ApplyDefaultWindowInsets(AView v, WindowInsetsCompat insets)
		{
			var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
			var displayCutout = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());

			// Handle MaterialToolbar special case early
			if (v is MaterialToolbar)
			{
				v.SetPadding(displayCutout?.Left ?? 0, 0, displayCutout?.Right ?? 0, 0);
				return WindowInsetsCompat.Consumed;
			}

			// Find AppBarLayout - check direct child first, then first two children
			var appBarLayout = v.FindViewById<AppBarLayout>(Resource.Id.navigationlayout_appbar);
			if (appBarLayout is null && v is ViewGroup group)
			{
				if (group.ChildCount > 0 && group.GetChildAt(0) is AppBarLayout firstChild)
				{
					appBarLayout = firstChild;
				}
				else if (group.ChildCount > 1 && group.GetChildAt(1) is AppBarLayout secondChild)
				{
					appBarLayout = secondChild;
				}
			}

			// Check if AppBarLayout has meaningful content
			bool appBarHasContent = appBarLayout?.MeasuredHeight > 0;
			if (!appBarHasContent && appBarLayout is not null)
			{
				for (int i = 0; i < appBarLayout.ChildCount; i++)
				{
					var child = appBarLayout.GetChildAt(i);
					if (child?.MeasuredHeight > 0)
					{
						appBarHasContent = true;
						break;
					}
				}
			}

			// Apply padding to AppBarLayout based on content and system insets
			if (appBarLayout is not null)
			{
				if (appBarHasContent)
				{
					var topInset = Math.Max(systemBars?.Top ?? 0, displayCutout?.Top ?? 0);
					appBarLayout.SetPadding(systemBars?.Left ?? 0, topInset, systemBars?.Right ?? 0, 0);
				}
				else
				{
					appBarLayout.SetPadding(0, 0, 0, 0);
				}
			}

			// Handle bottom navigation
			var hasBottomNav = v.FindViewById(Resource.Id.navigationlayout_bottomtabs)?.MeasuredHeight > 0;
			if (hasBottomNav)
			{
				var bottomInset = Math.Max(systemBars?.Bottom ?? 0, displayCutout?.Bottom ?? 0);
				v.SetPadding(0, 0, 0, bottomInset);
			}
			else
			{
				v.SetPadding(0, 0, 0, 0);
			}

			// Create new insets with consumed values
			var newSystemBars = Insets.Of(
				systemBars?.Left ?? 0,
				appBarHasContent ? 0 : systemBars?.Top ?? 0,
				systemBars?.Right ?? 0,
				hasBottomNav ? 0 : systemBars?.Bottom ?? 0
			) ?? Insets.None;

			var newDisplayCutout = Insets.Of(
				displayCutout?.Left ?? 0,
				appBarHasContent ? 0 : displayCutout?.Top ?? 0,
				displayCutout?.Right ?? 0,
				hasBottomNav ? 0 : displayCutout?.Bottom ?? 0
			) ?? Insets.None;

			return new WindowInsetsCompat.Builder(insets)
				?.SetInsets(WindowInsetsCompat.Type.SystemBars(), newSystemBars)
				?.SetInsets(WindowInsetsCompat.Type.DisplayCutout(), newDisplayCutout)
				?.Build() ?? insets;
		}

		public void TrackView(AView view)
		{
			_trackedViews.Add(view);
		}

		public bool HasTrackedView => _trackedViews.Count > 0;

		public void ResetView(AView view)
		{
			if (view is IHandleWindowInsets customHandler)
			{
				customHandler.ResetWindowInsets(view);
			}

			_trackedViews.Remove(view);
		}

		public void ResetAllViews()
		{
			// Create a copy to avoid modification during enumeration
			var viewsToReset = _trackedViews.ToArray();
			foreach (var view in viewsToReset)
			{
				ResetView(view);
			}
		}

		/// <summary>
		/// Resets all tracked descendant views of the specified parent view to their original padding.
		/// This should be called before applying new insets when SafeArea settings change.
		/// </summary>
		/// <param name="view">The parent view whose descendants should be reset</param>
		public void ResetAppliedSafeAreas(AView view)
		{
			ResetView(view);

			// Find all tracked views that are descendants of the parent view and reset them
			foreach (var trackedView in _trackedViews.ToArray()) // Use ToArray to avoid modification during enumeration
			{
				if (IsDescendantOf(trackedView, view))
				{
					ResetView(trackedView);
				}
			}
		}

		/// <summary>
		/// Checks if a view is a descendant of a parent view
		/// </summary>
		static bool IsDescendantOf(AView? child, AView parent)
		{
			if (child is null)
			{
				return false;
			}

			var currentParent = child.Parent;
			while (currentParent is not null)
			{
				if (currentParent == parent)
				{
					return true;
				}

				currentParent = currentParent.Parent;
			}
			return false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ResetAllViews();
			}
			base.Dispose(disposing);
		}

		public override void OnPrepare(WindowInsetsAnimationCompat? animation)
		{
			base.OnPrepare(animation);
			if (IsImeAnimation(animation))
			{
				IsImeAnimating = true;
			}
		}

		public override WindowInsetsAnimationCompat.BoundsCompat? OnStart(WindowInsetsAnimationCompat? animation, WindowInsetsAnimationCompat.BoundsCompat? bounds)
		{
			if (IsImeAnimation(animation))
			{
				IsImeAnimating = true;
			}

			return bounds;
		}

		public override WindowInsetsCompat? OnProgress(WindowInsetsCompat? insets, IList<WindowInsetsAnimationCompat>? runningAnimations)
		{
			if (insets is null || runningAnimations is null)
			{
				return insets;
			}

			// Process any IME animations
			foreach (var animation in runningAnimations)
			{
				if (IsImeAnimation(animation))
				{
					var imeInsets = insets.GetInsets(WindowInsetsCompat.Type.Ime());
					// IME height available as: imeInsets?.Bottom ?? 0
					break; // Only need to process one IME animation
				}
			}
			return insets;
		}

		public override void OnEnd(WindowInsetsAnimationCompat? animation)
		{
			base.OnEnd(animation);

			if (IsImeAnimation(animation))
			{
				if (_pendingView is AView view)
				{
					_pendingView = null;
					view.Post(() =>
					{
						IsImeAnimating = false;
						ViewCompat.RequestApplyInsets(view);
					});
				}
				else
				{
					IsImeAnimating = false;
				}
			}
		}

		/// <summary>
		/// Helper method to check if an animation involves the IME
		/// </summary>
		static bool IsImeAnimation(WindowInsetsAnimationCompat? animation) =>
			animation is not null && (animation.TypeMask & WindowInsetsCompat.Type.Ime()) != 0;
	}
}

/// <summary>
/// Extension methods to access WindowInsetListener instances.
/// These methods support both the legacy global listener pattern and the new
/// per-view local listener pattern.
/// </summary>
internal static class GlobalWindowInsetListenerExtensions
{
	/// <summary>
	/// Sets the appropriate GlobalWindowInsetListener on the specified view.
	/// This prioritizes local view listeners over global ones.
	/// </summary>
	/// <param name="view">The Android view to set the listener on</param>
	/// <param name="context">The Android context to get the listener from</param>
	public static bool TrySetGlobalWindowInsetListener(this View view, Context context)
	{
		// Check if this view is contained within a registered view first
		if (GlobalWindowInsetListener.FindListenerForView(view) is GlobalWindowInsetListener localListener)
		{
			ViewCompat.SetOnApplyWindowInsetsListener(view, localListener);
			ViewCompat.SetWindowInsetsAnimationCallback(view, localListener);
			return true;
		}

		// If no listener available, this is likely a configuration issue but not critical
		return false;
	}

	/// <summary>
	/// Removes the GlobalWindowInsetListener from the specified view and resets its tracked state.
	/// This should be called when a view is being detached to ensure proper cleanup.
	/// </summary>
	/// <param name="view">The Android view to remove the listener from</param>
	/// <param name="context">The Android context to get the listener from</param>
	public static void RemoveGlobalWindowInsetListener(this View view, Context context)
	{
		// Clear the listeners first
		ViewCompat.SetOnApplyWindowInsetsListener(view, null);
		ViewCompat.SetWindowInsetsAnimationCallback(view, null);

		// Reset view state - prefer local listener if available, otherwise use global
		var listener = GlobalWindowInsetListener.FindListenerForView(view);
		listener?.ResetView(view);
	}
}