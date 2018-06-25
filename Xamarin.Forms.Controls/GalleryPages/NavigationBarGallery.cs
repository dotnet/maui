using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific.AppCompat;
using static Xamarin.Forms.PlatformConfiguration.AndroidSpecific.AppCompat.NavigationPage;
namespace Xamarin.Forms.Controls
{
	public class NavigationBarGallery : ContentPage
	{
		NavigationPage _rootNavPage;
		public NavigationBarGallery(NavigationPage rootNavPage)
		{
			_rootNavPage = rootNavPage;

			int toggleBarTextColor = 0;
			int toggleBarBackgroundColor = 0;

			ToolbarItems.Add(new ToolbarItem { Text = "Save" });

			NavigationPage.SetTitleIcon(this, "coffee.png");

			SearchBar searchBar = new SearchBar { HeightRequest = 44, WidthRequest = 100 };

			// Note: Large and complex controls, such as ListView and TableView, are not recommended.
			var controls = new List<View>
			{
				searchBar,
				new ActivityIndicator{ IsRunning = true },
				new BoxView{ BackgroundColor = Color.Red },
				new Button{ Text = "Button!"},
				new DatePicker{},
				new Editor{ Text = "Editor"},
				new Entry{ Placeholder = "Entry"},
				new Image{ Source = "crimson.jpg", HeightRequest = 44 },
				new Label{ Text = "Title View Label!" },
				new Picker{ ItemsSource = Enumerable.Range(0,10).Select(i => $"Item {i}").ToList(), Title = "Picker" },
				new ProgressBar{ Progress = 50 },
				new Slider{},
				new Stepper{},
				new Switch{},
				new TimePicker{}
			};

			int idx = 0;

			NavigationPage.SetTitleView(this, CreateTitleView(controls[idx]));

			rootNavPage.On<Android>().SetBarHeight(450);
			rootNavPage.On<iOS>().SetPrefersLargeTitles(false);

			Content = new ScrollView
			{
				Content =
					new StackLayout
					{
						Children = {
							new Button {
							Text = "Go to SearchBarTitlePage",
							Command = new Command (() => {
								rootNavPage.PushAsync(new SearchBarTitlePage(rootNavPage));
							})
						},
						new Button {
							Text = "Change BarTextColor",
							Command = new Command (() => {
								if (toggleBarTextColor % 2 == 0) {
									rootNavPage.BarTextColor = Color.Teal;
								} else {
									rootNavPage.BarTextColor = Color.Default;
								}
								toggleBarTextColor++;
							})
						},
						new Button {
							Text = "Change BarBackgroundColor",
							Command = new Command (() => {
								if (toggleBarBackgroundColor % 2 == 0) {
									rootNavPage.BarBackgroundColor = Color.Navy;
								} else {
									rootNavPage.BarBackgroundColor = Color.Default;
								}
								toggleBarBackgroundColor++;

							})
						},
						new Button {
							Text = "Change Both to default",
							Command = new Command (() => {
								rootNavPage.BarTextColor = Color.Default;
								rootNavPage.BarBackgroundColor = Color.Default;
							})
						},
						new Button {
							Text = "Make sure Tint still works",
							Command = new Command (() => {
	#pragma warning disable 618
								rootNavPage.Tint = Color.Red;
	#pragma warning restore 618
							})
						},
						new Button {
							Text = "Black background, white text",
							Command = new Command (() => {
								rootNavPage.BarTextColor = Color.White;
								rootNavPage.BarBackgroundColor = Color.Black;
							})
						},
						new Button {
							Text = "Toggle TitleIcon",
							Command = new Command (() => {

								var titleIcon = NavigationPage.GetTitleIcon(this);

								if (titleIcon == null)
									titleIcon = "coffee.png";
								else
									titleIcon = null;

								NavigationPage.SetTitleIcon(this, titleIcon);
							})
						},
						new Button {
							Text = "Toggle TitleView",
							Command = new Command (() => {

								var titleView = NavigationPage.GetTitleView(this);

								if (titleView == null)
									titleView = CreateTitleView(controls[idx]);
								else
									titleView = null;

								NavigationPage.SetTitleView(this, titleView);
							})
						},
						new Button {
							Text = "Next TitleView",
							Command = new Command (() => {

								idx++;
								if(idx >=controls.Count)
									idx = 0;

								var titleView = CreateTitleView(controls[idx]);

								NavigationPage.SetTitleView(this, titleView);
							})
						},
						new Button {
							Text = "Toggle Back Title",
							Command = new Command (() => {

								var backTitle = NavigationPage.GetBackButtonTitle(rootNavPage);

								if (backTitle == null)
									backTitle= "Go back home";
								else
									backTitle = null;

								NavigationPage.SetBackButtonTitle(rootNavPage, backTitle);
							})
						},
						new Button {
							Text = "Toggle Toolbar Item",
							Command = new Command (() => {

								if (ToolbarItems.Count > 0)
									ToolbarItems.Clear();
								else
									ToolbarItems.Add(new ToolbarItem { Text = "Save" });
							})
						},
						new Button {
							Text = "Toggle Title",
							Command = new Command (() => {

								if (Title == null)
									Title = "NavigationBar Gallery - Legacy";
								else
									Title = null;
							})
						},
						new Button {
							Text = "Toggle BarHeight",
							Command = new Command (() => {

								if (rootNavPage.On<Android>().GetBarHeight() == -1)
									rootNavPage.On<Android>().SetBarHeight(450);
								else
									rootNavPage.ClearValue(BarHeightProperty);
							})
						}
					}
					}
			};
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			_rootNavPage.ClearValue(BarHeightProperty);
		}

