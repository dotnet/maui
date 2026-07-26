#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Microsoft.Maui.Storage;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.Media;

enum PendingMediaPickerState
{
	Pending,
	ResultAccepted
}

/// <summary>
/// Persists Android MediaPicker activity-result state so selected or captured files can be recovered
/// if the app process is recreated before the live API call can return.
/// </summary>
internal static class MediaPickerRecoveryManager
{
	internal const int MaxRecoveredResultCount = 32;

	static readonly Lock Locker = new();
	static readonly HashSet<string> InProcessOperationIds = new(StringComparer.Ordinal);
	static readonly List<MediaPickerRecoveryWaiter> RecoveryWaiters = [];
	static readonly SemaphoreSlim RecoveryPromotionSemaphore = new(1, 1);
	static Func<AndroidUri, bool> PersistPickerUriReadAccessHandler = PersistPickerUriReadAccessCore;
	static Action<AndroidUri> ReleasePickerUriReadAccessHandler = ReleasePickerUriReadAccessCore;
	static Action? BeginOperationWithRecoveryCheckpointHandler;
	// Lets waiters detect an empty recovery outcome that happened before they could be registered.
	static long RecoveryReconciliationGeneration;

	internal static PendingMediaPickerOperation BeginOperation(
		RecoveredMediaPickerResultKind kind,
		IReadOnlyList<string> filePaths,
		PersistedPhotoProcessingOptions photoProcessingOptions)
	{
		ValidateBeginOperationArguments(kind, filePaths);

		// Persist the one active MediaPicker operation before launching AndroidX. Later AndroidX
		// callbacks are matched back to this durable record, including after process recreation.
		lock (Locker)
		{
			if (MediaPickerRecoveryStore.ReadActiveOperation() is { } activeOperation)
			{
				ThrowIfActiveOperationBlocksNewOperation(activeOperation);
			}

			return BeginOperationUnderLock(kind, filePaths, photoProcessingOptions);
		}
	}

	internal static async Task<PendingMediaPickerOperation> BeginOperationWithRecoveryAsync(
		RecoveredMediaPickerResultKind kind,
		IReadOnlyList<string> filePaths,
		PersistedPhotoProcessingOptions photoProcessingOptions)
	{
		ValidateBeginOperationArguments(kind, filePaths);

		await RecoveryPromotionSemaphore.WaitAsync().ConfigureAwait(false);

		try
		{
			// Retry once for a callback that records an accepted recreated result after the first recovery pass.
			for (var attempt = 0; attempt < 2; attempt++)
			{
				var reconciliation = await RecoverOperationIfAvailableUnderSemaphoreAsync().ConfigureAwait(false);
				if (reconciliation.WasReconciled)
				{
					CompleteRecoveryWaitersForReconciliation(reconciliation.Results);
				}

				BeginOperationWithRecoveryCheckpointHandler?.Invoke();

				lock (Locker)
				{
					var activeOperation = MediaPickerRecoveryStore.ReadActiveOperation();
					if (activeOperation is null)
					{
						return BeginOperationUnderLock(kind, filePaths, photoProcessingOptions);
					}

					if (!ShouldPromoteRecreatedOperation(activeOperation) || attempt == 1)
					{
						ThrowIfActiveOperationBlocksNewOperation(activeOperation);
					}
				}
			}

			throw new InvalidOperationException("A MediaPicker result is pending recovery.");
		}
		finally
		{
			RecoveryPromotionSemaphore.Release();
		}
	}

	internal static void ClearActiveOperation(string id)
	{
		if (id is null)
		{
			return;
		}

		IReadOnlyList<string> pickerUriStringsToRelease = [];

		lock (Locker)
		{
			var operation = MediaPickerRecoveryStore.ReadActiveOperation();
			if (operation?.Id == id)
			{
				pickerUriStringsToRelease = ClearActiveOperationUnderLock(operation);
			}
		}

		ReleasePickerUriReadAccess(pickerUriStringsToRelease);
	}

	internal static async Task<IReadOnlyList<RecoveredMediaPickerResult>> GetRecoveredResultsAsync()
	{
		var reconciliation = await RecoverOperationIfAvailableCoreAsync().ConfigureAwait(false);
		return reconciliation.Results;
	}

