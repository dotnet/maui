using System.Threading.Tasks;
using AndroidX.Fragment.App;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Page)]
	public partial class PageTests : ControlsHandlerTestBase
	{
		//src/Compatibility/Core/tests/Android/EmbeddingTests.cs
		[Fact(DisplayName = "Can Create Platform View From ContentPage")]
		public async Task CanCreatePlatformViewFromContentPage()
		{

			var contentPage = new ContentPage { Title = "Embedded Page" };
			var handler = CreateHandler<PageHandler>(contentPage);
			var mauiContext = handler.MauiContext;

			await contentPage.Dispatcher.DispatchAsync(() =>
			{

				AView platformView = contentPage.ToPlatform(mauiContext);
				if (platformView is FragmentContainerView containerView)
				{
					var activity = mauiContext.Context as AndroidX.Fragment.App.FragmentActivity;
					var fragmentManager = activity.SupportFragmentManager;
					var fragment = fragmentManager.FindFragmentById(containerView.Id);
					Assert.NotNull(fragment);
				}
			});
		}
	}
}