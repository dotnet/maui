using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 10234, "CarouselView disposed on iOS when navigating back in Shell", PlatformAffected.iOS)]
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
							image.SetBinding(Image.SourceProperty, new Binding(nameof(PhotoItem.Image)));
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

			public ObservableCollection<PhotoItem> Items { get; set; }

			public void LoadData()
			{
				var images = new List<PhotoItem>();

				for (int i = 0; i < 4; i++)
				{
					images.Add(new PhotoItem { Image = "oasis.jpg", Title = $"Title {i}" });
				}

				Items = new ObservableCollection<PhotoItem>(images);
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

		class PhotoItem
		{
			public string Image { get; set; }
			public string Title { get; set; }
		}
	}
}
