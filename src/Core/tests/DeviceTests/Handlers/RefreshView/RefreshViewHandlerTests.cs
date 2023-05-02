using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category("RefreshViewHandler")]
	public partial class RefreshViewHandlerTests : CoreHandlerTestBase<RefreshViewHandler, RefreshViewStub>
	{
		[Theory(DisplayName = "Is Refreshing Initializes Correctly")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task IsRefreshingInitializesCorrectly(bool isRefreshing)
		{
			var RefreshView = new RefreshViewStub()
			{
				IsRefreshing = isRefreshing,
			};

#if !WINDOWS
			await Assert();
#else
			await AttachAndRun(RefreshView, async (handler) =>
			{
				await Wait(() => GetPlatformIsRefreshing((RefreshViewHandler)handler) == isRefreshing);
				await Assert();
			});
#endif
			Task Assert()
			{
				return ValidatePropertyInitValue(RefreshView, () => isRefreshing, GetPlatformIsRefreshing, isRefreshing);
			}
		}
	}
}