using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using AndroidSpecific = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
namespace Maui.Controls.Sample.Pages
{
	public partial class TabbedPageGallery
	{
		public TabbedPageGallery()
		{
			InitializeComponent();
			this.Children.Add(new NavigationGallery());
		}

		void OnTabbedPageAsRoot(object sender, EventArgs e)
		{
			var topTabs =
				new TabbedPage()
				{
					Children =
					{
						Handler.MauiContext.Services.GetRequiredService<Page>(),
						new NavigationPage(new Pages.NavigationGallery()) { Title = "Navigation Gallery" }
					}
				};

			this.Handler?.DisconnectHandler();
			Application.Current.MainPage?.Handler?.DisconnectHandler();
			Application.Current.MainPage = topTabs;
		}

		void OnSetToBottomTabs(object sender, EventArgs e)
		{
			var bottomTabs = new TabbedPage()
			{
				Children =
				{
					Handler.MauiContext.Services.GetRequiredService<Page>(),
					new NavigationPage(new Pages.NavigationGallery()) { Title = "Navigation Gallery" }
				}
			};

			this.Handler?.DisconnectHandler();
			Application.Current.MainPage?.Handler?.DisconnectHandler();

			AndroidSpecific.TabbedPage.SetToolbarPlacement(bottomTabs, AndroidSpecific.ToolbarPlacement.Bottom);
			Application.Current.MainPage = bottomTabs;
		}
	}
}