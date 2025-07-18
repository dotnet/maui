namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21119, "Search Handler visual and functional bug in subtabs", PlatformAffected.Android)]
public class Issue21119 : Shell
{
	public Issue21119()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;

		var flyoutItem = new FlyoutItem
		{
			Route = "animals",
			FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems
		};

		var domesticTab = new Tab
		{
			Title = "Domestic",
			Route = "domestic"
		};

		domesticTab.Items.Add(new ShellContent
		{
			Title = "CatsPage",
			Route = "cats",
			ContentTemplate = new DataTemplate(typeof(_21119CatsPage))
		});

		domesticTab.Items.Add(new ShellContent
		{
			Title = "DogsPage",
			Route = "dogs",
			ContentTemplate = new DataTemplate(typeof(_21119DogsPage))
		});

		flyoutItem.Items.Add(domesticTab);

		Items.Add(flyoutItem);
	}

	public class _21119CatsPage : ContentPage
	{
		public _21119CatsPage()
		{
			Title = "CatsPage";

			var searchHandler = new _21119AnimalSearchHandler
			{
				Placeholder = "Search cats...",
				ShowsResults = true
			};

			var catPageButton = new Button
			{
				Text = "CatPageButton",
				AutomationId = "CatPageButton"
			};

			Content = new StackLayout
			{
				Children =
				{
					catPageButton
				}
			};

			Shell.SetSearchHandler(this, searchHandler);
		}
	}

	public class _21119DogsPage : ContentPage
	{
		public _21119DogsPage()
		{
			Title = "DogsPage";

			var searchHandler = new _21119AnimalSearchHandler
			{
				Placeholder = "Search dogs...",
				ShowsResults = true
			};

			Shell.SetSearchHandler(this, searchHandler);

			var dogPageButton = new Button
			{
				Text = "DogPageButton",
				AutomationId = "DogsPageButton"
			};

			Content = new StackLayout
			{
				Children =
				{
					dogPageButton
				}
			};
		}
	}

	public class _21119AnimalSearchHandler : SearchHandler
	{
		public _21119AnimalSearchHandler()
		{
			SearchBoxVisibility = SearchBoxVisibility.Collapsible;
		}
	}
}