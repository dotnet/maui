using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 55674, "[UWP] Using a navigation page changes the tabbed bar's color", PlatformAffected.UWP)]
	public class Bugzilla55674 : TestTabbedPage
	{
		protected override void Init()
		{
			Children.Add(new FirstPage());
			Children[0].Title = "Page One";

			Children.Add(new SecondPage());
			Children[1].Title = "Page Two";

			Children.Add(new NavigationPage(new ThirdPage()));
			Children[2].Title = "Page Three";

			var tabbedPage = new TabbedPage();
			tabbedPage.Children.Add(new ContentPage
			{
				Title = "Page One",
				Content = new StackLayout
				{
					Children =
					{
						new Label { Text = "Page Four / TabbedPage"}
					}
				}
			});
			tabbedPage.Children.Add(new ContentPage
			{
				Title = "Page Two",
				Content = new StackLayout
				{
					Children =
					{
						new Label { Text = "Page Four / TabbedPage's Page Two"}
					}
				}
			});

			Children.Add(new NavigationPage(tabbedPage));
			Children[3].Title = "Page Four";

			CurrentPageChanged += (s, e) =>
			{
				if (CurrentPage is FirstPage)
					BarBackgroundColor = Color.Default;
				if (CurrentPage is SecondPage)
					BarBackgroundColor = Color.MediumOrchid;
			};
		}

		class FirstPage : ContentPage
		{
			Entry valueEntry;

			public FirstPage()
			{
				BackgroundColor = Color.White;
			}

			private View LoadPage()
			{
				Label transLabel = new Label()
				{
					Text = "Please enter the value ",
					WidthRequest = 325,
					HorizontalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center
				};

				valueEntry = new Entry()
				{
					TextColor = Color.Black,
					WidthRequest = 250,
					HorizontalOptions = LayoutOptions.Center
				};

				Button doSomething = new Button()
				{
					Text = "new main page"
				};

				doSomething.Clicked += (sender, e) => { Application.Current.MainPage = new Bugzilla55674(); };

				return new StackLayout()
				{
					Children =
					{
						transLabel,
						valueEntry,
						doSomething
					},
					Spacing = 25,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.StartAndExpand,
					Padding = new Thickness(0, 20, 0, 0)
				};
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
				Content = LoadPage();
				valueEntry.Focus();
			}

			protected override void OnDisappearing()
			{
				base.OnDisappearing();
				Content = null;
			}
		}

		class SecondPage : ContentPage
		{
			public SecondPage()
			{
				Title = "Second Page";
			}

			View LoadContent()
			{
				Label xyz = new Label()
				{
					Text = "This is the second page. The BarBackgroundColor will remain purple until returning to the first page.",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};

				return new StackLayout()
				{
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.FillAndExpand,
					Children =
					{
						xyz
					}
				};
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
				Content = LoadContent();
			}

			protected override void OnDisappearing()
			{
				base.OnDisappearing();
				Content = null;
			}
		}

		class ThirdPage : ContentPage
		{
			TableView tv;
			SwitchCell showThis, showThat;
			EntryCell eCell;
			ViewCell myCell;

			public ThirdPage() { }

			public View LoadSettings()
			{
				eCell = new EntryCell { Label = "Some label here", Text = "Some text here", IsEnabled = false, HorizontalTextAlignment = TextAlignment.End };

				Button myButton = new Button() { Text = "do something", HorizontalOptions = LayoutOptions.CenterAndExpand, HeightRequest = 40 };
				StackLayout myStack = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
				myStack.Children.Add(myButton);

				myCell = new ViewCell { View = myStack };

				showThis = new SwitchCell { Text = "turn this on/off?", On = true };
				showThat = new SwitchCell { Text = "turn this on/off?", On = false };

				tv = new TableView()
				{
					Root = new TableRoot
					{
						new TableSection("Some settings")
						{
							eCell,
							myCell
						}
					},
					Intent = TableIntent.Settings
				};

				tv.Root.Add(new TableSection("Other Settings") { });
				tv.Root.Last().Add(showThis);
				tv.Root.Last().Add(showThat);

				StackLayout pageItems = new StackLayout()
				{
					VerticalOptions = LayoutOptions.FillAndExpand,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					BackgroundColor = Color.White
				};

				pageItems.Children.Add(tv);

				return pageItems;
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
				Content = LoadSettings();
			}

			protected override void OnDisappearing()
			{
				base.OnDisappearing();
				Content = null;
			}
		}
	}
}
