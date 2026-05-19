#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
	static readonly Lock Locker = new();
	static readonly HashSet<string> InProcessOperationIds = new(StringComparer.Ordinal);
	static readonly List<MediaPickerRecoveryWaiter> RecoveryWaiters = [];
	static readonly SemaphoreSlim RecoveryPromotionSemaphore = new(1, 1);
	// Lets waiters detect an empty recovery outcome that happened before they could be registered.
	static long RecoveryReconciliationGeneration;

	internal static PendingMediaPickerOperation BeginOperation(
		RecoveredMediaPickerResultKind kind,
		IReadOnlyList<string> filePaths,
		PersistedPhotoProcessingOptions photoProcessingOptions)
	{
		if (!IsKnownKind(kind))
		{
			throw new ArgumentOutOfRangeException(nameof(kind));
		}

		if (filePaths is null)
		{
			throw new ArgumentNullException(nameof(filePaths));
		}

		PendingMediaPickerOperation operation;

		// Persist the one active MediaPicker operation before launching AndroidX. Later AndroidX
		// callbacks are matched back to this durable record, including after process recreation.
		lock (Locker)
		{
			if (MediaPickerRecoveryStore.ReadActiveOperation() is { } activeOperation)
			{
				ThrowIfActiveOperationBlocksNewOperation(activeOperation);
			}

			operation = new PendingMediaPickerOperation(
				Guid.NewGuid().ToString("N"),
				kind,
				PendingMediaPickerState.Pending,
				filePaths.ToArray(),
				[],
				photoProcessingOptions);

			MediaPickerRecoveryStore.WriteActiveOperation(operation);
			InProcessOperationIds.Add(operation.Id);
		}

		return operation;
	}

	internal static void ClearActiveOperation(string id)
	{
		if (id is null)
		{
			return;
		}

		lock (Locker)
		{
			var operation = MediaPickerRecoveryStore.ReadActiveOperation();
			if (operation?.Id == id)
			{
				ClearActiveOperationUnderLock(operation);
			}
		}
	}

	internal static async Task<IReadOnlyList<RecoveredMediaPickerResult>> GetRecoveredResultsAsync()
	{
		var reconciliation = await RecoverOperationIfAvailableCoreAsync().ConfigureAwait(false);
		return reconciliation.Results;
	}

	internal static async Task<IReadOnlyList<RecoveredMediaPickerResult>> WaitForRecoveredResultsAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var observedReconciliationGeneration = GetRecoveryReconciliationGeneration();
		var reconciliation = await RecoverOperationIfAvailableCoreAsync().ConfigureAwait(false);
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

	internal static void RecoverOrphanedCaptureResult(RecoveredMediaPickerResultKind kind, bool success)
		=> _ = RecoverOrphanedCaptureResultAsync(kind, success);

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
				ClearActiveOperationUnderLock(operation);
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
		=> _ = RecoverOrphanedSinglePickResultAsync(uri);

	internal static async Task RecoverOrphanedSinglePickResultAsync(AndroidUri? uri)
		=> await RecoverOrphanedOperationResultAsync(
			() => RecordSinglePickCallbackResult(uri),
			"Unable to recover picked media result").ConfigureAwait(false);

	internal static void RecoverOrphanedMultiplePickResult(IReadOnlyList<AndroidUri>? uris)
		=> _ = RecoverOrphanedMultiplePickResultAsync(uris);

	internal static async Task RecoverOrphanedMultiplePickResultAsync(IReadOnlyList<AndroidUri>? uris)
		=> await RecoverOrphanedOperationResultAsync(
			() => RecordMultiplePickCallbackResult(uris),
			"Unable to recover picked media results").ConfigureAwait(false);

	internal static async Task<IReadOnlyList<RecoveredMediaPickerResult>> RecoverOperationIfAvailableAsync()
	{
		var reconciliation = await RecoverOperationIfAvailableCoreAsync().ConfigureAwait(false);
		return reconciliation.Results;
	}

	// Promotes a recreated-process operation only after AndroidX has accepted the result.
	// A Pending record means the result callback has not been replayed yet.
	static async Task<MediaPickerRecoveryReconciliation> RecoverOperationIfAvailableCoreAsync()
	{
		await RecoveryPromotionSemaphore.WaitAsync().ConfigureAwait(false);

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

		lock (Locker)
		{
			var current = MediaPickerRecoveryStore.ReadActiveOperation();
			if (current?.Id != operation.Id)
			{
				return false;
			}

			// AndroidX accepted the picker result. Persist the URI payload before copying from it so
			// process death during materialization can still be retried after recreation.
			MediaPickerRecoveryStore.WriteActiveOperation(current.WithAcceptedPickerUris(uriStrings));
		}

		foreach (var uri in uris)
		{
			TryPersistPickerUriReadAccess(uri);
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

		lock (Locker)
		{
			var current = MediaPickerRecoveryStore.ReadActiveOperation();
			if (current?.Id != operation.Id || current.State != PendingMediaPickerState.ResultAccepted)
			{
				return [];
			}

			MediaPickerRecoveryStore.WriteActiveOperation(current.WithAcceptedFiles(filePaths));
		}

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

	static void TryPersistPickerUriReadAccess(AndroidUri? uri)
	{
		if (uri is null ||
			uri.Equals(AndroidUri.Empty) ||
			!string.Equals(uri.Scheme, FileSystemUtils.UriSchemeContent, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		try
		{
			Application.Context?.ContentResolver?.TakePersistableUriPermission(uri, ActivityFlags.GrantReadUriPermission);
		}
		catch (Exception ex)
		{
			Trace.WriteLine($"Unable to persist picked media URI access: {ex}");
		}
	}

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
				ClearActiveOperationUnderLock(operation);
				return;
			}

			var recoveredResult = new RecoveredMediaPickerRecord(operation.Id, operation.Kind, recoveredPaths);
			var recoveredResults = MediaPickerRecoveryStore.ReadRecoveredResults();
			recoveredResults.RemoveAll(result => string.Equals(result.Id, recoveredResult.Id, StringComparison.Ordinal));
			recoveredResults.Add(recoveredResult);

			MediaPickerRecoveryStore.WriteRecoveredResults(recoveredResults);
			ClearActiveOperationUnderLock(operation);
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
		=> MediaPickerRecoveryStore.ReadRecoveredResults()
			.Select(result => result.ToPublicResult())
			.ToArray();

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

	static void ClearActiveOperationUnderLock(PendingMediaPickerOperation operation)
	{
		InProcessOperationIds.Remove(operation.Id);
		MediaPickerRecoveryStore.RemoveActiveOperation();
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
	const string PendingOperationSerializedRecordVersion = "5";
	const string PendingOperationWithoutPickerUrisSerializedRecordVersion = "4";
	const string LegacyPendingCaptureSerializedRecordVersion = "1";
	const string LegacyPendingCaptureWithStateAndOutputUriSerializedRecordVersion = "2";
	const string LegacyPendingCaptureWithStateSerializedRecordVersion = "3";
	const string RecoveredResultSerializedRecordVersion = "2";
	const string LegacyRecoveredCaptureResultSerializedRecordVersion = "1";
	const string FieldSeparator = "|";
	const string FilePathSeparator = ",";
	const string RecoveredResultSeparator = "\n";

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

		return value
			.Split([RecoveredResultSeparator], StringSplitOptions.RemoveEmptyEntries)
			.Select(DeserializeRecoveredResult)
			.Where(result => result is not null)
			.Cast<RecoveredMediaPickerRecord>()
			.ToList();
	}

	internal static void WriteRecoveredResults(List<RecoveredMediaPickerRecord> results)
	{
		if (results.Count == 0)
		{
			Preferences.Remove(RecoveredResultsKey, PreferencesSharedName);
			return;
		}

		Preferences.Set(RecoveredResultsKey, string.Join(RecoveredResultSeparator, results.Select(SerializeRecoveredResult)), PreferencesSharedName);
	}

	static string SerializePendingOperation(PendingMediaPickerOperation operation)
		=> string.Join(FieldSeparator, new[]
		{
			PendingOperationSerializedRecordVersion,
			Encode(operation.Id),
			((int)operation.Kind).ToString(CultureInfo.InvariantCulture),
			((int)operation.State).ToString(CultureInfo.InvariantCulture),
			EncodeMany(operation.FilePaths),
			EncodeMany(operation.PickerUriStrings),
			SerializeNullableInt(operation.PhotoProcessingOptions.MaximumWidth),
			SerializeNullableInt(operation.PhotoProcessingOptions.MaximumHeight),
			operation.PhotoProcessingOptions.CompressionQuality.ToString(CultureInfo.InvariantCulture),
			SerializeBool(operation.PhotoProcessingOptions.RotateImage),
			SerializeBool(operation.PhotoProcessingOptions.PreserveMetaData)
		});

	static PendingMediaPickerOperation? DeserializePendingOperation(string? value)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return null;
			}

			var parts = value.Split([FieldSeparator], StringSplitOptions.None);
			if (parts.Length == 11 && parts[0] == PendingOperationSerializedRecordVersion)
			{
				return DeserializeCurrentPendingOperation(parts, hasPickerUris: true);
			}

			if (parts.Length == 10 && parts[0] == PendingOperationWithoutPickerUrisSerializedRecordVersion)
			{
				return DeserializeCurrentPendingOperation(parts, hasPickerUris: false);
			}

			return DeserializeLegacyPendingCapture(parts);
		}
		catch
		{
			return null;
		}
	}

	static PendingMediaPickerOperation? DeserializeCurrentPendingOperation(string[] parts, bool hasPickerUris)
	{
		var maximumWidthIndex = hasPickerUris ? 6 : 5;
		var maximumHeightIndex = hasPickerUris ? 7 : 6;
		var compressionQualityIndex = hasPickerUris ? 8 : 7;
		var rotateImageIndex = hasPickerUris ? 9 : 8;
		var preserveMetaDataIndex = hasPickerUris ? 10 : 9;

		if (!TryDeserializeResultKind(parts[2], out var kind) ||
		    !TryDeserializePendingState(parts[3], out var state) ||
		    !int.TryParse(parts[compressionQualityIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out var compressionQuality))
		{
			return null;
		}

		return new PendingMediaPickerOperation(
			Decode(parts[1]),
			kind,
			state,
			DecodeMany(parts[4]),
			hasPickerUris ? DecodeMany(parts[5]) : [],
			new PersistedPhotoProcessingOptions(
				DeserializeNullableInt(parts[maximumWidthIndex]),
				DeserializeNullableInt(parts[maximumHeightIndex]),
				compressionQuality,
				DeserializeBool(parts[rotateImageIndex]),
				DeserializeBool(parts[preserveMetaDataIndex])));
	}

	static PendingMediaPickerOperation? DeserializeLegacyPendingCapture(string[] parts)
	{
		var isLegacyPendingCapture = parts.Length == 10 && parts[0] == LegacyPendingCaptureSerializedRecordVersion;
		var isPendingCaptureWithStateAndOutputUri = parts.Length == 11 && parts[0] == LegacyPendingCaptureWithStateAndOutputUriSerializedRecordVersion;
		var isPendingCaptureWithState = parts.Length == 10 && parts[0] == LegacyPendingCaptureWithStateSerializedRecordVersion;

		if (!isLegacyPendingCapture && !isPendingCaptureWithStateAndOutputUri && !isPendingCaptureWithState)
		{
			return null;
		}

		var state = PendingMediaPickerState.Pending;
		var filePathIndex = 3;
		var maximumWidthIndex = 5;
		var maximumHeightIndex = 6;
		var compressionQualityIndex = 7;
		var rotateImageIndex = 8;
		var preserveMetaDataIndex = 9;

		if (isPendingCaptureWithStateAndOutputUri)
		{
			if (!TryDeserializePendingState(parts[3], out state))
			{
				return null;
			}

			filePathIndex = 4;
			maximumWidthIndex = 6;
			maximumHeightIndex = 7;
			compressionQualityIndex = 8;
			rotateImageIndex = 9;
			preserveMetaDataIndex = 10;
		}
		else if (isPendingCaptureWithState)
		{
			if (!TryDeserializePendingState(parts[3], out state))
			{
				return null;
			}

			filePathIndex = 4;
			maximumWidthIndex = 5;
			maximumHeightIndex = 6;
			compressionQualityIndex = 7;
			rotateImageIndex = 8;
			preserveMetaDataIndex = 9;
		}

		if (!TryDeserializeLegacyCaptureKind(parts[2], out var kind) ||
		    !int.TryParse(parts[compressionQualityIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out var compressionQuality))
		{
			return null;
		}

		return new PendingMediaPickerOperation(
			Decode(parts[1]),
			kind,
			state,
			[Decode(parts[filePathIndex])],
			[],
			new PersistedPhotoProcessingOptions(
				DeserializeNullableInt(parts[maximumWidthIndex]),
				DeserializeNullableInt(parts[maximumHeightIndex]),
				compressionQuality,
				DeserializeBool(parts[rotateImageIndex]),
				DeserializeBool(parts[preserveMetaDataIndex])));
	}

	static bool TryDeserializePendingState(string value, out PendingMediaPickerState state)
	{
		if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var stateValue) &&
		    Enum.IsDefined(typeof(PendingMediaPickerState), stateValue))
		{
			state = (PendingMediaPickerState)stateValue;
			return true;
		}

		state = PendingMediaPickerState.Pending;
		return false;
	}

	static bool TryDeserializeResultKind(string value, out RecoveredMediaPickerResultKind kind)
	{
		if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var kindValue) &&
		    Enum.IsDefined(typeof(RecoveredMediaPickerResultKind), kindValue))
		{
			kind = (RecoveredMediaPickerResultKind)kindValue;
			return true;
		}

		kind = RecoveredMediaPickerResultKind.CapturePhoto;
		return false;
	}

	static bool TryDeserializeLegacyCaptureKind(string value, out RecoveredMediaPickerResultKind kind)
	{
		if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var mediaTypeValue))
		{
			if (mediaTypeValue == 0)
			{
				kind = RecoveredMediaPickerResultKind.CapturePhoto;
				return true;
			}

			if (mediaTypeValue == 1)
			{
				kind = RecoveredMediaPickerResultKind.CaptureVideo;
				return true;
			}
		}

		kind = RecoveredMediaPickerResultKind.CapturePhoto;
		return false;
	}

	static string SerializeRecoveredResult(RecoveredMediaPickerRecord result)
		=> string.Join(FieldSeparator, new[]
		{
			RecoveredResultSerializedRecordVersion,
			Encode(result.Id),
			((int)result.Kind).ToString(CultureInfo.InvariantCulture),
			EncodeMany(result.FilePaths)
		});

	static RecoveredMediaPickerRecord? DeserializeRecoveredResult(string value)
	{
		try
		{
			var parts = value.Split([FieldSeparator], StringSplitOptions.None);
			if (parts.Length != 4)
			{
				return null;
			}

			if (parts[0] == RecoveredResultSerializedRecordVersion)
			{
				if (!TryDeserializeResultKind(parts[2], out var kind))
				{
					return null;
				}

				var filePaths = DecodeMany(parts[3]);
				return filePaths.Count > 0 ? new RecoveredMediaPickerRecord(Decode(parts[1]), kind, filePaths) : null;
			}

			if (parts[0] == LegacyRecoveredCaptureResultSerializedRecordVersion &&
			    TryDeserializeLegacyCaptureKind(parts[2], out var legacyKind))
			{
				return new RecoveredMediaPickerRecord(Decode(parts[1]), legacyKind, [Decode(parts[3])]);
			}

			return null;
		}
		catch
		{
			return null;
		}
	}

	static string EncodeMany(IReadOnlyList<string> values)
		=> string.Join(FilePathSeparator, values.Select(Encode));

	static IReadOnlyList<string> DecodeMany(string value)
		=> string.IsNullOrEmpty(value)
			? []
			: value.Split([FilePathSeparator], StringSplitOptions.RemoveEmptyEntries)
				.Select(Decode)
				.ToArray();

	static string Encode(string value)
		=> Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

	static string Decode(string value)
		=> Encoding.UTF8.GetString(Convert.FromBase64String(value));

	static string SerializeNullableInt(int? value)
		=> value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

	static int? DeserializeNullableInt(string value)
		=> string.IsNullOrEmpty(value) ? null : int.Parse(value, CultureInfo.InvariantCulture);

	static string SerializeBool(bool value)
		=> value ? "1" : "0";

	static bool DeserializeBool(string value)
		=> value == "1";
}

