namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 11214, "When adding FlyoutItems during Navigating only first one is shown", PlatformAffected.iOS)]
public class Issue11214 : TestShell
{
	FlyoutItem _itemexpanderItems;
	protected override void Init()
	{
		_itemexpanderItems = new FlyoutItem()
		{
			Title = "Expando Magic",
			FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems
		};

		ContentPage contentPage = new ContentPage()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "Open the Flyout",
						AutomationId = "PageLoaded"
					}
				}
			}
		};

		AddFlyoutItem(contentPage, "Top Item");

		var flyoutItem = AddFlyoutItem("Click Me and You Should see 2 Items show up");
		flyoutItem.Route = "ExpandMe";
		flyoutItem.AutomationId = "ExpandMe";
		Items.Add(_itemexpanderItems);
	}

	protected override void OnNavigating(ShellNavigatingEventArgs args)
	{
		base.OnNavigating(args);

		if (!args.Target.FullLocation.ToString().Contains("ExpandMe", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		args.Cancel();

		if (_itemexpanderItems.Items.Count == 0 ||
			_itemexpanderItems.Items[0].Items.Count == 0)
		{
			for (int i = 0; i < 2; i++)
			{
				_itemexpanderItems.Items.Add(new ShellContent()
				{
					Title = $"Some Item: {i}",
					Content = new ContentPage()
				});
			}
		}
		else
		{
			_itemexpanderItems.Items.Clear();
		}
	}
}
