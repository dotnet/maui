using System;
using CoreAnimation;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Behavior that automatically resizes a CALayer to match its superlayer's bounds
	/// </summary>
	[BaseType(typeof(NSObject), Name = "MauiCALayerAutosizeToSuperLayerBehavior")]
	[Internal]
	interface MauiCALayerAutosizeToSuperLayerBehavior
	{
		/// <summary>
		/// Attaches this behavior to the given layer.
		/// The layer must have a superlayer when this method is called.
		/// The layer's frame will be kept in sync with the superlayer's bounds.
		/// </summary>
		/// <param name="layer">The layer that needs to be resized to match the superlayer's bounds.</param>
		[Export("attachWithLayer:")]
		MauiCALayerAutosizeToSuperLayerResult Attach(CALayer layer);

		/// <summary>
		/// Detaches this behavior from the current layer and stops observing
		/// </summary>
		[Export("detach")]
		void Detach();
	}

	/// <summary>
	/// Observes a closed set of KVO-compliant properties through Swift's <c>NSKeyValueObservation</c>,
	/// whose registrations cannot outlive their observer.
	/// Use <c>KeyValueObservation</c> rather than this type directly: the handler is retained natively,
	/// so it must not keep this object reachable.
	/// </summary>
	[BaseType(typeof(NSObject), Name = "MauiKeyValueObserver")]
	[Internal]
	interface MauiKeyValueObserver
	{
		[Export("observeBoundsOfLayer:handler:")]
		void ObserveBounds(CALayer layer, Action handler);

		[Export("observeFrameOfView:handler:")]
		void ObserveFrame(UIView view, Action handler);

		[Export("observeContentOffsetOfScrollView:handler:")]
		void ObserveContentOffset(UIScrollView scrollView, Action handler);

		[iOS(16, 0), MacCatalyst(16, 0)]
		[Export("observeEffectiveGeometryOfWindowScene:handler:")]
		void ObserveEffectiveGeometry(UIWindowScene windowScene, Action handler);

		/// <summary>
		/// Stops every observation started by this instance and releases their handlers.
		/// </summary>
		[Export("detach")]
		void Detach();
	}
}