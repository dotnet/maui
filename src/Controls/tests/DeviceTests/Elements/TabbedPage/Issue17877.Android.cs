using System.Threading.Tasks;
using AndroidX.CoordinatorLayout.Widget;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using AViewGroup = Android.Views.ViewGroup;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TabbedPage)]
	public class Issue17877 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ReselectingCurrentBottomTabRaisesCurrentPageChanged()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(VerticalStackLayout), typeof(LayoutHandler));
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(Button), typeof(ButtonHandler));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var firstPage = new ContentPage { Title = "Tab 1" };
			var secondPage = new ContentPage { Title = "Tab 2" };
			var tabbedPage = new TabbedPage
			{
				Children =
				{
					firstPage,
					secondPage
				}
			};

			Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(
				tabbedPage,
				Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);

			var currentPageChangedCount = 0;
			tabbedPage.CurrentPageChanged += (_, _) => currentPageChangedCount++;

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, handler =>
			{
				var coordinatorLayout = handler.PlatformView.FindParent(view => view is CoordinatorLayout) as CoordinatorLayout;
				Assert.NotNull(coordinatorLayout);

				var bottomNavigationView = coordinatorLayout.GetFirstChildOfType<BottomNavigationView>();
				Assert.NotNull(bottomNavigationView);

				var menuView = Assert.IsAssignableFrom<AViewGroup>(bottomNavigationView.GetChildAt(0));
				var firstTab = menuView.GetChildAt(0);
				var secondTab = menuView.GetChildAt(1);

				secondTab.PerformClick();
				Assert.Same(secondPage, tabbedPage.CurrentPage);

				firstTab.PerformClick();
				Assert.Same(firstPage, tabbedPage.CurrentPage);

				currentPageChangedCount = 0;
				firstTab.PerformClick();

				Assert.True(currentPageChangedCount == 1,
					"Reselecting the current bottom tab should raise CurrentPageChanged.");
			});
		}
	}
}
