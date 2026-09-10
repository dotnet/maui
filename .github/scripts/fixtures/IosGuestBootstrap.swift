import AppKit
import CryptoKit
import Foundation
import ScreenCaptureKit
import Virtualization

private let gibibyte: UInt64 = 1_073_741_824

private struct Arguments {
	let outputDirectory: URL
	let sourceVersion: String
	let buildID: Int

	static func parse(_ values: [String]) throws -> Arguments {
		guard values.count == 7,
			values[1] == "--output-directory",
			values[3] == "--source-version",
			values[5] == "--build-id"
		else {
			throw BootstrapFailure("invalid-arguments", "Expected --output-directory PATH --source-version 40hex --build-id positiveinteger.")
		}

		let output = URL(fileURLWithPath: values[2], isDirectory: true).standardizedFileURL
		guard output.path.hasPrefix("/") else {
			throw BootstrapFailure("invalid-output-directory", "The output directory must be an absolute path.")
		}
		let commit = values[4]
		guard commit.utf8.count == 40, commit.utf8.allSatisfy({
			(48...57).contains($0) || (65...70).contains($0) || (97...102).contains($0)
		}) else {
			throw BootstrapFailure("invalid-source-version", "The source version must be exactly 40 hexadecimal characters.")
		}
		guard let buildID = Int(values[6]), buildID > 0 else {
			throw BootstrapFailure("invalid-build-id", "The build ID must be a positive integer.")
		}
		return Arguments(outputDirectory: output, sourceVersion: commit.lowercased(), buildID: buildID)
	}
}

private struct BootstrapFailure: LocalizedError {
	let reasonCode: String
	let message: String

	init(_ reasonCode: String, _ message: String) {
		self.reasonCode = reasonCode
		self.message = String(message.prefix(2_048))
	}

	var errorDescription: String? { message }
}

private final class CompletionGate<T>: @unchecked Sendable {
	private let lock = NSLock()
	private var continuation: CheckedContinuation<T, Error>?

	init(_ continuation: CheckedContinuation<T, Error>) {
		self.continuation = continuation
	}

	func finish(_ result: Result<T, Error>) {
		lock.lock()
		guard let continuation else {
			lock.unlock()
			return
		}
		self.continuation = nil
		lock.unlock()
		continuation.resume(with: result)
	}
}

private func awaitResult<T>(
	timeout: TimeInterval,
	timeoutCode: String,
	_ start: (@escaping (Result<T, Error>) -> Void) -> Void
) async throws -> T {
	try await withCheckedThrowingContinuation { continuation in
		let gate = CompletionGate(continuation)
		DispatchQueue.main.asyncAfter(deadline: .now() + timeout) {
			gate.finish(.failure(BootstrapFailure(timeoutCode, "The operation exceeded its bounded timeout.")))
		}
		start { gate.finish($0) }
	}
}

private struct DownloadResult {
	let bytes: UInt64
	let sha256: String
}

private final class BoundedDownloader: NSObject, URLSessionDataDelegate {
	private let maximumBytes = 32 * gibibyte
	private let delegateQueue: OperationQueue = {
		let queue = OperationQueue()
		queue.name = "IosGuestBootstrap.Download"
		queue.maxConcurrentOperationCount = 1
		return queue
	}()
	private var continuation: CheckedContinuation<DownloadResult, Error>?
	private var session: URLSession?
	private var task: URLSessionDataTask?
	private var file: FileHandle?
	private var destination: URL?
	private var received: UInt64 = 0
	private var expected: UInt64?
	private var nextProgressPercent = 10
	private var nextUnknownProgress = 2 * gibibyte
	private var redirectCount = 0
	private var hasher = SHA256()
	private var timeoutItem: DispatchWorkItem?

	static func validateAppleRestoreURL(_ url: URL) throws {
		guard url.scheme?.lowercased() == "https",
			url.user == nil, url.password == nil,
			url.port == nil || url.port == 443,
			let host = url.host?.lowercased(),
			host == "updates.cdn-apple.com" || host.hasSuffix(".cdn-apple.com")
		else {
			throw BootstrapFailure("restore-url-rejected", "The restore image URL is not an authenticated Apple CDN HTTPS URL.")
		}
	}

