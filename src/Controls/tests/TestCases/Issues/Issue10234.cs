
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 10234, "CarouselView disposed on iOS when navigating back in Shell", PlatformAffected.iOS)]
	public class Issue10234Test : ContentPage
	{
		public Issue10234Test()
		{
			Content = new VerticalStackLayout()
			{
				Children =
				{
					new Button()
					{
						Text = "Go To Test",
						AutomationId = "GoToTest",
						Command = new Command(() => Application.Current.MainPage = new Issue10234())
					}
				}
			};
		}
	}

	public class Issue10234 : Shell
	{
		public Issue10234()
		{
			TabBar tabBar = new TabBar
			{
				Title = "Main",
				Route = "main",
				Items =
				{
					new Tab
					{
						Route = "tab1",
						Title = "Tab 1",
						Items =
						{
							new ShellContent()
							{
								ContentTemplate = new DataTemplate(() => new ContentPage
								{
									Content  = new StackLayout
									{
										Children =
										{
											new Label { Text = "Hej" },
											new Button {
												AutomationId = "goToShow",
												Text = "Show",
												Command = new Command(async () => await GoToAsync("//photos?id=1"))
											}
										}
									}
								}),
							}
						}
					}
				}
			};

			TabBar photosTab = new TabBar()
			{
				Title = "Photos",
				Route = "photos",
				Items =
				{
					new ShellSection()
					{
						Items =
						{
							new ShellContent()
							{
								ContentTemplate = new DataTemplate(() => new PhotosPage()),
							}
						}
					}
				}
			};

			Items.Add(tabBar);
			Items.Add(photosTab);
		}

		class PhotosPage : ContentPage
		{

			CarouselView Photos;
			public PhotosPage()
			{
				Photos = new CarouselView
				{
					AutomationId = "carouselView",
					ItemTemplate = new DataTemplate(
						() =>
						{
							var image = new Image();
							image.SetBinding(Image.SourceProperty, new Binding("."));
							return image;
						}
					)
				};

				var grid = new Grid();
				grid.RowDefinitions.Add(new RowDefinition());
				grid.RowDefinitions.Add(new RowDefinition());
				var btn = new Button
				{
					Text = "Go back",
					AutomationId = "goToBack",
					Command = new Command(async () => await Shell.Current.GoToAsync("//main"))
				};
				Grid.SetRow(Photos, 0);
				Grid.SetRow(btn, 1);
				grid.Children.Add(Photos);
				grid.Children.Add(btn);
				Content = grid;
			}

			public ObservableCollection<string> Items { get; set; }

			public void LoadData()
			{
				var images = new List<string>();

				images.Add("oasis.jpg");
				images.Add("dotnet_bot.jpg");
				images.Add("shopping_cart.jpg");
				images.Add("groceries.png");


				Items = new ObservableCollection<string>(images);
				Photos.ItemsSource = Items;
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();

				LoadData();
			}

			public int PageId
			{
				get; set;
			}
		}
	}
}
