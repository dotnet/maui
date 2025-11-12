
import Foundation
import FoundationModels

@objc(ChatOptionsNative)
public class ChatOptionsNative: NSObject {
    @objc public var topK: NSNumber? = nil
    @objc public var seed: NSNumber? = nil
    @objc public var temperature: NSNumber? = nil
    @objc public var maxOutputTokens: NSNumber? = nil
    @objc public var responseJsonSchema: NSString? = nil
}
