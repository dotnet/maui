using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 44129, "Crash when adding tabbed page after removing all pages using DataTemplates")]
public class Bugzilla44129 : TestTabbedPage
{
	protected override void Init()
	{
		// Initialize ui here instead of ctor
		var viewModels = new ObservableCollection<string>();
		viewModels.Add("First");
		viewModels.Add("Second");
		var template = new DataTemplate(() =>
		{
			ContentPage page = new ContentPage();
			var crashMe = new Button { Text = "Crash Me" };
			crashMe.Clicked += (sender, args) =>
			{
				viewModels.Clear();
				viewModels.Add("Third");
			};

			page.Content = crashMe;
			page.SetBinding(ContentPage.TitleProperty, ".");
			page.SetBinding(ContentPage.AutomationIdProperty, ".");

			return page;
		});

		ItemTemplate = template;
		ItemsSource = viewModels;
	}
}