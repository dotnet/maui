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
	internal record ViewEntry(WeakReference<object> View, MauiWindowInsetListener Listener);

	/// <summary>
	/// Manages window insets and safe area handling for Android views.
	/// This class can be used as a global listener (one per activity) or as local listeners
	/// attached to specific views for better isolation in complex navigation scenarios.
	///
	/// Thread Safety: All public methods should be called on the UI thread.
	/// Android view operations are not thread-safe and must execute on the main thread.
	/// </summary>
	internal class MauiWindowInsetListener : WindowInsetsAnimationCompat.Callback, IOnApplyWindowInsetsListener
	{
		readonly HashSet<AView> _trackedViews = [];
		bool IsImeAnimating { get; set; }

		// Set when a dispatch was gated (or an exempted view applied animation-time insets)
		// while an IME animation was in flight, so the end of the animation re-applies the
		// settled insets. RequestApplyInsets is not per-view: it walks up to the ViewRootImpl
		// and re-dispatches insets across the whole hierarchy on the next traversal, so a
		// single call on any attached view covers every gated view.
		bool _reapplyInsetsWhenAnimationEnds;

		// The most recent view whose dispatch was gated. _trackedViews only holds views that
		// have had padding applied, so a gated view is often absent from it; this keeps a
		// poster candidate for the end-of-animation re-apply. Cleared when the animation ends.
		AView? _lastGatedView;

		// Views that started using this listener while an IME animation was in flight.
		// The IsImeAnimating gate exists to keep already-correct views stable during the
		// animation; a view that just (re)attached has no valid safe-area padding yet, so it
		// must bypass the gate or it stays without padding until the animation ends (#37012).
		// Cleared when an animation ends or a new one starts.
		readonly HashSet<AView> _viewsAttachedDuringImeAnimation = [];

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
		internal static MauiWindowInsetListener? UnregisterView(AView view)
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
		/// Finds the appropriate MauiWindowInsetListener for a given view by walking
		/// up the view hierarchy until a registered view is found.
		/// Must be called on UI thread.
		/// </summary>
		/// <param name="view">The view to find a listener for</param>
		/// <returns>The local listener if view is in a registered view hierarchy, null otherwise</returns>
		internal static MauiWindowInsetListener? FindListenerForView(AView view)
		{
			if (!ShouldSetMauiWindowInsetListener(view))
			{
				return null;
			}

			return FindRegisteredListenerForView(view);
		}

		internal static MauiWindowInsetListener? FindRegisteredListenerForView(AView view)
		{
			// Walk up the view hierarchy looking for a registered view
			var parent = view.Parent;
			while (parent is not null)
			{
				if (parent is AView parentView)
				{
					if (FindRegisteredListener(parentView) is MauiWindowInsetListener listener)
					{
						return listener;
					}
				}

				parent = parent.Parent;
			}

			return null;
		}

		internal static bool ShouldSetMauiWindowInsetListener(AView view)
		{
			var parent = view.Parent;
			var isInsideRecyclerEmptyView = false;

			while (parent is not null)
			{
				if (parent is IMauiRecyclerViewEmptyView)
				{
					isInsideRecyclerEmptyView = true;
				}

				// MaterialToolbar needs its own inset handling, so it is exempt from all listener-suppression branches.
				// Skip listeners for views inside AppBarLayout/MauiScrollView, and for recycler item views
				// unless SafeAreaEdges was explicitly set.
				if (view is not MaterialToolbar &&
					(parent is AppBarLayout ||
						parent is MauiScrollView ||
						(parent is IMauiRecyclerView && !isInsideRecyclerEmptyView && !HasExplicitSafeAreaEdges(view))))
				{
					return false;
				}

				parent = parent.Parent;
			}

			return true;
		}

		static MauiWindowInsetListener? FindRegisteredListener(AView parentView)
		{
			// Check if this parent view is registered. Clean up dead references while searching.
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

			return null;
		}

		static bool HasExplicitSafeAreaEdges(AView view)
		{
			return view is ICrossPlatformLayoutBacking { CrossPlatformLayout: ISafeAreaView2 safeAreaView } &&
				safeAreaView.HasExplicitSafeAreaEdges;
		}

		/// <summary>
		/// Sets up a view to use this listener for inset handling.
		/// This method registers the view and attaches the listener.
		/// Must be called on UI thread.
		/// </summary>
		/// <param name="view">The view to set up</param>
		/// <returns>The same view for method chaining</returns>
		internal static AView SetupViewWithLocalListener(AView view, MauiWindowInsetListener? listener = null)
		{
			listener ??= new MauiWindowInsetListener();
			ViewCompat.SetOnApplyWindowInsetsListener(view, listener);
			ViewCompat.SetWindowInsetsAnimationCallback(view, listener);

			listener.RegisterView(view);

			return view;
		}

		/// <summary>
		/// Registers a parent view so its children can find an inset listener, without attaching
		/// the listener to the parent itself. This is useful when you want child views to handle
		/// insets but don't want the parent view to consume them.
		/// Must be called on UI thread.
		/// </summary>
		/// <param name="parentView">The parent view to register</param>
		/// <param name="listener">Optional listener to use. If null, a new one is created.</param>
		/// <returns>The listener that was registered</returns>
		internal static MauiWindowInsetListener RegisterParentForChildViews(AView parentView, MauiWindowInsetListener? listener = null)
		{
			listener ??= new MauiWindowInsetListener();
			listener.RegisterView(parentView);
			return listener;
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

		public MauiWindowInsetListener() : base(DispatchModeStop)
		{
		}

		/// <summary>
		/// Notifies this listener that a view has (re)attached to the window and started using it.
		/// Views attached while an IME animation is in flight are exempted from the IsImeAnimating
		/// gate for the remainder of that animation so they can obtain their safe-area padding.
		/// Must be called on UI thread.
		/// </summary>
		/// <param name="view">The view that attached</param>
		internal void NotifyViewAttached(AView view)
		{
			if (IsImeAnimating)
			{
				_viewsAttachedDuringImeAnimation.Add(view);
			}
		}

		public virtual WindowInsetsCompat? OnApplyWindowInsets(AView? v, WindowInsetsCompat? insets)
		{
			if (insets is null || !insets.HasInsets || v is null ||
				(IsImeAnimating && !_viewsAttachedDuringImeAnimation.Contains(v)))
			{
				if (IsImeAnimating && v is not null)
				{
					_reapplyInsetsWhenAnimationEnds = true;
					_lastGatedView = v;
				}

				return insets;
			}

			if (IsImeAnimating)
			{
				// The exemption is one-shot: it exists to give a freshly attached view its initial
				// padding, and after this first successful apply the view is correct and gets gated
				// like every other view for the rest of the animation. The insets it just applied
				// are animation-time values (e.g. keyboard-height bottom padding mid
				// hide-animation), so the end of the animation must re-apply the settled ones.
				_viewsAttachedDuringImeAnimation.Remove(v);
				_reapplyInsetsWhenAnimationEnds = true;
			}

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

			var bottomTabContainer = v.FindViewById<ViewGroup>(Resource.Id.navigationlayout_bottomtabs);
			var hasBottomNav = bottomTabContainer?.MeasuredHeight > 0;
			var contentView = v.FindViewById(Resource.Id.navigationlayout_content);

			if (hasBottomNav)
			{
				var bottomInset = Math.Max(systemBars?.Bottom ?? 0, displayCutout?.Bottom ?? 0);

				// Only pad the bottom of contentView to prevent content from sliding under the
				// BottomNavigationView + system navigation bar. Left/right are intentionally
				// excluded: landscape cutout padding on the content area is handled by
				// SafeAreaExtensions which applies per-view overlap logic.
				contentView?.SetPadding(0, 0, 0, bottomInset);
			}
			else
			{
				// Reset contentView padding when bottom navigation is removed dynamically
				contentView?.SetPadding(0, 0, 0, 0);
			}

			// Consume top inset when AppBar is visible — it already pads itself, so downstream
			// views must not receive a top inset or SafeAreaExtensions will double-apply it.
			// Bottom inset is passed through unconsumed so BottomNavigationView can extend its
			// background into the system navigation bar area (issue #33344).
			var newSystemBars = Insets.Of(
				systemBars?.Left ?? 0,
				appBarHasContent ? 0 : systemBars?.Top ?? 0,
				systemBars?.Right ?? 0,
				systemBars?.Bottom ?? 0) ?? Insets.None;

			var newDisplayCutout = Insets.Of(
				displayCutout?.Left ?? 0,
				appBarHasContent ? 0 : displayCutout?.Top ?? 0,
				displayCutout?.Right ?? 0,
				displayCutout?.Bottom ?? 0) ?? Insets.None;

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

        public bool IsViewTracked(AView view)
        {
            return _trackedViews.Contains(view);
        }
		public void ResetView(AView view)
		{
			if (view is IHandleWindowInsets customHandler)
			{
				customHandler.ResetWindowInsets(view);
			}

			_trackedViews.Remove(view);
			_viewsAttachedDuringImeAnimation.Remove(view);

			if (ReferenceEquals(_lastGatedView, view))
			{
				_lastGatedView = null;
			}
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
				_viewsAttachedDuringImeAnimation.Clear();
			}
			base.Dispose(disposing);
		}

		public override void OnPrepare(WindowInsetsAnimationCompat? animation)
		{
			base.OnPrepare(animation);
			if (IsImeAnimation(animation))
			{
				StartImeAnimation();
			}
		}

		public override WindowInsetsAnimationCompat.BoundsCompat? OnStart(WindowInsetsAnimationCompat? animation, WindowInsetsAnimationCompat.BoundsCompat? bounds)
		{
			if (IsImeAnimation(animation))
			{
				StartImeAnimation();
			}

			return bounds;
		}

		// Set when OnEnd posts a gate release and cleared when an animation starts, so a
		// release posted by a previous animation cannot open the gate mid-flight: with
		// back-to-back animations (a hide immediately followed by a show) the new OnPrepare
		// can arrive before the posted runnable executes.
		bool _gateReleaseScheduled;

		void StartImeAnimation()
		{
			_gateReleaseScheduled = false;

			if (!IsImeAnimating)
			{
				IsImeAnimating = true;

				// Exemptions only apply to the animation during which the view attached
				_viewsAttachedDuringImeAnimation.Clear();
			}
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

			if (!IsImeAnimation(animation))
			{
				return;
			}

			_viewsAttachedDuringImeAnimation.Clear();

			// Keep the gate up for one more main-looper turn: the system's deferred
			// post-animation inset dispatches can still carry animation-time IME insets.
			// The release must be posted through an *attached* view — a detached view's Post
			// only runs if that view re-attaches, which would leave the gate closed for every
			// view sharing this listener.
			var poster = FindAttachedTrackedView();

			if (poster is not null)
			{
				_gateReleaseScheduled = true;
				poster.Post(() =>
				{
					// StartImeAnimation clears the flag, so a new animation started before
					// this ran means the release belongs to the old one and must be skipped
					if (_gateReleaseScheduled)
					{
						EndImeAnimation(poster);
					}
				});
			}
			else
			{
				// No attached view to post through; release synchronously rather than
				// leaving the gate stuck
				EndImeAnimation(null);
			}
		}

		AView? FindAttachedTrackedView()
		{
			foreach (var view in _trackedViews)
			{
				// A view can be disposed while still tracked when a cleanup path is skipped
				if (view.IsAlive() && view.IsAttachedToWindow)
				{
					return view;
				}
			}

			// _trackedViews only holds views that actually had padding applied, and a view
			// gated during this animation has by definition not applied any yet — so fall
			// back to the gated view itself, which is what the pre-PR code posted through
			if (_lastGatedView.IsAlive() && _lastGatedView.IsAttachedToWindow)
			{
				return _lastGatedView;
			}

			return null;
		}

		void EndImeAnimation(AView? reapplyThrough)
		{
			IsImeAnimating = false;
			_gateReleaseScheduled = false;
			_viewsAttachedDuringImeAnimation.Clear();
			_lastGatedView = null;

			if (!_reapplyInsetsWhenAnimationEnds)
			{
				return;
			}

			// IsAlive covers null as well as a peer disposed during the one-looper-turn delay.
			// Leave the flag set when we cannot act on it: the re-apply is still owed, and
			// consuming it here would drop the settled insets entirely.
			if (!reapplyThrough.IsAlive())
			{
				return;
			}

			_reapplyInsetsWhenAnimationEnds = false;

			// One call is enough: this reaches the ViewRootImpl and re-dispatches insets
			// across the whole hierarchy, so every gated view gets its settled insets
			ViewCompat.RequestApplyInsets(reapplyThrough);
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
internal static class MauiWindowInsetListenerExtensions
{
	/// <summary>
	/// Sets the appropriate MauiWindowInsetListener on the specified view.
	/// This prioritizes local view listeners over global ones.
	/// </summary>
	/// <param name="view">The Android view to set the listener on</param>
	/// <param name="context">The Android context to get the listener from</param>
	public static bool TrySetMauiWindowInsetListener(this View view, Context context)
	{
		if (MauiWindowInsetListener.FindListenerForView(view) is MauiWindowInsetListener localListener)
		{
			ViewCompat.SetOnApplyWindowInsetsListener(view, localListener);
			ViewCompat.SetWindowInsetsAnimationCallback(view, localListener);
			localListener.NotifyViewAttached(view);
			return true;
		}

		// If no listener available, this is likely a configuration issue but not critical
		return false;
	}

	/// <summary>
	/// Refreshes the MauiWindowInsetListener attached to the specified view after SafeAreaEdges eligibility changes.
	/// Unlike TrySetMauiWindowInsetListener, this finds the registered parent listener before applying
	/// eligibility checks so it can detach the listener and reset applied safe areas when the view is
	/// no longer eligible.
	/// </summary>
	/// <param name="view">The Android view to refresh the listener on</param>
	/// <param name="context">The Android context to get the listener from</param>
	public static bool RefreshMauiWindowInsetListener(this View view, Context context)
	{
		var listener = MauiWindowInsetListener.FindRegisteredListenerForView(view);
		if (listener is null)
		{
			ViewCompat.SetOnApplyWindowInsetsListener(view, null);
			ViewCompat.SetWindowInsetsAnimationCallback(view, null);
			return false;
		}

		if (MauiWindowInsetListener.ShouldSetMauiWindowInsetListener(view))
		{
			ViewCompat.SetOnApplyWindowInsetsListener(view, listener);
			ViewCompat.SetWindowInsetsAnimationCallback(view, listener);
			// Deliberately no NotifyViewAttached here: this is a SafeAreaEdges configuration
			// change, which for an already-listening view means it is already padded and must
			// stay subject to the IME gate (it gets its updated insets at the end of any
			// in-flight animation), whereas the exemption is for fresh attaches.
			// Caveat: a view transitioning from ineligible to eligible (e.g. SafeAreaEdges
			// None -> All) has no padding yet, so if that lands mid-animation it stays
			// unpadded until the animation ends. Closing that would mean threading the
			// callers' _isInsetListenerSet state through this method.
			return true;
		}

		ViewCompat.SetOnApplyWindowInsetsListener(view, null);
		ViewCompat.SetWindowInsetsAnimationCallback(view, null);
		listener.ResetAppliedSafeAreas(view);
		return false;
	}

	/// <summary>
	/// Removes the MauiWindowInsetListener from the specified view and resets its tracked state.
	/// This should be called when a view is being detached to ensure proper cleanup.
	/// </summary>
	/// <param name="view">The Android view to remove the listener from</param>
	/// <param name="context">The Android context to get the listener from</param>
	public static void RemoveMauiWindowInsetListener(this View view, Context context)
	{
		// Clear the listeners first
		ViewCompat.SetOnApplyWindowInsetsListener(view, null);
		ViewCompat.SetWindowInsetsAnimationCallback(view, null);

		// Reset view state - prefer local listener if available, otherwise use global
		var listener = MauiWindowInsetListener.FindRegisteredListenerForView(view);
		listener?.ResetView(view);
	}
}