	internal static async Task<IReadOnlyList<RecoveredMediaPickerResult>> WaitForRecoveredResultsAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!cancellationToken.CanBeCanceled)
		{
			throw new ArgumentException(
				"A cancellable token is required when waiting for MediaPicker recovery.",
				nameof(cancellationToken));
		}

		var observedReconciliationGeneration = GetRecoveryReconciliationGeneration();
		var reconciliation = await RecoverOperationIfAvailableCoreAsync(cancellationToken).ConfigureAwait(false);
		if (reconciliation.WasReconciled || reconciliation.Results.Count > 0)
		{
			return reconciliation.Results;
		}

		var waiter = new MediaPickerRecoveryWaiter(cancellationToken);

		lock (Locker)
		{
			var results = ReadPublicRecoveredResultsUnderLock();
			if (results.Count > 0)
			{
				return results;
			}

			if (observedReconciliationGeneration != RecoveryReconciliationGeneration)
			{
				return results;
			}

			if (cancellationToken.IsCancellationRequested)
			{
				cancellationToken.ThrowIfCancellationRequested();
			}

			RecoveryWaiters.Add(waiter);
		}

		waiter.SetCancellationRegistration(cancellationToken.Register(static state =>
			CancelRecoveryWaiter((MediaPickerRecoveryWaiter)state!), waiter));

		if (cancellationToken.IsCancellationRequested)
		{
			CancelRecoveryWaiter(waiter);
		}

		return await waiter.Task.ConfigureAwait(false);
	}

	internal static Task ClearRecoveredResultAsync(string id)
	{
		lock (Locker)
		{
			var results = MediaPickerRecoveryStore.ReadRecoveredResults();
			var removed = results.RemoveAll(result => string.Equals(result.Id, id, StringComparison.Ordinal)) > 0;

			if (removed)
			{
				MediaPickerRecoveryStore.WriteRecoveredResults(results);
			}
		}

		return Task.CompletedTask;
	}

	internal static void SetPickerUriPermissionHandlersForTests(
		Func<AndroidUri, bool>? persistHandler,
		Action<AndroidUri>? releaseHandler)
	{
		PersistPickerUriReadAccessHandler = persistHandler ?? PersistPickerUriReadAccessCore;
		ReleasePickerUriReadAccessHandler = releaseHandler ?? ReleasePickerUriReadAccessCore;
	}

	internal static void SetBeginOperationWithRecoveryCheckpointForTests(Action? checkpointHandler)
		=> BeginOperationWithRecoveryCheckpointHandler = checkpointHandler;

	internal static async Task DiscardPendingOperationAsync()
	{
		IReadOnlyList<RecoveredMediaPickerResult>? waiterResults = null;
		IReadOnlyList<string> pickerUriStringsToRelease = [];

		await RecoveryPromotionSemaphore.WaitAsync().ConfigureAwait(false);

		try
		{
			lock (Locker)
			{
				var operation = MediaPickerRecoveryStore.ReadActiveOperation();
				if (operation is null)
				{
					return;
				}

				if (IsInProcessOperation(operation))
				{
					throw new InvalidOperationException("A MediaPicker operation is already in progress.");
				}

				pickerUriStringsToRelease = ClearActiveOperationUnderLock(operation);
				waiterResults = ReadPublicRecoveredResultsUnderLock();
			}

			ReleasePickerUriReadAccess(pickerUriStringsToRelease);

			if (waiterResults is not null)
			{
				CompleteRecoveryWaitersForReconciliation(waiterResults);
			}
		}
		finally
		{
			RecoveryPromotionSemaphore.Release();
		}
	}

	internal static void RecoverOrphanedCaptureResult(RecoveredMediaPickerResultKind kind, bool success)
		=> ObserveOrphanedRecoveryTask(
			RecoverOrphanedCaptureResultAsync(kind, success),
			"Unhandled media capture recovery task failure");

	internal static bool RecordCaptureCallbackResult(RecoveredMediaPickerResultKind kind, bool success)
	{
		if (!IsCaptureKind(kind))
		{
			return false;
		}

		lock (Locker)
		{
			var operation = MediaPickerRecoveryStore.ReadActiveOperation();

			if (operation is null || operation.Kind != kind)
			{
				return false;
			}

			var outputPath = operation.FilePaths.Count == 1 ? operation.FilePaths[0] : null;
			if (!success || !IsFileAvailable(outputPath))
			{
				_ = ClearActiveOperationUnderLock(operation);
				return true;
			}

			// AndroidX accepted the capture result. Persist that fact before completing any live task so
			// process death after this callback still leaves the recoverable state behind.
			MediaPickerRecoveryStore.WriteActiveOperation(operation.WithState(PendingMediaPickerState.ResultAccepted));
			return true;
		}
	}

	// AndroidX calls this when it delivers a capture result, but the original launch task is not active in this process.
	internal static async Task RecoverOrphanedCaptureResultAsync(RecoveredMediaPickerResultKind kind, bool success)
		=> await RecoverOrphanedOperationResultAsync(
			() => RecordCaptureCallbackResult(kind, success),
			"Unable to recover media capture result").ConfigureAwait(false);

	internal static bool RecordSinglePickCallbackResult(AndroidUri? uri)
		=> RecordPickCallbackResult(
			operation => IsSinglePickCallbackKind(operation.Kind),
			uri is null || uri.Equals(AndroidUri.Empty) ? Array.Empty<AndroidUri>() : new[] { uri });

	internal static bool RecordMultiplePickCallbackResult(IReadOnlyList<AndroidUri>? uris)
		=> RecordPickCallbackResult(
			operation => IsMultiplePickCallbackKind(operation.Kind),
			uris ?? []);

	internal static void RecoverOrphanedSinglePickResult(AndroidUri? uri)
		=> ObserveOrphanedRecoveryTask(
			RecoverOrphanedSinglePickResultAsync(uri),
			"Unhandled picked media recovery task failure");

	internal static async Task RecoverOrphanedSinglePickResultAsync(AndroidUri? uri)
		=> await RecoverOrphanedOperationResultAsync(
			() => RecordSinglePickCallbackResult(uri),
			"Unable to recover picked media result").ConfigureAwait(false);

	internal static void RecoverOrphanedMultiplePickResult(IReadOnlyList<AndroidUri>? uris)
		=> ObserveOrphanedRecoveryTask(
			RecoverOrphanedMultiplePickResultAsync(uris),
			"Unhandled picked media recovery task failure");

	internal static async Task RecoverOrphanedMultiplePickResultAsync(IReadOnlyList<AndroidUri>? uris)
		=> await RecoverOrphanedOperationResultAsync(
			() => RecordMultiplePickCallbackResult(uris),
			"Unable to recover picked media results").ConfigureAwait(false);

	static void ObserveOrphanedRecoveryTask(Task task, string failureMessage)
	{
		_ = task.ContinueWith(
			static (faultedTask, state) =>
			{
				Trace.WriteLine($"{state}: {faultedTask.Exception}");
			},
			failureMessage,
			CancellationToken.None,
			TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
			TaskScheduler.Default);
	}

	internal static async Task<IReadOnlyList<RecoveredMediaPickerResult>> RecoverOperationIfAvailableAsync()
	{
		var reconciliation = await RecoverOperationIfAvailableCoreAsync().ConfigureAwait(false);
		return reconciliation.Results;
	}

	// Promotes a recreated-process operation only after AndroidX has accepted the result.
	// A Pending record means the result callback has not been replayed yet.
	static async Task<MediaPickerRecoveryReconciliation> RecoverOperationIfAvailableCoreAsync(CancellationToken cancellationToken = default)
	{
		await RecoveryPromotionSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

		try
		{
			var reconciliation = await RecoverOperationIfAvailableUnderSemaphoreAsync().ConfigureAwait(false);
			if (reconciliation.WasReconciled)
			{
				CompleteRecoveryWaitersForReconciliation(reconciliation.Results);
			}

			return reconciliation;
		}
		finally
		{
			RecoveryPromotionSemaphore.Release();
		}
	}

	static async Task<MediaPickerRecoveryReconciliation> RecoverOperationIfAvailableUnderSemaphoreAsync()
	{
		PendingMediaPickerOperation? operation;

		lock (Locker)
		{
			operation = MediaPickerRecoveryStore.ReadActiveOperation();
			if (operation is null || !ShouldPromoteRecreatedOperation(operation))
			{
				return new MediaPickerRecoveryReconciliation(ReadPublicRecoveredResultsUnderLock(), false);
			}
		}

		if (!HasAcceptedResultPayload(operation))
		{
			ClearActiveOperation(operation.Id);
			return new MediaPickerRecoveryReconciliation(ReadPublicRecoveredResults(), true);
		}

		await PublishRecoveredOperationAsync(operation).ConfigureAwait(false);
		return new MediaPickerRecoveryReconciliation(ReadPublicRecoveredResults(), true);
	}

	static bool RecordPickCallbackResult(
		Func<PendingMediaPickerOperation, bool> matchesOperation,
		IReadOnlyList<AndroidUri> uris)
	{
		PendingMediaPickerOperation? operation;

		lock (Locker)
		{
			operation = MediaPickerRecoveryStore.ReadActiveOperation();
			if (operation is null || !matchesOperation(operation))
			{
				return false;
			}
		}

		if (uris.Count == 0)
		{
			ClearActiveOperation(operation.Id);
			return true;
		}

		var uriStrings = GetPickerUriStrings(uris);
		if (uriStrings.Count == 0)
		{
			ClearActiveOperation(operation.Id);
			return true;
		}

		var persistedUriStrings = new List<string>();
		foreach (var uri in uris)
		{
			if (TryPersistPickerUriReadAccess(uri) && uri is not null)
			{
				var uriString = uri.ToString();
				if (!string.IsNullOrWhiteSpace(uriString))
				{
					persistedUriStrings.Add(uriString);
				}
			}
		}

		var callbackRecorded = false;
		try
		{
			lock (Locker)
			{
				var current = MediaPickerRecoveryStore.ReadActiveOperation();
				if (current?.Id == operation.Id)
				{
					// AndroidX accepted the picker result. Take durable URI access first, then persist the
					// URI payload before copying from it so process death during materialization can be retried.
					MediaPickerRecoveryStore.WriteActiveOperation(current.WithAcceptedPickerUris(uriStrings));
					callbackRecorded = true;
				}
			}
		}
		catch
		{
			ReleasePickerUriReadAccess(persistedUriStrings);
			throw;
		}

		if (!callbackRecorded)
		{
			ReleasePickerUriReadAccess(persistedUriStrings);
			return false;
		}

		return true;
	}

	internal static async Task<IReadOnlyList<string>> MaterializeAcceptedFilePathsAsync(string id, bool throwOnMaterializationFailure)
	{
		if (id is null)
		{
			return [];
		}

		PendingMediaPickerOperation? operation;
		lock (Locker)
		{
			operation = MediaPickerRecoveryStore.ReadActiveOperation();
			if (operation?.Id != id || operation.State != PendingMediaPickerState.ResultAccepted)
			{
				return [];
			}

			if (operation.FilePaths.Count > 0)
			{
				return operation.FilePaths.ToArray();
			}

			if (operation.PickerUriStrings.Count == 0)
			{
				return [];
			}
		}

		IReadOnlyList<string> filePaths;
		try
		{
			filePaths = await MaterializePickerUrisAsync(operation.PickerUriStrings).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			Trace.WriteLine($"Unable to materialize picked media result: {ex}");
			ClearActiveOperation(operation.Id);

			if (throwOnMaterializationFailure)
			{
				throw;
			}

			return [];
		}

		if (filePaths.Count == 0)
		{
			ClearActiveOperation(operation.Id);
			return [];
		}

		IReadOnlyList<string> pickerUriStringsToRelease;
		lock (Locker)
		{
			var current = MediaPickerRecoveryStore.ReadActiveOperation();
			if (current?.Id != operation.Id || current.State != PendingMediaPickerState.ResultAccepted)
			{
				return [];
			}

			pickerUriStringsToRelease = current.PickerUriStrings.ToArray();
			MediaPickerRecoveryStore.WriteActiveOperation(current.WithAcceptedFiles(filePaths));
		}

		ReleasePickerUriReadAccess(pickerUriStringsToRelease);
		return filePaths;
	}

	static IReadOnlyList<string> GetPickerUriStrings(IReadOnlyList<AndroidUri> uris)
		=> uris
			.Where(uri => uri is not null && !uri.Equals(AndroidUri.Empty))
			.Select(uri => uri.ToString())
			.Where(uri => !string.IsNullOrWhiteSpace(uri))
			.Select(uri => uri!)
			.ToArray();

	static async Task<IReadOnlyList<string>> MaterializePickerUrisAsync(IReadOnlyList<string> uriStrings)
	{
		var filePaths = new List<string>();

		foreach (var uriString in uriStrings)
		{
			if (string.IsNullOrWhiteSpace(uriString))
			{
				continue;
			}

			var uri = AndroidUri.Parse(uriString);
			if (uri is null)
			{
				continue;
			}

			var filePath = await FileSystemUtils.EnsurePhysicalPathAsync(uri).ConfigureAwait(false);
			if (!string.IsNullOrEmpty(filePath))
			{
				filePaths.Add(filePath);
			}
		}

		return filePaths;
	}

	static bool TryPersistPickerUriReadAccess(AndroidUri? uri)
	{
		if (!IsPersistablePickerUri(uri))
		{
			return false;
		}

		try
		{
			return PersistPickerUriReadAccessHandler(uri!);
		}
		catch (Exception ex)
		{
			Trace.WriteLine($"Unable to persist picked media URI access: {ex}");
			return false;
		}
	}

	static bool PersistPickerUriReadAccessCore(AndroidUri uri)
	{
		var contentResolver = Application.Context?.ContentResolver;
		if (contentResolver is null)
		{
			return false;
		}

		contentResolver.TakePersistableUriPermission(uri, ActivityFlags.GrantReadUriPermission);
		return true;
	}

	static void ReleasePickerUriReadAccess(IReadOnlyList<string> uriStrings)
	{
		foreach (var uriString in uriStrings)
		{
			if (string.IsNullOrWhiteSpace(uriString))
			{
				continue;
			}

			TryReleasePickerUriReadAccess(AndroidUri.Parse(uriString));
		}
	}

	static void TryReleasePickerUriReadAccess(AndroidUri? uri)
	{
		if (!IsPersistablePickerUri(uri))
		{
			return;
		}

		try
		{
			ReleasePickerUriReadAccessHandler(uri!);
		}
		catch (Exception ex)
		{
			Trace.WriteLine($"Unable to release picked media URI access: {ex}");
		}
	}

	static void ReleasePickerUriReadAccessCore(AndroidUri uri)
		=> Application.Context?.ContentResolver?.ReleasePersistableUriPermission(uri, ActivityFlags.GrantReadUriPermission);

	static bool IsPersistablePickerUri(AndroidUri? uri)
		=> uri is not null &&
		   !uri.Equals(AndroidUri.Empty) &&
		   string.Equals(uri.Scheme, FileSystemUtils.UriSchemeContent, StringComparison.OrdinalIgnoreCase);

	static async Task RecoverOrphanedOperationResultAsync(Func<bool> recordResult, string failureMessage)
	{
		IReadOnlyList<RecoveredMediaPickerResult>? waiterResults = null;

		try
		{
			if (!recordResult())
			{
				return;
			}

			var reconciliation = await RecoverOperationIfAvailableCoreAsync().ConfigureAwait(false);
			if (!reconciliation.WasReconciled)
			{
				waiterResults = reconciliation.Results;
			}
		}
		catch (Exception ex)
		{
			Trace.WriteLine($"{failureMessage}: {ex}");
			waiterResults = ReadPublicRecoveredResults();
		}
		finally
		{
			if (waiterResults is not null)
			{
				CompleteRecoveryWaitersForReconciliation(waiterResults);
			}
		}
	}

	// Publishes the accepted operation as a non-consuming recovered result.
	static async Task PublishRecoveredOperationAsync(PendingMediaPickerOperation operation)
	{
		var recoveredPaths = new List<string>();
		var acceptedFilePaths = await MaterializeAcceptedFilePathsAsync(operation.Id, throwOnMaterializationFailure: false).ConfigureAwait(false);

		foreach (var filePath in acceptedFilePaths)
		{
			var recoveredPath = filePath;

			if (IsPhotoKind(operation.Kind))
			{
				try
				{
					recoveredPath = await MediaPickerImplementation.ProcessPhotoPreservingSourceAsync(recoveredPath, operation.PhotoProcessingOptions).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					Trace.WriteLine($"Unable to process recovered photo result: {ex}");
				}
			}

			if (IsFileAvailable(recoveredPath))
			{
				recoveredPaths.Add(recoveredPath);
			}
		}

		lock (Locker)
		{
			var current = MediaPickerRecoveryStore.ReadActiveOperation();
			if (current?.Id != operation.Id)
			{
				return;
			}

			if (recoveredPaths.Count == 0)
			{
				_ = ClearActiveOperationUnderLock(operation);
				return;
			}

			var recoveredResult = new RecoveredMediaPickerRecord(operation.Id, operation.Kind, recoveredPaths);
			var recoveredResults = MediaPickerRecoveryStore.ReadRecoveredResults();
			recoveredResults.RemoveAll(result => string.Equals(result.Id, recoveredResult.Id, StringComparison.Ordinal));
			recoveredResults.Add(recoveredResult);

			MediaPickerRecoveryStore.WriteRecoveredResults(NormalizeRecoveredResults(recoveredResults));
			_ = ClearActiveOperationUnderLock(operation);
		}
	}

	internal static bool IsFileAvailable(string? filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
		{
			return false;
		}

		var fileInfo = new FileInfo(filePath);
		return fileInfo is { Exists: true, Length: > 0 };
	}

	static IReadOnlyList<RecoveredMediaPickerResult> ReadPublicRecoveredResults()
	{
		lock (Locker)
		{
			return ReadPublicRecoveredResultsUnderLock();
		}
	}

	static IReadOnlyList<RecoveredMediaPickerResult> ReadPublicRecoveredResultsUnderLock()
		=> ReadNormalizedRecoveredResultsUnderLock()
			.Select(result => result.ToPublicResult())
			.ToArray();

	static List<RecoveredMediaPickerRecord> ReadNormalizedRecoveredResultsUnderLock()
	{
		var results = MediaPickerRecoveryStore.ReadRecoveredResults();
		var normalizedResults = NormalizeRecoveredResults(results);

		if (!AreRecoveredResultsEqual(results, normalizedResults))
		{
			MediaPickerRecoveryStore.WriteRecoveredResults(normalizedResults);
		}

		return normalizedResults;
	}

	static List<RecoveredMediaPickerRecord> NormalizeRecoveredResults(IReadOnlyList<RecoveredMediaPickerRecord> results)
	{
		var normalizedResults = new List<RecoveredMediaPickerRecord>();

		foreach (var result in results)
		{
			var availableFilePaths = result.FilePaths
				.Where(IsFileAvailable)
				.ToArray();

			if (availableFilePaths.Length > 0)
			{
				normalizedResults.Add(new RecoveredMediaPickerRecord(result.Id, result.Kind, availableFilePaths));
			}
		}

		var excessCount = normalizedResults.Count - MaxRecoveredResultCount;
		if (excessCount > 0)
		{
			normalizedResults.RemoveRange(0, excessCount);
		}

		return normalizedResults;
	}

	static bool AreRecoveredResultsEqual(IReadOnlyList<RecoveredMediaPickerRecord> first, IReadOnlyList<RecoveredMediaPickerRecord> second)
	{
		if (first.Count != second.Count)
		{
			return false;
		}

		for (var i = 0; i < first.Count; i++)
		{
			var firstResult = first[i];
			var secondResult = second[i];

			if (!string.Equals(firstResult.Id, secondResult.Id, StringComparison.Ordinal) ||
				firstResult.Kind != secondResult.Kind ||
				!firstResult.FilePaths.SequenceEqual(secondResult.FilePaths))
			{
				return false;
			}
		}

		return true;
	}

	static long GetRecoveryReconciliationGeneration()
	{
		lock (Locker)
		{
			return RecoveryReconciliationGeneration;
		}
	}

	static bool IsInProcessOperation(PendingMediaPickerOperation operation)
		=> InProcessOperationIds.Contains(operation.Id);

	static bool ShouldPromoteRecreatedOperation(PendingMediaPickerOperation operation)
	{
		if (IsInProcessOperation(operation))
		{
			return false;
		}

		return operation.State == PendingMediaPickerState.ResultAccepted;
	}

	static bool HasAcceptedResultPayload(PendingMediaPickerOperation operation)
		=> operation.FilePaths.Count > 0 || operation.PickerUriStrings.Count > 0;

	static void ValidateBeginOperationArguments(RecoveredMediaPickerResultKind kind, IReadOnlyList<string> filePaths)
	{
		if (!IsKnownKind(kind))
		{
			throw new ArgumentOutOfRangeException(nameof(kind));
		}

		if (filePaths is null)
		{
			throw new ArgumentNullException(nameof(filePaths));
		}
	}

	static PendingMediaPickerOperation BeginOperationUnderLock(
		RecoveredMediaPickerResultKind kind,
		IReadOnlyList<string> filePaths,
		PersistedPhotoProcessingOptions photoProcessingOptions)
	{
		var operation = new PendingMediaPickerOperation(
			Guid.NewGuid().ToString("N"),
			kind,
			PendingMediaPickerState.Pending,
			filePaths.ToArray(),
			[],
			photoProcessingOptions);

		MediaPickerRecoveryStore.WriteActiveOperation(operation);
		InProcessOperationIds.Add(operation.Id);

		return operation;
	}

	static void ThrowIfActiveOperationBlocksNewOperation(PendingMediaPickerOperation activeOperation)
	{
		if (IsInProcessOperation(activeOperation))
		{
			throw new InvalidOperationException("A MediaPicker operation is already in progress.");
		}

		if (activeOperation.State == PendingMediaPickerState.ResultAccepted)
		{
			throw new InvalidOperationException("A MediaPicker result is pending recovery.");
		}

		throw new InvalidOperationException("A MediaPicker operation is pending AndroidX result replay.");
	}

	static IReadOnlyList<string> ClearActiveOperationUnderLock(PendingMediaPickerOperation operation)
	{
		InProcessOperationIds.Remove(operation.Id);
		MediaPickerRecoveryStore.RemoveActiveOperation();
		return operation.PickerUriStrings.ToArray();
	}

	static void CancelRecoveryWaiter(MediaPickerRecoveryWaiter waiter)
	{
		lock (Locker)
		{
			RecoveryWaiters.Remove(waiter);
		}

		waiter.TrySetCanceled();
	}

	static void CompleteRecoveryWaitersForReconciliation(IReadOnlyList<RecoveredMediaPickerResult> results)
	{
		List<MediaPickerRecoveryWaiter> waiters;

		lock (Locker)
		{
			waiters = MarkRecoveryReconciledAndTakeWaitersUnderLock();
		}

		CompleteRecoveryWaiters(waiters, results);
	}

	static List<MediaPickerRecoveryWaiter> MarkRecoveryReconciledAndTakeWaitersUnderLock()
	{
		RecoveryReconciliationGeneration++;
		return TakeRecoveryWaitersUnderLock();
	}

	static List<MediaPickerRecoveryWaiter> TakeRecoveryWaitersUnderLock()
	{
		var waiters = RecoveryWaiters.ToList();
		RecoveryWaiters.Clear();
		return waiters;
	}

	static void CompleteRecoveryWaiters(List<MediaPickerRecoveryWaiter> waiters, IReadOnlyList<RecoveredMediaPickerResult> results)
	{
		foreach (var waiter in waiters)
		{
			waiter.TrySetResult(results);
		}
	}

	static bool IsCaptureKind(RecoveredMediaPickerResultKind kind)
		=> kind == RecoveredMediaPickerResultKind.CapturePhoto ||
		   kind == RecoveredMediaPickerResultKind.CaptureVideo;

	static bool IsPhotoKind(RecoveredMediaPickerResultKind kind)
		=> kind == RecoveredMediaPickerResultKind.CapturePhoto ||
		   kind == RecoveredMediaPickerResultKind.PickPhoto ||
		   kind == RecoveredMediaPickerResultKind.PickPhotos;

	// PickPhotos/PickVideos with SelectionLimit == 1 use AndroidX's single-picker launcher, but
	// higher selection limits use the multiple-picker launcher. AndroidX replays the launcher that
	// produced the result, so plural operation kinds intentionally match both callback shapes.
	static bool IsSinglePickCallbackKind(RecoveredMediaPickerResultKind kind)
		=> kind == RecoveredMediaPickerResultKind.PickPhoto ||
		   kind == RecoveredMediaPickerResultKind.PickVideo ||
		   kind == RecoveredMediaPickerResultKind.PickPhotos ||
		   kind == RecoveredMediaPickerResultKind.PickVideos;

	static bool IsMultiplePickCallbackKind(RecoveredMediaPickerResultKind kind)
		=> kind == RecoveredMediaPickerResultKind.PickPhotos ||
		   kind == RecoveredMediaPickerResultKind.PickVideos;

	static bool IsKnownKind(RecoveredMediaPickerResultKind kind)
		=> Enum.IsDefined(typeof(RecoveredMediaPickerResultKind), kind);

}

