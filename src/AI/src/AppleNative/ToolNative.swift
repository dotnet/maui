import Foundation
import FoundationModels

/// Swift wrapper that makes an AIToolBase conform to FoundationModels.Tool protocol
final class ToolNative: Tool {
    let tool: AIToolNative
    let onToolCall: (@Sendable (String, String, String) -> Void)?  // id, name, arguments
    let onToolResult: (@Sendable (String, String, String) -> Void)?  // id, name, result
    let name: String
    let description: String
    let parameters: GenerationSchema
    let output: GenerationSchema

    init(
        tool: AIToolNative,
        onToolCall: (@Sendable (String, String, String) -> Void)?,
        onToolResult: (@Sendable (String, String, String) -> Void)?
    ) {
        self.tool = tool
        self.onToolCall = onToolCall
        self.onToolResult = onToolResult

        self.name = tool.name
        self.description = tool.desc

        // Parse the JSON schema for parameters
        do {
            self.parameters =
                try JsonSchemaDecoder.parse(tool.argumentsSchema) ?? JsonSchemaDecoder.StringGenerationSchema
        } catch {
            self.parameters = JsonSchemaDecoder.StringGenerationSchema
        }

        // Output is always a string
        self.output = JsonSchemaDecoder.StringGenerationSchema
    }

    func call(arguments: Arguments) async throws -> Output {
        let argumentsJson = arguments.jsonString
        let callId = arguments.id.map { String(describing: $0) } ?? UUID().uuidString

        // Notify that tool is being called
        onToolCall?(callId, name, argumentsJson)

        // Call the C# tool
        let resultJson: String = await withCheckedContinuation { continuation in
            tool.call(arguments: argumentsJson) { result in
                continuation.resume(returning: String(result))
            }
        }

        // Notify that tool execution completed
        onToolResult?(callId, name, resultJson)

        return try Output(json: resultJson)
    }

    struct Arguments: ConvertibleFromGeneratedContent {
        let id: GenerationID?
        let jsonString: String
        let generatedContent: GeneratedContent

        init(_ content: GeneratedContent) throws {
            self.id = content.id
            self.jsonString = content.jsonString
            self.generatedContent = content
        }
    }

    struct Output: ConvertibleToGeneratedContent {
        let promptRepresentation: Prompt
        let generatedContent: GeneratedContent

        init(json: String) throws {
            self.promptRepresentation = .init({ json })

            // Try to parse as JSON, otherwise use as plain text
            do {
                self.generatedContent = try .init(json: json)
            } catch {
                self.generatedContent = .init(json)
            }
        }
    }
}
