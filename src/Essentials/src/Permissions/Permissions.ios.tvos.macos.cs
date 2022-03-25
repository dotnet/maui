using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Photos;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class Permissions
	{
		public partial class Photos : BasePlatformPermission
		{
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[] { "NSPhotoLibraryUsageDescription" };

			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(GetPhotoPermissionStatus(PHAccessLevel.ReadWrite));
			}

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
						WindowStateManager.Default.GetCurrentUIViewController());
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
				PHAuthorizationStatus.Limited => PermissionStatus.Limited,
				PHAuthorizationStatus.Denied => PermissionStatus.Denied,
				PHAuthorizationStatus.Restricted => PermissionStatus.Restricted,
				_ => PermissionStatus.Unknown,
			};

		static bool CheckOSVersionForPhotos()
		{
			return OperatingSystem.IsIOSVersionAtLeast(14, 0) ||
				OperatingSystem.IsMacOSVersionAtLeast(11, 0) ||
				OperatingSystem.IsTvOSVersionAtLeast(14, 0);
		}
	}
}
