using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Provider;
using Android.Webkit;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Environment = Android.OS.Environment;
using File = Java.IO.File;
using Path = System.IO.Path;
using Stream = System.IO.Stream;
using Uri = Android.Net.Uri;
#if ANDROID30_0_OR_GREATER
using MediaColumns = Android.Provider.MediaStore.IMediaColumns;
#elif MONOANDROID10_0
global using MediaColumns = Android.Provider.MediaStore.MediaColumns;
#endif

namespace Microsoft.Maui.Media
{
	partial class MediaGalleryImplementation : IMediaGallery
	{
		const string ImageType = "image/*";
		const string VideoType = "video/*";
		static TaskCompletionSource<(Intent, Result)> TcsPick;
		static TaskCompletionSource<(Intent, Result)> TcsCamera;

		public bool IsSupported => OperatingSystem.IsAndroidVersionAtLeast(21);

		public bool CheckCaptureSupport(MediaFileType type)
		{
			if (!IsSupported || !(Platform.AppContext?.PackageManager?.HasSystemFeature(PackageManager.FeatureCameraAny) ?? false))
				return false;
			using var intent = GetCameraIntent(type);
			return PlatformUtils.IsIntentSupported(intent);
		}

		public async Task<FileResult> CaptureAsync(MediaFileType type, CancellationToken token = default)
		{
			token.ThrowIfCancellationRequested();
			Intent intent = null;
			Uri outputUri = null;

			try
			{
				TcsCamera = new TaskCompletionSource<(Intent, Result)>(TaskCreationOptions.RunContinuationsAsynchronously);
				intent = GetCameraIntent(type);

				var fileName = $"{GetNewImageName()}.jpg";
				var tempFilePath = GetFilePath(fileName);
				using var file = new File(tempFilePath);
				if (!file.Exists())
					file.CreateNewFile();
				outputUri = FileProvider.GetUriForFile(file);
				intent.PutExtra(MediaStore.ExtraOutput, outputUri);

				CancelTaskIfRequested(token, TcsCamera);

				StartActivity(intent, PlatformUtils.requestCodeMediaCapture, token, TcsCamera);

				CancelTaskIfRequested(token, TcsCamera, false);
				var result = await TcsCamera.Task.ConfigureAwait(false);
				if (result.Item2 == Result.Ok)
					return new MediaFile(fileName, outputUri, tempFilePath);

				outputUri?.Dispose();
				return null;
			}
			catch
			{
				outputUri?.Dispose();
				throw;
			}
			finally
			{
				intent?.Dispose();
				intent = null;
				TcsCamera = null;
			}
		}

		public Task<FileResult> PickAsync(int selectionLimit = 1, params MediaFileType[] types)
			=> PickAsync(new MediaPickRequest(null, selectionLimit, default, types), default);

		public async Task<FileResult> PickAsync(MediaPickRequest request, CancellationToken token = default)
		{
			token.ThrowIfCancellationRequested();
			Intent intent = null;

			try
			{
				TcsPick = new TaskCompletionSource<(Intent, Result)>(TaskCreationOptions.RunContinuationsAsynchronously);

				CancelTaskIfRequested(token, TcsPick, false);

				intent = GetPickerIntent(request);

				CancelTaskIfRequested(token, TcsPick);

				StartActivity(intent, PlatformUtils.requestCodeMediaPicker, token, TcsPick);

				CancelTaskIfRequested(token, TcsPick, false);
				var result = await TcsPick.Task.ConfigureAwait(false);
				return GetFilesFromIntent(result.Item1);
			}
			finally
			{
				intent?.Dispose();
				intent = null;
				TcsPick = null;
			}
		}

		public async Task SaveAsync(MediaFileType type, byte[] data, string fileName)
		{
			using var ms = new MemoryStream(data);
			await SaveAsync(type, ms, fileName).ConfigureAwait(false);
		}

		public async Task SaveAsync(MediaFileType type, string filePath)
		{
			await using var fileStream = System.IO.File.OpenRead(filePath);
			await SaveAsync(type, fileStream, Path.GetFileName(filePath)).ConfigureAwait(false);
		}