	func download(from source: URL, to destination: URL) async throws -> DownloadResult {
		try Self.validateAppleRestoreURL(source)
		guard !FileManager.default.fileExists(atPath: destination.path) else {
			throw BootstrapFailure("download-file-not-fresh", "The restore-image destination already exists.")
		}
		guard FileManager.default.createFile(atPath: destination.path, contents: nil) else {
			throw BootstrapFailure("download-file-create-failed", "Could not create the bounded restore-image destination.")
		}

		self.destination = destination
		file = try FileHandle(forWritingTo: destination)
		let configuration = URLSessionConfiguration.ephemeral
		configuration.timeoutIntervalForRequest = 120
		configuration.timeoutIntervalForResource = 45 * 60
		configuration.requestCachePolicy = .reloadIgnoringLocalAndRemoteCacheData
		configuration.urlCache = nil
		configuration.urlCredentialStorage = nil
		configuration.httpCookieStorage = nil
		configuration.httpShouldSetCookies = false

		return try await withCheckedThrowingContinuation { continuation in
			self.continuation = continuation
			let session = URLSession(configuration: configuration, delegate: self, delegateQueue: delegateQueue)
			self.session = session
			var request = URLRequest(url: source, cachePolicy: .reloadIgnoringLocalAndRemoteCacheData, timeoutInterval: 120)
			request.httpMethod = "GET"
			request.setValue("identity", forHTTPHeaderField: "Accept-Encoding")
			let task = session.dataTask(with: request)
			self.task = task
			let timeout = DispatchWorkItem { [weak self] in
				self?.delegateQueue.addOperation {
					self?.fail(BootstrapFailure("restore-download-timeout", "Restore image download exceeded 45 minutes."))
				}
			}
			timeoutItem = timeout
			DispatchQueue.global(qos: .utility).asyncAfter(deadline: .now() + 45 * 60, execute: timeout)
			task.resume()
		}
	}

	func urlSession(
		_ session: URLSession,
		task: URLSessionTask,
		willPerformHTTPRedirection response: HTTPURLResponse,
		newRequest request: URLRequest,
		completionHandler: @escaping (URLRequest?) -> Void
	) {
		do {
			redirectCount += 1
			guard redirectCount <= 5, let url = request.url else {
				throw BootstrapFailure("restore-redirect-rejected", "The restore image exceeded the redirect limit.")
			}
			try Self.validateAppleRestoreURL(url)
			completionHandler(request)
		} catch {
			completionHandler(nil)
			fail(error)
		}
	}

	func urlSession(
		_ session: URLSession,
		dataTask: URLSessionDataTask,
		didReceive response: URLResponse,
		completionHandler: @escaping (URLSession.ResponseDisposition) -> Void
	) {
		do {
			guard let http = response as? HTTPURLResponse, http.statusCode == 200, let url = http.url else {
				throw BootstrapFailure("restore-response-rejected", "The restore image response was not HTTP 200.")
			}
			try Self.validateAppleRestoreURL(url)
			if response.expectedContentLength > 0 {
				let length = UInt64(response.expectedContentLength)
				guard length <= maximumBytes else {
					throw BootstrapFailure("restore-image-too-large", "The restore image exceeds the 32 GiB limit.")
				}
				expected = length
			}
			completionHandler(.allow)
		} catch {
			completionHandler(.cancel)
			fail(error)
		}
	}

	func urlSession(_ session: URLSession, dataTask: URLSessionDataTask, didReceive data: Data) {
		guard continuation != nil else { return }
		do {
			let newTotal = received + UInt64(data.count)
			guard newTotal <= maximumBytes else {
				throw BootstrapFailure("restore-image-too-large", "The streamed restore image exceeded the 32 GiB limit.")
			}
			guard let file else {
				throw BootstrapFailure("download-file-closed", "The active download has no open destination.")
			}
			try file.write(contentsOf: data)
			hasher.update(data: data)
			received = newTotal
			if let expected, expected > 0 {
				let percent = Int((received * 100) / expected)
				if percent >= nextProgressPercent {
					print("Restore download progress: \(min(percent, 100))%")
					nextProgressPercent += 10
				}
			} else if received >= nextUnknownProgress {
				print("Restore download progress: \(received / gibibyte) GiB")
				nextUnknownProgress += 2 * gibibyte
			}
		} catch {
			fail(error)
		}
	}