/// <summary>
/// Stores active and recovered MediaPicker records in app-private preferences.
/// </summary>
internal static class MediaPickerRecoveryStore
{
	const string ActiveOperationKey = "active_operation";
	const string RecoveredResultsKey = "recovered_results";
	const string PreferencesFeatureName = "media_picker";
	const int SerializedRecordVersion = 1;

	static readonly string PreferencesSharedName = Preferences.GetPrivatePreferencesSharedName(PreferencesFeatureName);

	internal static PendingMediaPickerOperation? ReadActiveOperation()
		=> DeserializePendingOperation(Preferences.Get(ActiveOperationKey, null, PreferencesSharedName));

	internal static void WriteActiveOperation(PendingMediaPickerOperation operation)
		=> Preferences.Set(ActiveOperationKey, SerializePendingOperation(operation), PreferencesSharedName);

	internal static void RemoveActiveOperation()
		=> Preferences.Remove(ActiveOperationKey, PreferencesSharedName);

	internal static List<RecoveredMediaPickerRecord> ReadRecoveredResults()
	{
		var value = Preferences.Get(RecoveredResultsKey, null, PreferencesSharedName);

		if (string.IsNullOrWhiteSpace(value))
		{
			return [];
		}

		try
		{
			var records = JsonSerializer.Deserialize(value, MediaPickerRecoveryJsonContext.Default.RecoveredResults);

			return records is null
				? []
				: records
					.Select(DeserializeRecoveredResult)
					.Where(result => result is not null)
					.Cast<RecoveredMediaPickerRecord>()
					.ToList();
		}
		catch
		{
			return [];
		}
	}

