import Foundation
import FoundationModels

@objc(ResponseUpdateTypeNative)
public enum ResponseUpdateTypeNative: Int, Sendable {
    case content = 0
    case toolCall = 1
    case toolResult = 2
}

@objc(ResponseUpdateNative)
public final class ResponseUpdateNative: NSObject, Sendable {
    @objc public let updateType: ResponseUpdateTypeNative
    @objc public let text: String?
    @objc public let toolCallId: String?
    @objc public let toolCallName: String?
    @objc public let toolCallArguments: String?
    @objc public let toolCallResult: String?

    @objc public init(
        updateType: ResponseUpdateTypeNative = .content,
        text: String? = nil,
        toolCallId: String? = nil,
        toolCallName: String? = nil,
        toolCallArguments: String? = nil,
        toolCallResult: String? = nil
    ) {
        self.updateType = updateType
        self.text = text
        self.toolCallId = toolCallId
        self.toolCallName = toolCallName
        self.toolCallArguments = toolCallArguments
        self.toolCallResult = toolCallResult
        super.init()
    }
}