		static View CreateTitleView(View control)
		{
			control.HorizontalOptions = LayoutOptions.Fill;
			control.VerticalOptions = LayoutOptions.CenterAndExpand;

			var titleView = new StackLayout
			{
				Children = { control },
				BackgroundColor = Color.FromHex("#ccc"),
				Margin = new Thickness(15, 0),
			};
			return titleView;
		}

		class SearchBarTitlePage : ContentPage
		{
			bool _extended = false;
			List<string> items = new List<string> { "The Ocean at the End of the Lane", "So Long, and Thanks for All the Fish", "Twenty Thousand Leagues Under the Sea", "Rosencrantz and Guildenstern Are Dead" };
			ObservableCollection<string> filtereditems;
			SearchBar search;
			Button button;
			ListView list;
			public SearchBarTitlePage(NavigationPage parent)
			{
				filtereditems = new ObservableCollection<string>(items);

				search = new SearchBar { BackgroundColor = Color.Cornsilk, HorizontalOptions = LayoutOptions.FillAndExpand, Margin = new Thickness(10, 0) };
				search.Effects.Add(Effect.Resolve($"{Issues.Effects.ResolutionGroupName}.SearchbarEffect"));
				search.TextChanged += Search_TextChanged;

				list = new ListView
				{
					ItemsSource = filtereditems
				};

				parent.BarBackgroundColor = Color.Cornsilk;
				parent.BarTextColor = Color.Orange;
				NavigationPage.SetBackButtonTitle(parent, "");

				switch (Device.RuntimePlatform)
				{
					case Device.iOS:

						button = new Button();

						button.Clicked += (s, e) =>
						{
							ToggleContent(parent);
						};

						ToggleContent(parent);
						break;

					default:
						NavigationPage.SetTitleView(this, search);
						Content = list;
						break;
				}
			}

			void ToggleContent(NavigationPage parent)
			{
				StackLayout topStack = new StackLayout { Children = { button }, BackgroundColor = Color.Cornsilk };
				StackLayout layout = new StackLayout { Children = { topStack, list } };

				if (_extended)
				{
					parent.On<iOS>().SetPrefersLargeTitles(false)
									.SetHideNavigationBarSeparator(false);

					NavigationPage.SetTitleView(this, new StackLayout { Children = { search }, HorizontalOptions = LayoutOptions.Fill });
					NavigationPage.SetHasBackButton(this, false);

					button.Text = "Expand";
					Title = "Small Titles";
				}
				else
				{
					topStack.Children.Insert(0, search);

					parent.On<iOS>().SetPrefersLargeTitles(true)
									.SetHideNavigationBarSeparator(true);

					ClearValue(NavigationPage.TitleViewProperty);
					NavigationPage.SetHasBackButton(this, true);

					button.Text = "Collapse";
					Title = "Large Titles";
				}

				_extended = !_extended;
				Content = layout;
			}

			void Search_TextChanged(object sender, TextChangedEventArgs e)
			{
				for (int i = 0; i < filtereditems.Count; i++)
				{
					filtereditems.RemoveAt(0);
				}

				if (search.Text?.Length >= 3)
				{
					foreach (var item in items.Where(i => i.ToLower().Contains(search.Text.ToLower())))
					{
						if (!filtereditems.Contains(item))
							filtereditems.Add(item);
					}
				}
				else
				{
					foreach (var item in items)
					{
						if (!filtereditems.Contains(item))
							filtereditems.Add(item);
					}
				}
			}
		}
	}
}
