import Foundation
import FoundationModels

@objc(AIContentNative)
public class AIContentNative: NSObject {}

@objc(TextContentNative)
public class TextContentNative: AIContentNative {
    @objc public init(text: String) {
        self.text = text
    }
    @objc public var text: String
}
