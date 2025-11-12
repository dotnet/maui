
import Foundation
import FoundationModels

@objc(ChatRoleNative)
public enum ChatRoleNative: Int {
    case user = 1
    case assistant = 2
    case system = 3
}

@objc(ChatMessageNative)
public class ChatMessageNative: NSObject {
    @objc public var role: ChatRoleNative = .user
    @objc public var contents: [AIContentNative] = []
}
