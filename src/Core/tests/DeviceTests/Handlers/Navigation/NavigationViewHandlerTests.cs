
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.NavigationView)]
	public partial class NavigationViewHandlerTests : CoreHandlerTestBase<NavigationViewHandler, NavigationViewStub>
	{
#if ANDROID || WINDOWS
		[Fact(DisplayName = "Push Multiple Pages At Start")]
		public async Task PushMultiplePagesAtStart()
		{
			EnsureHandlerCreated(builder =>
				builder.ConfigureMauiHandlers(handler =>
					handler.AddHandler<ButtonStub, ButtonHandler>()));

			var page1 = new ButtonStub();
			var page2 = new ButtonStub();
			NavigationViewStub navigationViewStub = new NavigationViewStub()
			{
				NavigationStack = new List<IView>()
				{
					page1, page2
				}
			};

			await CreateNavigationViewHandlerAsync(navigationViewStub, (handler) =>
			{
				Assert.Equal(2, GetNativeNavigationStackCount(handler));
				return Task.CompletedTask;
			});
		}

#endif

	}
}