import Foundation

final class ToolCallWatcher: Sendable {
    let onToolCall: (@Sendable (String, String, String) -> Void)
    let onToolResult: (@Sendable (String, String, String) -> Void)

    init(
        onToolCall: @escaping (@Sendable (String, String, String) -> Void),
        onToolResult: @escaping (@Sendable (String, String, String) -> Void)
    ) {
        self.onToolCall = onToolCall
        self.onToolResult = onToolResult
    }

    func notifyToolCall(id: String, name: String, arguments: String) {
        onToolCall(id, name, arguments)
    }

    func notifyToolResult(id: String, name: String, result: String) {
        onToolResult(id, name, result)
    }
}
