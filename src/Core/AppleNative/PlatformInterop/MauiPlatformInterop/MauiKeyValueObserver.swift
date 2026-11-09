import Foundation
import QuartzCore
import UIKit

/// Observes a closed set of KVO-compliant properties and invokes a parameterless handler on each change.
///
/// This exists because an observer registered from managed code can be freed while it is still
/// registered: KVO does not retain observers, and the garbage collector releases an observer and the
/// object it observes in no particular order. Up to iOS 17 the registration keeps an unretained pointer
/// to the observer, so the next KVO notification on the observed object (UIView's dealloc sends one for
/// its layer's delegate) dereferences freed memory; from iOS 18 the pointer is a zeroing weak reference.
///
/// Here the registrations are owned by `NSKeyValueObservation` tokens, which unregister when they are
/// invalidated or deallocated, so a registration can never outlive its observer.
///
/// The handler is retained until `detach()` or deinit. A managed caller must therefore pass a handler
/// that does not keep this object reachable, otherwise the pair can never be collected.
@objc(MauiKeyValueObserver)
public class MauiKeyValueObserver: NSObject {

    private var observations: [NSKeyValueObservation] = []

    /// Invokes `handler` whenever the layer's `bounds` changes.
    /// (A layer's `frame` is derived and never notifies: CALayer raises KVO from its stored attributes only.)
    @objc(observeBoundsOfLayer:handler:)
    public func observeBounds(of layer: CALayer, handler: @escaping @Sendable () -> Void) {
        observations.append(layer.observe(\.bounds) { _, _ in handler() })
    }

    // UIKit properties are main-actor isolated, so their key paths can only be formed on the main actor.
    // Every caller is UI code on the main thread.

    /// Invokes `handler` whenever the view's `frame` changes.
    @MainActor
    @objc(observeFrameOfView:handler:)
    public func observeFrame(of view: UIView, handler: @escaping @Sendable () -> Void) {
        observations.append(view.observe(\.frame) { _, _ in handler() })
    }

    /// Invokes `handler` whenever the scroll view's `contentOffset` changes.
    @MainActor
    @objc(observeContentOffsetOfScrollView:handler:)
    public func observeContentOffset(of scrollView: UIScrollView, handler: @escaping @Sendable () -> Void) {
        observations.append(scrollView.observe(\.contentOffset) { _, _ in handler() })
    }

    /// Invokes `handler` whenever the window scene's `effectiveGeometry` changes.
    @available(iOS 16.0, macCatalyst 16.0, *)
    @MainActor
    @objc(observeEffectiveGeometryOfWindowScene:handler:)
    public func observeEffectiveGeometry(of windowScene: UIWindowScene, handler: @escaping @Sendable () -> Void) {
        observations.append(windowScene.observe(\.effectiveGeometry) { _, _ in handler() })
    }

    /// Stops every observation started by this instance and releases their handlers.
    @objc
    public func detach() {
        for observation in observations {
            observation.invalidate()
        }
        observations.removeAll()
    }

    deinit {
        detach()
    }
}
