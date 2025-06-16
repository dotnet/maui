using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19219, "[Android, iOS, macOS] Shell SearchHandler Command Not Executed on Item Selection", PlatformAffected.iOS)]
public class Issue19219Shell : Shell
{
	public Issue19219Shell()
	{
		this.FlyoutBehavior = FlyoutBehavior.Flyout;

		var shellContent = new ShellContent
		{
			Title = "Home",
			Route = "MainPage",
			Content = new Issue19219() { Title = "Home" }
		};

		Items.Add(shellContent);
	}

	class Issue19219 : ContentPage
	{
		public Issue19219()
		{
			Label label = new Label
			{
				Text = "SearchHandler command will execute when tap on item",
				AutomationId = "SearchHandlerLabel"
			};

			var fruits = new List<string> { "New York", "London", "Sydney", "Toronto", "Los Angeles", "Chicago", "Melbourne", "Vancouver",
  			"Manchester", "Birmingham", "San Francisco", "Dublin", "Auckland", "Glasgow", "Perth", "Houston",
  			"Seattle", "Cape Town", "Ottawa", "Brisbane", "Boston", "Phoenix", "Washington", "Edinburgh" };
			var searchHandler = new SearchHandler
			{
				AutomationId = "searchBar",
				Placeholder = "SearchHandler",
				Command = (new Command(() =>
				{
					label.Text = "SearchHandler Command Executed when tap on item";
				})),
				ShowsResults = true,
			};
			searchHandler.SetBinding(SearchHandler.ItemsSourceProperty, new Binding
			{
				Source = fruits,
				Mode = BindingMode.OneWay
			});

			Shell.SetSearchHandler(this, searchHandler);
			Button button = new Button
			{
				Text = "Set query in SearchHandler",
				Command = new Command(() =>
				{
					searchHandler.Query = "a";

				}),
				AutomationId = "SetQueryButton"
			};

			StackLayout stackLayout = new StackLayout
			{
				Children = { button, label }
			};

			Content = stackLayout;
		}
	}
}
