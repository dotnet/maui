using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 58875, "Back navigation disables Context Action in whole app, if Context Action left open", PlatformAffected.iOS)]
public class Bugzilla58875 : TestNavigationPage
{
	const string Button1Id = "Button1Id";
	const string ContextAction = "More";
	const string Target = "Swipe me";

	protected override void Init()
	{
		var page1 = new Page1();
		Navigation.PushAsync(page1);
	}


	class ListViewPage : ContentPage
	{
		public ListViewPage()
		{
			BindingContext = this;

			var listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label { };
					label.SetBinding(Label.TextProperty, ".");
					var viewcell = new ViewCell
					{
						View = new StackLayout { Children = { label } }
					};
					viewcell.ContextActions.Add(new MenuItem { Text = ContextAction });
					viewcell.ContextActions.Add(new MenuItem { Text = "Delete", IsDestructive = true });
					return viewcell;
				})
			};

			listView.SetBinding(ListView.ItemsSourceProperty, nameof(Items));

			Items = new ObservableCollection<string> {
					"Item 1",
					Target,
					"Item 3",
					"Swipe me too, leave me open",
					"Swipe left -> right (trigger back navigation)"
			};

			Content = listView;
		}

		public ObservableCollection<string> Items { get; set; }
	}


	class Page1 : ContentPage
	{
		public Page1()
		{
			var button = new Button { Text = "Tap me", AutomationId = Button1Id };
			button.Clicked += Button_Clicked;
			Content = button;
		}

		void Button_Clicked(object sender, System.EventArgs e)
		{
			var listPage = new ListViewPage();
			Navigation.PushAsync(listPage);
		}
	}
}