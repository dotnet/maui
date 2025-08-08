using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Xunit;

namespace Tests
{
	public class AppActions_Tests
	{
		[Fact]
		public async Task AppActions_SetActions() =>
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => AppActions.SetAsync(new List<AppAction>()));

		[Fact]
		public async Task AppActions_GetActions() =>
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => AppActions.GetAsync());

		[Fact]
		public void AppActions_IsSupported() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => AppActions.IsSupported);

		[Fact]
		public void BaseInterfacesWork_IDeviceCapability()
		{
			var stub = new StubAppActions();
			IDeviceCapability deviceCapability = stub;
			IAppActions appActions = stub;

			stub.IsSupported = true;

			Assert.True(appActions.IsSupported);
			Assert.True(deviceCapability.IsSupported);

			stub.IsSupported = false;

			Assert.False(appActions.IsSupported);
			Assert.False(deviceCapability.IsSupported);
		}

		class StubAppActions : IAppActions
		{
			public bool IsSupported { get; set; }

			public event EventHandler<AppActionEventArgs> AppActionActivated;

			public Task<IEnumerable<AppAction>> GetAsync() => throw new NotImplementedException();

			public Task SetAsync(IEnumerable<AppAction> actions) => throw new NotImplementedException();
		}
	}
}
