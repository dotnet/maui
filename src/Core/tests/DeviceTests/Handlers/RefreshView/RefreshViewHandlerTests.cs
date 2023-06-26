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
			await InvokeOnMainThreadAsync(async () =>
			{
				await CreateHandler(RefreshView)
					.PlatformView
					.AttachAndRun(async () =>
					{
						await Wait(() => GetPlatformIsRefreshing((RefreshViewHandler)RefreshView.Handler) == isRefreshing);
						await Assert();
					}
#if WINDOWS
					, MauiContext
#endif
				);
			});
#endif
			Task Assert()
			{
				return ValidatePropertyInitValue(RefreshView, () => isRefreshing, GetPlatformIsRefreshing, isRefreshing);
			}
		}
	}
}