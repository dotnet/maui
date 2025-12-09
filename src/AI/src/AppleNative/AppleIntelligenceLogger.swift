import Foundation
import FoundationModels

/// Type alias for the logging action block.
public typealias AppleIntelligenceLogAction = (String) -> Void

/// Singleton holder for the logger action.
@objc(AppleIntelligenceLogger)
public class AppleIntelligenceLogger: NSObject {
    /// The logging action. Set this to receive log callbacks.
    /// Example: AppleIntelligenceLogger.log = { message in print("[Native] \(message)") }
    @objc public static var log: AppleIntelligenceLogAction?
}
