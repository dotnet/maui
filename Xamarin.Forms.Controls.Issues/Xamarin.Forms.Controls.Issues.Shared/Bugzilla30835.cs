using System;

using Xamarin.Forms.CustomAttributes;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 30835, "Navigating to and from the Carousel page with MasterDetail page creates an Out of memory exception")]
	public class Bugzilla30835 : TestMasterDetailPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init ()
		{
			var menuPage = new MenuPage ();

			menuPage.Menu.ItemSelected += (sender, e) => NavigateTo (e.SelectedItem as MenuItem);

			Master = menuPage;
			Detail = new NavigationPage (new HolderCarouselPages ());
		}

		void NavigateTo (MenuItem menu)
		{
			var displayPage = (Page)Activator.CreateInstance (menu.TargetType);
			Detail = new NavigationPage (displayPage);
			IsPresented = false;
		}

		[Preserve (AllMembers = true)]
		public class HolderCarouselPages : CarouselPage
		{
			public HolderCarouselPages ()
			{
				Device.BeginInvokeOnMainThread (() => {
					ItemsSource = new HolderImage[] {				
						new HolderImage ("frog", "photo.jpg"),
						new HolderImage ("history", "photo.jpg"),
						new HolderImage ("Test", "photo.jpg"), 
					};			
				});

				ItemTemplate = new DataTemplate (() => new DisplayContentPage ());
			}
		}

		[Preserve (AllMembers = true)]
		public class HolderImage
		{
			public HolderImage (string name, string images)
			{
				Name = name;
				Homeimages = images;
			}

			public string Name { private set; get; }

			public string Homeimages { private set; get; }

			public override string ToString ()
			{
				return Name;
			}
		}

		[Preserve (AllMembers = true)]
		public class DisplayContentPage : ContentPage
		{
			public DisplayContentPage ()
			{
				var imageView = new Image {
					HorizontalOptions = LayoutOptions.Center,
				};

				Content = new StackLayout {
					Children = {
						new StackLayout {   
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.StartAndExpand,
							Children = {
								imageView
							}
						},
					}
				};
				imageView.SetBinding (Image.SourceProperty, "Homeimages");
			}
		}

		[Preserve (AllMembers = true)]
		public class MenuPage : ContentPage
		{
			public ListView Menu { get; set; }

			public MenuPage ()
			{
				Icon = "bank.png";
				Title = "Carsousel Memory Bug";
				BackgroundColor = Color.FromHex ("#000000");

				Menu = new MenuListView ();

				var menuLabel = new ContentView {
					Padding = new Thickness (10, 36, 0, 5),
					Content = new Label {
						TextColor = Color.FromHex ("#FFFFFF"),
						Text = "Carsousel Memory Bug",
					}
				};

				var layout = new StackLayout { 
					Spacing = 0, 
					VerticalOptions = LayoutOptions.FillAndExpand,
				};
				layout.Children.Add (menuLabel);
				layout.Children.Add (Menu);

				Content = layout;
			}
		}

		[Preserve (AllMembers = true)]
		public class MenuListView : ListView
		{
			public MenuListView ()
			{
				List<MenuItem> data = new MenuListData ();

				ItemsSource = data;
				VerticalOptions = LayoutOptions.FillAndExpand;
				BackgroundColor = Color.Black;

				var cell = new DataTemplate (typeof(ImageCell));
				cell.SetBinding (TextCell.TextColorProperty, "TextColor");
				cell.SetBinding (TextCell.TextProperty, "Title");
				cell.SetBinding (ImageCell.ImageSourceProperty, "IconSource");

				ItemTemplate = cell;
				SelectedItem = data [0];
			}
		}

		[Preserve (AllMembers = true)]
		public class MenuListData : List<MenuItem>
		{
			public MenuListData ()
			{
				Add (new MenuItem () { 
					Title = "Carousel", 
					IconSource = "icon.png", 
					TargetType = typeof(HolderCarouselPages),
					TextColor = Color.White
				});

				Add (new MenuItem () { 
					Title = "Pic 1",
					IconSource = "icon.png", 
					TargetType = typeof(Pic1),
					TextColor = Color.White
				});

				Add (new MenuItem () { 
					Title = "Pic 2", 
					IconSource = "icon.png", 
					TargetType = typeof(Pic1),
					TextColor = Color.White
				});	

				Add (new MenuItem () {
					Title = "Pic 3",
					IconSource = "icon.png",
					TargetType = typeof(Pic1),
					TextColor = Color.White
				});
			}
		}

		[Preserve (AllMembers = true)]
		public class MenuItem
		{
			public string Title { get; set; }

			public string IconSource { get; set; }

			public Type TargetType { get; set; }

			public Color TextColor { get ; set; }
		}

		[Preserve (AllMembers = true)]
		public class Pic1 : ContentPage
		{
			public Pic1 ()
			{
				Content = new StackLayout { 
					Children = {
						new Image { Source = "photo.jpg" }
					}
				};
			}
		}
	}
}
