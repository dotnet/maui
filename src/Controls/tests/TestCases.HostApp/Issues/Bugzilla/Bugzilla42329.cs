namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 42329, "ListView in Frame and FormsAppCompatActivity Memory Leak")]
public class Bugzilla42329 : TestFlyoutPage
{
	const string DestructorMessage = "ContentPageEx Destructor called";
	const string Page1Title = "Page1";
	const string Page2Title = "Page2";
	const string Page3Title = "Page3";
	const string LabelPage1 = "Open the drawer menu and select Page2";
	const string LabelPage2 = "Open the drawer menu and select Page3";
	readonly static string LabelPage3 = $"The console should have displayed the text '{DestructorMessage}' at least once. If not, this test has failed.";
	static string Success { get; set; } = string.Empty;
	static FlyoutPage Reference;

	protected override void Init()
	{
		var rootPage = new RootPage();
		Flyout = rootPage;

		// Set FlyoutBehavior to Popover to ensure consistent behavior across desktop and mobile platforms.
		// Windows and Catalyst default (FlyoutLayoutBehavior.Default) uses Split mode, which differs from mobile platforms.
		FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
		rootPage.ListView.ItemSelected += (sender, e) =>
		{
			var item = e.SelectedItem as RootPageItem;
			if (item != null)
			{
				Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
				rootPage.ListView.SelectedItem = null;
				IsPresented = false;
			}
		};

		Detail = new NavigationPage(new _42329_FrameWithListView());
		Reference = this;
	}


	public class RootPage : ContentPage
	{
		public RootPage()
		{
			Title = "Menu";
#pragma warning disable CS0618 // Type or member is obsolete
			ListView = new ListView { VerticalOptions = LayoutOptions.FillAndExpand, SeparatorVisibility = SeparatorVisibility.None };
#pragma warning restore CS0618 // Type or member is obsolete

			ListView.ItemTemplate = new DataTemplate(() =>
			{
				var ic = new ImageCell();
				ic.SetBinding(TextCell.TextProperty, "Title");
				ic.SetBinding(TextCell.AutomationIdProperty, "Title");
				return ic;
			});

			Content = new StackLayout
			{
				Children = { ListView }
			};

			var rootPageItems = new List<RootPageItem>();
			rootPageItems.Add(new RootPageItem
			{
				Title = Page1Title,
				TargetType = typeof(Bugzilla42329._42329_FrameWithListView)
			});
			rootPageItems.Add(new RootPageItem
			{
				Title = Page2Title,
				TargetType = typeof(Bugzilla42329._42329_Page2)
			});
			rootPageItems.Add(new RootPageItem
			{
				Title = Page3Title,
				TargetType = typeof(Bugzilla42329._42329_Page3)
			});

			ListView.ItemsSource = rootPageItems;
		}

		public ListView ListView { get; }
	}


	public class RootPageItem
	{
		public string IconSource { get; set; }

		public Type TargetType { get; set; }

		public string Title { get; set; }
	}


	public class ContentPageEx : ContentPage
	{
		~ContentPageEx()
		{
			Success = "Destructor called";
			Log.Warning("Bugzilla42329", DestructorMessage);
		}
	}


	public class _42329_FrameWithListView : ContentPageEx
	{
		public _42329_FrameWithListView()
		{
			var lv = new ListView();
			var label = new Label() { Text = LabelPage1, AutomationId = LabelPage1 };
			label.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(OpenRoot)
			});
			var frame = new Frame { Content = lv };

			Title = Page1Title;
			Content = new StackLayout
			{
				Children =
				{
					label,
					frame
				}
			};
		}
	}

	static void OpenRoot()
	{
		Reference.IsPresented = true;
	}


	public class _42329_Page2 : ContentPage
	{
		public _42329_Page2()
		{
			var lbl = new Label
			{
				Text = LabelPage2,
				AutomationId = LabelPage2
			};
			lbl.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(OpenRoot)
			});

			Title = Page2Title;
			Content = new StackLayout
			{
				Children =
				{
					lbl
				}
			};
		}
	}


	public class _42329_Page3 : ContentPage
	{
		Label lblFlag;
		Label otherLabel;
		public _42329_Page3()
		{
			Title = Page3Title;
			Success = Success;
			lblFlag = new Label
			{
				Text = LabelPage3,
				HorizontalTextAlignment = TextAlignment.Center,
				TextColor = Colors.Red
			};

			otherLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				FontAttributes = FontAttributes.Bold
			};
			Content = new StackLayout
			{
				Children =
				{
					lblFlag,
					otherLabel
				}
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			GarbageCollectionHelper.Collect();
			otherLabel.Text = Success;
			otherLabel.AutomationId = Success;
		}
	}
}