	func urlSession(_ session: URLSession, task: URLSessionTask, didCompleteWithError error: Error?) {
		guard continuation != nil else { return }
		if let error {
			fail(BootstrapFailure("restore-download-failed", error.localizedDescription))
			return
		}
		do {
			try file?.close()
			file = nil
			guard received > 0, expected == nil || expected == received else {
				throw BootstrapFailure("restore-download-truncated", "The restore image byte count did not match the response.")
			}
			let digest = hasher.finalize().map { String(format: "%02x", $0) }.joined()
			finish(.success(DownloadResult(bytes: received, sha256: digest)))
		} catch {
			fail(BootstrapFailure("restore-download-close-failed", error.localizedDescription))
		}
	}

	private func fail(_ error: Error) {
		guard continuation != nil else { return }
		task?.cancel()
		var cleanupErrors: [String] = []
		do {
			try file?.close()
		} catch {
			cleanupErrors.append("close: \(error.localizedDescription)")
		}
		file = nil
		if let destination {
			do {
				try FileManager.default.removeItem(at: destination)
			} catch {
				cleanupErrors.append("remove: \(error.localizedDescription)")
			}
		}
		if !cleanupErrors.isEmpty {
			finish(.failure(BootstrapFailure("restore-download-cleanup-failed",
				"\(error.localizedDescription); \(cleanupErrors.joined(separator: "; "))")))
		} else {
			finish(.failure(error))
		}
	}

	private func finish(_ result: Result<DownloadResult, Error>) {
		guard let continuation else { return }
		self.continuation = nil
		timeoutItem?.cancel()
		session?.finishTasksAndInvalidate()
		continuation.resume(with: result)
	}
}

@MainActor
private final class BootstrapController: NSObject, NSApplicationDelegate {
	private let arguments: Arguments
	private let workDirectory: URL
	private let launchedAt = Date()
	private let operationalLimit: TimeInterval = 73 * 60
	private let hardLimit: TimeInterval = 75 * 60
	private var window: NSWindow!
	private var virtualMachineView: VZVirtualMachineView!
	private var virtualMachine: VZVirtualMachine?
	private var restoreURL: String?
	private var restoreVersion: String?
	private var restoreBuild: String?
	private var restoreHash: String?
	private var screenshotFiles: [String] = []
	private var guestInstalled = false
	private var guestStarted = false
	private var completing = false
	var exitCode: Int32 = 1

	init(arguments: Arguments) throws {
		self.arguments = arguments
		workDirectory = arguments.outputDirectory.appendingPathComponent("work", isDirectory: true)
		super.init()
		try prepareOutputDirectory()
		try writeResult(outcome: "in-progress", reasonCode: "in-progress", diagnostic: "Bootstrap has not completed.")
		do {
			try createFreshWorkDirectory()
		} catch {
			let failure = error as? BootstrapFailure ??
				BootstrapFailure("work-directory-create-failed", error.localizedDescription)
			do {
				try writeResult(outcome: "failed", reasonCode: failure.reasonCode, diagnostic: failure.message)
			} catch {
				fputs("Failed to write initialization result: \(error.localizedDescription)\n", stderr)
			}
			throw failure
		}
	}

	func applicationDidFinishLaunching(_ notification: Notification) {
		do {
			guard NSApp.setActivationPolicy(.regular) else {
				throw BootstrapFailure("gui-unavailable", "NSApplication could not enter regular GUI activation policy.")
			}
			let frame = NSRect(x: 0, y: 0, width: 1280, height: 800)
			virtualMachineView = VZVirtualMachineView(frame: frame)
			virtualMachineView.capturesSystemKeys = true
			window = NSWindow(contentRect: frame, styleMask: [.titled], backing: .buffered, defer: false)
			window.title = "Trusted iOS Guest Bootstrap - macOS Setup Evidence"
			window.contentView = virtualMachineView
			window.isReleasedWhenClosed = false
			window.center()
			window.orderFrontRegardless()
			NSApp.activate(ignoringOtherApps: true)
			scheduleDeadlines()
			Task { await run() }
		} catch {
			Task { await finishFailure(error) }
		}
	}

