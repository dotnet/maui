import Foundation
import FoundationModels

@objc(ChatClientNative)
public class ChatClientNative: NSObject {

    @objc public func getResponse(
        messages: [ChatMessageNative],
        options: ChatOptionsNative?,
        onComplete: @escaping (NSObject?, NSError?) -> Void
    ) -> CancellationTokenNative? {
        guard !messages.isEmpty else {
            let error = NSError.chatError(
                .emptyMessages,
                description: "No messages provided."
            )
            onComplete(nil, error)
            return nil
        }

        let lastMessage = messages.last!
        let otherMessages = messages.dropLast()

        let callerQueue = OperationQueue.current?.underlyingQueue

        let task = Task {
            do {
                try Task.checkCancellation()

                let model = SystemLanguageModel.default
                let tools: [any Tool] = []
                let transcript = try Transcript(
                    entries: otherMessages.map(self.toTranscriptEntry)
                )
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

                let response: (text: String, transcript: ArraySlice<Transcript.Entry>) = try await {
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

                try Task.checkCancellation()

                let resp = response.text
                callerQueue?.async { onComplete(NSString(string: resp), nil) }
                    ?? onComplete(NSString(string: resp), nil)
            } catch is CancellationError {
                let error = NSError.chatError(
                    .cancelled,
                    description: "Request was cancelled."
                )
                callerQueue?.async { onComplete(nil, error) } ?? onComplete(nil, error)
            } catch let error as NSError {
                callerQueue?.async { onComplete(nil, error) } ?? onComplete(nil, error)
            }
        }

        return CancellationTokenNative(task: task)
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
}