	internal static void WriteRecoveredResults(List<RecoveredMediaPickerRecord> results)
	{
		if (results.Count == 0)
		{
			Preferences.Remove(RecoveredResultsKey, PreferencesSharedName);
			return;
		}

		var records = results.Select(ToPreferenceRecord).ToArray();
		Preferences.Set(RecoveredResultsKey, JsonSerializer.Serialize(records, MediaPickerRecoveryJsonContext.Default.RecoveredResults), PreferencesSharedName);
	}

	static string SerializePendingOperation(PendingMediaPickerOperation operation)
		=> JsonSerializer.Serialize(ToPreferenceRecord(operation), MediaPickerRecoveryJsonContext.Default.PendingOperation);

	static PendingMediaPickerOperation? DeserializePendingOperation(string? value)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return null;
			}

			var record = JsonSerializer.Deserialize(value, MediaPickerRecoveryJsonContext.Default.PendingOperation);
			if (record is null ||
				record.Version != SerializedRecordVersion ||
				string.IsNullOrWhiteSpace(record.Id) ||
				record.PhotoProcessingOptions is null ||
				!TryDeserializeResultKind(record.Kind, out var kind) ||
				!TryDeserializePendingState(record.State, out var state))
			{
				return null;
			}

			return new PendingMediaPickerOperation(
				record.Id,
				kind,
				state,
				GetValidStrings(record.FilePaths),
				GetValidStrings(record.PickerUriStrings),
				new PersistedPhotoProcessingOptions(
					record.PhotoProcessingOptions.MaximumWidth,
					record.PhotoProcessingOptions.MaximumHeight,
					record.PhotoProcessingOptions.CompressionQuality,
					record.PhotoProcessingOptions.RotateImage,
					record.PhotoProcessingOptions.PreserveMetaData));
		}
		catch
		{
			return null;
		}
	}

	static MediaPickerPendingOperationPreferenceRecord ToPreferenceRecord(PendingMediaPickerOperation operation)
		=> new()
		{
			Version = SerializedRecordVersion,
			Id = operation.Id,
			Kind = (int)operation.Kind,
			State = (int)operation.State,
			FilePaths = operation.FilePaths.ToArray(),
			PickerUriStrings = operation.PickerUriStrings.ToArray(),
			PhotoProcessingOptions = new MediaPickerPhotoProcessingOptionsPreferenceRecord
			{
				MaximumWidth = operation.PhotoProcessingOptions.MaximumWidth,
				MaximumHeight = operation.PhotoProcessingOptions.MaximumHeight,
				CompressionQuality = operation.PhotoProcessingOptions.CompressionQuality,
				RotateImage = operation.PhotoProcessingOptions.RotateImage,
				PreserveMetaData = operation.PhotoProcessingOptions.PreserveMetaData
			}
		};

	static RecoveredMediaPickerRecord? DeserializeRecoveredResult(MediaPickerRecoveredResultPreferenceRecord? record)
	{
		if (record is null ||
			record.Version != SerializedRecordVersion ||
			string.IsNullOrWhiteSpace(record.Id) ||
			!TryDeserializeResultKind(record.Kind, out var kind))
		{
			return null;
		}

		var filePaths = GetValidStrings(record.FilePaths);
		return filePaths.Count > 0 ? new RecoveredMediaPickerRecord(record.Id, kind, filePaths) : null;
	}

	static MediaPickerRecoveredResultPreferenceRecord ToPreferenceRecord(RecoveredMediaPickerRecord result)
		=> new()
		{
			Version = SerializedRecordVersion,
			Id = result.Id,
			Kind = (int)result.Kind,
			FilePaths = result.FilePaths.ToArray()
		};

	static IReadOnlyList<string> GetValidStrings(string[]? values)
		=> values is null
			? []
			: values
				.Where(value => !string.IsNullOrEmpty(value))
				.Select(value => value!)
				.ToArray();

	static bool TryDeserializePendingState(int value, out PendingMediaPickerState state)
	{
		if (Enum.IsDefined(typeof(PendingMediaPickerState), value))
		{
			state = (PendingMediaPickerState)value;
			return true;
		}

		state = PendingMediaPickerState.Pending;
		return false;
	}

	static bool TryDeserializeResultKind(int value, out RecoveredMediaPickerResultKind kind)
	{
		if (Enum.IsDefined(typeof(RecoveredMediaPickerResultKind), value))
		{
			kind = (RecoveredMediaPickerResultKind)value;
			return true;
		}

		kind = RecoveredMediaPickerResultKind.CapturePhoto;
		return false;
	}
}

