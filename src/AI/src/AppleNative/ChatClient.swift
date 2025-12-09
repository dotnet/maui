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

    // MARK: - Stream Response

    @objc public func streamResponse(
        messages: [ChatMessageNative],
        options: ChatOptionsNative?,
        onUpdate: @escaping (ResponseUpdateNative) -> Void,
        onComplete: @escaping (ChatResponseNative?, NSError?) -> Void
    ) -> CancellationTokenNative? {

        let methodName = "streamResponse"
        let cq = OperationQueue.current?.underlyingQueue

        if let log = AppleIntelligenceLogger.log {
            log("[\(methodName)] Invoked with \(messages.count) messages")
            log("[\(methodName)] Messages: \(formatMessagesDetailed(messages))")
            if let opts = options {
                log("[\(methodName)] Options: \(formatOptionsDetailed(opts))")
            }
        }

        let toolWatcher =
            options?.tools == nil
            ? nil
            : ToolCallWatcher(
                onToolCall: { id, name, arguments in
                    if let log = AppleIntelligenceLogger.log {
                        log("[\(methodName)] Tool invoking: \(name) (id=\(id)) with arguments: \(arguments)")
                    }

                    let update = ResponseUpdateNative(toolCallId: id, toolCallName: name, toolCallArguments: arguments)
                    cq?.async { onUpdate(update) } ?? onUpdate(update)
                },
                onToolResult: { id, name, result in
                    if let log = AppleIntelligenceLogger.log {
                        log("[\(methodName)] Tool completed: \(name) (id=\(id)) with result: \(result)")
                    }

                    let update = ResponseUpdateNative(toolCallId: id, toolCallName: name, toolCallResult: result)
                    cq?.async { onUpdate(update) } ?? onUpdate(update)
                }
            )

        return executeTask(methodName, messages, options, toolWatcher, onComplete) { session, prompt, schema, genOptions in

            if let jsonSchema = schema {
                if let log = AppleIntelligenceLogger.log {
                    log("[\(methodName)] Starting schema-based stream response")
                }

                let responseStream = session.streamResponse(to: prompt, schema: jsonSchema, includeSchemaInPrompt: false, options: genOptions)

                for try await response in responseStream {
                    try Task.checkCancellation()
                    let text = response.content.jsonString
                    if let log = AppleIntelligenceLogger.log {
                        log("[\(methodName)] Streaming update: \(text)")
                    }
                    let update = ResponseUpdateNative(text: text)
                    cq?.async { onUpdate(update) } ?? onUpdate(update)
                }

                let response = try await responseStream.collect()
                if let log = AppleIntelligenceLogger.log {
                    log("[\(methodName)] Stream collected, content length: \(response.content.jsonString.count)")
                }
                return (response.content.jsonString, response.transcriptEntries)
            } else {
                if let log = AppleIntelligenceLogger.log {
                    log("[\(methodName)] Starting text-based stream response")
                }

                let responseStream = session.streamResponse(to: prompt, options: genOptions)

                for try await response in responseStream {
                    try Task.checkCancellation()
                    let text = response.content
                    if let log = AppleIntelligenceLogger.log {
                        log("[\(methodName)] Streaming update: \(text)")
                    }
                    let update = ResponseUpdateNative(text: text)
                    cq?.async { onUpdate(update) } ?? onUpdate(update)
                }

                let response = try await responseStream.collect()
                if let log = AppleIntelligenceLogger.log {
                    log("[\(methodName)] Stream collected, content length: \(response.content.count)")
                }
                return (response.content, response.transcriptEntries)
            }
        }

    }

    // MARK: - Get Response

    @objc public func getResponse(
        messages: [ChatMessageNative],
        options: ChatOptionsNative?,
        onUpdate: @escaping (ResponseUpdateNative) -> Void,
        onComplete: @escaping (ChatResponseNative?, NSError?) -> Void
    ) -> CancellationTokenNative? {

        let methodName = "getResponse"
        let cq = OperationQueue.current?.underlyingQueue

        if let log = AppleIntelligenceLogger.log {
            log("[\(methodName)] Invoked with \(messages.count) messages")
            log("[\(methodName)] Messages: \(formatMessagesDetailed(messages))")
            if let opts = options {
                log("[\(methodName)] Options: \(formatOptionsDetailed(opts))")
            }
        }

        let toolWatcher =
            options?.tools == nil
            ? nil
            : ToolCallWatcher(
                onToolCall: { id, name, arguments in
                    if let log = AppleIntelligenceLogger.log {
                        log("[\(methodName)] Tool invoking: \(name) (id=\(id)) with arguments: \(arguments)")
                    }

                    let update = ResponseUpdateNative(toolCallId: id, toolCallName: name, toolCallArguments: arguments)
                    cq?.async { onUpdate(update) } ?? onUpdate(update)
                },
                onToolResult: { id, name, result in
                    if let log = AppleIntelligenceLogger.log {
                        log("[\(methodName)] Tool completed: \(name) (id=\(id)) with result: \(result)")
                    }

                    let update = ResponseUpdateNative(toolCallId: id, toolCallName: name, toolCallResult: result)
                    cq?.async { onUpdate(update) } ?? onUpdate(update)
                }
            )

        return executeTask(methodName, messages, options, toolWatcher, onComplete) { session, prompt, schema, genOptions in

            if let log = AppleIntelligenceLogger.log {
                log("[\(methodName)] \(schema != nil ? "Getting schema-based response" : "Getting text-based response")")
            }

            let response = try await {
                if let jsonSchema = schema {
                    let inner = try await session.respond(to: prompt, schema: jsonSchema, includeSchemaInPrompt: false, options: genOptions)
                    if let log = AppleIntelligenceLogger.log {
                        log("[\(methodName)] Response received, content length: \(inner.content.jsonString.count)")
                    }
                    return (inner.content.jsonString, inner.transcriptEntries)
                } else {
                    let inner = try await session.respond(to: prompt, options: genOptions)
                    if let log = AppleIntelligenceLogger.log {
                        log("[\(methodName)] Response received, content length: \(inner.content.count)")
                    }
                    return (inner.content, inner.transcriptEntries)
                }
            }()

            return response
        }
    }

    // MARK: - Session Helpers

    private func prepareSession(
        _ methodName: String,
        _ messages: [ChatMessageNative],
        _ options: ChatOptionsNative?,
        _ toolWatcher: ToolCallWatcher?
    ) async throws -> (
        session: LanguageModelSession,
        prompt: Prompt,
        schema: GenerationSchema?,
        genOptions: GenerationOptions
    ) {

        if let log = AppleIntelligenceLogger.log {
            log("[\(methodName)] Preparing session with \(messages.count) messages, hasTools=\(options?.tools != nil)")
        }

        let lastMessage = messages.last!
        let otherMessages = messages.dropLast()

        let model = SystemLanguageModel.default
        let tools = options?.tools?.map { ToolNative($0, toolWatcher?.notifyToolCall, toolWatcher?.notifyToolResult) } ?? []

        if let log = AppleIntelligenceLogger.log, let toolList = options?.tools {
            for tool in toolList {
                log("[\(methodName)] Tool registered: \(tool.name) - \(tool.desc)")
                log("[\(methodName)] Tool \(tool.name) argumentsSchema: \(tool.argumentsSchema)")
                log("[\(methodName)] Tool \(tool.name) outputSchema: \(tool.outputSchema)")
            }
        }

        let transcript = try Transcript(entries: otherMessages.map(self.toTranscriptEntry))
        let prompt = try self.toPrompt(message: lastMessage)

        // Parse the JSON schema from the options
        let schema: GenerationSchema? = try {
            if let jsonSchema = options?.responseJsonSchema {
                if let log = AppleIntelligenceLogger.log {
                    log("[\(methodName)] Parsing JSON schema for structured output: \(jsonSchema)")
                }

                let parsed = try JsonSchemaDecoder.parse(String(jsonSchema))
                return parsed
            }
            return nil
        }()

        // Map options into GenerationOptions
        let genOptions = GenerationOptions(
            sampling: {
                if let topK = options?.topK?.intValue {
                    return .random(top: topK, seed: options?.seed?.uint64Value)
                }
                return .greedy
            }(),
            temperature: options?.temperature?.doubleValue,
            maximumResponseTokens: options?.maxOutputTokens?.intValue
        )

        let session = LanguageModelSession(model: model, tools: tools, transcript: transcript)

        if let log = AppleIntelligenceLogger.log {
            log("[\(methodName)] Session ready, hasSchema=\(schema != nil)")
        }

        return (session, prompt, schema, genOptions)
    }

    private func executeTask(
        _ methodName: String,
        _ messages: [ChatMessageNative],
        _ options: ChatOptionsNative?,
        _ toolWatcher: ToolCallWatcher?,
        _ onComplete: @escaping (ChatResponseNative?, NSError?) -> Void,
        operation:
            @escaping (LanguageModelSession, Prompt, GenerationSchema?, GenerationOptions) async throws
            -> (String, ArraySlice<Transcript.Entry>)
    ) -> CancellationTokenNative? {

        let cq = OperationQueue.current?.underlyingQueue

        guard !messages.isEmpty else {
            let error = NSError.chatError(.emptyMessages, description: "No messages provided.")
            if let log = AppleIntelligenceLogger.log {
                log("[\(methodName)] Failed: No messages provided")
            }

            cq?.async { onComplete(nil, error.toNSError()) } ?? onComplete(nil, error.toNSError())
            return nil
        }

        let task = Task {
            do {
                try Task.checkCancellation()

                let (session, prompt, schema, genOptions) = try await self.prepareSession(methodName, messages, options, toolWatcher)

                let result = try await operation(session, prompt, schema, genOptions)

                try Task.checkCancellation()

                // Convert transcript entries to messages
                let transcriptMessages = try result.1.compactMap(self.fromTranscriptEntry)

                // Create response with all transcript messages
                let response = ChatResponseNative(messages: transcriptMessages)

                if let log = AppleIntelligenceLogger.log {
                    log("[\(methodName)] Completed with \(transcriptMessages.count) messages")
                    log("[\(methodName)] Response: \(self.formatResponseDetailed(response))")
                }

                cq?.async { onComplete(response, nil) } ?? onComplete(response, nil)
            } catch {
                if let log = AppleIntelligenceLogger.log {
                    log("[\(methodName)] Failed: \(error.localizedDescription)")
                }

                cq?.async { onComplete(nil, error.toNSError()) } ?? onComplete(nil, error.toNSError())
            }
        }

        return CancellationTokenNative(task: task)
    }

    // MARK: - Conversion to Foundation Models Helpers

    private func toPrompt(message: ChatMessageNative) throws -> Prompt {
        guard message.role == .user else {
            throw NSError.chatError(.invalidRole, description: "Only user messages can be prompts. Found: \(message.role)")
        }

        return try Prompt {
            try message.contents.map {
                switch $0 {
                case let textContent as TextContentNative:
                    return textContent.text
                default:
                    throw NSError.chatError(.invalidContent, description: "Unsupported content type in prompt. Found: \(type(of: $0))")
                }
            }
        }
    }

    private func toTranscriptEntry(message: ChatMessageNative) throws -> Transcript.Entry {
        switch message.role {
        case .user:
            return try .prompt(Transcript.Prompt(segments: message.contents.map(self.toTranscriptSegment)))
        case .assistant:
            return try .response(Transcript.Response(assetIDs: [], segments: message.contents.map(self.toTranscriptSegment)))
        case .system:
            return try .instructions(Transcript.Instructions(segments: message.contents.map(self.toTranscriptSegment), toolDefinitions: []))
        default:
            throw NSError.chatError(.invalidRole, description: "Unsupported role in transcript. Found: \(message.role)")
        }
    }

    private func toTranscriptSegment(content: AIContentNative) throws -> Transcript.Segment {
        switch content {
        case let textContent as TextContentNative:
            return .text(Transcript.TextSegment(content: textContent.text))
        default:
            throw NSError.chatError(.invalidContent, description: "Unsupported content type in transcript. Found: \(type(of: content))")
        }
    }

    // MARK: - Conversion to Essentials AI Helpers

    private func fromTranscriptEntry(_ entry: Transcript.Entry) throws -> ChatMessageNative? {
        switch entry {
        case .prompt(let prompt):
            let message = ChatMessageNative()
            message.role = .user
            message.contents = prompt.segments.compactMap(fromTranscriptSegment)
            return message

        case .response(let response):
            let message = ChatMessageNative()
            message.role = .assistant
            message.contents = response.segments.compactMap(fromTranscriptSegment)
            return message

        case .instructions(let instructions):
            let message = ChatMessageNative()
            message.role = .system
            message.contents = instructions.segments.compactMap(fromTranscriptSegment)
            return message

        case .toolCalls(let toolCalls):
            let message = ChatMessageNative()
            message.role = .assistant
            message.contents = toolCalls.map(fromToolCall)
            return message

        case .toolOutput(let toolOutput):
            let message = ChatMessageNative()
            message.role = .tool
            message.contents = fromToolOutput(toolOutput)
            return message

        @unknown default:
            return nil
        }
    }

    private func fromToolCall(_ toolCall: Transcript.ToolCall) -> AIContentNative {
        let argsJson = toolCall.arguments.jsonString
        return FunctionCallContentNative(callId: toolCall.id, name: toolCall.toolName, arguments: argsJson)
    }

    private func fromToolOutput(_ toolOutput: Transcript.ToolOutput) -> [AIContentNative] {
        return toolOutput.segments.compactMap { segment -> AIContentNative? in
            let resultText: String
            switch segment {
            case .text(let textSegment):
                resultText = textSegment.content
            case .structure(let structuredSegment):
                resultText = structuredSegment.content.jsonString
            @unknown default:
                return nil
            }

            return FunctionResultContentNative(callId: toolOutput.id, result: resultText)
        }
    }

    private func fromTranscriptSegment(_ segment: Transcript.Segment) -> AIContentNative? {
        switch segment {
        case .text(let textSegment):
            return TextContentNative(text: textSegment.content)

        case .structure(let structuredSegment):
            // For now, convert structured content to text
            let jsonString = structuredSegment.content.jsonString
            return TextContentNative(text: jsonString)

        @unknown default:
            return nil
        }
    }

    // MARK: - Logging Helpers

    private func formatMessagesDetailed(_ messages: [ChatMessageNative]) -> String {
        let formatted = messages.map { message -> String in
            let role = "\(message.role)"
            let contents = message.contents.map { content -> String in
                switch content {
                case let text as TextContentNative:
                    return "TextContent(text=\"\(text.text)\")"
                case let funcCall as FunctionCallContentNative:
                    return "FunctionCallContent(name=\"\(funcCall.name)\", callId=\"\(funcCall.callId)\", arguments=\(funcCall.arguments))"
                case let funcResult as FunctionResultContentNative:
                    return "FunctionResultContent(callId=\"\(funcResult.callId)\", result=\"\(funcResult.result)\")"
                default:
                    return "UnknownContent(\(type(of: content)))"
                }
            }.joined(separator: ", ")
            return "Message(role=\(role), contents=[\(contents)])"
        }.joined(separator: ", ")
        return "[\(formatted)]"
    }

    private func formatOptionsDetailed(_ options: ChatOptionsNative) -> String {
        var parts: [String] = []
        if let topK = options.topK { parts.append("topK=\(topK)") }
        if let temp = options.temperature { parts.append("temperature=\(temp)") }
        if let maxTokens = options.maxOutputTokens { parts.append("maxOutputTokens=\(maxTokens)") }
        if let seed = options.seed { parts.append("seed=\(seed)") }
        if let schema = options.responseJsonSchema { parts.append("responseJsonSchema=\(schema)") }
        if let tools = options.tools {
            let toolNames = tools.map { "\($0.name)" }.joined(separator: ", ")
            parts.append("tools=[\(toolNames)]")
        }
        return "Options(\(parts.joined(separator: ", ")))"
    }

    private func formatResponseDetailed(_ response: ChatResponseNative) -> String {
        let messagesStr = response.messages.map { message -> String in
            let role = "\(message.role)"
            let contents = message.contents.map { content -> String in
                switch content {
                case let text as TextContentNative:
                    return "TextContent(text=\"\(text.text)\")"
                case let funcCall as FunctionCallContentNative:
                    return "FunctionCallContent(name=\"\(funcCall.name)\", callId=\"\(funcCall.callId)\", arguments=\(funcCall.arguments))"
                case let funcResult as FunctionResultContentNative:
                    return "FunctionResultContent(callId=\"\(funcResult.callId)\", result=\"\(funcResult.result)\")"
                default:
                    return "UnknownContent(\(type(of: content)))"
                }
            }.joined(separator: ", ")
            return "Message(role=\(role), contents=[\(contents)])"
        }.joined(separator: ", ")
        return "ChatResponse(messages=[\(messagesStr)])"
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
