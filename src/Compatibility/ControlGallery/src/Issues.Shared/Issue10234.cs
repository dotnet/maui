using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10234, "CarouselView disposed on iOS when navigating back in Shell ", PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue10234 : TestShell
	{
		protected override void Init()
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
				//InitializeComponent();
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

				images.Add("FlowerBuds.jpg");
				images.Add("Fruits.jpg");
				images.Add("Legumes.jpg");
				images.Add("Vegetables.jpg");


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


#if UITEST && __IOS__

		[Test]
		[MovedToAppium]
		public void ScrollCarouselViewAfterDispose()
		{
			RunningApp.WaitForElement("goToShow");
			RunningApp.Tap("goToShow");
			RunningApp.WaitForElement("goToBack");
			ScrollNextItem();
			RunningApp.Tap("goToBack");
			RunningApp.WaitForElement("goToShow");
			RunningApp.Tap("goToShow");
			ScrollNextItem();
			RunningApp.WaitForElement("goToBack");
			RunningApp.Tap("goToBack");
			RunningApp.WaitForElement("goToShow");
		}

		void ScrollNextItem()
		{
			var rect = RunningApp.Query(c => c.Marked("carouselView")).First().Rect;
			var centerX = rect.CenterX;
			var rightX = rect.X - 5;
			RunningApp.DragCoordinates(centerX + 40, rect.CenterY, rightX, rect.CenterY);
		}

#endif
	}
}
