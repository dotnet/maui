import Foundation
import FoundationModels

@objc(ChatClientError)
public enum ChatClientError: Int {
    case emptyMessages = 1
    case invalidRole = 2
    case invalidContent = 3
    case cancelled = 4
}

@objc(ChatClientNative)
public class ChatClientNative: NSObject {

    @objc public func streamResponse(
        messages: [ChatMessageNative],
        options: ChatOptionsNative?,
        onUpdate: @escaping (NSString) -> Void,
        onComplete: @escaping (NSString?, NSError?) -> Void
    ) -> CancellationTokenNative? {
        let cq = OperationQueue.current?.underlyingQueue

        return executeTask(messages, options, onComplete) { session, prompt, schema, genOptions in
            if let jsonSchema = schema {
                let responseStream = session.streamResponse(
                    to: prompt,
                    schema: jsonSchema,
                    includeSchemaInPrompt: false,
                    options: genOptions
                )

                for try await response in responseStream {
                    try Task.checkCancellation()
                    let text = response.content.jsonString
                    cq?.async { onUpdate(NSString(string: text)) } ?? onUpdate(NSString(string: text))
                }

                let response = try await responseStream.collect()
                return (response.content.jsonString, response.transcriptEntries)
            } else {
                let responseStream = session.streamResponse(
                    to: prompt,
                    options: genOptions
                )

                for try await response in responseStream {
                    try Task.checkCancellation()
                    let text = response.content
                    cq?.async { onUpdate(NSString(string: text)) } ?? onUpdate(NSString(string: text))
                }

                let response = try await responseStream.collect()
                return (response.content, response.transcriptEntries)
            }
        }

    }

    @objc public func getResponse(
        messages: [ChatMessageNative],
        options: ChatOptionsNative?,
        onComplete: @escaping (NSString?, NSError?) -> Void
    ) -> CancellationTokenNative? {
        return executeTask(messages, options, onComplete) { session, prompt, schema, genOptions in
            let response = try await {
                if let jsonSchema = schema {
                    let inner = try await session.respond(
                        to: prompt,
                        schema: jsonSchema,
                        includeSchemaInPrompt: false,
                        options: genOptions
                    )
                    return (inner.content.jsonString, inner.transcriptEntries)
                } else {
                    let inner = try await session.respond(
                        to: prompt,
                        options: genOptions
                    )
                    return (inner.content, inner.transcriptEntries)
                }
            }()

            return response
        }
    }

    private func toPrompt(message: ChatMessageNative) throws -> Prompt {
        guard message.role == .user else {
            throw NSError.chatError(
                .invalidRole,
                description: "Only user messages can be prompts. Found: \(message.role)"
            )
        }

        return try Prompt {
            try message.contents.map {
                switch $0 {
                case let textContent as TextContentNative:
                    return textContent.text
                default:
                    throw NSError.chatError(
                        .invalidContent,
                        description: "Unsupported content type in prompt. Found: \(type(of: $0))"
                    )
                }
            }
        }
    }

    private func toTranscriptEntry(message: ChatMessageNative) throws -> Transcript.Entry {
        switch message.role {
        case .user:
            return try .prompt(
                Transcript.Prompt(
                    segments: message.contents.map(self.toTranscriptSegment)
                )
            )
        case .assistant:
            return try .response(
                Transcript.Response(
                    assetIDs: [],
                    segments: message.contents.map(self.toTranscriptSegment)
                )
            )
        case .system:
            return try .instructions(
                Transcript.Instructions(
                    segments: message.contents.map(self.toTranscriptSegment),
                    toolDefinitions: []
                )
            )
        default:
            throw NSError.chatError(
                .invalidRole,
                description: "Unsupported role in transcript. Found: \(message.role)"
            )
        }
    }

    private func toTranscriptSegment(content: AIContentNative) throws -> Transcript.Segment {
        switch content {
        case let textContent as TextContentNative:
            return .text(Transcript.TextSegment(content: textContent.text))
        default:
            throw NSError.chatError(
                .invalidContent,
                description: "Unsupported content type in transcript. Found: \(type(of: content))"
            )
        }
    }

