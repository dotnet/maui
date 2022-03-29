using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Photos;

namespace Microsoft.Maui.Essentials
{
	public static partial class Permissions
	{
		public partial class Photos : BasePlatformPermission
		{
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[] { "NSPhotoLibraryUsageDescription" };

			[SupportedOSPlatform("tvos14.0")]
			[SupportedOSPlatform("macos11.0")]
			[SupportedOSPlatform("ios14.0")]
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(GetPhotoPermissionStatus(PHAccessLevel.ReadWrite));
			}

			[SupportedOSPlatform("tvos14.0")]
			[SupportedOSPlatform("macos11.0")]
			[SupportedOSPlatform("ios14.0")]
			public override async Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = GetPhotoPermissionStatus(PHAccessLevel.ReadWrite);
				if (status == PermissionStatus.Granted)
				{
					return status;
				}
#if __IOS__
                else if (status == PermissionStatus.Limited)
                {
                    PhotosUI.PHPhotoLibrary_PhotosUISupport.PresentLimitedLibraryPicker(
                        PHPhotoLibrary.SharedPhotoLibrary,
                        Platform.GetCurrentUIViewController());
                    return status;
                }
#endif

				EnsureMainThread();

				return await RequestPhotoPermissionStatus(PHAccessLevel.ReadWrite);
			}
		}

		public partial class PhotosAddOnly : BasePlatformPermission
		{
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[] { "NSPhotoLibraryAddUsageDescription" };

			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(GetPhotoPermissionStatus(PHAccessLevel.AddOnly));
			}

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
#pragma warning disable CA1416
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
