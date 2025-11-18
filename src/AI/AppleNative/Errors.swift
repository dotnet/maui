
import Foundation
import FoundationModels

extension NSError {
    static func chatError(_ code: ChatClientError, description: String) -> NSError {
        NSError(
            domain: "ChatClientNative",
            code: code.rawValue,
            userInfo: [NSLocalizedDescriptionKey: description]
        )
    }
}

@objc(ChatClientError)
public enum ChatClientError: Int {
    case emptyMessages = 1
    case invalidRole = 2
    case invalidContent = 3
    case cancelled = 4
}
