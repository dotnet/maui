#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Tests
{
	public class Permissions_Tests
	{
		[Fact]
		public async Task StaticMethodsUseCurrentImplementation()
		{
			var permissions = new TestPermissions();

			Permissions.SetCurrent(permissions);

			try
			{
				Assert.Same(permissions, Permissions.Current);

				Assert.Equal(PermissionStatus.Denied, await Permissions.CheckStatusAsync<TestPermission>());
				permissions.AssertLastCall(nameof(IPermissions.CheckStatusAsync));

				Assert.Equal(PermissionStatus.Restricted, await Permissions.RequestAsync<TestPermission>());
				permissions.AssertLastCall(nameof(IPermissions.RequestAsync));

				Assert.True(Permissions.ShouldShowRationale<TestPermission>());
				permissions.AssertLastCall(nameof(IPermissions.ShouldShowRationale));
			}
			finally
			{
				Permissions.SetCurrent(null);
			}
		}

		class TestPermission : Permissions.BasePermission
		{
			public override Task<PermissionStatus> CheckStatusAsync() =>
				throw new InvalidOperationException();

			public override Task<PermissionStatus> RequestAsync() =>
				throw new InvalidOperationException();

			public override void EnsureDeclared() =>
				throw new InvalidOperationException();

			public override bool ShouldShowRationale() =>
				throw new InvalidOperationException();
		}

		class TestPermissions : IPermissions
		{
			string? lastCall;
			Type? lastPermissionType;

			public Task<PermissionStatus> CheckStatusAsync<TPermission>()
				where TPermission : Permissions.BasePermission, new()
			{
				CaptureCall<TPermission>(nameof(IPermissions.CheckStatusAsync));
				return Task.FromResult(PermissionStatus.Denied);
			}

			public Task<PermissionStatus> RequestAsync<TPermission>()
				where TPermission : Permissions.BasePermission, new()
			{
				CaptureCall<TPermission>(nameof(IPermissions.RequestAsync));
				return Task.FromResult(PermissionStatus.Restricted);
			}

			public bool ShouldShowRationale<TPermission>()
				where TPermission : Permissions.BasePermission, new()
			{
				CaptureCall<TPermission>(nameof(IPermissions.ShouldShowRationale));
				return true;
			}

			public void AssertLastCall(string expectedCall)
			{
				Assert.Equal(expectedCall, lastCall);
				Assert.Equal(typeof(TestPermission), lastPermissionType);
			}

			void CaptureCall<TPermission>(string call)
				where TPermission : Permissions.BasePermission, new()
			{
				lastCall = call;
				lastPermissionType = typeof(TPermission);
			}
		}
	}
}
