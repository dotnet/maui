#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Media
{
	/// <summary>
	/// A MediaPicker result that was recovered after the app process was recreated.
	/// </summary>
	public sealed class RecoveredMediaPickerResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RecoveredMediaPickerResult"/> class.
		/// </summary>
		/// <param name="id">The identifier of the recovered result.</param>
		/// <param name="kind">The kind of MediaPicker operation that produced the result.</param>
		/// <param name="files">The recovered media files.</param>
		public RecoveredMediaPickerResult(string id, RecoveredMediaPickerResultKind kind, IReadOnlyList<FileResult> files)
		{
			Id = id ?? throw new ArgumentNullException(nameof(id));
			Kind = kind;

			ArgumentNullException.ThrowIfNull(files);

			var fileArray = files.ToArray();
			if (fileArray.Length == 0)
			{
				throw new ArgumentException("At least one recovered media file is required.", nameof(files));
			}

			if (fileArray.Any(file => file is null))
			{
				throw new ArgumentException("Recovered media files cannot contain null values.", nameof(files));
			}

			Files = Array.AsReadOnly(fileArray);
		}

		/// <summary>
		/// Gets the identifier of the recovered result.
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// Gets the kind of MediaPicker operation that produced the recovered result.
		/// </summary>
		public RecoveredMediaPickerResultKind Kind { get; }

		/// <summary>
		/// Gets the recovered media files.
		/// </summary>
		public IReadOnlyList<FileResult> Files { get; }
	}

	/// <summary>
	/// Describes the Android MediaPicker operation that produced a recovered result.
	/// </summary>
	public enum RecoveredMediaPickerResultKind
	{
		/// <summary>A captured photo.</summary>
		CapturePhoto,

		/// <summary>A captured video.</summary>
		CaptureVideo,

		/// <summary>A single picked photo.</summary>
		PickPhoto,

		/// <summary>One or more picked photos.</summary>
		PickPhotos,

		/// <summary>A single picked video.</summary>
		PickVideo,

		/// <summary>One or more picked videos.</summary>
		PickVideos
	}

	/// <summary>
	/// The MediaPicker API lets a user pick or take a photo or video on the device.
	/// </summary>
	public static partial class MediaPicker
	{
		/// <summary>
		/// Gets Android MediaPicker results that were recovered after the app process was recreated.
		/// </summary>
		/// <returns>A non-consuming list of recovered MediaPicker results.</returns>
		/// <remarks>
		/// <para>The operating system may destroy the app process while a system picker or camera is foregrounded. AndroidX activity-result replay is the recovery signal; file existence alone does not publish a recovered result. Apps should persist their own workflow state before starting MediaPicker operations, then use this method during startup or resume to associate recovered media with that state.</para>
		/// </remarks>
		public static Task<IReadOnlyList<RecoveredMediaPickerResult>> GetRecoveredMediaPickerResultsAsync()
			=> MediaPickerRecoveryManager.GetRecoveredResultsAsync();

		/// <summary>
		/// Waits for Android MediaPicker recovery to reconcile a pending result.
		/// </summary>
		/// <param name="cancellationToken">A cancellable token that cancels the wait and removes the recovery listener.</param>
		/// <returns>A non-consuming list of recovered MediaPicker results.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="cancellationToken"/> cannot be canceled.</exception>
		/// <remarks>
		/// <para>With a cancellable token, if recovered results are already available, this method returns them immediately. Otherwise, it waits until AndroidX result replay publishes or terminally clears a pending MediaPicker result. This method is one-shot; apps that need continuous observation should call it again with a lifecycle-scoped cancellation token.</para>
		/// <para>If AndroidX does not replay or reconcile a pending result, this method may wait until <paramref name="cancellationToken"/> is canceled.</para>
		/// </remarks>
		public static Task<IReadOnlyList<RecoveredMediaPickerResult>> WaitForRecoveredMediaPickerResultsAsync(CancellationToken cancellationToken)
			=> MediaPickerRecoveryManager.WaitForRecoveredResultsAsync(cancellationToken);

		/// <summary>
		/// Discards an Android MediaPicker operation that is still pending recovery.
		/// </summary>
		/// <returns>A task that represents the asynchronous discard operation.</returns>
		/// <remarks>
		/// <para>This Android recovery escape hatch is intended for cases where AndroidX does not replay or reconcile a pending MediaPicker result. Calling this method may discard a result that AndroidX has not replayed or that has not yet been published as recovered.</para>
		/// <para>This method does not cancel an in-process picker or capture operation.</para>
		/// </remarks>
		public static Task DiscardPendingMediaPickerOperationAsync()
			=> MediaPickerRecoveryManager.DiscardPendingOperationAsync();

		/// <summary>
		/// Clears an Android MediaPicker result that was recovered after the app process was recreated.
		/// </summary>
		/// <param name="id">The identifier of the recovered result to clear.</param>
		/// <returns>A task that represents the asynchronous clear operation.</returns>
		public static Task ClearRecoveredMediaPickerResultAsync(string id)
		{
			if (id is null)
			{
				throw new ArgumentNullException(nameof(id));
			}

			return MediaPickerRecoveryManager.ClearRecoveredResultAsync(id);
		}
	}
}