	private func run() async {
		do {
			try await boundedSleep(seconds: 1)
			try await captureOwnedWindow(named: "preflight-window.png")
			try checkResources()

			let image: VZMacOSRestoreImage = try await awaitResult(timeout: 120, timeoutCode: "restore-fetch-timeout") {
				VZMacOSRestoreImage.fetchLatestSupported(completionHandler: $0)
			}
			try BoundedDownloader.validateAppleRestoreURL(image.url)
			guard let requirements = image.mostFeaturefulSupportedConfiguration else {
				throw BootstrapFailure("restore-image-unsupported", "The latest restore image has no supported configuration on this host.")
			}
			restoreURL = image.url.absoluteString
			restoreBuild = image.buildVersion
			let version = image.operatingSystemVersion
			restoreVersion = "\(version.majorVersion).\(version.minorVersion).\(version.patchVersion)"
			try writeRestoreImage(bytes: nil)

			let localRestore = workDirectory.appendingPathComponent("restore.ipsw")
			let download: DownloadResult
			do {
				download = try await BoundedDownloader().download(from: image.url, to: localRestore)
			} catch {
				throw staged("restore-download-failed", error)
			}
			restoreHash = download.sha256
			try writeRestoreImage(bytes: download.bytes)

			let configuration = try makeConfiguration(requirements: requirements)
			try configuration.validate()
			try writeGuestConfiguration(configuration)
			let vm = VZVirtualMachine(configuration: configuration)
			virtualMachine = vm
			virtualMachineView.virtualMachine = vm

			let installer = VZMacOSInstaller(virtualMachine: vm, restoringFromImageAt: localRestore)
			do {
				let _: Void = try await awaitResult(
					timeout: remainingOperationalTime(),
					timeoutCode: "guest-install-timeout"
				) { installer.install(completionHandler: $0) }
			} catch {
				throw staged("guest-install-failed", error)
			}
			guestInstalled = true

			do {
				let _: Void = try await awaitResult(timeout: min(300, remainingOperationalTime()), timeoutCode: "guest-start-timeout") {
					vm.start(completionHandler: $0)
				}
			} catch {
				throw staged("guest-start-failed", error)
			}
			guestStarted = true

			try await boundedSleep(seconds: 60)
			for index in 1...3 {
				guard vm.state == .running else {
					throw BootstrapFailure("guest-stopped-unexpectedly", "The guest was not running when visual evidence was captured.")
				}
				try await captureOwnedWindow(named: String(format: "guest-window-%02d.png", index))
				if index < 3 {
					try await boundedSleep(seconds: 15)
				}
			}
			await finishSuccess()
		} catch {
			await finishFailure(error)
		}
	}

