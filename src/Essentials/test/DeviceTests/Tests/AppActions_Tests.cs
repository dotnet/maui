using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("AppActions")]
	public class AppActions_Tests
	{
		[Fact]
		public void IsSupported()
		{
			var expectSupported = false;

#if __ANDROID__
			expectSupported = OperatingSystem.IsAndroidVersionAtLeast(25);
#elif __IOS__ || WINDOWS
			expectSupported = true;
#endif

			Assert.Equal(expectSupported, AppActions.IsSupported);
		}

		[Fact]
		public async Task GetSetItems()
		{
			if (!AppActions.IsSupported)
				return;

			var actions = new List<AppAction>
			{
				new AppAction("TEST1", "Test 1", "This is a test", "myapp://test1"),
				new AppAction("TEST2", "Test 2", "This is a test 2", "myapp://test2"),
			};

			await AppActions.SetAsync(actions);

			var get = await AppActions.GetAsync();

			Assert.Contains(get, a => a.Id == "TEST1");
		}
	}
}