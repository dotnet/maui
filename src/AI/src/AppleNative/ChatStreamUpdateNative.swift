import Foundation
import FoundationModels

@objc(StreamUpdateNative)
public final class StreamUpdateNative: NSObject, Sendable {
    @objc public let text: String?
    @objc public let toolCallId: String?
    @objc public let toolCallName: String?
    @objc public let toolCallArguments: String?
    @objc public let toolCallResult: String?

    @objc public init(
        text: String? = nil,
        toolCallId: String? = nil,
        toolCallName: String? = nil,
        toolCallArguments: String? = nil,
        toolCallResult: String? = nil
    ) {
        self.text = text
        self.toolCallId = toolCallId
        self.toolCallName = toolCallName
        self.toolCallArguments = toolCallArguments
        self.toolCallResult = toolCallResult
        super.init()
    }
}