	private func makeConfiguration(requirements: VZMacOSConfigurationRequirements) throws -> VZVirtualMachineConfiguration {
		let hardwareModel = requirements.hardwareModel
		let machineIdentifier = VZMacMachineIdentifier()
		let hardwareURL = workDirectory.appendingPathComponent("hardware-model.bin")
		let machineURL = workDirectory.appendingPathComponent("machine-identifier.bin")
		let auxiliaryURL = workDirectory.appendingPathComponent("auxiliary-storage.bin")
		try hardwareModel.dataRepresentation.write(to: hardwareURL, options: .withoutOverwriting)
		try machineIdentifier.dataRepresentation.write(to: machineURL, options: .withoutOverwriting)
		let auxiliary = try VZMacAuxiliaryStorage(
			creatingStorageAt: auxiliaryURL,
			hardwareModel: hardwareModel,
			options: []
		)

		let diskURL = workDirectory.appendingPathComponent("guest-disk.img")
		guard FileManager.default.createFile(atPath: diskURL.path, contents: nil) else {
			throw BootstrapFailure("guest-disk-create-failed", "Could not create the guest disk.")
		}
		let diskFile = try FileHandle(forWritingTo: diskURL)
		try diskFile.truncate(atOffset: 96 * gibibyte)
		try diskFile.close()

		let maximumCPU = VZVirtualMachineConfiguration.maximumAllowedCPUCount
		let cpuCount = min(4, maximumCPU)
		guard cpuCount >= VZVirtualMachineConfiguration.minimumAllowedCPUCount,
			cpuCount >= requirements.minimumSupportedCPUCount else {
			throw BootstrapFailure("insufficient-cpu", "The bounded four-CPU configuration does not satisfy the restore image requirements.")
		}
		let requiredMemory = max(
			8 * gibibyte,
			max(requirements.minimumSupportedMemorySize, VZVirtualMachineConfiguration.minimumAllowedMemorySize)
		)
		guard requiredMemory <= VZVirtualMachineConfiguration.maximumAllowedMemorySize,
			ProcessInfo.processInfo.physicalMemory >= requiredMemory + (2 * gibibyte) else {
			throw BootstrapFailure("insufficient-memory", "The host cannot safely provide the required guest memory plus 2 GiB host reserve.")
		}

		let platform = VZMacPlatformConfiguration()
		platform.hardwareModel = hardwareModel
		platform.machineIdentifier = machineIdentifier
		platform.auxiliaryStorage = auxiliary
		let graphics = VZMacGraphicsDeviceConfiguration()
		graphics.displays = [VZMacGraphicsDisplayConfiguration(widthInPixels: 1280, heightInPixels: 800, pixelsPerInch: 80)]
		let configuration = VZVirtualMachineConfiguration()
		configuration.bootLoader = VZMacOSBootLoader()
		configuration.platform = platform
		configuration.cpuCount = cpuCount
		configuration.memorySize = requiredMemory
		configuration.graphicsDevices = [graphics]
		configuration.keyboards = [VZUSBKeyboardConfiguration()]
		configuration.pointingDevices = [VZUSBScreenCoordinatePointingDeviceConfiguration()]
		configuration.entropyDevices = [VZVirtioEntropyDeviceConfiguration()]
		configuration.storageDevices = [
			VZVirtioBlockDeviceConfiguration(
				attachment: try VZDiskImageStorageDeviceAttachment(url: diskURL, readOnly: false)
			)
		]
		configuration.networkDevices = []
		configuration.directorySharingDevices = []
		configuration.socketDevices = []
		return configuration
	}

	private func captureOwnedWindow(named name: String) async throws {
		guard screenshotFiles.count < 6, window.isVisible, window.windowNumber > 0 else {
			throw BootstrapFailure("owned-window-unavailable", "The owned VM window is not visible or the screenshot limit was reached.")
		}
		do {
			let content = try await SCShareableContent.currentProcess
			guard let ownedWindow = content.windows.first(where: { $0.windowID == CGWindowID(window.windowNumber) }) else {
				throw BootstrapFailure("owned-window-unavailable", "ScreenCaptureKit current-process content did not contain the owned VM window.")
			}
			let filter = SCContentFilter(desktopIndependentWindow: ownedWindow)
			let capture = SCStreamConfiguration()
			capture.width = 1280
			capture.height = 800
			capture.showsCursor = false
			capture.capturesAudio = false
			let image = try await SCScreenshotManager.captureImage(contentFilter: filter, configuration: capture)
			let bitmap = NSBitmapImageRep(cgImage: image)
			guard let png = bitmap.representation(using: .png, properties: [:]) else {
				throw BootstrapFailure("screenshot-encoding-failed", "Could not encode the owned VM window as PNG.")
			}
			try png.write(to: arguments.outputDirectory.appendingPathComponent(name), options: .withoutOverwriting)
			screenshotFiles.append(name)
		} catch let error as BootstrapFailure {
			throw error
		} catch {
			throw BootstrapFailure("screen-capture-failed", error.localizedDescription)
		}
	}

	private func stopVirtualMachine() async throws {
		guard let vm = virtualMachine else { return }
		for _ in 0..<30 where vm.state != .stopped && !vm.canStop {
			try await Task.sleep(nanoseconds: 1_000_000_000)
		}
		guard vm.state != .stopped else { return }
		guard vm.canStop else {
			throw BootstrapFailure("guest-shutdown-unavailable", "The virtual machine is not in a stoppable state.")
		}
		do {
			let _: Void = try await awaitResult(timeout: 90, timeoutCode: "guest-shutdown-timeout") {
				let completion = $0
				vm.stop { error in
					if let error {
						completion(.failure(error))
					} else {
						completion(.success(()))
					}
					guard vm.state == .stopped else {
						throw BootstrapFailure("guest-shutdown-incomplete", "The stop callback did not leave the guest stopped.")
					}
				}
			}
		} catch {
			throw staged("guest-shutdown-failed", error)
		}
	}

