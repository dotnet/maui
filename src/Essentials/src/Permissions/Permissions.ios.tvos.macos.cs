using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Photos;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class Permissions
	{
		[SupportedOSPlatform("tvos14.0")]
		[SupportedOSPlatform("macos11.0")]
		[SupportedOSPlatform("ios14.0")]
		public partial class Photos : BasePlatformPermission
		{
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[] { "NSPhotoLibraryUsageDescription" };

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(GetPhotoPermissionStatus(PHAccessLevel.ReadWrite));
			}

			/// <inheritdoc/>
			public override async Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = GetPhotoPermissionStatus(PHAccessLevel.ReadWrite);
				if (status == PermissionStatus.Granted)
				{
					return status;
				}
				else if (OperatingSystem.IsIOSVersionAtLeast(14) && status == PermissionStatus.Limited)
				{
					PhotosUI.PHPhotoLibrary_PhotosUISupport.PresentLimitedLibraryPicker(
						PHPhotoLibrary.SharedPhotoLibrary,
						WindowStateManager.Default.GetCurrentUIViewController());
					return status;
				}

				EnsureMainThread();

				return await RequestPhotoPermissionStatus(PHAccessLevel.ReadWrite);
			}
		}

		[SupportedOSPlatform("tvos14.0")]
		[SupportedOSPlatform("macos11.0")]
		[SupportedOSPlatform("ios14.0")] // The enum PHAccessLevel has these attributes
		public partial class PhotosAddOnly : BasePlatformPermission
		{
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[] { "NSPhotoLibraryAddUsageDescription" };

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(GetPhotoPermissionStatus(PHAccessLevel.AddOnly));
			}

			/// <inheritdoc/>
			public override async Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = GetPhotoPermissionStatus(PHAccessLevel.AddOnly);
				if (status == PermissionStatus.Granted)
					return status;

				EnsureMainThread();

				return await RequestPhotoPermissionStatus(PHAccessLevel.AddOnly);
			}
		}

		static PermissionStatus GetPhotoPermissionStatus(PHAccessLevel level)
			=> Convert(CheckOSVersionForPhotos()
				? PHPhotoLibrary.GetAuthorizationStatus(level)
				: PHPhotoLibrary.AuthorizationStatus);

		static async Task<PermissionStatus> RequestPhotoPermissionStatus(PHAccessLevel level)
			=> Convert(CheckOSVersionForPhotos()
				? await PHPhotoLibrary.RequestAuthorizationAsync(level)
				: await PHPhotoLibrary.RequestAuthorizationAsync());

		static PermissionStatus Convert(PHAuthorizationStatus status)
			=> status switch
			{
				PHAuthorizationStatus.Authorized => PermissionStatus.Granted,
#pragma warning disable CA1416 // 'PHAuthorizationStatus.Limited' is only supported on 'ios' 14.0 and later. 
				PHAuthorizationStatus.Limited => PermissionStatus.Limited,
#pragma warning restore CA1416
				PHAuthorizationStatus.Denied => PermissionStatus.Denied,
				PHAuthorizationStatus.Restricted => PermissionStatus.Restricted,
				_ => PermissionStatus.Unknown,
			};

		[SupportedOSPlatformGuard("iOS14.0")]
		[SupportedOSPlatformGuard("macOS11.0")]
		[SupportedOSPlatformGuard("tvOS14.0")]
		static bool CheckOSVersionForPhotos()
		{
			return OperatingSystem.IsIOSVersionAtLeast(14, 0) ||
				OperatingSystem.IsMacOSVersionAtLeast(11, 0) ||
				OperatingSystem.IsTvOSVersionAtLeast(14, 0);
		}
	}
}
