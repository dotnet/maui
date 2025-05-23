namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 29570, "Unable to Programmatically Update SearchHandler Query Value on Second Tab within ShellSection", PlatformAffected.Android)]
public class Issue29570 : Shell
{
	public Issue29570()
	{
		this.FlyoutBehavior = FlyoutBehavior.Flyout;
		this.FlyoutBackgroundImageAspect = Aspect.AspectFill;
		this.FlyoutHeaderBehavior = FlyoutHeaderBehavior.CollapseOnScroll;

		FlyoutItem flyoutItem = new FlyoutItem
		{
			Route = "animals",
			FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems
		};

		Tab domesticTab = new Tab
		{
			Title = "Domestic",
			Route = "domestic"
		};

		domesticTab.Items.Add(new ShellContent
		{
			Title = "CatsPage",
			Route = "cats",
			ContentTemplate = new DataTemplate(typeof(Issue29570CatsPage))
		});

		domesticTab.Items.Add(new ShellContent
		{
			Title = "DogsPage",
			Route = "dogs",
			ContentTemplate = new DataTemplate(typeof(Issue29570DogsPage))
		});

		flyoutItem.Items.Add(domesticTab);
		Items.Add(flyoutItem);
	}

	public class Issue29570CatsPage : ContentPage
	{
		Issue29570AnimalSearchHandler searchHandler;
		public Issue29570CatsPage()
		{
			Title = "CatsPage";

			searchHandler = new Issue29570AnimalSearchHandler
			{
				Placeholder = "Enter cat's name",
				ShowsResults = true,
				Animals = new List<string>() { "Abyssinian", "Arabian Mau" },
				ItemTemplate = new DataTemplate(() =>
				{
					Label nameLabel = new Label
					{
						FontAttributes = FontAttributes.Bold,
					};

					nameLabel.SetBinding(Label.TextProperty, ".");
					return nameLabel;
				})
			};

			Shell.SetSearchHandler(this, searchHandler);

			Button button = new Button
			{
				Text = "UpdateQuery",
				AutomationId = "CatsPageUpdateQueryBtn"
			};

			VerticalStackLayout layout = new VerticalStackLayout
			{
				Children = { button }
			};

			Content = layout;
		}
	}

	public class Issue29570DogsPage : ContentPage
	{
		Issue29570AnimalSearchHandler searchHandler;
		public Issue29570DogsPage()
		{
			Title = "DogsPage";

			searchHandler = new Issue29570AnimalSearchHandler
			{
				Placeholder = "Enter dog's name",
				ShowsResults = true,
				Animals = new List<string>() { "Afghan Hound", "Alpine Dachsbracke" },
				ItemTemplate = new DataTemplate(() =>
				{
					Label nameLabel = new Label
					{
						FontAttributes = FontAttributes.Bold,
						VerticalOptions = LayoutOptions.Center
					};

					nameLabel.SetBinding(Label.TextProperty, ".");
					return nameLabel;
				})
			};

			Shell.SetSearchHandler(this, searchHandler);

			Button button = new Button
			{
				Text = "UpdateQuery",
				AutomationId = "UpdateQueryButton"
			};

			button.Clicked += (s, e) =>
			{
				searchHandler.Query = "Hound";
			};

			VerticalStackLayout layout = new VerticalStackLayout
			{
				Children = { button }
			};

			Content = layout;
		}
	}

	public class Issue29570AnimalSearchHandler : SearchHandler
	{
		public IList<string> Animals { get; set; }

		protected override void OnQueryChanged(string oldValue, string newValue)
		{
			base.OnQueryChanged(oldValue, newValue);
		}
	}
}