	private func finishSuccess() async {
		guard !completing else { return }
		completing = true
		do {
			try await stopVirtualMachine()
			try writeResult(
				outcome: "succeeded",
				reasonCode: "guest-restored-booted-visual-evidence",
				diagnostic: "The macOS guest was restored and booted, and its owned VM window was captured for manual inspection. This does not assert Setup Assistant or account readiness and prepares no controller, iOS fix, or generated execution."
			)
			complete(exitCode: 0)
		} catch {
			completing = false
			await finishFailure(error)
		}
	}

	private func finishFailure(_ error: Error) async {
		guard !completing else { return }
		completing = true
		var failure = error as? BootstrapFailure ??
			BootstrapFailure("unexpected-error", error.localizedDescription)
		do {
			try await stopVirtualMachine()
		} catch {
			failure = BootstrapFailure(
				"guest-shutdown-failed",
				"\(failure.reasonCode): \(failure.message); shutdown: \(error.localizedDescription)"
			)
		}
		do {
			try writeResult(outcome: "failed", reasonCode: failure.reasonCode, diagnostic: failure.message)
		} catch {
			fputs("Failed to write result.json: \(error.localizedDescription)\n", stderr)
		}
		complete(exitCode: 1)
	}

	private func prepareOutputDirectory() throws {
		let manager = FileManager.default
		try manager.createDirectory(at: arguments.outputDirectory, withIntermediateDirectories: true)
		let values = try arguments.outputDirectory.resourceValues(forKeys: [.isSymbolicLinkKey])
		guard values.isSymbolicLink != true else {
			throw BootstrapFailure("output-directory-symlink", "The output directory must not be a symbolic link.")
		}
	}

	private func createFreshWorkDirectory() throws {
		let manager = FileManager.default
		guard !manager.fileExists(atPath: workDirectory.path) else {
			throw BootstrapFailure("work-directory-not-fresh", "The bootstrap-owned output/work directory already exists.")
		}
		try manager.createDirectory(at: workDirectory, withIntermediateDirectories: false)
	}

	private func checkResources() throws {
		let keys: Set<URLResourceKey> = [.volumeAvailableCapacityForImportantUsageKey, .volumeAvailableCapacityKey]
		let values = try arguments.outputDirectory.resourceValues(forKeys: keys)
		let available = values.volumeAvailableCapacityForImportantUsage ?? Int64(values.volumeAvailableCapacity ?? 0)
		guard available >= Int64(64 * gibibyte) else {
			throw BootstrapFailure("insufficient-disk-space", "At least 64 GiB of actual free disk space is required.")
		}
	}

	private func writeRestoreImage(bytes: UInt64?) throws {
		var document: [String: Any] = [
			"schemaVersion": 1,
			"url": restoreURL ?? "",
			"version": restoreVersion ?? "",
			"build": restoreBuild ?? "",
			"sha256": restoreHash ?? ""
		]
		if let bytes { document["bytes"] = bytes }
		try writeJSON(document, named: "restore-image.json")
	}

	private func writeGuestConfiguration(_ configuration: VZVirtualMachineConfiguration) throws {
		try writeJSON([
			"schemaVersion": 1,
			"cpuCount": configuration.cpuCount,
			"memoryBytes": configuration.memorySize,
			"diskCapacityBytes": 96 * gibibyte,
			"displayWidth": 1280,
			"displayHeight": 800,
			"hardwareModel": "work/hardware-model.bin",
			"machineIdentifier": "work/machine-identifier.bin",
			"auxiliaryStorage": "work/auxiliary-storage.bin",
			"diskImage": "work/guest-disk.img",
			"networkDeviceCount": 0,
			"directoryShareCount": 0,
			"socketDeviceCount": 0
		], named: "guest-configuration.json")
	}

