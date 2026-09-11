using System.Collections;
using System.Reflection;
using System.Threading.Tasks;
using Android;
using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("Permissions")]
	[Collection("AndroidPermissions")]
	public class Android_Permissions_Tests
	{
		[Fact]
		public async Task RequestAsync_FromBackgroundThread_ThrowsPermissionException()
		{
			var initialPendingRequestCount = GetPendingRequestCount();

			await Task.Run(async () =>
			{
				await Assert.ThrowsAsync<PermissionException>(
					() => Permissions.RequestAsync<TestRuntimePermission>()).ConfigureAwait(false);
			}).ConfigureAwait(false);

			Assert.Equal(initialPendingRequestCount, GetPendingRequestCount());
		}

		static int GetPendingRequestCount()
		{
			var permissionType = typeof(Permissions.BasePlatformPermission);
			var requestsField = permissionType.GetField("requests", BindingFlags.NonPublic | BindingFlags.Static);
			var lockerField = permissionType.GetField("locker", BindingFlags.NonPublic | BindingFlags.Static);

			Assert.NotNull(requestsField);
			Assert.NotNull(lockerField);

			var requests = (IDictionary)requestsField.GetValue(null);
			var locker = lockerField.GetValue(null);

			Assert.NotNull(requests);
			Assert.NotNull(locker);

			lock (locker)
			{
				return requests.Count;
			}
		}

		public class TestRuntimePermission : Permissions.BasePlatformPermission
		{
			public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
				[(Manifest.Permission.Camera, true)];

			public override Task<PermissionStatus> CheckStatusAsync() =>
				Task.FromResult(PermissionStatus.Denied);
		}
	}

	[CollectionDefinition("AndroidPermissions", DisableParallelization = true)]
	public class AndroidPermissionsCollection
	{
	}
}
