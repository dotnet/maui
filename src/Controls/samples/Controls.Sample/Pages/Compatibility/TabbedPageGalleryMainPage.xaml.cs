using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using AndroidSpecific = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class TabbedPageGalleryMainPage
	{
		public TabbedPageGalleryMainPage()
		{
			InitializeComponent();
		}

		TabbedPage? _tabbedPage;
		TabbedPage GetTabbedPage() => _tabbedPage ??= (TabbedPage)Parent;

		void SetNewMainPage(Page page)
		{
			Application.Current!.Windows[0].Page = page;
		}

		void OnTabbedPageAsRoot(object sender, EventArgs e)
		{
			var topTabs =
				new TabbedPage()
				{
					Children =
					{
						Handler!.MauiContext!.Services.GetRequiredService<Page>(),
						new NavigationPage(new Pages.NavigationGallery()) { Title = "Navigation Gallery" }
					}
				};

			SetNewMainPage(topTabs);
		}

		void OnSetToBottomTabs(object sender, EventArgs e)
		{
			var bottomTabs = new TabbedPage()
			{
				Children =
				{
					Handler!.MauiContext!.Services.GetRequiredService<Page>(),
					new NavigationPage(new Pages.NavigationGallery()) { Title = "Navigation Gallery" }
				}
			};

			SetNewMainPage(bottomTabs);
			AndroidSpecific.TabbedPage.SetToolbarPlacement(bottomTabs, AndroidSpecific.ToolbarPlacement.Bottom);
			this.Window!.Page = bottomTabs;
		}

		void OnChangeTabIndex(object sender, EventArgs e)
		{
			GetTabbedPage().CurrentPage = GetTabbedPage().Children[1];
		}

		void OnToggleTabBar(object sender, EventArgs e)
		{
			if ((GetTabbedPage().BarBackground as SolidColorBrush)?.Color == SolidColorBrush.Purple.Color)
				GetTabbedPage().BarBackground = null;
			else
				GetTabbedPage().BarBackground = SolidColorBrush.Purple;
		}

		void OnToggleTabBarTextColor(object sender, EventArgs e)
		{
			if (GetTabbedPage().BarTextColor == Colors.Green)
				GetTabbedPage().BarTextColor = null;
			else
				GetTabbedPage().BarTextColor = Colors.Green;
		}

		void OnToggleTabItemUnSelectedColor(object sender, EventArgs e)
		{
			if (GetTabbedPage().UnselectedTabColor == Colors.Blue)
				GetTabbedPage().UnselectedTabColor = null;
			else
				GetTabbedPage().UnselectedTabColor = Colors.Blue;
		}

		void OnToggleTabItemSelectedColor(object sender, EventArgs e)
		{
			if (GetTabbedPage().SelectedTabColor == Colors.Pink)
				GetTabbedPage().SelectedTabColor = null;
			else
				GetTabbedPage().SelectedTabColor = Colors.Pink;
		}

		void OnRemoveTab(object sender, EventArgs e)
		{
			if (GetTabbedPage().Children.LastOrDefault() is TabbedPageGalleryMainPage mainPage)
			{
				GetTabbedPage().Children.Remove(mainPage);
			}
		}

		void OnRemoveAllTabs(object sender, EventArgs e)
		{
			while (GetTabbedPage().Children.LastOrDefault() is TabbedPageGalleryMainPage mainPage)
			{
				GetTabbedPage().Children.Remove(mainPage);
			}
		}

		void OnAddTab(object sender, EventArgs e)
		{
			GetTabbedPage()
				.Children
				.Add(new TabbedPageGalleryMainPage() { Title = $"Tab {GetTabbedPage().Children.Count}" });
		}
	}
}