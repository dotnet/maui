import Foundation
import FoundationModels

// MARK: - Errors

@objc(ChatClientError)
public enum ChatClientError: Int {
    case emptyMessages = 1
    case invalidRole = 2
    case invalidContent = 3
    case cancelled = 4
}

extension NSError {
    static func chatError(_ code: ChatClientError, description: String) -> NSError {
        NSError(
            domain: "ChatClientNative",
            code: code.rawValue,
            userInfo: [NSLocalizedDescriptionKey: description]
        )
    }
}

// MARK: - Options

@objc(ChatOptionsNative)
public class ChatOptionsNative: NSObject {
    @objc public var topK: NSNumber? = nil
    @objc public var seed: NSNumber? = nil
    @objc public var temperature: NSNumber? = nil
    @objc public var maxOutputTokens: NSNumber? = nil
}

// MARK: - Messages

@objc(ChatMessageNative)
public class ChatMessageNative: NSObject {
    @objc public var role: ChatRoleNative = .user
    @objc public var contents: [AIContentNative] = []
}

@objc(AIContentNative)
public class AIContentNative: NSObject {}

@objc(TextContentNative)
public class TextContentNative: AIContentNative {
    @objc public init(text: String) {
        self.text = text
    }
    @objc public var text: String
}

@objc(ChatRoleNative)
public enum ChatRoleNative: Int {
    case user
    case assistant
    case system
}

// MARK: - Cancellation

@objc(CancellationTokenNative)
public class CancellationTokenNative: NSObject {
    private var task: Task<Void, Never>

    init(task: Task<Void, Never>) {
        self.task = task
    }

    @objc public func cancel() {
        task.cancel()
    }

    @objc public var isCancelled: Bool {
        task.isCancelled
    }
}

// MARK: - Client

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
                description: "No messages provided"
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

                let response = try await session.respond(
                    to: prompt,
                    options: genOptions
                )

                try Task.checkCancellation()

                let resp = response.content
                callerQueue?.async { onComplete(NSString(string: resp), nil) }
                    ?? onComplete(NSString(string: resp), nil)
            } catch is CancellationError {
                let error = NSError.chatError(
                    .cancelled,
                    description: "Request was cancelled"
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
                description: "Only user messages can be prompts"
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
                        description: "Unsupported content type in prompt"
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
        default:
            throw NSError.chatError(
                .invalidRole,
                description: "Unsupported role in transcript"
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
                description: "Unsupported content type in transcript"
            )
        }
    }
}
