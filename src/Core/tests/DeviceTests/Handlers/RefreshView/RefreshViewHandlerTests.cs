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
			await ValidatePropertyInitValue(RefreshView, () => isRefreshing, GetNativeIsRefreshing, isRefreshing);
		}
	}
}