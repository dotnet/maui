import Foundation
import FoundationModels

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
