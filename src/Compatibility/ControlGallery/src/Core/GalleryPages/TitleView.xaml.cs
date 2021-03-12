using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TitleView : ContentPage
	{
		public TitleView(bool initialLoad)
		{
			InitializeComponent();

			if (initialLoad)
			{
				Device.BeginInvokeOnMainThread(() => masterDetailsPage_Clicked(this, EventArgs.Empty));
			}
		}

		static NavigationPage CreateNavigationPage()
		{
			var page = new TitleView(false) { Title = "Nav Title" };
			return new NavigationPage(page);
		}

		public static Page GetPage()
		{
			return new FlyoutPage()
			{
				Detail = CreateNavigationPage(),
				Flyout = new ContentPage() { Title = "Flyout" }
			};
		}

		void swapDetails_Page(object sender, EventArgs e)
		{
			if (App.Current.MainPage is FlyoutPage mdp)
			{
				mdp.Detail = CreateNavigationPage();
			}
		}

		void masterDetailsPage_Clicked(object sender, EventArgs e)
		{
			App.Current.MainPage =
				new FlyoutPage()
				{
					Detail = CreateNavigationPage(),
					Flyout = new ContentPage() { Title = "Flyout" },
				};

		}

		void toggleBackButtonText_Clicked(object sender, EventArgs e)
		{
			var page = Navigation.NavigationStack.First();
			var titleText = NavigationPage.GetBackButtonTitle(page);

			if (titleText == null)
				titleText = "Custom Text";
			else if (titleText == "Custom Text")
				titleText = "";
			else
				titleText = null;

			NavigationPage.SetBackButtonTitle(page, titleText);
			changeTitleView_Clicked(this, EventArgs.Empty);

			string result = (titleText == null) ? "<null>" : titleText;
			btnToggleBackButtonTitle.Text = $"Toggle Back Button Title Text: {result}";
		}

		void tabbedPage_Clicked(object sender, EventArgs e)
		{

			var page = new ContentPage() { Title = "other title page" };
			NavigationPage.SetTitleView(page, createGrid());

			App.Current.MainPage =
				new TabbedPage()
				{
					Children =
					{
						CreateNavigationPage(),
						new ContentPage(){ Title = "no title Page"},
						new NavigationPage(page),
					}
				};
		}

		void navigationPage_Clicked(object sender, EventArgs e)
		{
			App.Current.MainPage = CreateNavigationPage();
		}

		void nextPage_Clicked(object sender, EventArgs e)
		{
			ContentPage page = null;
			page = new ContentPage()
			{
				Title = "second page",
				Content = new StackLayout()
				{
					Children =
					{
						new Button()
						{
							Text = "Toggle Back Button",
							Command = new Command(()=>
							{
								NavigationPage.SetHasBackButton(page, !NavigationPage.GetHasBackButton(page));
							})
						},
						new Button()
						{
							Text = "Toggle Title View",
							Command = new Command(()=>
							{
								changeTitleView_Clicked(page, EventArgs.Empty);
							})
						}
					}
				}
			};

			NavigationPage.SetTitleView(page, createGrid());
			Navigation.PushAsync(page);
		}

		static View createSearchBarView()
		{
			return new SearchBar { BackgroundColor = Color.Cornsilk, HorizontalOptions = LayoutOptions.FillAndExpand, Margin = new Thickness(10, 0) };
		}

		static View createGrid()
		{
			var grid = new Grid
			{
				BackgroundColor = Color.LightGray
			};

			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition());
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

			var label = new Label { Text = "hello", HorizontalOptions = LayoutOptions.Start, BackgroundColor = Color.Yellow };
			var label2 = new Label { Text = "hello 2", HorizontalOptions = LayoutOptions.Start, BackgroundColor = Color.Yellow };
			grid.Children.Add(
				new StackLayout()
				{
					Orientation = StackOrientation.Horizontal,
					Children =
					{
						label,
						new ImageButton()
						{
							Source = "bank"
						}
					}
				});

			grid.Children.Add(label2);
			Grid.SetRow(label2, 1);

			var label3 = new Label { Text = "right aligned", HorizontalTextAlignment = TextAlignment.End };
			Grid.SetColumn(label3, 1);
			grid.Children.Add(label3);
			return grid;
		}


		void titleIcon_Clicked(object sender, EventArgs e)
		{
			toggleTitleIcon(this);

		}

		static void toggleTitleIcon(Page page)
		{
			var titleIcon = NavigationPage.GetTitleIconImageSource(page);

			if (titleIcon == null)
				NavigationPage.SetTitleIconImageSource(page, "coffee.png");
			else
				NavigationPage.SetTitleIconImageSource(page, null);
		}

		void masterDetailsPageIcon_Clicked(object sender, EventArgs e)
		{
			if (App.Current.MainPage is FlyoutPage mdp)
			{
				if (mdp.Flyout.IconImageSource == null || mdp.Flyout.IconImageSource.IsEmpty)
					mdp.Flyout.IconImageSource = "menuIcon";
				else
					mdp.Flyout.IconImageSource = null;
			}
		}

		void toggleLargeTitles_Clicked(object sender, EventArgs e)
		{
			var navPage = (NavigationPage)Navigation.NavigationStack.Last().Parent;
			navPage.On<iOS>().SetPrefersLargeTitles(!navPage.On<iOS>().PrefersLargeTitles());
		}

		void backToGallery_Clicked(object sender, EventArgs e)
		{
			(App.Current as App).Reset();
		}

		void toggleToolBarItem_Clicked(object sender, EventArgs e) => toggleToolBarItem(Navigation.NavigationStack.Last());

		static void toggleToolBarItem(Page page)
		{
			var items = page.ToolbarItems.Where(x => x.Order == ToolbarItemOrder.Primary).ToList();

			if (items.Any())
				foreach (var item in items)
					page.ToolbarItems.Remove(item);
			else
				page.ToolbarItems.Add(new ToolbarItem() { Text = "Save", Order = ToolbarItemOrder.Primary });
		}

		void toggleSecondaryToolBarItem_Clicked(object sender, EventArgs e)
		{
			var page = Navigation.NavigationStack.Last();
			var items = page.ToolbarItems.Where(x => x.Order == ToolbarItemOrder.Secondary).ToList();

			if (items.Any())
				foreach (var item in items)
					page.ToolbarItems.Remove(item);
			else
				page.ToolbarItems.Add(new ToolbarItem() { Text = "Save", Order = ToolbarItemOrder.Secondary });
		}


		void changeTitleView_Clicked(object sender, EventArgs e)
		{
			changeTitleView(Navigation.NavigationStack.Last());
		}

		static void changeTitleView(Page page)
		{
			var currentView = NavigationPage.GetTitleView(page);

			if (currentView is Grid)
				NavigationPage.SetTitleView(page, createSearchBarView());
			else if (currentView is SearchBar)
				NavigationPage.SetTitleView(page, null);
			else
				NavigationPage.SetTitleView(page, createGrid());

		}
	}
}