internal sealed class MediaPickerPendingOperationPreferenceRecord
{
	public int Version { get; set; }

	public string? Id { get; set; }

	public int Kind { get; set; }

	public int State { get; set; }

	public string[]? FilePaths { get; set; }

	public string[]? PickerUriStrings { get; set; }

	public MediaPickerPhotoProcessingOptionsPreferenceRecord? PhotoProcessingOptions { get; set; }
}

internal sealed class MediaPickerRecoveredResultPreferenceRecord
{
	public int Version { get; set; }

	public string? Id { get; set; }

	public int Kind { get; set; }

	public string[]? FilePaths { get; set; }
}

internal sealed class MediaPickerPhotoProcessingOptionsPreferenceRecord
{
	public int? MaximumWidth { get; set; }

	public int? MaximumHeight { get; set; }

	public int CompressionQuality { get; set; }

	public bool RotateImage { get; set; }

	public bool PreserveMetaData { get; set; }
}

[JsonSerializable(typeof(MediaPickerPendingOperationPreferenceRecord), TypeInfoPropertyName = nameof(PendingOperation))]
[JsonSerializable(typeof(MediaPickerRecoveredResultPreferenceRecord[]), TypeInfoPropertyName = nameof(RecoveredResults))]
internal sealed partial class MediaPickerRecoveryJsonContext : JsonSerializerContext
{
}

