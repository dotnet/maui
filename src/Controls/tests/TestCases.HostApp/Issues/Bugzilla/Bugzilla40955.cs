using System.Diagnostics;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 40955, "Memory leak with FormsAppCompatActivity and NavigationPage", PlatformAffected.Android)]
public class Bugzilla40955 : TestFlyoutPage
{
	const string DestructorMessage = "NavigationPageEx Destructor called";
	const string Page1Title = "Page1";
	const string Page2Title = "Page2";
	const string Page3Title = "Page3";
	const string LabelPage1 = "Open the drawer menu and select Page2";
	const string LabelPage2 = "Open the drawer menu and select Page3";
	static string LabelPage3 = $"The console should have displayed the text '{DestructorMessage}' at least once. If not, this test has failed.";
	static string Success = string.Empty;

	static FlyoutPage Reference;

	protected override void Init()
	{
		// Set FlyoutBehavior to Popover to ensure consistent behavior across desktop and mobile platforms.
		// Windows and Catalyst default (FlyoutLayoutBehavior.Default) uses Split mode, which differs from mobile platforms.
		FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
		var masterPage = new MasterPage();
		Flyout = masterPage;
		masterPage.ListView.ItemSelected += (sender, e) =>
		{
			var item = e.SelectedItem as MasterPageItem;
			if (item != null)
			{
				Detail = new NavigationPageEx((Page)Activator.CreateInstance(item.TargetType));
				masterPage.ListView.SelectedItem = null;
				IsPresented = false;
			}
		};

		Detail = new NavigationPageEx(new _409555_Page1());
		Reference = this;
	}


	public class MasterPageItem
	{
		public string IconSource { get; set; }

		public Type TargetType { get; set; }

		public string Title { get; set; }
	}


	public class MasterPage : ContentPage
	{
		public MasterPage()
		{
			Title = "Menu";
#pragma warning disable CS0618 // Type or member is obsolete
			ListView = new ListView { VerticalOptions = LayoutOptions.FillAndExpand, SeparatorVisibility = SeparatorVisibility.None };
#pragma warning restore CS0618 // Type or member is obsolete

			ListView.ItemTemplate = new DataTemplate(() =>
			{
				var ic = new ImageCell();
				ic.SetBinding(TextCell.TextProperty, "Title");
				return ic;
			});

			Content = new StackLayout
			{
				Children = { ListView }
			};

			var masterPageItems = new List<MasterPageItem>();
			masterPageItems.Add(new MasterPageItem
			{
				Title = Page1Title,
				TargetType = typeof(_409555_Page1)
			});
			masterPageItems.Add(new MasterPageItem
			{
				Title = Page2Title,
				TargetType = typeof(_409555_Page2)
			});
			masterPageItems.Add(new MasterPageItem
			{
				Title = Page3Title,
				TargetType = typeof(_409555_Page3)
			});

			ListView.ItemsSource = masterPageItems;
		}

		public ListView ListView { get; }
	}


	public class NavigationPageEx : NavigationPage
	{
		public NavigationPageEx(Page root) : base(root)
		{
		}

		~NavigationPageEx()
		{
			Debug.WriteLine(DestructorMessage);
			Success = DestructorMessage;
		}
	}


	public class _409555_Page1 : ContentPage
	{
		public _409555_Page1()
		{
			Title = Page1Title;

			var lbl = new Label
			{
				Text = LabelPage1,
				AutomationId = "LabelOne"
			};

			lbl.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(OpenMaster)
			});

			Content = new StackLayout
			{
				Children = { lbl }
			};
		}
	}

	static void OpenMaster()
	{
		Reference.IsPresented = true;
	}


	public class _409555_Page2 : ContentPage
	{
		public _409555_Page2()
		{
			Title = Page2Title;
			var lbl = new Label
			{
				Text = LabelPage2,
				AutomationId = "LabelTwo"
			};

			lbl.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(OpenMaster)
			});
			Content = new StackLayout { Children = { lbl } };
		}
	}


	public class _409555_Page3 : ContentPage
	{
		public _409555_Page3()
		{
			Title = Page3Title;

			var lbl = new Label
			{
				Text = LabelPage3,
				AutomationId = "LabelThree"
			};

			lbl.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(async () => await DisplayAlertAsync("Alert", Success, "Ok"))
			});

			var successLabel = new Label();
			Content = new StackLayout
			{
				Children =
				{
					lbl
				}
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			GarbageCollectionHelper.Collect();
		}
	}
}