using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Android.Material.AppBar;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.NavigationPage)]
	[Collection(HandlerTestBase.RunInNewWindowCollection)]
	public partial class NavigationPageTests : HandlerTestBase
	{
		// We only want to fire BackButtonVisible Toolbar events if the user
		// is changing the default behavior of the BackButtonVisibility
		// this way the platform animations are allowed to just happen naturally
		[Fact(DisplayName = "Pushing And Popping Doesnt Fire BackButtonVisible Toolbar Events")]
		public async Task PushingAndPoppingDoesntFireBackButtonVisibleToolbarEvents()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()
			{
				Title = "Page Title"
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				bool failed = false;
				var toolbar = (NavigationPageToolbar)navPage.FindMyToolbar();
				toolbar.PropertyChanged += (_, args) =>
				{
					if (args.PropertyName == nameof(Toolbar.BackButtonVisible) ||
						args.PropertyName == nameof(Toolbar.DrawerToggleVisible))
					{
						failed = true;
					}
				};

				await navPage.Navigation.PushAsync(new ContentPage());
				Assert.False(failed);
				await navPage.Navigation.PopAsync();
				Assert.False(failed);
			});
		}

		public bool IsNavigationBarVisible(IElementHandler handler) =>
			IsNavigationBarVisible(handler.MauiContext);

		public bool IsNavigationBarVisible(IMauiContext mauiContext)
		{
			return GetPlatformToolbar(mauiContext)?
					.LayoutParameters?.Height > 0;
		}

		public bool ToolbarItemsMatch(
			IElementHandler handler,
			params ToolbarItem[] toolbarItems)
		{
			var toolbar = GetPlatformToolbar(handler);
			var menu = toolbar.Menu;

			Assert.Equal(toolbarItems.Length, menu.Size());

			for (var i = 0; i < toolbarItems.Length; i++)
			{
				ToolbarItem toolbarItem = toolbarItems[i];
				var primaryCommand = menu.GetItem(i);
				Assert.Equal(toolbarItem.Text, $"{primaryCommand.TitleFormatted}");
			}

			return true;
		}

		string GetToolbarTitle(IElementHandler handler) =>
			GetPlatformToolbar(handler).Title;
	}
}
