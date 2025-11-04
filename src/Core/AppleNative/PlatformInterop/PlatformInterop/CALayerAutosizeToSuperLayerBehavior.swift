import Foundation
import QuartzCore

/// Behavior that automatically resizes a CALayer to match its superlayer's bounds
@objc(CALayerAutosizeToSuperLayerBehavior)
public class CALayerAutosizeToSuperLayerBehavior: NSObject {
    
    private var observation: NSKeyValueObservation?
    
    /// Attaches this behavior to the given layer
    /// The layer must have a superlayer when this method is called
    /// The layer's frame will be kept in sync with the superlayer's bounds.
    /// - Parameter layer: The layer that needs to be resized to match the superlayer's bounds.
    @objc
    public func attach(layer: CALayer) {
        // Detach from any previous layer
        detach()
        
        guard let superLayer = layer.superlayer else {
            fatalError("SuperLayer must be set before attaching CALayerAutosizeToSuperLayerBehavior")
        }
        
        // Set initial frame to match superlayer bounds
        layer.frame = superLayer.bounds
        
        // Observe superlayer's bounds using modern block-based KVO
        // Note: CALayer is not Sendable in Swift 6, which triggers a compiler warning.
        // This is safe because:
        // 1. KVO change handlers for UI objects are delivered on the main thread
        // 2. The weak capture prevents retain cycles
        // 3. CALayer operations are inherently main-thread bound
        observation = superLayer.observe(\.bounds, options: .new) { [weak layer] observedLayer, change in
            guard let layer = layer else { return }
            layer.frame = observedLayer.bounds
        }
    }
    
    /// Detaches this behavior from the current layer, stopping the automatic resizing.
    @objc
    public func detach() {
        observation?.invalidate()
        observation = nil
    }
    
    deinit {
        detach()
    }
}

