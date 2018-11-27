using System;

using Xamarin.Forms;
using System.Diagnostics;
using System.Collections.Generic;

namespace Xamarin.Forms.Controls
{
	public class AppearingGalleryPage : ContentPage
	{
		const string NavPageTitle = "NavAppearingPage";
		const string MasterPageTitle = "MasterAppearingPage";
		const string TabbedPageTitle = "TabbedAppearingPage";
		const string CarouselPageTitle = "CarouselAppearingPage";

		public AppearingGalleryPage ()
		{
			App.AppearingMessages.Clear();

			var initalPage = new AppearingPage (1);
			var initalPage2 = new AppearingPage (2);

			Content = new StackLayout { 
				Children = {
					new Button { Text = NavPageTitle, Command = new Command (() => {
						Application.Current.MainPage = new NavAppearingPage(initalPage);
						}) 
					},
					new Button { Text = MasterPageTitle, Command = new Command (() => {
						var page = new MasterDetailPage {
							Title = MasterPageTitle,
							Master = new ContentPage { Title = "Master", BackgroundColor = Color.Red },
							Detail =  new NavAppearingPage(initalPage)
						};
						SetMainPage (page);
					})
					},
					new Button { Text = TabbedPageTitle, Command = new Command (() => {
						var page = new TabbedPage {
							Title = TabbedPageTitle,
							Children = { initalPage, initalPage2 }
						};
						SetMainPage (page);
					})
					},
					new Button { Text =  CarouselPageTitle, Command = new Command (() => {
						
						var page = new CarouselPage {
							Title = CarouselPageTitle,
							Children = { initalPage, initalPage2 }
						};
						SetMainPage (page);
					})
					}	
				}
			};
		}

		static void SetMainPage (Page page)
		{
			var tracker = new AppearingTracker (page);
			Application.Current.MainPage = page;
		}

		class AppearingTracker 
		{
			int _isAppearingFired;
			int _isDisappearingFired;

			public AppearingTracker (Page page)
			{
				page.Appearing += (object sender, EventArgs e) => {
					_isAppearingFired++;
					App.AppearingMessages.Insert (0, $"Appearing {page.Title}");
					Debug.WriteLine ($"Appearing {page.Title}");
				};

				page.Disappearing += (object sender, EventArgs e) => {
					_isDisappearingFired++;
					App.AppearingMessages.Insert (0, $"Disappearing {page.Title}");
					Debug.WriteLine( $"Disappearing {page.Title}");
				};
			}
		}

		class AppearingPage : ContentPage
		{
			int _theId;
			ListView _listMessages;
			public AppearingPage (int id)
			{
				var tracker = new AppearingTracker (this);
				_listMessages = new ListView ();
				_theId = id;
				Title = $"Page {_theId}";
				Padding = new Thickness (20);
				Content = new StackLayout {
					Children = {
						new Label { Text = $"Hello Appearing {_theId} page" },
						new Button { Text = "Push new Page", Command = new Command ( async () => { await Navigation.PushAsync( new AppearingPage(2)); }) },
						new Button { Text = "Add new Page", Command = new Command ( () => { (Parent as CarouselPage).Children.Add( new AppearingPage(3)); }) },
						new Button { Text = "Pop page", Command = new Command ( async () => { await Navigation.PopAsync(); }) },
						new Button { Text = "Pop to root", Command = new Command ( async () => { await Navigation.PopToRootAsync(); }) },
						new Button { Text = "Change Main Page", Command = new Command ( () => { 
							App.AppearingMessages.Clear();
							Application.Current.MainPage = new AppearingPage(3); }) 
						},
						_listMessages
					}
				};
			}
			protected override void OnAppearing ()
			{
				base.OnAppearing ();

				Device.StartTimer (TimeSpan.FromMilliseconds(750), () => {
					_listMessages.ItemsSource = App.AppearingMessages;
					return false;
				});
			}
		}

		class NavAppearingPage : NavigationPage
		{
			public NavAppearingPage (Page page) : base(page)
			{
				Title = NavPageTitle;
				var tracker = new AppearingTracker (this);
			}
		}
	}
}

