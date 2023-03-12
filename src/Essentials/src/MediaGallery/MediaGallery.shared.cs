#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Media
{
	/// <summary>
	/// Performs operations with media files.
	/// </summary>
	public interface IMediaGallery
	{
		/// <summary>
		/// Gets a value indicating whether <see cref="IMediaGallery"/> is supported on this platform.
		/// </summary>
		bool IsSupported { get; }

		/// <summary>
		/// Gets a value indicating whether capturing media is supported on this device.
		/// </summary>
		/// <param name="type">Media file type use for capture</param>
		/// <returns></returns>
		bool CheckCaptureSupport(MediaFileType type);

		/// <summary>
		/// Opens the camera to take a photo or video.
		/// </summary>
		/// <param name="type">Media file type use for capture</param>
		/// <param name="token">A token that can be used for cancelling the operation.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the captured photo.</returns>
		Task<MediaFilesResult> CaptureAsync(MediaFileType type, CancellationToken token = default);

		/// <summary>
		/// Opens media files Picker
		/// </summary>
		/// <param name="selectionLimit"><inheritdoc cref="MediaPickRequest.SelectionLimit" path="/summary"/></param>
		/// <param name="types"><inheritdoc cref="MediaPickRequest.Types" path="/summary"/></param>
		/// <returns>Media files selected by a user.</returns>
		Task<MediaFilesResult> PickAsync(int selectionLimit = 1, params MediaFileType[] types);

		/// <param name="request">Media file request to pick.</param>
		/// <param name="token">A token that can be used for cancelling the operation.</param>
		/// <inheritdoc cref = "PickAsync(int, MediaFileType[])" path="//*[not(self::param)]"/>
		Task<MediaFilesResult> PickAsync(MediaPickRequest request, CancellationToken token = default);

		/// <summary>
		/// <inheritdoc cref="MultiPickingBehaviour" path="/summary"/>
		/// </summary>
		/// <returns></returns>
		MultiPickingBehaviour GetMultiPickingBehaviour();

		/// <summary>
		/// Saves a media file with metadata
		/// </summary>
		/// <param name="type">Type of media file to save.</param>
		/// <param name="fileStream">The stream to output the file to.</param>
		/// <param name="fileName">The name of the saved file including the extension.</param>
		/// <returns>A task representing the asynchronous save operation.</returns>
		Task SaveAsync(MediaFileType type, Stream fileStream, string fileName);

		/// <summary>
		/// <inheritdoc cref="SaveAsync(MediaFileType, Stream, string)" path="/summary"/>
		/// </summary>
		/// <param name="type"><inheritdoc cref="SaveAsync(MediaFileType, Stream, string)" path="/param/type"/></param>
		/// <param name="data">A byte array to save to the file.</param>
		/// <param name="fileName"><inheritdoc cref="SaveAsync(MediaFileType, Stream, string)" path="/param/fileName"/></param>
		/// <returns><inheritdoc cref="SaveAsync(MediaFileType, Stream, string)" path="/returns"/></returns>
		Task SaveAsync(MediaFileType type, byte[] data, string fileName);

		/// <summary>
		/// <inheritdoc cref="SaveAsync(MediaFileType, Stream, string)" path="/summary"/>
		/// </summary>
		/// <param name="type"><inheritdoc cref="SaveAsync(MediaFileType, Stream, string)" path="/param/type"/></param>
		/// <param name="filePath">Full path to a local file.</param>
		/// <returns><inheritdoc cref="SaveAsync(MediaFileType, Stream, string)" path="/returns"/></returns>
		Task SaveAsync(MediaFileType type, string filePath);
	}

	/// <inheritdoc cref="IMediaGallery" />
	public static class MediaGallery
	{
		static IMediaGallery? DefaultImplementation;

		/// <inheritdoc cref="IMediaGallery.CheckCaptureSupport(MediaFileType)" />
		public static bool CheckCaptureSupport(MediaFileType type)
			=> Default.CheckCaptureSupport(type);

		/// <inheritdoc cref="IMediaGallery.CaptureAsync(MediaFileType, CancellationToken)" />
		public static Task<MediaFilesResult> CaptureAsync(MediaFileType type, CancellationToken token = default)
			=> Default.CaptureAsync(type, token);

		/// <inheritdoc cref="IMediaGallery.PickAsync(int, MediaFileType[])" />
		public static Task<MediaFilesResult> PickAsync(int selectionLimit = 1, params MediaFileType[] types)
			=> Default.PickAsync(selectionLimit, types);

		/// <inheritdoc cref="IMediaGallery.PickAsync(MediaPickRequest, CancellationToken)" />
		public static Task<MediaFilesResult> PickAsync(MediaPickRequest request, CancellationToken token = default)
			=> Default.PickAsync(request, token);

		/// <inheritdoc cref="IMediaGallery.GetMultiPickingBehaviour()" />
		public static MultiPickingBehaviour GetMultiPickingBehaviour()
			=> Default.GetMultiPickingBehaviour();

		/// <inheritdoc cref="IMediaGallery.SaveAsync(MediaFileType, Stream, string)" />
		public static Task SaveAsync(MediaFileType type, Stream fileStream, string fileName)
			=> Default.SaveAsync(type, fileStream, fileName);

		/// <inheritdoc cref="IMediaGallery.SaveAsync(MediaFileType, byte[], string)" />
		public static Task SaveAsync(MediaFileType type, byte[] data, string fileName)
			=> Default.SaveAsync(type, data, fileName);

		/// <inheritdoc cref="IMediaGallery.SaveAsync(MediaFileType, string)" />
		public static Task SaveAsync(MediaFileType type, string filePath)
			=> Default.SaveAsync(type, filePath);

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IMediaGallery Default =>
			DefaultImplementation ??= new MediaGalleryImplementation();

		internal static void SetDefault(IMediaGallery? implementation) =>
			DefaultImplementation = implementation;
	}


	partial class MediaGalleryImplementation : IMediaGallery
	{
		const string cacheDir = "MediaGalleryCacheDir";

		public async Task<MediaFilesResult> CaptureAsync(MediaFileType type, CancellationToken token = default)
		{
			await CheckPossibilityCamera(type);
			return new MediaFilesResult(await PlatformCaptureAsync(type, token));
		}

		public Task<MediaFilesResult> PickAsync(int selectionLimit = 1, params MediaFileType[] types)
			=> PickAsync(new MediaPickRequest(null, selectionLimit, default, types), default);

		public async Task<MediaFilesResult> PickAsync(MediaPickRequest? request, CancellationToken token = default)
		{
			CheckSupport();

			if (request == null)
				throw new ArgumentNullException(nameof(request));

			return new MediaFilesResult(await PlatformPickAsync(request, token).ConfigureAwait(false));
		}

		public async Task SaveAsync(MediaFileType type, Stream fileStream, string fileName)
		{
			await CheckPossibilitySave();
			if (fileStream == null)
				throw new ArgumentNullException(nameof(fileStream));
			CheckFileName(fileName);

			await PlatformSaveAsync(type, fileStream, fileName).ConfigureAwait(false);
		}

		public async Task SaveAsync(MediaFileType type, byte[] data, string fileName)
		{
			await CheckPossibilitySave();
			if (!(data?.Length > 0))
				throw new ArgumentNullException(nameof(data));
			CheckFileName(fileName);

			await PlatformSaveAsync(type, data, fileName).ConfigureAwait(false);
		}

		public async Task SaveAsync(MediaFileType type, string filePath)
		{
			await CheckPossibilitySave();
			if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
				throw new ArgumentException(nameof(filePath));

			await PlatformSaveAsync(type, filePath).ConfigureAwait(false);
		}

		async Task CheckPossibilitySave()
		{
			CheckSupport();
			var status = await Permissions.CheckStatusAsync<Permissions.SaveMediaPermission>();

			if (status != PermissionStatus.Granted)
				throw new PermissionException($"{nameof(Permissions.Camera)} permission was not granted: {status}");
		}

		async Task CheckPossibilityCamera(MediaFileType type)
		{
			CheckSupport();
			if (!CheckCaptureSupport(type))
				throw new FeatureNotSupportedException();


			var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

			if (status != PermissionStatus.Granted)
				throw new PermissionException($"{nameof(Permissions.Camera)} permission was not granted: {status}");
		}

		void CheckSupport()
		{
			if (!IsSupported)
				throw new NotImplementedInReferenceAssemblyException();
		}

		static void CheckFileName(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				throw new ArgumentNullException(nameof(fileName));
		}

		static string GetNewImageName(string? imgName = null)
			=> GetNewImageName(DateTime.Now, imgName);

		static string GetNewImageName(DateTime val, string? imgName = null)
			=> $"{imgName ?? "IMG"}_{val:yyyyMMdd_HHmmss}";

		static string GetMimeType(MediaFileType type)
			=> type switch
			{
				MediaFileType.Image => FileMimeTypes.ImageAll,
				MediaFileType.Video => FileMimeTypes.VideoAll,
				_ => string.Empty,
			};

		static void DeleteFile(string filePath)
		{
			if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
				File.Delete(filePath);
		}

		static string GetFilePath(string fileName)
		{
			fileName = fileName.Trim();
			var dirPath = Path.Combine(FileSystem.CacheDirectory, cacheDir);
			var filePath = Path.Combine(dirPath, fileName);

			if (!Directory.Exists(dirPath))
				Directory.CreateDirectory(dirPath);
			return filePath;
		}
	}
}