    private func prepareSession(messages: [ChatMessageNative], options: ChatOptionsNative?) async throws -> (
        session: LanguageModelSession, prompt: Prompt, schema: GenerationSchema?, genOptions: GenerationOptions
    ) {
        let lastMessage = messages.last!
        let otherMessages = messages.dropLast()

        let model = SystemLanguageModel.default
        let tools: [any Tool] = []
        let transcript = try Transcript(entries: otherMessages.map(self.toTranscriptEntry))
        let prompt = try self.toPrompt(message: lastMessage)

        // Parse the JSON schema from the options
        let schema: GenerationSchema? = try {
            if let jsonSchema = options?.responseJsonSchema {
                let decoder = JSONDecoder()
                let data = String(jsonSchema).data(using: .utf8)!
                let decoded = try decoder.decode(GenerationSchema.self, from: data)
                return decoded
            }
            return nil
        }()

        // Map options into GenerationOptions
        let genOptions = GenerationOptions(
            sampling: {
                if let topK = options?.topK?.intValue {
                    return .random(top: topK, seed: options?.seed?.uint64Value)
                }
                return nil
            }(),
            temperature: options?.temperature?.doubleValue,
            maximumResponseTokens: options?.maxOutputTokens?.intValue
        )

        let session = LanguageModelSession(
            model: model,
            tools: tools,
            transcript: transcript
        )

        return (session, prompt, schema, genOptions)
    }

    private func executeTask(
        _ messages: [ChatMessageNative],
        _ options: ChatOptionsNative?,
        _ onComplete: @escaping (NSString?, NSError?) -> Void,
        operation:
            @escaping (LanguageModelSession, Prompt, GenerationSchema?, GenerationOptions) async throws
            -> (String, ArraySlice<Transcript.Entry>)
    ) -> CancellationTokenNative? {

        let cq = OperationQueue.current?.underlyingQueue

        guard !messages.isEmpty else {
            let error = NSError.chatError(
                .emptyMessages,
                description: "No messages provided."
            )
            cq?.async { onComplete(nil, error.toNSError()) } ?? onComplete(nil, error.toNSError())
            return nil
        }

        let task = Task {
            do {
                try Task.checkCancellation()

                let (session, prompt, schema, genOptions) = try await self.prepareSession(
                    messages: messages,
                    options: options
                )

                let result = try await operation(session, prompt, schema, genOptions)

                try Task.checkCancellation()

                cq?.async { onComplete(NSString(string: result.0), nil) } ?? onComplete(NSString(string: result.0), nil)
            } catch {
                cq?.async { onComplete(nil, error.toNSError()) } ?? onComplete(nil, error.toNSError())
            }
        }

        return CancellationTokenNative(task: task)
    }
}

extension NSError {

    fileprivate static func chatError(_ code: ChatClientError, description: String) -> NSError {
        NSError(
            domain: "ChatClientNative",
            code: code.rawValue,
            userInfo: [NSLocalizedDescriptionKey: description]
        )
    }
}

extension Error {

    fileprivate func toNSError() -> NSError {
        switch self
        {
        case let error as LanguageModelSession.GenerationError:
            return NSError(
                domain: "ChatClientNative",
                code: 0,
                userInfo: [
                    NSUnderlyingErrorKey: error.errorDescription ?? "",
                    NSLocalizedRecoverySuggestionErrorKey: error.recoverySuggestion ?? "",
                    NSLocalizedFailureReasonErrorKey: error.failureReason ?? "",
                    NSLocalizedDescriptionKey: error.localizedDescription,
                ]
            )

        case let error as LanguageModelSession.ToolCallError:
            return NSError(
                domain: "ChatClientNative",
                code: 0,
                userInfo: [
                    NSUnderlyingErrorKey: error.errorDescription ?? "",
                    NSLocalizedRecoverySuggestionErrorKey: error.recoverySuggestion ?? "",
                    NSLocalizedFailureReasonErrorKey: error.failureReason ?? "",
                    NSLocalizedDescriptionKey: error.localizedDescription,
                ]
            )

        case let error as LocalizedError:
            return NSError(
                domain: "ChatClientNative",
                code: 0,
                userInfo: [
                    NSUnderlyingErrorKey: error.errorDescription ?? "",
                    NSLocalizedRecoverySuggestionErrorKey: error.recoverySuggestion ?? "",
                    NSLocalizedFailureReasonErrorKey: error.failureReason ?? "",
                    NSLocalizedDescriptionKey: error.localizedDescription,
                ]
            )

        case is CancellationError:
            return NSError.chatError(.cancelled, description: "Request was cancelled.")

        case let error as NSError:
            return error

        default:
            return NSError(
                domain: "ChatClientNative",
                code: 0,
                userInfo: [
                    NSLocalizedDescriptionKey: self.localizedDescription
                ]
            )
        }

    }

}