/// <summary>
/// One-shot awaiter completed when AndroidX recovery reconciles a pending MediaPicker operation.
/// </summary>
internal sealed class MediaPickerRecoveryWaiter
{
	readonly CancellationToken cancellationToken;
	readonly TaskCompletionSource<IReadOnlyList<RecoveredMediaPickerResult>> completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
	readonly Lock completionLock = new();
	CancellationTokenRegistration cancellationRegistration;
	bool completed;

	public MediaPickerRecoveryWaiter(CancellationToken cancellationToken)
	{
		this.cancellationToken = cancellationToken;
	}

	public Task<IReadOnlyList<RecoveredMediaPickerResult>> Task => completionSource.Task;

	public void SetCancellationRegistration(CancellationTokenRegistration registration)
	{
		var disposeRegistration = false;

		lock (completionLock)
		{
			if (completed)
			{
				disposeRegistration = true;
			}
			else
			{
				cancellationRegistration = registration;
			}
		}

		if (disposeRegistration)
		{
			registration.Dispose();
		}
	}

	public void TrySetResult(IReadOnlyList<RecoveredMediaPickerResult> results)
	{
		if (!TryComplete(out var registration))
		{
			return;
		}

		registration.Dispose();
		completionSource.TrySetResult(results);
	}

	public void TrySetCanceled()
	{
		if (!TryComplete(out var registration))
		{
			return;
		}

		registration.Dispose();
		completionSource.TrySetCanceled(cancellationToken);
	}