	private func writeResult(outcome: String, reasonCode: String, diagnostic: String) throws {
		let document: [String: Any] = [
			"schemaVersion": 1,
			"mode": "ios-guest-bootstrap",
			"pipelineCommit": arguments.sourceVersion,
			"buildId": arguments.buildID,
			"guestInstalled": guestInstalled,
			"guestStarted": guestStarted,
			"guestStopped": virtualMachine?.state == .stopped,
			"outcome": outcome,
			"reasonCode": reasonCode,
			"exitCode": outcome == "succeeded" ? 0 : (outcome == "failed" ? 1 : -1),
			"exitDiagnostic": String(diagnostic.prefix(2_048)),
			"successScope": "restore-and-boot-with-owned-window-screenshots-only",
			"screenshots": screenshotFiles,
			"networkDeviceCount": 0,
			"directoryShareCount": 0,
			"socketDeviceCount": 0,
			"restoreImageURL": restoreURL ?? "",
			"restoreImageVersion": restoreVersion ?? "",
			"restoreImageBuild": restoreBuild ?? "",
			"restoreImageSHA256": restoreHash ?? "",
			"certifiesIssue": false,
			"enforcesEgress": false,
			"allowsGeneratedExecution": false
		]
		try writeJSON(document, named: "result.json")
	}

	private func writeJSON(_ object: [String: Any], named name: String) throws {
		let data = try JSONSerialization.data(withJSONObject: object, options: [.prettyPrinted, .sortedKeys])
		try data.write(to: arguments.outputDirectory.appendingPathComponent(name), options: .atomic)
	}

	private func boundedSleep(seconds: TimeInterval) async throws {
		guard try remainingOperationalTime() > seconds else {
			throw BootstrapFailure("absolute-timeout", "The 73-minute operational deadline was reached.")
		}
		try await Task.sleep(nanoseconds: UInt64(seconds * 1_000_000_000))
	}

	private func remainingOperationalTime() throws -> TimeInterval {
		let remaining = operationalLimit - Date().timeIntervalSince(launchedAt)
		guard remaining > 1 else {
			throw BootstrapFailure("absolute-timeout", "The 73-minute operational deadline was reached.")
		}
		return remaining
	}

	private func staged(_ code: String, _ error: Error) -> BootstrapFailure {
		if let failure = error as? BootstrapFailure {
			return failure
		}
		return BootstrapFailure(code, error.localizedDescription)
	}

	private func scheduleDeadlines() {
		DispatchQueue.main.asyncAfter(deadline: .now() + operationalLimit) { [weak self] in
			guard let self, !self.completing else { return }
			Task { await self.finishFailure(BootstrapFailure("absolute-timeout", "The 73-minute operational deadline was reached.")) }
		}
		DispatchQueue.main.asyncAfter(deadline: .now() + hardLimit) { [weak self] in
			guard let self else { return }
			do {
				try self.writeResult(
					outcome: "failed",
					reasonCode: "absolute-hard-timeout",
					diagnostic: "The 75-minute absolute limit was reached; the bounded shutdown path did not complete."
				)
			} catch {
				fputs("Failed to write deadline result: \(error.localizedDescription)\n", stderr)
			}
			self.complete(exitCode: 1)
		}
	}

	private func complete(exitCode: Int32) {
		self.exitCode = exitCode
		NSApp.stop(nil)
		if let event = NSEvent.otherEvent(
			with: .applicationDefined,
			location: .zero,
			modifierFlags: [],
			timestamp: 0,
			windowNumber: 0,
			context: nil,
			subtype: 0,
			data1: 0,
			data2: 0
		) {
			NSApp.postEvent(event, atStart: false)
		}
	}
}

@main
private struct IosGuestBootstrap {
	@MainActor
	static func main() {
		do {
			let arguments = try Arguments.parse(CommandLine.arguments)
			let controller = try BootstrapController(arguments: arguments)
			let application = NSApplication.shared
			application.delegate = controller
			application.run()
			exit(controller.exitCode)
		} catch {
			fputs("IosGuestBootstrap: \(error.localizedDescription)\n", stderr)
			exit(2)
		}
	}
}
