using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using AndroidSpecific = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class TabbedPageGallery
	{
		public TabbedPageGallery()
		{
			InitializeComponent();
			this.Children.Add(new NavigationGallery());
			this.Children.Add(new NavigationPage(new NavigationGallery()) { Title = "With Nav Page" });
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

		void OnChangeTabIndex(object sender, EventArgs e)
		{
			CurrentPage = Children[1];
		}

		void OnToggleTabBar(object sender, EventArgs e)
		{
			if ((this.BarBackground as SolidColorBrush)?.Color == SolidColorBrush.Purple.Color)
				this.BarBackground = null;
			else
				this.BarBackground = SolidColorBrush.Purple;
		}

		void OnToggleTabBarTextColor(object sender, EventArgs e)
		{
			if (this.BarTextColor == Colors.Green)
				this.BarTextColor = null;
			else
				this.BarTextColor = Colors.Green;
		}

		void OnToggleTabItemUnSelectedColor(object sender, EventArgs e)
		{
			if (this.UnselectedTabColor == Colors.Blue)
				this.UnselectedTabColor = null;
			else
				this.UnselectedTabColor = Colors.Blue;
		}

		void OnToggleTabItemSelectedColor(object sender, EventArgs e)
		{
			if (this.SelectedTabColor == Colors.Pink)
				this.SelectedTabColor = null;
			else
				this.SelectedTabColor = Colors.Pink;
		}
	}
}