	bool TryComplete(out CancellationTokenRegistration registration)
	{
		lock (completionLock)
		{
			if (completed)
			{
				registration = default;
				return false;
			}

			completed = true;
			registration = cancellationRegistration;
			cancellationRegistration = default;
			return true;
		}
	}
}

/// <summary>
/// Durable record for the single AndroidX MediaPicker operation currently in flight.
/// </summary>
internal sealed class PendingMediaPickerOperation
{
	public PendingMediaPickerOperation(
		string id,
		RecoveredMediaPickerResultKind kind,
		PendingMediaPickerState state,
		IReadOnlyList<string> filePaths,
		IReadOnlyList<string> pickerUriStrings,
		PersistedPhotoProcessingOptions photoProcessingOptions)
	{
		Id = id;
		Kind = kind;
		State = state;
		FilePaths = filePaths.ToArray();
		PickerUriStrings = pickerUriStrings.ToArray();
		PhotoProcessingOptions = photoProcessingOptions;
	}

	public string Id { get; }

	public RecoveredMediaPickerResultKind Kind { get; }

	public PendingMediaPickerState State { get; }

	public IReadOnlyList<string> FilePaths { get; }

	public IReadOnlyList<string> PickerUriStrings { get; }

	public PersistedPhotoProcessingOptions PhotoProcessingOptions { get; }

