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
        _ tool: AIToolNative,
        _ onToolCall: (@Sendable (String, String, String) -> Void)?,
        _ onToolResult: (@Sendable (String, String, String) -> Void)?
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

        // Parse the JSON schema for output
        do {
            self.output =
                try JsonSchemaDecoder.parse(tool.outputSchema) ?? JsonSchemaDecoder.StringGenerationSchema
        } catch {
            self.output = JsonSchemaDecoder.StringGenerationSchema
        }
    }

    func call(arguments: Arguments) async throws -> Output {
        let argumentsJson = arguments.jsonString
        let callId = arguments.id.map { extractGuid(from: String(describing: $0)) } ?? UUID().uuidString

        // Notify that tool is being called
        onToolCall?(callId, name, argumentsJson)

        // Call the C# tool
        let resultJson: String = try await tool.call(arguments: argumentsJson)

        // Notify that tool execution completed
        onToolResult?(callId, name, resultJson)

        return try Output(json: resultJson)
    }

    /// Extracts a GUID from a string matching the exact format `GenerationID(value: "UUID")`.
    /// Returns the original string if the pattern doesn't match.
    private func extractGuid(from string: String) -> String {
        // Pattern matches exactly: GenerationID(value: "UUID")
        let pattern = #"^GenerationID\(value: "([0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12})"\)$"#
        
        guard let regex = try? NSRegularExpression(pattern: pattern),
              let match = regex.firstMatch(in: string, range: NSRange(string.startIndex..., in: string)),
              let guidRange = Range(match.range(at: 1), in: string) else {
            return string
        }
        
        return String(string[guidRange])
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
