using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Maui.Controls.Sample.Issues;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26498, "Null Exception on clearing collection in list view after navigation",
		PlatformAffected.UWP)]
	public class Issue26498 : NavigationPage
	{
		public Issue26498() : base(new Issue26498TestPage())
		{
		}

		public class Issue26498TestPage() : TestContentPage
		{
			Page _listviewPage = null;

			protected override void Init()
			{
				var layout = new StackLayout();
				var openListViewPageButton = new Button
				{
					Text = "Open ListView Page",
					AutomationId = "OpenListViewPage",
					Command = new Command(OpenListViewPage)
				};
				layout.Children.Add(openListViewPageButton);

				Content = layout;
			}

			void OpenListViewPage()
			{
				_listviewPage ??= new ListViewPage();
				Navigation.PushAsync(_listviewPage);
			}
		}

	}

	public class ListViewPage : ContentPage
	{
		readonly ObservableCollection<string> _listOfStrings = new ObservableCollection<string>();
		ListView _stringListView = new ListView();
		public ListViewPage()
		{
			var stackLayout = new StackLayout();

			var ClearListButton = new Button
			{
				Text = "Clear list",
				AutomationId = "ClearButton",

			};

			ClearListButton.Clicked += (s, e) =>
			{
				ClearListItems();
			};

			var BackButton = new Button
			{
				Text = "Back to Main Page",
				AutomationId = "BackButton",
			};

			BackButton.Clicked += (s, e) =>
			{
				OpenMainPage();
			};

			stackLayout.Children.Add(ClearListButton);
			stackLayout.Children.Add(BackButton);
			_listOfStrings.Add("Item1");
			_listOfStrings.Add("Item2");
			_listOfStrings.Add("Item3");

			_stringListView = new ListView
			{
				ItemsSource = _listOfStrings
			};

			stackLayout.Children.Add(_stringListView);

			Content = stackLayout;
		}
		void ClearListItems()
		{
			_listOfStrings.Clear();
		}
		void OpenMainPage()
		{
			Navigation.PopAsync();
		}
	}

}