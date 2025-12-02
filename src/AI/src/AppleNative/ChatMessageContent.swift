import Foundation
import FoundationModels

@objc(AIContentNative)
public class AIContentNative: NSObject {}

@objc(TextContentNative)
public class TextContentNative: AIContentNative {
    @objc public init(text: String) {
        self.text = text
    }
    @objc public var text: String
}

@objc(FunctionCallContentNative)
public class FunctionCallContentNative: AIContentNative {
    @objc public var callId: String
    @objc public var name: String
    @objc public var arguments: String  // JSON string
    
    @objc public init(callId: String, name: String, arguments: String) {
        self.callId = callId
        self.name = name
        self.arguments = arguments
        super.init()
    }
}

@objc(FunctionResultContentNative)
public class FunctionResultContentNative: AIContentNative {
    @objc public var callId: String
    @objc public var result: String
    
    @objc public init(callId: String, result: String) {
        self.callId = callId
        self.result = result
        super.init()
    }
}
