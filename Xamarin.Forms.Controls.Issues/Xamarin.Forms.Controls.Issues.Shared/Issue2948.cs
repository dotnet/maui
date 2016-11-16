using System;

using Xamarin.Forms.CustomAttributes;
using System.Collections.Generic;
using System.Threading;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Android;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2948, "MasterDetailPage Detail is interactive even when Master is open when in Landscape")]
	public class Issue2948 : TestMasterDetailPage
	{
		static MasterDetailPage s_mdp;

		protected override void Init ()
		{
			s_mdp = this;
			var menuPage = new MenuPage ();

			menuPage.Menu.ItemSelected += (sender, e) => NavigateTo (e.SelectedItem as MenuItem);

			Master = menuPage;
			Detail = new NavigationPage (new ContractsPage ());
		}

		[Preserve (AllMembers = true)]
		public class MenuListData : List<MenuItem>
		{
			public MenuListData ()
			{
				Add (new MenuItem () { 
					Title = "Contracts", 
					IconSource = "bank.png", 
					TargetType = typeof(ContractsPage)
				});

				Add (new MenuItem () { 
					Title = "Leads", 
					IconSource = "bank.png", 
					TargetType = typeof(ContractsPage)
				});

				Add (new MenuItem () { 
					Title = "Accounts", 
					IconSource = "bank.png", 
					TargetType = typeof(ContractsPage)
				});

				Add (new MenuItem () {
					Title = "Opportunities",
					IconSource = "bank.png",
					TargetType = typeof(ContractsPage)
				});
			}
		}

		[Preserve (AllMembers = true)]
		public class ContractsPage : ContentPage
		{
			public ContractsPage ()
			{
				Title = "Contracts";
				Icon = "bank.png";

				var grid = new Grid();
				grid.ColumnDefinitions.Add(new ColumnDefinition());
				grid.ColumnDefinitions.Add(new ColumnDefinition());

				var btn = new Button {
					HeightRequest = 300,
					HorizontalOptions = LayoutOptions.End,
					BackgroundColor = Color.Pink,
					AutomationId = "btnOnDetail"
				};

				btn.Clicked+= (object sender, EventArgs e) => {
					DisplayAlert("Clicked","I was clicked","Ok");
				};

				Grid.SetColumn(btn,1);

				grid.Children.Add(btn);

				var showMasterButton = new Button {
					AutomationId = "ShowMasterBtn",
					Text = "Show Master"
				};
				showMasterButton.Clicked += (sender, e) => {
					s_mdp.IsPresented = true;
				};

				Content = new ScrollView {

					Content = new StackLayout {
						Children = {
							showMasterButton,
							grid,
							new BoxView {
								HeightRequest = 100,
								Color = Color.Red,
							},
							new BoxView {
								HeightRequest = 200,
								Color = Color.Green,
							},
							new BoxView {
								HeightRequest = 300,
								Color = Color.Red,
							},
							new BoxView {
								HeightRequest = 400,
								Color = Color.Green,
							},
							new BoxView {
								HeightRequest = 500,
								Color = Color.Red,
							}
						}
					},

				};
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
				BackgroundColor = Color.Transparent;

				var cell = new DataTemplate (typeof(ImageCell));
				cell.SetBinding (TextCell.TextProperty, "Title");
				cell.SetBinding (ImageCell.ImageSourceProperty, "IconSource");

				ItemTemplate = cell;
				SelectedItem = data [0];
			}
		}

		public class MenuPage : ContentPage
		{
			public ListView Menu { get; set; }

			public MenuPage ()
			{
				Title = "Menu";
				BackgroundColor = Color.FromHex ("333333");

				Menu = new MenuListView ();

				var menuLabel = new ContentView {
					Padding = new Thickness (10, 36, 0, 5),
					Content = new Label {
						TextColor = Color.FromHex ("AAAAAA"),
						Text = "MENU", 
					}
				};

				var layout = new StackLayout { 
					Spacing = 0, 
					VerticalOptions = LayoutOptions.FillAndExpand
				};
				layout.Children.Add (menuLabel);
				layout.Children.Add (Menu);

				Content = layout;
			}
		}

		void NavigateTo (MenuItem menu)
		{
			var displayPage = (Page)Activator.CreateInstance (menu.TargetType);

			Detail = new NavigationPage (displayPage);

		}

		[Preserve (AllMembers = true)]
		public class MenuItem
		{
			public string Title { get; set; }

			public string IconSource { get; set; }

			public Type TargetType { get; set; }
		}

#if UITEST
		[Test]
		public void Issue2948Test ()
		{
			RunningApp.Screenshot ("I am at Issue 2948");
			RunningApp.SetOrientationLandscape ();
			Thread.Sleep (5000);
			if (ShouldRunTest ()) {
				OpenMDP ();
				var btns = RunningApp.Query (c => c.Marked ("btnOnDetail"));
				if (btns.Length > 0) {
					// on iOS the button could be out of screen
					RunningApp.Tap (c => c.Marked ("btnOnDetail"));
					RunningApp.Screenshot ("I in landscape and master is open");
				}
				RunningApp.WaitForNoElement (c => c.Marked ("Clicked"),"Time out",new TimeSpan(0,0,1));
			}
		}

		[TearDown]
		public void TestTearDown()
		{
			RunningApp.SetOrientationPortrait();
		}

		public bool ShouldRunTest() {
			var isMasterVisible = RunningApp.Query (q => q.Marked ("Leads")).Length > 0;
			return !isMasterVisible;
		}
		public void OpenMDP() {
#if __IOS__
			RunningApp.Tap (q => q.Marked ("Menu"));
#else
			RunningApp.Tap ("ShowMasterBtn");
#endif
		}
#endif
	}
}
