using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using Xunit;
using static AndroidX.Activity.Result.Contract.ActivityResultContracts;
using ABitmap = Android.Graphics.Bitmap;
using AColor = Android.Graphics.Color;
using AndroidUri = Android.Net.Uri;
using JavaBoolean = Java.Lang.Boolean;
using JavaList = Android.Runtime.JavaList;

namespace Microsoft.Maui.Essentials.DeviceTests.Shared
{
	[Category("MediaPicker")]
	public class MediaPickerRecovery_Tests : IDisposable
	{
		const string ActiveOperationPreferenceKey = "active_operation";
		const string RecoveredResultsPreferenceKey = "recovered_results";
		static readonly string RecoveryPreferencesSharedName = Preferences.GetPrivatePreferencesSharedName("media_picker");

		// These tests drive the recovery manager and AndroidX callback wrappers directly.
		// They avoid launching real camera/picker apps while still preserving the important
		// process-recreation shape: preference-backed state survives, in-process launch state does not.
		public MediaPickerRecovery_Tests()
			=> ResetRecoveryState();

		public void Dispose()
			=> ResetRecoveryState();

		[Fact]
		public void Activity_State_Manager_Registers_All_MediaPicker_Launchers()
		{
			var capturePhotoRegistrations = 0;
			var captureVideoRegistrations = 0;
			var pickVisualMediaRegistrations = 0;
			var pickMultipleVisualMediaRegistrations = 0;

			// MediaPicker launchers must be registered unconditionally so AndroidX can replay a
			// pending picker or capture result after activity or process recreation.
			ActivityStateManagerImplementation.RegisterActivityResultLaunchers(
				() => capturePhotoRegistrations++,
				() => captureVideoRegistrations++,
				() => pickVisualMediaRegistrations++,
				() => pickMultipleVisualMediaRegistrations++);

			Assert.Equal(1, capturePhotoRegistrations);
			Assert.Equal(1, captureVideoRegistrations);
			Assert.Equal(1, pickVisualMediaRegistrations);
			Assert.Equal(1, pickMultipleVisualMediaRegistrations);
		}

		[Fact]
		public async Task Recovered_Results_Are_NonConsuming_And_Clear_By_Id()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Jpg);

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			await MediaPickerRecoveryManager.RecoverOrphanedCaptureResultAsync(RecoveredMediaPickerResultKind.CapturePhoto, true);

			Assert.Null(GetActiveOperation());

			var firstRead = await MediaPicker.GetRecoveredMediaPickerResultsAsync();
			var secondRead = await MediaPicker.GetRecoveredMediaPickerResultsAsync();

			var result = Assert.Single(firstRead);
			Assert.Single(secondRead);
			Assert.Equal(pendingCapture.Id, result.Id);
			Assert.Equal(capturePath, GetSingleRecoveredFile(result).FullPath);
			Assert.StartsWith("image/", GetSingleRecoveredFile(result).ContentType, StringComparison.OrdinalIgnoreCase);

			await MediaPicker.ClearRecoveredMediaPickerResultAsync(result.Id);

