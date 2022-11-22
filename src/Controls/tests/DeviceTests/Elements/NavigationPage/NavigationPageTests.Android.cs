using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Android.Material.AppBar;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.NavigationPage)]
	public partial class NavigationPageTests : ControlsHandlerTestBase
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

		string GetToolbarTitle(IElementHandler handler) =>
			GetPlatformToolbar(handler).Title;
	}
}
