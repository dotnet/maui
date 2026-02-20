import Foundation
import FoundationModels

@objc(ChatResponseNative)
public class ChatResponseNative: NSObject, @unchecked Sendable {
    @objc public var messages: [ChatMessageNative]
    
    @objc public init(messages: [ChatMessageNative]) {
        self.messages = messages
        super.init()
    }
}
