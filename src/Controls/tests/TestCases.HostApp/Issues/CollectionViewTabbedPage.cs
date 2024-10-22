using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	class Group : List<string>
	{
		public string Text { get; set; }

		public Group()
		{
			Add("Uno");
			Add("Dos");
			Add("Tres");
		}
	}

	// AddingGroupToUnviewedGroupedCollectionViewShouldNotCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue7700.cs)
	[Issue(IssueTracker.None, 7700, "If CollectionView in other Tab gets changed before it's displayed, it stays invisible", PlatformAffected.iOS)]
	public class CollectionViewTabbedPage : TabbedPage
	{
		const string Add1 = "Add1";
		const string Add2 = "Add2";
		const string Success = "Success";
		const string Tab2 = "Tab2";
		const string Tab3 = "Tab3";
		const string Add1Label = "Add to List";
		const string Add2Label = "Add to Grouped List";

		readonly ObservableCollection<string> _source = new ObservableCollection<string>() { "one", "two", "three" };
		readonly ObservableCollection<Group> _groupedSource = new ObservableCollection<Group>();

		public CollectionViewTabbedPage()
		{
			Children.Add(FirstPage());
			Children.Add(CollectionViewPage());
			Children.Add(GroupedCollectionViewPage());
		}

		ContentPage FirstPage()
		{
			var page = new ContentPage() { Title = "7700 First Page", Padding = 40 };

			var instructions = new Label { Text = $"Tap the button marked '{Add1Label}'. Then tap the button marked '{Add2Label}'. If the application does not crash, the test has passed." };

			var button1 = new Button() { Text = Add1Label, AutomationId = Add1 };
			button1.Clicked += Button1Clicked;

			var button2 = new Button() { Text = Add2Label, AutomationId = Add2 };
			button2.Clicked += Button2Clicked;

			var layout = new StackLayout { Children = { instructions, button1, button2 } };

			page.Content = layout;

			return page;
		}

		ContentPage CollectionViewPage()
		{
			var cv = new CollectionView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("."));
					return label;
				}),

				ItemsSource = _source
			};

			var page = new ContentPage() { AutomationId = Tab2, Title = Tab2, Padding = 40 };

			page.Content = cv;

			return page;
		}

		ContentPage GroupedCollectionViewPage()
		{
			var cv = new CollectionView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("."));
					return label;
				}),

				GroupHeaderTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("Text"));
					return label;
				}),

				GroupFooterTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("Text"));
					return label;
				}),

				ItemsSource = _groupedSource,
				IsGrouped = true
			};

			var page = new ContentPage() { AutomationId = Tab3, Title = Tab3, Padding = 40 };

			page.Content = cv;

			return page;
		}

		void Button1Clicked(object sender, EventArgs e)
		{
			_source.Insert(0, Success);
		}

		void Button2Clicked(object sender, EventArgs e)
		{
			_groupedSource.Insert(0, new Group() { Text = Success });
		}
	}
}