	public PendingMediaPickerOperation WithState(PendingMediaPickerState state)
		=> new(Id, Kind, state, FilePaths, PickerUriStrings, PhotoProcessingOptions);

	public PendingMediaPickerOperation WithAcceptedFiles(IReadOnlyList<string> filePaths)
		=> new(Id, Kind, PendingMediaPickerState.ResultAccepted, filePaths, [], PhotoProcessingOptions);

	public PendingMediaPickerOperation WithAcceptedPickerUris(IReadOnlyList<string> pickerUriStrings)
		=> new(Id, Kind, PendingMediaPickerState.ResultAccepted, [], pickerUriStrings, PhotoProcessingOptions);
}

readonly struct MediaPickerRecoveryReconciliation
{
	public MediaPickerRecoveryReconciliation(IReadOnlyList<RecoveredMediaPickerResult> results, bool wasReconciled)
	{
		Results = results;
		WasReconciled = wasReconciled;
	}

	public IReadOnlyList<RecoveredMediaPickerResult> Results { get; }

	public bool WasReconciled { get; }
}

/// <summary>
/// Durable queue item for a recovered MediaPicker result that app code has not cleared yet.
/// </summary>
internal sealed class RecoveredMediaPickerRecord
{
	public RecoveredMediaPickerRecord(string id, RecoveredMediaPickerResultKind kind, IReadOnlyList<string> filePaths)
	{
		Id = id;
		Kind = kind;
		FilePaths = filePaths.ToArray();
	}

	public string Id { get; }

	public RecoveredMediaPickerResultKind Kind { get; }

	public IReadOnlyList<string> FilePaths { get; }

	public RecoveredMediaPickerResult ToPublicResult()
		=> new(Id, Kind, FilePaths.Select(path => new FileResult(path)).ToArray());
}

/// <summary>
/// Durable photo post-processing policy needed to finish processing a recovered photo result.
/// </summary>
internal readonly struct PersistedPhotoProcessingOptions
{
	public static PersistedPhotoProcessingOptions Default { get; } = new(null, null, 100, false, true);

	public PersistedPhotoProcessingOptions(int? maximumWidth, int? maximumHeight, int compressionQuality, bool rotateImage, bool preserveMetaData)
	{
		MaximumWidth = maximumWidth;
		MaximumHeight = maximumHeight;
		CompressionQuality = compressionQuality;
		RotateImage = rotateImage;
		PreserveMetaData = preserveMetaData;
	}

	public int? MaximumWidth { get; }

	public int? MaximumHeight { get; }

	public int CompressionQuality { get; }

	public bool RotateImage { get; }

	public bool PreserveMetaData { get; }
}
