namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Shell Flyout Content",
	PlatformAffected.All)]

public class ShellFlyoutContent : TestShell
{
	protected override void Init()
	{
		var page = new ContentPage();

		this.BindingContext = this;
		AddFlyoutItem(page, "Flyout Item Top");
		for (int i = 0; i < 50; i++)
		{
			AddFlyoutItem($"Flyout Item :{i}");
			Items[i].AutomationId = "FlyoutItem";
		}

		Items.Add(new MenuItem() { Text = "Menu Item" });
		var bottomItem = AddFlyoutItem("Flyout Item Bottom");
		bottomItem.AutomationId = "Flyout Item Bottom";

		var layout = new StackLayout()
		{
			new Label()
			{
				Text = "Open the Flyout and Toggle the Content, Header and Footer. If it changes after each click test has passed",
				AutomationId = "PageLoaded"
			}
		};

		page.Content = layout;

		layout.Add(new Button()
		{
			Text = "Toggle Flyout Content Template",
			Command = new Command(() =>
			{
				if (FlyoutContentTemplate == null)
				{
					FlyoutContentTemplate = new DataTemplate(() =>
					{
						var collectionView = new CollectionView();

						collectionView.SetBinding(CollectionView.ItemsSourceProperty, "FlyoutItems");
						collectionView.IsGrouped = true;

						collectionView.ItemTemplate =
							new DataTemplate(() =>
							{
								var label = new Label();

								label.SetBinding(Label.TextProperty, "Title");

								var button = new Button()
								{
									Text = "Click to Reset",
									AutomationId = "Reset",
									Command = new Command(() =>
									{
										FlyoutContentTemplate = null;
									})
								};

								return new StackLayout()
								{
									label,
									button
								};
							});

						return collectionView;
					});
				}
				else if (FlyoutContentTemplate != null)
				{
					FlyoutContentTemplate = null;
				}
			}),
			AutomationId = "ToggleFlyoutContentTemplate"
		});

		layout.Children.Add(new Button()
		{
			Text = "Toggle Flyout Content",
			Command = new Command(() =>
			{
				if (FlyoutContent != null)
				{
					FlyoutContent = null;
				}
				else
				{
					var stackLayout = new StackLayout()
					{
						Background = SolidColorBrush.Green
					};

					FlyoutContent = new ScrollView()
					{
						Content = stackLayout
					};

					AddButton("Top Button");

					for (int i = 0; i < 50; i++)
					{
						AddButton("Content View");
					}

					AddButton("Bottom Button");

					void AddButton(string text)
					{
						stackLayout.Children.Add(new Button()
						{
							Text = text,
							AutomationId = "ContentView",
							Command = new Command(() =>
							{
								FlyoutContent = null;
							}),
							TextColor = Colors.White
						});
					}
				}
			}),
			AutomationId = "ToggleContent"
		});

		layout.Add(new Button()
		{
			Text = "Toggle Header/Footer View",
			Command = new Command(() =>
			{
				if (FlyoutHeader != null)
				{
					FlyoutHeader = null;
					FlyoutFooter = null;
				}
				else
				{
					var flyoutHeader = new StackLayout()
					{
						AutomationId = "Header View",
						Background = SolidColorBrush.Yellow
					};

					flyoutHeader.Add(new Label() { Text = "Header" });

					FlyoutHeader = flyoutHeader;

					var flyoutFooter = new StackLayout()
					{
						Background = SolidColorBrush.Orange,
						Orientation = StackOrientation.Horizontal,
						AutomationId = "Footer View"
					};

					flyoutFooter.Add(new Label() { Text = "Footer" });

					FlyoutFooter = flyoutFooter;
				}
			}),
			AutomationId = "ToggleHeaderFooter"
		});
	}
}