			Assert.Empty(await MediaPicker.GetRecoveredMediaPickerResultsAsync());
		}

		[Fact]
		public async Task Canceled_Capture_Clears_Active_State_Without_Recovered_Result()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Mp4);

			MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CaptureVideo,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			await MediaPickerRecoveryManager.RecoverOrphanedCaptureResultAsync(RecoveredMediaPickerResultKind.CaptureVideo, false);

			Assert.Null(GetActiveOperation());
			Assert.Empty(await MediaPicker.GetRecoveredMediaPickerResultsAsync());
		}

		[Fact]
		public async Task Missing_Output_File_Clears_Active_State_Without_Recovered_Result()
		{
			var capturePath = CreateMissingMediaFilePath(FileExtensions.Jpg);

			MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			await MediaPickerRecoveryManager.RecoverOrphanedCaptureResultAsync(RecoveredMediaPickerResultKind.CapturePhoto, true);

			Assert.Null(GetActiveOperation());
			Assert.Empty(await MediaPicker.GetRecoveredMediaPickerResultsAsync());
		}

		[Fact]
		public async Task Wait_For_Recovered_Results_Returns_Existing_Result()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Jpg);

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			await MediaPickerRecoveryManager.RecoverOrphanedCaptureResultAsync(RecoveredMediaPickerResultKind.CapturePhoto, true);

			var results = await MediaPicker.WaitForRecoveredMediaPickerResultsAsync(CancellationToken.None);

			var result = Assert.Single(results);
			Assert.Equal(pendingCapture.Id, result.Id);
			Assert.Equal(capturePath, GetSingleRecoveredFile(result).FullPath);
		}

		[Fact]
		public async Task Wait_For_Recovered_Results_Completes_When_Capture_Is_Recovered()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Jpg);
			using var cancellationTokenSource = new CancellationTokenSource();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);

			Assert.False(waitTask.IsCompleted);
			Assert.Equal(1, GetPendingWaiterCount());

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			await MediaPickerRecoveryManager.RecoverOrphanedCaptureResultAsync(RecoveredMediaPickerResultKind.CapturePhoto, true);

			var results = await WaitForCompletion(waitTask);
			var result = Assert.Single(results);
			Assert.Equal(pendingCapture.Id, result.Id);
			Assert.Equal(capturePath, GetSingleRecoveredFile(result).FullPath);
			Assert.Equal(0, GetPendingWaiterCount());
		}

		[Fact]
		public async Task Wait_For_Recovered_Results_Completes_Empty_When_Capture_Is_Canceled()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Mp4);
			using var cancellationTokenSource = new CancellationTokenSource();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);

			MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CaptureVideo,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			await MediaPickerRecoveryManager.RecoverOrphanedCaptureResultAsync(RecoveredMediaPickerResultKind.CaptureVideo, false);

			Assert.Empty(await WaitForCompletion(waitTask));
			Assert.Equal(0, GetPendingWaiterCount());
		}

		[Fact]
		public async Task Wait_For_Recovered_Results_Completes_Empty_When_Output_File_Is_Missing()
		{
			var capturePath = CreateMissingMediaFilePath(FileExtensions.Jpg);
			using var cancellationTokenSource = new CancellationTokenSource();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);

			MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			await MediaPickerRecoveryManager.RecoverOrphanedCaptureResultAsync(RecoveredMediaPickerResultKind.CapturePhoto, true);

			Assert.Empty(await WaitForCompletion(waitTask));
			Assert.Equal(0, GetPendingWaiterCount());
		}

		[Fact]
		public async Task Wait_For_Recovered_Results_Can_Be_Canceled()
		{
			using var cancellationTokenSource = new CancellationTokenSource();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);

			Assert.False(waitTask.IsCompleted);
			Assert.Equal(1, GetPendingWaiterCount());

			await cancellationTokenSource.CancelAsync();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waitTask);
			Assert.Equal(0, GetPendingWaiterCount());
		}

		[Fact]
		public async Task Wait_For_Recovered_Results_Completes_Multiple_Waiters()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Jpg);
			using var cancellationTokenSource = new CancellationTokenSource();

			var firstWaitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			var secondWaitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);

			Assert.Equal(2, GetPendingWaiterCount());

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			await MediaPickerRecoveryManager.RecoverOrphanedCaptureResultAsync(RecoveredMediaPickerResultKind.CapturePhoto, true);

			var firstResults = await WaitForCompletion(firstWaitTask);
			var secondResults = await WaitForCompletion(secondWaitTask);

			Assert.Equal(pendingCapture.Id, Assert.Single(firstResults).Id);
			Assert.Equal(pendingCapture.Id, Assert.Single(secondResults).Id);
			Assert.Equal(0, GetPendingWaiterCount());
		}

		[Fact]
		public async Task App_Startup_Scan_Returns_Recovered_Result_Without_Active_Waiter()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Jpg);

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			MediaPickerRecoveryManager.RecordCaptureCallbackResult(RecoveredMediaPickerResultKind.CapturePhoto, true);
			SimulateProcessRecreation();

			var results = await MediaPicker.GetRecoveredMediaPickerResultsAsync();

			var result = Assert.Single(results);
			Assert.Equal(pendingCapture.Id, result.Id);
			Assert.Equal(capturePath, GetSingleRecoveredFile(result).FullPath);
			Assert.Equal(0, GetPendingWaiterCount());
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public async Task App_Startup_Scan_Does_Not_Promote_Pending_Capture_When_Callback_Is_Not_Replayed()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Jpg);

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();

			var results = await MediaPicker.GetRecoveredMediaPickerResultsAsync();

			Assert.Empty(results);

			var activeCapture = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(pendingCapture.Id, activeCapture.Id);
			Assert.Equal(PendingMediaPickerState.Pending, activeCapture.State);
			Assert.Equal(capturePath, GetSingleActiveOperationFilePath(activeCapture));
		}

		[Fact]
		public async Task Wait_For_Recovered_Results_Does_Not_Complete_For_Pending_Capture_When_Callback_Is_Not_Replayed()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Mp4);
			using var cancellationTokenSource = new CancellationTokenSource();

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CaptureVideo,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);

			Assert.False(waitTask.IsCompleted);
			Assert.Equal(1, GetPendingWaiterCount());

			var activeCapture = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(pendingCapture.Id, activeCapture.Id);
			Assert.Equal(PendingMediaPickerState.Pending, activeCapture.State);
			Assert.Equal(capturePath, GetSingleActiveOperationFilePath(activeCapture));

			await cancellationTokenSource.CancelAsync();
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waitTask);
			Assert.Equal(0, GetPendingWaiterCount());
		}

		[Fact]
		public async Task App_Scoped_Wait_Completes_When_AndroidX_Publishes_Orphaned_Result()
		{
			var capturePath = CreateMissingMediaFilePath(FileExtensions.Mp4);
			var captureForResult = new TestMediaCaptureForResult(RecoveredMediaPickerResultKind.CaptureVideo);
			using var cancellationTokenSource = new CancellationTokenSource();

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CaptureVideo,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();

			Assert.Empty(await MediaPicker.GetRecoveredMediaPickerResultsAsync());

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			Assert.Equal(1, GetPendingWaiterCount());

			WriteNonEmptyMediaFile(capturePath);
			captureForResult.DispatchResultForTests(JavaBoolean.True);

			var results = await WaitForCompletion(waitTask);
			var result = Assert.Single(results);
			Assert.Equal(pendingCapture.Id, result.Id);
			Assert.Equal(capturePath, GetSingleRecoveredFile(result).FullPath);
			Assert.Equal(0, GetPendingWaiterCount());
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public async Task App_Scoped_Wait_Cancellation_Does_Not_Strand_Recoverable_Result()
		{
			var capturePath = CreateMissingMediaFilePath(FileExtensions.Jpg);
			using var cancellationTokenSource = new CancellationTokenSource();

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			Assert.Equal(1, GetPendingWaiterCount());

			await cancellationTokenSource.CancelAsync();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waitTask);
			Assert.Equal(0, GetPendingWaiterCount());
			Assert.Equal(pendingCapture.Id, GetActiveOperation()?.Id);

			WriteNonEmptyMediaFile(capturePath);
			await MediaPickerRecoveryManager.RecoverOrphanedCaptureResultAsync(RecoveredMediaPickerResultKind.CapturePhoto, true);

			var results = await MediaPicker.GetRecoveredMediaPickerResultsAsync();
			var result = Assert.Single(results);
			Assert.Equal(pendingCapture.Id, result.Id);
			Assert.Equal(capturePath, GetSingleRecoveredFile(result).FullPath);
			Assert.Equal(0, GetPendingWaiterCount());
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public async Task AndroidX_Orphaned_Success_Publishes_Recovered_Result_And_Completes_Waiter()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Mp4);
			var captureForResult = new TestMediaCaptureForResult(RecoveredMediaPickerResultKind.CaptureVideo);
			using var cancellationTokenSource = new CancellationTokenSource();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CaptureVideo,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			captureForResult.DispatchResultForTests(JavaBoolean.True);

			var recoveredResults = await WaitForCompletion(waitTask);
			var recoveredResult = Assert.Single(recoveredResults);
			Assert.Equal(pendingCapture.Id, recoveredResult.Id);
			Assert.Equal(capturePath, GetSingleRecoveredFile(recoveredResult).FullPath);
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public async Task AndroidX_Orphaned_Canceled_Result_Clears_Active_State_And_Completes_Waiter_Empty()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Mp4);
			var captureForResult = new TestMediaCaptureForResult(RecoveredMediaPickerResultKind.CaptureVideo);
			using var cancellationTokenSource = new CancellationTokenSource();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CaptureVideo,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			captureForResult.DispatchResultForTests(JavaBoolean.False);

			Assert.Empty(await WaitForCompletion(waitTask));
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public async Task AndroidX_Orphaned_Missing_Output_Clears_Active_State_And_Completes_Waiter_Empty()
		{
			var capturePath = CreateMissingMediaFilePath(FileExtensions.Jpg);
			var captureForResult = new TestMediaCaptureForResult(RecoveredMediaPickerResultKind.CapturePhoto);
			using var cancellationTokenSource = new CancellationTokenSource();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			captureForResult.DispatchResultForTests(JavaBoolean.True);

			Assert.Empty(await WaitForCompletion(waitTask));
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public async Task AndroidX_Orphaned_Single_Photo_Pick_Publishes_Recovered_Result_And_Completes_Waiter()
		{
			var pickPath = CreateNonEmptyMediaFile(FileExtensions.Jpg);
			var pickForResult = new TestPickVisualMediaForResult();
			using var cancellationTokenSource = new CancellationTokenSource();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			var pendingPick = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.PickPhoto,
				[],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			pickForResult.DispatchResultForTests(CreateFileUri(pickPath));

			var recoveredResult = Assert.Single(await WaitForCompletion(waitTask));
			Assert.Equal(pendingPick.Id, recoveredResult.Id);
			Assert.Equal(RecoveredMediaPickerResultKind.PickPhoto, recoveredResult.Kind);
			Assert.Equal(pickPath, GetSingleRecoveredFile(recoveredResult).FullPath);
			Assert.Null(GetActiveOperation());
		}

		[Theory]
		[InlineData(RecoveredMediaPickerResultKind.PickPhotos, FileExtensions.Jpg)]
		[InlineData(RecoveredMediaPickerResultKind.PickVideos, FileExtensions.Mp4)]
		public async Task AndroidX_Orphaned_Plural_Pick_From_Single_Picker_Publishes_Recovered_Result(
			RecoveredMediaPickerResultKind kind,
			string extension)
		{
			var pickPath = CreateNonEmptyMediaFile(extension);
			var pickForResult = new TestPickVisualMediaForResult();
			using var cancellationTokenSource = new CancellationTokenSource();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			var pendingPick = MediaPickerRecoveryManager.BeginOperation(
				kind,
				[],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			pickForResult.DispatchResultForTests(CreateFileUri(pickPath));

			var recoveredResult = Assert.Single(await WaitForCompletion(waitTask));
			Assert.Equal(pendingPick.Id, recoveredResult.Id);
			Assert.Equal(kind, recoveredResult.Kind);
			Assert.Equal(pickPath, GetSingleRecoveredFile(recoveredResult).FullPath);
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public async Task AndroidX_Orphaned_Multiple_Photo_Pick_Publishes_Recovered_Result_With_Multiple_Files()
		{
			var firstPickPath = CreateNonEmptyMediaFile(FileExtensions.Jpg);
			var secondPickPath = CreateNonEmptyMediaFile(FileExtensions.Jpg);
			var pickForResult = new TestPickMultipleVisualMediaForResult();
			using var cancellationTokenSource = new CancellationTokenSource();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			var pendingPick = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.PickPhotos,
				[],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			pickForResult.DispatchResultForTests(CreateUriList(firstPickPath, secondPickPath));

			var recoveredResult = Assert.Single(await WaitForCompletion(waitTask));
			Assert.Equal(pendingPick.Id, recoveredResult.Id);
			Assert.Equal(RecoveredMediaPickerResultKind.PickPhotos, recoveredResult.Kind);
			Assert.Equal(new[] { firstPickPath, secondPickPath }, recoveredResult.Files.Select(file => file.FullPath).ToArray());
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public void Pick_Callback_Records_Accepted_Uri_Before_Materialization()
		{
			var pickUri = AndroidUri.Parse("content://maui-test/missing-picked-media") ?? throw new InvalidOperationException("Unable to create invalid picker URI.");
			var pendingPick = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.PickPhoto,
				[],
				PersistedPhotoProcessingOptions.Default);

			Assert.True(MediaPickerRecoveryManager.RecordSinglePickCallbackResult(pickUri));

			var activePick = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(pendingPick.Id, activePick.Id);
			Assert.Equal(PendingMediaPickerState.ResultAccepted, activePick.State);
			Assert.Empty(activePick.FilePaths);
			Assert.Equal(pickUri.ToString(), GetSingleActiveOperationPickerUri(activePick));
		}

		[Fact]
		public async Task Accepted_Pick_Materialization_Writes_Accepted_File_Paths()
		{
			var pickPath = CreateNonEmptyMediaFile(FileExtensions.Jpg);
			var pendingPick = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.PickPhoto,
				[],
				PersistedPhotoProcessingOptions.Default);

			Assert.True(MediaPickerRecoveryManager.RecordSinglePickCallbackResult(CreateFileUri(pickPath)));

			var acceptedPaths = await MediaPickerRecoveryManager.MaterializeAcceptedFilePathsAsync(pendingPick.Id, throwOnMaterializationFailure: true);

			Assert.Equal(pickPath, Assert.Single(acceptedPaths));
			var activePick = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(pendingPick.Id, activePick.Id);
			Assert.Equal(PendingMediaPickerState.ResultAccepted, activePick.State);
			Assert.Equal(pickPath, GetSingleActiveOperationFilePath(activePick));
			Assert.Empty(activePick.PickerUriStrings);
		}

		[Fact]
		public async Task Accepted_Pick_Materialization_Failure_Throws_And_Clears_Active_State()
		{
			var invalidPickerUri = AndroidUri.Parse("content://maui-test/missing-picked-media") ?? throw new InvalidOperationException("Unable to create invalid picker URI.");
			var pendingPick = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.PickPhoto,
				[],
				PersistedPhotoProcessingOptions.Default);

			Assert.True(MediaPickerRecoveryManager.RecordSinglePickCallbackResult(invalidPickerUri));

			await Assert.ThrowsAnyAsync<Exception>(async () =>
				await MediaPickerRecoveryManager.MaterializeAcceptedFilePathsAsync(pendingPick.Id, throwOnMaterializationFailure: true));
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public async Task Accepted_Pick_Materialization_Failure_Clears_Active_State_And_Completes_Waiter_Empty()
		{
			var invalidPickerUri = AndroidUri.Parse("content://maui-test/missing-picked-media") ?? throw new InvalidOperationException("Unable to create invalid picker URI.");
			MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.PickPhoto,
				[],
				PersistedPhotoProcessingOptions.Default);

			Assert.True(MediaPickerRecoveryManager.RecordSinglePickCallbackResult(invalidPickerUri));
			SimulateProcessRecreation();

			var results = await MediaPicker.WaitForRecoveredMediaPickerResultsAsync(CancellationToken.None);

			Assert.Empty(results);
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public async Task AndroidX_Orphaned_Single_Pick_Empty_Result_Clears_Active_State_And_Completes_Waiter_Empty()
		{
			var pickForResult = new TestPickVisualMediaForResult();
			using var cancellationTokenSource = new CancellationTokenSource();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.PickVideo,
				[],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			pickForResult.DispatchResultForTests(AndroidUri.Empty);

			Assert.Empty(await WaitForCompletion(waitTask));
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public async Task AndroidX_Orphaned_Multiple_Pick_Empty_Result_Clears_Active_State_And_Completes_Waiter_Empty()
		{
			var pickForResult = new TestPickMultipleVisualMediaForResult();
			using var cancellationTokenSource = new CancellationTokenSource();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.PickVideos,
				[],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();
			pickForResult.DispatchResultForTests(new JavaList());

			Assert.Empty(await WaitForCompletion(waitTask));
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public async Task Live_Process_Pick_Clear_After_Accepted_Result_Prevents_Recovery()
		{
			var pickPath = CreateNonEmptyMediaFile(FileExtensions.Jpg);
			using var cancellationTokenSource = new CancellationTokenSource();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			var pendingPick = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.PickPhoto,
				[],
				PersistedPhotoProcessingOptions.Default);

			Assert.True(MediaPickerRecoveryManager.RecordSinglePickCallbackResult(CreateFileUri(pickPath)));
			MediaPickerRecoveryManager.ClearActiveOperation(pendingPick.Id);
			SimulateProcessRecreation();

			Assert.False(waitTask.IsCompleted);
			Assert.Empty(await MediaPicker.GetRecoveredMediaPickerResultsAsync());

			await cancellationTokenSource.CancelAsync();
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waitTask);
		}

		[Fact]
		public async Task AndroidX_Orphaned_Mismatched_Pick_Callback_Does_Not_Complete_Waiter()
		{
			var pickPath = CreateNonEmptyMediaFile(FileExtensions.Jpg);
			var pickForResult = new TestPickMultipleVisualMediaForResult();
			using var cancellationTokenSource = new CancellationTokenSource();

			var pendingPick = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.PickPhoto,
				[],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			Assert.Equal(1, GetPendingWaiterCount());

			pickForResult.DispatchResultForTests(CreateUriList(pickPath));

			Assert.False(waitTask.IsCompleted);
			Assert.Equal(1, GetPendingWaiterCount());

			var activePick = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(pendingPick.Id, activePick.Id);
			Assert.Equal(RecoveredMediaPickerResultKind.PickPhoto, activePick.Kind);
			Assert.Equal(PendingMediaPickerState.Pending, activePick.State);

			await cancellationTokenSource.CancelAsync();
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waitTask);
			Assert.Equal(0, GetPendingWaiterCount());
		}

		[Fact]
		public async Task ActivityForResultRequest_Rejects_Concurrent_Launch()
		{
			var captureForResult = new TestMediaCaptureForResult(RecoveredMediaPickerResultKind.CapturePhoto);
			var activeTaskCompletionSource = new TaskCompletionSource<JavaBoolean>(TaskCreationOptions.RunContinuationsAsynchronously);
			var activeLaunchCompletionSourceField = typeof(ActivityForResultRequest<TakePicture, JavaBoolean>)
				.GetField("activeLaunchCompletionSource", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(activeLaunchCompletionSourceField);

			// Seed the base request as if Launch already has one in-process activity result pending.
			activeLaunchCompletionSourceField.SetValue(captureForResult, activeTaskCompletionSource);

			await Assert.ThrowsAsync<InvalidOperationException>(() => captureForResult.Launch(AndroidUri.Empty));
		}

		[Fact]
		public async Task AndroidX_Orphaned_Mismatched_Media_Type_Does_Not_Complete_Waiter()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Jpg);
			using var cancellationTokenSource = new CancellationTokenSource();

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			Assert.Equal(1, GetPendingWaiterCount());

			await MediaPickerRecoveryManager.RecoverOrphanedCaptureResultAsync(RecoveredMediaPickerResultKind.CaptureVideo, true);

			Assert.False(waitTask.IsCompleted);
			Assert.Equal(1, GetPendingWaiterCount());

			var activeCapture = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(pendingCapture.Id, activeCapture.Id);
			Assert.Equal(RecoveredMediaPickerResultKind.CapturePhoto, activeCapture.Kind);
			Assert.Equal(PendingMediaPickerState.Pending, activeCapture.State);
			Assert.Equal(capturePath, GetSingleActiveOperationFilePath(activeCapture));

			await cancellationTokenSource.CancelAsync();
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waitTask);
			Assert.Equal(0, GetPendingWaiterCount());
		}

		[Fact]
		public void BeginOperation_Rejects_Second_InProcess_Capture_Before_Output_File_Exists()
		{
			var firstCapturePath = CreateMissingMediaFilePath(FileExtensions.Jpg);
			var secondCapturePath = CreateMissingMediaFilePath(FileExtensions.Mp4);

			var firstCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[firstCapturePath],
				PersistedPhotoProcessingOptions.Default);

			var exception = Assert.Throws<InvalidOperationException>(() =>
				MediaPickerRecoveryManager.BeginOperation(
					RecoveredMediaPickerResultKind.CaptureVideo,
					[secondCapturePath],
					PersistedPhotoProcessingOptions.Default));

			Assert.Equal("A MediaPicker operation is already in progress.", exception.Message);

			var activeCapture = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(firstCapture.Id, activeCapture.Id);
			Assert.Equal(RecoveredMediaPickerResultKind.CapturePhoto, activeCapture.Kind);
			Assert.Equal(PendingMediaPickerState.Pending, activeCapture.State);
			Assert.Equal(firstCapturePath, GetSingleActiveOperationFilePath(activeCapture));
		}

		[Fact]
		public void Rejected_Concurrent_Capture_Does_Not_Overwrite_Active_Record()
		{
			var firstCapturePath = CreateMissingMediaFilePath(FileExtensions.Jpg);
			var secondCapturePath = CreateNonEmptyMediaFile(FileExtensions.Mp4);

			var firstCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[firstCapturePath],
				new PersistedPhotoProcessingOptions(640, null, 70, false, true));

			Assert.Throws<InvalidOperationException>(() =>
				MediaPickerRecoveryManager.BeginOperation(
					RecoveredMediaPickerResultKind.CaptureVideo,
					[secondCapturePath],
					new PersistedPhotoProcessingOptions(null, 480, 80, false, true)));

			var activeCapture = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(firstCapture.Id, activeCapture.Id);
			Assert.Equal(RecoveredMediaPickerResultKind.CapturePhoto, activeCapture.Kind);
			Assert.Equal(firstCapturePath, GetSingleActiveOperationFilePath(activeCapture));
			Assert.Equal(640, activeCapture.PhotoProcessingOptions.MaximumWidth.GetValueOrDefault());
			Assert.Null(activeCapture.PhotoProcessingOptions.MaximumHeight);
			Assert.Equal(70, activeCapture.PhotoProcessingOptions.CompressionQuality);
		}

		[Fact]
		public void Recreated_Pending_Capture_Blocks_New_Capture_Until_Replayed()
		{
			var firstCapturePath = CreateMissingMediaFilePath(FileExtensions.Jpg);
			var secondCapturePath = CreateMissingMediaFilePath(FileExtensions.Mp4);

			var firstCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[firstCapturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();

			var exception = Assert.Throws<InvalidOperationException>(() =>
				MediaPickerRecoveryManager.BeginOperation(
					RecoveredMediaPickerResultKind.CaptureVideo,
					[secondCapturePath],
					PersistedPhotoProcessingOptions.Default));

			Assert.Equal("A MediaPicker operation is pending AndroidX result replay.", exception.Message);

			var activeCapture = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(firstCapture.Id, activeCapture.Id);
			Assert.Equal(RecoveredMediaPickerResultKind.CapturePhoto, activeCapture.Kind);
			Assert.Equal(PendingMediaPickerState.Pending, activeCapture.State);
			Assert.Equal(firstCapturePath, GetSingleActiveOperationFilePath(activeCapture));
		}

		[Fact]
		public async Task Recreated_Pending_Capture_Blocked_New_Capture_Does_Not_Complete_Waiter()
		{
			var firstCapturePath = CreateMissingMediaFilePath(FileExtensions.Jpg);
			var secondCapturePath = CreateMissingMediaFilePath(FileExtensions.Mp4);
			using var cancellationTokenSource = new CancellationTokenSource();

			var firstCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[firstCapturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			Assert.Equal(1, GetPendingWaiterCount());

			var exception = Assert.Throws<InvalidOperationException>(() =>
				MediaPickerRecoveryManager.BeginOperation(
					RecoveredMediaPickerResultKind.CaptureVideo,
					[secondCapturePath],
					PersistedPhotoProcessingOptions.Default));

			Assert.Equal("A MediaPicker operation is pending AndroidX result replay.", exception.Message);
			Assert.False(waitTask.IsCompleted);
			Assert.Equal(1, GetPendingWaiterCount());

			var activeCapture = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(firstCapture.Id, activeCapture.Id);
			Assert.Equal(RecoveredMediaPickerResultKind.CapturePhoto, activeCapture.Kind);
			Assert.Equal(PendingMediaPickerState.Pending, activeCapture.State);
			Assert.Equal(firstCapturePath, GetSingleActiveOperationFilePath(activeCapture));

			await cancellationTokenSource.CancelAsync();
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waitTask);
			Assert.Equal(0, GetPendingWaiterCount());
		}

		[Theory]
		[InlineData(RecoveredMediaPickerResultKind.PickPhoto)]
		[InlineData(RecoveredMediaPickerResultKind.PickPhotos)]
		public async Task Recreated_Pending_Pick_Blocks_New_Operation_And_Does_Not_Complete_Waiter(RecoveredMediaPickerResultKind kind)
		{
			var capturePath = CreateMissingMediaFilePath(FileExtensions.Mp4);
			using var cancellationTokenSource = new CancellationTokenSource();

			var pendingPick = MediaPickerRecoveryManager.BeginOperation(
				kind,
				[],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			Assert.Equal(1, GetPendingWaiterCount());

			var exception = Assert.Throws<InvalidOperationException>(() =>
				MediaPickerRecoveryManager.BeginOperation(
					RecoveredMediaPickerResultKind.CaptureVideo,
					[capturePath],
					PersistedPhotoProcessingOptions.Default));

			Assert.Equal("A MediaPicker operation is pending AndroidX result replay.", exception.Message);
			Assert.False(waitTask.IsCompleted);
			Assert.Equal(1, GetPendingWaiterCount());

			var activePick = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(pendingPick.Id, activePick.Id);
			Assert.Equal(kind, activePick.Kind);
			Assert.Equal(PendingMediaPickerState.Pending, activePick.State);
			Assert.Empty(activePick.FilePaths);
			Assert.Empty(activePick.PickerUriStrings);

			await cancellationTokenSource.CancelAsync();
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waitTask);
			Assert.Equal(0, GetPendingWaiterCount());
		}

		[Fact]
		public void Recreated_Accepted_Capture_Blocks_New_Capture_Until_Recovered()
		{
			var firstCapturePath = CreateNonEmptyMediaFile(FileExtensions.Jpg);
			var secondCapturePath = CreateMissingMediaFilePath(FileExtensions.Mp4);

			var firstCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[firstCapturePath],
				PersistedPhotoProcessingOptions.Default);

			MediaPickerRecoveryManager.RecordCaptureCallbackResult(RecoveredMediaPickerResultKind.CapturePhoto, true);
			SimulateProcessRecreation();

			var exception = Assert.Throws<InvalidOperationException>(() =>
				MediaPickerRecoveryManager.BeginOperation(
					RecoveredMediaPickerResultKind.CaptureVideo,
					[secondCapturePath],
					PersistedPhotoProcessingOptions.Default));

			Assert.Equal("A MediaPicker result is pending recovery.", exception.Message);

			var activeCapture = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(firstCapture.Id, activeCapture.Id);
			Assert.Equal(RecoveredMediaPickerResultKind.CapturePhoto, activeCapture.Kind);
			Assert.Equal(PendingMediaPickerState.ResultAccepted, activeCapture.State);
			Assert.Equal(firstCapturePath, GetSingleActiveOperationFilePath(activeCapture));
		}

		[Fact]
		public async Task New_Capture_Can_Start_After_Recreated_Pending_Capture_Is_Canceled()
		{
			var firstCapturePath = CreateMissingMediaFilePath(FileExtensions.Jpg);
			var secondCapturePath = CreateMissingMediaFilePath(FileExtensions.Mp4);

			var firstCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[firstCapturePath],
				PersistedPhotoProcessingOptions.Default);

			SimulateProcessRecreation();

			await MediaPickerRecoveryManager.RecoverOrphanedCaptureResultAsync(RecoveredMediaPickerResultKind.CapturePhoto, false);

			Assert.Null(GetActiveOperation());

			var secondCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CaptureVideo,
				[secondCapturePath],
				PersistedPhotoProcessingOptions.Default);

			Assert.NotEqual(firstCapture.Id, secondCapture.Id);
			Assert.Equal(secondCapture.Id, GetActiveOperation()?.Id);
		}

		[Fact]
		public void Pending_Capture_Persists_PhotoProcessingOptions()
		{
			var capturePath = CreateMissingMediaFilePath(FileExtensions.Jpg);
			var options = new PersistedPhotoProcessingOptions(640, 480, 70, true, false);

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				options);

			SimulateProcessRecreation();

			var activeCapture = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(pendingCapture.Id, activeCapture.Id);
			Assert.Equal(options.MaximumWidth, activeCapture.PhotoProcessingOptions.MaximumWidth);
			Assert.Equal(options.MaximumHeight, activeCapture.PhotoProcessingOptions.MaximumHeight);
			Assert.Equal(options.CompressionQuality, activeCapture.PhotoProcessingOptions.CompressionQuality);
			Assert.Equal(options.RotateImage, activeCapture.PhotoProcessingOptions.RotateImage);
			Assert.Equal(options.PreserveMetaData, activeCapture.PhotoProcessingOptions.PreserveMetaData);
		}

		[Fact]
		public void PhotoProcessingOptions_Are_Created_From_MediaPickerOptions()
		{
			var options = new MediaPickerOptions
			{
				MaximumWidth = 640,
				MaximumHeight = 480,
				CompressionQuality = 70,
				RotateImage = true,
				PreserveMetaData = false
			};

			var processingOptions = MediaPickerImplementation.GetPhotoProcessingOptions(options);

			Assert.Equal(options.MaximumWidth, processingOptions.MaximumWidth);
			Assert.Equal(options.MaximumHeight, processingOptions.MaximumHeight);
			Assert.Equal(options.CompressionQuality, processingOptions.CompressionQuality);
			Assert.Equal(options.RotateImage, processingOptions.RotateImage);
			Assert.Equal(options.PreserveMetaData, processingOptions.PreserveMetaData);
		}

		[Theory]
		[InlineData("1", (int)PendingMediaPickerState.Pending)]
		[InlineData("2", (int)PendingMediaPickerState.ResultAccepted)]
		public void Pending_Capture_Reads_Previous_Unshipped_Formats(string version, int expectedStateValue)
		{
			var expectedState = (PendingMediaPickerState)expectedStateValue;
			var id = Guid.NewGuid().ToString("N");
			var capturePath = CreateMissingMediaFilePath(FileExtensions.Jpg);
			var serializedCapture = version == "1"
				? SerializeLegacyPendingCaptureForTests(id, RecoveredMediaPickerResultKind.CapturePhoto, capturePath)
				: SerializePendingCaptureWithOutputUriForTests(id, RecoveredMediaPickerResultKind.CapturePhoto, expectedState, capturePath);

			SetSerializedActiveOperation(serializedCapture);

			var activeCapture = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(id, activeCapture.Id);
			Assert.Equal(RecoveredMediaPickerResultKind.CapturePhoto, activeCapture.Kind);
			Assert.Equal(expectedState, activeCapture.State);
			Assert.Equal(capturePath, GetSingleActiveOperationFilePath(activeCapture));
			Assert.Equal(640, activeCapture.PhotoProcessingOptions.MaximumWidth.GetValueOrDefault());
			Assert.Equal(480, activeCapture.PhotoProcessingOptions.MaximumHeight.GetValueOrDefault());
			Assert.Equal(70, activeCapture.PhotoProcessingOptions.CompressionQuality);
			Assert.True(activeCapture.PhotoProcessingOptions.RotateImage);
			Assert.False(activeCapture.PhotoProcessingOptions.PreserveMetaData);
		}

		[Fact]
		public void Pending_Operation_Reads_Previous_Unshipped_Format_Without_PickerUris()
		{
			var id = Guid.NewGuid().ToString("N");
			var capturePath = CreateMissingMediaFilePath(FileExtensions.Jpg);
			var serializedCapture = SerializePendingOperationWithoutPickerUrisForTests(
				id,
				RecoveredMediaPickerResultKind.CapturePhoto,
				PendingMediaPickerState.ResultAccepted,
				capturePath);

			SetSerializedActiveOperation(serializedCapture);

			var activeCapture = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(id, activeCapture.Id);
			Assert.Equal(PendingMediaPickerState.ResultAccepted, activeCapture.State);
			Assert.Equal(capturePath, GetSingleActiveOperationFilePath(activeCapture));
			Assert.Empty(activeCapture.PickerUriStrings);
		}

		[Fact]
		public void Pending_Capture_Writes_Current_Format_Without_OutputUri()
		{
			var capturePath = CreateMissingMediaFilePath(FileExtensions.Jpg);

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				new PersistedPhotoProcessingOptions(640, 480, 70, true, false));

			var serializedCapture = Assert.IsType<string>(GetSerializedActiveOperation());
			var parts = serializedCapture.Split('|');

			Assert.Equal("5", parts[0]);
			Assert.Equal(11, parts.Length);
			Assert.Equal(EncodePreferenceValueForTests(pendingCapture.Id), parts[1]);
			Assert.Equal(((int)RecoveredMediaPickerResultKind.CapturePhoto).ToString(CultureInfo.InvariantCulture), parts[2]);
			Assert.Equal(((int)PendingMediaPickerState.Pending).ToString(CultureInfo.InvariantCulture), parts[3]);
			Assert.Equal(EncodePreferenceValueForTests(capturePath), parts[4]);
			Assert.Empty(parts[5]);
			Assert.Equal("640", parts[6]);
			Assert.Equal("480", parts[7]);
			Assert.Equal("70", parts[8]);
			Assert.Equal("1", parts[9]);
			Assert.Equal("0", parts[10]);
		}

		[Fact]
		public async Task ProcessPhotoPreservingSource_Writes_Separate_File_And_Leaves_Source_Intact()
		{
			var capturePath = CreateValidJpegMediaFile();
			var originalBytes = await File.ReadAllBytesAsync(capturePath);

			var processedPath = await MediaPickerImplementation.ProcessPhotoPreservingSourceAsync(
				capturePath,
				new PersistedPhotoProcessingOptions(16, null, 70, false, false));

			Assert.NotEqual(capturePath, processedPath);
			Assert.True(File.Exists(capturePath));
			Assert.Equal(originalBytes, await File.ReadAllBytesAsync(capturePath));
			Assert.True(new FileInfo(processedPath).Length > 0);
		}

		[Fact]
		public async Task ProcessPhotoPreservingSource_InvalidRotationInput_Leaves_Source_Intact()
		{
			var invalidJpegPath = CreateNonEmptyMediaFile(FileExtensions.Jpg);
			var originalBytes = await File.ReadAllBytesAsync(invalidJpegPath);

			var processedPath = await MediaPickerImplementation.ProcessPhotoPreservingSourceAsync(
				invalidJpegPath,
				new PersistedPhotoProcessingOptions(null, null, 100, true, true));

			Assert.NotEqual(invalidJpegPath, processedPath);
			Assert.True(File.Exists(invalidJpegPath));
			Assert.Equal(originalBytes, await File.ReadAllBytesAsync(invalidJpegPath));
			Assert.True(File.Exists(processedPath));
			Assert.Equal(originalBytes, await File.ReadAllBytesAsync(processedPath));
		}

		[Fact]
		public async Task Recovered_Photo_Processing_Queues_Processed_Result_And_Clears_Active_State()
		{
			var capturePath = CreateValidJpegMediaFile();
			var originalBytes = await File.ReadAllBytesAsync(capturePath);

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				new PersistedPhotoProcessingOptions(16, null, 70, false, false));

			MediaPickerRecoveryManager.RecordCaptureCallbackResult(RecoveredMediaPickerResultKind.CapturePhoto, true);
			SimulateProcessRecreation();

			var results = await MediaPicker.GetRecoveredMediaPickerResultsAsync();

			var result = Assert.Single(results);
			Assert.Equal(pendingCapture.Id, result.Id);
			Assert.NotEqual(capturePath, GetSingleRecoveredFile(result).FullPath);
			Assert.True(File.Exists(capturePath));
			Assert.Equal(originalBytes, await File.ReadAllBytesAsync(capturePath));
			Assert.True(new FileInfo(GetSingleRecoveredFile(result).FullPath).Length > 0);
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public async Task Accepted_Result_From_Recreated_Process_Is_Promoted_By_Get()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Jpg);

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			MediaPickerRecoveryManager.RecordCaptureCallbackResult(RecoveredMediaPickerResultKind.CapturePhoto, true);
			SimulateProcessRecreation();

			var results = await MediaPicker.GetRecoveredMediaPickerResultsAsync();

			var result = Assert.Single(results);
			Assert.Equal(pendingCapture.Id, result.Id);
			Assert.Equal(capturePath, GetSingleRecoveredFile(result).FullPath);
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public async Task Accepted_Result_From_Recreated_Process_Completes_Wait()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Mp4);

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CaptureVideo,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			MediaPickerRecoveryManager.RecordCaptureCallbackResult(RecoveredMediaPickerResultKind.CaptureVideo, true);
			SimulateProcessRecreation();

			var results = await MediaPicker.WaitForRecoveredMediaPickerResultsAsync(CancellationToken.None);

			var result = Assert.Single(results);
			Assert.Equal(pendingCapture.Id, result.Id);
			Assert.Equal(capturePath, GetSingleRecoveredFile(result).FullPath);
			Assert.Null(GetActiveOperation());
		}

		[Fact]
		public async Task Accepted_Result_In_Current_Process_Is_Not_Promoted()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Jpg);

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			MediaPickerRecoveryManager.RecordCaptureCallbackResult(RecoveredMediaPickerResultKind.CapturePhoto, true);

			Assert.Empty(await MediaPicker.GetRecoveredMediaPickerResultsAsync());

			var activeCapture = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(pendingCapture.Id, activeCapture.Id);
			Assert.Equal(PendingMediaPickerState.ResultAccepted, activeCapture.State);
		}

		[Fact]
		public async Task Pending_Result_In_Current_Process_Is_Not_Promoted_From_Output_File()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Jpg);

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			Assert.Empty(await MediaPicker.GetRecoveredMediaPickerResultsAsync());

			var activeCapture = Assert.IsType<PendingMediaPickerOperation>(GetActiveOperation());
			Assert.Equal(pendingCapture.Id, activeCapture.Id);
			Assert.Equal(PendingMediaPickerState.Pending, activeCapture.State);
		}

		[Fact]
		public async Task Live_Process_Capture_Clear_After_Accepted_Result_Prevents_Recovery()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Jpg);
			using var cancellationTokenSource = new CancellationTokenSource();

			var waitTask = MediaPicker.WaitForRecoveredMediaPickerResultsAsync(cancellationTokenSource.Token);
			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			MediaPickerRecoveryManager.RecordCaptureCallbackResult(RecoveredMediaPickerResultKind.CapturePhoto, true);
			MediaPickerRecoveryManager.ClearActiveOperation(pendingCapture.Id);
			SimulateProcessRecreation();

			Assert.False(waitTask.IsCompleted);
			Assert.Empty(await MediaPicker.GetRecoveredMediaPickerResultsAsync());

			await cancellationTokenSource.CancelAsync();
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waitTask);
		}

		[Fact]
		public async Task New_Capture_Can_Start_After_Recreated_Accepted_Result_Is_Recovered()
		{
			var firstCapturePath = CreateNonEmptyMediaFile(FileExtensions.Jpg);
			var secondCapturePath = CreateMissingMediaFilePath(FileExtensions.Mp4);

			var firstCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[firstCapturePath],
				PersistedPhotoProcessingOptions.Default);

			MediaPickerRecoveryManager.RecordCaptureCallbackResult(RecoveredMediaPickerResultKind.CapturePhoto, true);
			SimulateProcessRecreation();

			var recoveredResults = await MediaPickerRecoveryManager.RecoverOperationIfAvailableAsync();

			Assert.Equal(firstCapture.Id, Assert.Single(recoveredResults).Id);

			var secondCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CaptureVideo,
				[secondCapturePath],
				PersistedPhotoProcessingOptions.Default);

			Assert.NotEqual(firstCapture.Id, secondCapture.Id);
			Assert.Equal(secondCapture.Id, GetActiveOperation()?.Id);
		}

		[Fact]
		public async Task Concurrent_Accepted_Result_Promotion_Queues_Single_Result()
		{
			var capturePath = CreateNonEmptyMediaFile(FileExtensions.Mp4);

			var pendingCapture = MediaPickerRecoveryManager.BeginOperation(
				RecoveredMediaPickerResultKind.CaptureVideo,
				[capturePath],
				PersistedPhotoProcessingOptions.Default);

			MediaPickerRecoveryManager.RecordCaptureCallbackResult(RecoveredMediaPickerResultKind.CaptureVideo, true);
			SimulateProcessRecreation();

			var recoveryTasks = Enumerable.Range(0, 8)
				.Select(_ => MediaPicker.GetRecoveredMediaPickerResultsAsync())
				.ToArray();

			var allResults = await Task.WhenAll(recoveryTasks);
			var queuedResults = await MediaPicker.GetRecoveredMediaPickerResultsAsync();

			foreach (var results in allResults)
			{
				Assert.Equal(pendingCapture.Id, Assert.Single(results).Id);
			}

			Assert.Equal(pendingCapture.Id, Assert.Single(queuedResults).Id);
		}

		static string CreateNonEmptyMediaFile(string extension)
		{
			var path = CreateCacheFilePath(extension);
			WriteNonEmptyMediaFile(path);
			return path;
		}

		// Device-test hangs are expensive and obscure the failure, so expected completions go
		// through a bounded wait instead of awaiting recovery waiters directly.
		static async Task<T> WaitForCompletion<T>(Task<T> task)
		{
			var completedTask = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(5)));
			Assert.Same(task, completedTask);
			return await task;
		}

		static void WriteNonEmptyMediaFile(string path)
			=> File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });

		// Some photo-processing tests need a real JPEG. The smaller dummy media files above are
		// enough for recovery state tests, but not for bitmap processing.
		static string CreateValidJpegMediaFile()
		{
			var path = CreateCacheFilePath(FileExtensions.Jpg);

			var bitmapConfig = ABitmap.Config.Argb8888 ?? throw new InvalidOperationException("Unable to create a bitmap config.");
			using var bitmap = ABitmap.CreateBitmap(64, 64, bitmapConfig) ?? throw new InvalidOperationException("Unable to create a bitmap.");
			bitmap.EraseColor(AColor.Red);

			var jpegFormat = ABitmap.CompressFormat.Jpeg ?? throw new InvalidOperationException("Unable to create a JPEG format.");
			using var stream = File.Create(path);
			Assert.True(bitmap.Compress(jpegFormat, 100, stream));

			return path;
		}

		static string CreateMissingMediaFilePath(string extension)
			=> CreateCacheFilePath(extension);

		static void SimulateProcessRecreation()
		{
			// Clear only in-memory operation ownership. Durable preference state remains so the next
			// recovery scan behaves like a recreated app process receiving AndroidX result replay.
			ClearInProcessOperationIds();
		}

		static void ResetRecoveryState()
		{
			ClearInProcessOperationIds();
			CompleteAndClearRecoveryWaiters();
			SetRecoveryReconciliationGeneration(0);
			Preferences.Remove(ActiveOperationPreferenceKey, RecoveryPreferencesSharedName);
			Preferences.Remove(RecoveredResultsPreferenceKey, RecoveryPreferencesSharedName);
		}

		static PendingMediaPickerOperation GetActiveOperation()
			=> MediaPickerRecoveryStore.ReadActiveOperation();

		static int GetPendingWaiterCount()
			=> GetRecoveryWaiters().Count;

		static string GetSerializedActiveOperation()
			=> Preferences.Get(ActiveOperationPreferenceKey, null, RecoveryPreferencesSharedName);

		static void SetSerializedActiveOperation(string value)
			=> Preferences.Set(ActiveOperationPreferenceKey, value, RecoveryPreferencesSharedName);

		static void ClearInProcessOperationIds()
		{
			var ids = GetPrivateStaticField<object>("InProcessOperationIds");
			ids.GetType().GetMethod("Clear")?.Invoke(ids, null);
		}

		static void CompleteAndClearRecoveryWaiters()
		{
			var waiters = GetRecoveryWaiters();
			var waiterSnapshot = waiters.Cast<object>().ToArray();

			foreach (var waiter in waiterSnapshot)
			{
				waiter.GetType()
					.GetMethod("TrySetResult")
					?.Invoke(waiter, new object[] { Array.Empty<RecoveredMediaPickerResult>() });
			}

			waiters.Clear();
		}

		static System.Collections.IList GetRecoveryWaiters()
			=> GetPrivateStaticField<System.Collections.IList>("RecoveryWaiters");

		static void SetRecoveryReconciliationGeneration(long value)
		{
			var field = typeof(MediaPickerRecoveryManager)
				.GetField("RecoveryReconciliationGeneration", BindingFlags.Static | BindingFlags.NonPublic);
			Assert.NotNull(field);
			field.SetValue(null, value);
		}

		static T GetPrivateStaticField<T>(string name)
		{
			var field = typeof(MediaPickerRecoveryManager)
				.GetField(name, BindingFlags.Static | BindingFlags.NonPublic);
			Assert.NotNull(field);
			var value = field.GetValue(null);
			Assert.NotNull(value);
			return (T)value;
		}

		static string CreateCacheFilePath(string extension)
		{
			var cacheDirectory = FileSystem.CacheDirectory ?? throw new InvalidOperationException("FileSystem.CacheDirectory is not available.");
			return Path.Combine(cacheDirectory, $"{Guid.NewGuid():N}{extension}");
		}

		static AndroidUri CreateFileUri(string path)
			=> AndroidUri.FromFile(new Java.IO.File(path)) ?? throw new InvalidOperationException("Unable to create a file URI.");

		static JavaList CreateUriList(params string[] paths)
		{
			var list = new JavaList();

			foreach (var path in paths)
			{
				list.Add(CreateFileUri(path));
			}

			return list;
		}

		static FileResult GetSingleRecoveredFile(RecoveredMediaPickerResult result)
			=> Assert.Single(result.Files);

		static string GetSingleActiveOperationFilePath(PendingMediaPickerOperation operation)
			=> Assert.Single(operation.FilePaths);

		static string GetSingleActiveOperationPickerUri(PendingMediaPickerOperation operation)
			=> Assert.Single(operation.PickerUriStrings);

		static string SerializeLegacyPendingCaptureForTests(string id, RecoveredMediaPickerResultKind kind, string filePath)
			=> string.Join("|", new[]
			{
				"1",
				EncodePreferenceValueForTests(id),
				GetLegacyCaptureKindValue(kind).ToString(CultureInfo.InvariantCulture),
				EncodePreferenceValueForTests(filePath),
				EncodePreferenceValueForTests("content://maui-test/media-capture/legacy"),
				"640",
				"480",
				"70",
				"1",
				"0"
			});

		static string SerializePendingCaptureWithOutputUriForTests(string id, RecoveredMediaPickerResultKind kind, PendingMediaPickerState state, string filePath)
			=> string.Join("|", new[]
			{
				"2",
				EncodePreferenceValueForTests(id),
				GetLegacyCaptureKindValue(kind).ToString(CultureInfo.InvariantCulture),
				((int)state).ToString(CultureInfo.InvariantCulture),
				EncodePreferenceValueForTests(filePath),
				EncodePreferenceValueForTests("content://maui-test/media-capture/previous"),
				"640",
				"480",
				"70",
				"1",
				"0"
				});

		static string SerializePendingOperationWithoutPickerUrisForTests(string id, RecoveredMediaPickerResultKind kind, PendingMediaPickerState state, string filePath)
			=> string.Join("|", new[]
			{
				"4",
				EncodePreferenceValueForTests(id),
				((int)kind).ToString(CultureInfo.InvariantCulture),
				((int)state).ToString(CultureInfo.InvariantCulture),
				EncodePreferenceValueForTests(filePath),
				"640",
				"480",
				"70",
				"1",
				"0"
			});

		static int GetLegacyCaptureKindValue(RecoveredMediaPickerResultKind kind)
			=> kind == RecoveredMediaPickerResultKind.CaptureVideo ? 1 : 0;

		static string EncodePreferenceValueForTests(string value)
			=> Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? string.Empty));

		sealed class TestMediaCaptureForResult : MediaCaptureForResult<TakePicture>
		{
			public TestMediaCaptureForResult(RecoveredMediaPickerResultKind kind)
				: base(kind)
			{
			}

			public void DispatchResultForTests(JavaBoolean result)
				=> HandleActivityResult(result);
		}

		sealed class TestPickVisualMediaForResult : PickVisualMediaForResult
		{
			public void DispatchResultForTests(AndroidUri result)
				=> HandleActivityResult(result);
		}

		sealed class TestPickMultipleVisualMediaForResult : PickMultipleVisualMediaForResult
		{
			public void DispatchResultForTests(JavaList result)
				=> HandleActivityResult(result);
		}
	}
}
