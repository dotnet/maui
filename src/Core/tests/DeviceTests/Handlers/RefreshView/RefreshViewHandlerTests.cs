using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.RefreshView)]
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
				await AssertEventually(() => GetPlatformIsRefreshing((RefreshViewHandler)handler) == isRefreshing);
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