		public async Task SaveAsync(MediaFileType type, Stream fileStream, string fileName)
		{
			var albumName = AppInfo.Name;

			var context = Platform.AppContext;

			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
			var extension = Path.GetExtension(fileName).ToLower();
			var newFileName = $"{GetNewImageName(DateTime.Now, fileNameWithoutExtension)}{extension}";

			using var values = new ContentValues();

			values.Put(MediaColumns.DateAdded, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
			values.Put(MediaColumns.Title, fileNameWithoutExtension);
			values.Put(MediaColumns.DisplayName, newFileName);

			var mimeType = MimeTypeMap.Singleton!.GetMimeTypeFromExtension(extension.Replace(".", string.Empty, StringComparison.Ordinal));
			if (!string.IsNullOrWhiteSpace(mimeType))
				values.Put(MediaColumns.MimeType, mimeType);

			using var externalContentUri = type == MediaFileType.Image
				? MediaStore.Images.Media.ExternalContentUri
				: MediaStore.Video.Media.ExternalContentUri;

			var relativePath = type == MediaFileType.Image
				? Environment.DirectoryPictures
				: Environment.DirectoryMovies;

			if (OperatingSystem.IsAndroidVersionAtLeast(29))
			{
				values.Put(MediaColumns.RelativePath, Path.Combine(relativePath!, albumName));
				values.Put(MediaColumns.IsPending, true);

				using var uri = context.ContentResolver!.Insert(externalContentUri!, values);
				await using var stream = context.ContentResolver.OpenOutputStream(uri!);
				await fileStream.CopyToAsync(stream!);
				stream.Close();

				values.Put(MediaColumns.IsPending, false);
				context.ContentResolver.Update(uri, values, null, null);
			}
			else
			{
#pragma warning disable CS0618 // Type or member is obsolete
				using var directory = new File(Environment.GetExternalStoragePublicDirectory(relativePath), albumName);

				directory.Mkdirs();
				using var file = new File(directory, newFileName);

				await using var fileOutputStream = System.IO.File.Create(file.AbsolutePath);
				await fileStream.CopyToAsync(fileOutputStream);
				fileOutputStream.Close();

				values.Put(MediaColumns.Data, file.AbsolutePath);
				context.ContentResolver!.Insert(externalContentUri!, values);

				using var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
				mediaScanIntent.SetData(Uri.FromFile(file));
				context.SendBroadcast(mediaScanIntent);
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}

		static Intent GetPickerIntent(MediaPickRequest request)
		{
#if ANDROID33_0_OR_GREATER
			if (ActionPickImagesIsSupported())
				return GetPickerActionPickImagesIntent(request);
#endif
			return GetPickerActionGetContentIntent(request);
		}

		static Intent GetPickerActionGetContentIntent(MediaPickRequest request)
		{
			var mimeTypes = request.Types.Select(GetMimeType).ToArray();
			var intent = new Intent(Intent.ActionGetContent);

			intent.SetType(string.Join(", ", mimeTypes));
			intent.PutExtra(Intent.ExtraMimeTypes, mimeTypes);
			intent.AddCategory(Intent.CategoryOpenable);
			intent.PutExtra(Intent.ExtraLocalOnly, true);
			intent.PutExtra(Intent.ExtraAllowMultiple, request.SelectionLimit > 1);

			if (!string.IsNullOrWhiteSpace(request.Title))
				intent.PutExtra(Intent.ExtraTitle, request.Title);
			return intent;
		}

#if ANDROID33_0_OR_GREATER
#pragma warning disable CA1416
		static bool ActionPickImagesIsSupported()
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(33))
				return true;
			if (OperatingSystem.IsAndroidVersionAtLeast(30))
				return Android.OS.Ext.SdkExtensions.GetExtensionVersion(30) >= 2;
			return false;
		}

		static Intent GetPickerActionPickImagesIntent(MediaPickRequest request)
		{
			var intent = new Intent(MediaStore.ActionPickImages);
			if (request.SelectionLimit > 1)
				intent.PutExtra(MediaStore.ExtraPickImagesMax, request.SelectionLimit);
			if(request.Types.Length == 1)
				intent.SetType(GetMimeType(request.Types[0]));
			return intent;
		}
#pragma warning restore CA1416
#endif

		static Intent GetCameraIntent(MediaFileType type)
			=> new(type switch
			{
				MediaFileType.Video => MediaStore.ActionVideoCapture,
				_ => MediaStore.ActionImageCapture,
			});

		static string GetNewImageName(string imgName = null)
			=> GetNewImageName(DateTime.Now, imgName);

		static string GetNewImageName(DateTime val, string imgName = null)
			=> $"{imgName ?? "IMG"}_{val:yyyyMMdd_HHmmss}";

		static string GetMimeType(MediaFileType type)
			=> type switch
			{
				MediaFileType.Image => ImageType,
				MediaFileType.Video => VideoType,
				_ => string.Empty,
			};

		static void CancelTaskIfRequested(CancellationToken token, TaskCompletionSource<(Intent, Result)> tcs, bool needThrow = true)
		{
			if (!token.IsCancellationRequested)
				return;
			tcs?.TrySetCanceled(token);
			if (needThrow)
				token.ThrowIfCancellationRequested();
		}

		static void StartActivity(Intent intent, int requestCode, CancellationToken token, TaskCompletionSource<(Intent, Result)> tcs)
		{
			if (token.CanBeCanceled)
			{
				token.Register(() =>
				{
					Platform.CurrentActivity?.FinishActivity(requestCode);
					tcs?.TrySetCanceled(token);
				});
			}

			Platform.CurrentActivity?.StartActivityForResult(intent, requestCode);
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

		static IEnumerable<IMediaFile> GetFilesFromIntent(Intent intent)
		{
			var clipCount = intent?.ClipData?.ItemCount ?? 0;
			var data = intent?.Data;

			if (data != null && !(clipCount > 1))
			{
				var res = GetFileResult(data);
				if (res != null)
					yield return res;
			}
			else if (clipCount > 0)
			{
				for (var i = 0; i < clipCount; i++)
				{
					var item = intent!.ClipData!.GetItemAt(i);
					var res = GetFileResult(item!.Uri);
					if (res != null)
						yield return res;
				}
			}
		}

		static IMediaFile GetFileResult(Uri uri)
		{
			var name = QueryContentResolverColumn(uri, MediaColumns.DisplayName);
			return string.IsNullOrWhiteSpace(name)
				? null
				: new MediaFile(name, uri);
		}

		static string QueryContentResolverColumn(Uri contentUri, string columnName)
		{
			try
			{
				using var cursor = Platform.AppContext?.ContentResolver?
				   .Query(contentUri, new[] { columnName }, null, null, null);

				if (cursor?.MoveToFirst() ?? false)
				{
					var columnIndex = cursor.GetColumnIndex(columnName);
					if (columnIndex != -1)
						return cursor.GetString(columnIndex);
				}
				return null;
			}
			catch
			{
				return null;
			}
		}
	}
}