/// <summary>
/// One-shot awaiter completed when AndroidX recovery reconciles a pending MediaPicker operation.
/// </summary>
internal sealed class MediaPickerRecoveryWaiter
{
	readonly CancellationToken cancellationToken;
	readonly TaskCompletionSource<IReadOnlyList<RecoveredMediaPickerResult>> completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
	CancellationTokenRegistration cancellationRegistration;
	int completed;

	public MediaPickerRecoveryWaiter(CancellationToken cancellationToken)
	{
		this.cancellationToken = cancellationToken;
	}

	public Task<IReadOnlyList<RecoveredMediaPickerResult>> Task => completionSource.Task;

	public void SetCancellationRegistration(CancellationTokenRegistration registration)
	{
		if (Volatile.Read(ref completed) == 0)
		{
			cancellationRegistration = registration;

			if (Volatile.Read(ref completed) == 0)
			{
				return;
			}
		}

		registration.Dispose();
	}

	public void TrySetResult(IReadOnlyList<RecoveredMediaPickerResult> results)
	{
		if (Interlocked.Exchange(ref completed, 1) != 0)
		{
			return;
		}

		cancellationRegistration.Dispose();
		completionSource.TrySetResult(results);
	}

	public void TrySetCanceled()
	{
		if (Interlocked.Exchange(ref completed, 1) != 0)
		{
			return;
		}

		cancellationRegistration.Dispose();
		completionSource.TrySetCanceled(cancellationToken);
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
