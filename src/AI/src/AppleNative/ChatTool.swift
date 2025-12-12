import Foundation

@objc(AIToolNative)
public protocol AIToolNative : Sendable {
    @objc var name: String { get }
    @objc var desc: String { get }
    @objc var argumentsSchema: String { get }
    @objc var outputSchema: String { get }
    @objc func call(arguments: String, completion: @escaping (String) -> Void)
}
