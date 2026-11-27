using System.Collections.Generic;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 40414, "CollectionViewHandler2 does not display grouped CollectionView items after ItemsSource is reset", PlatformAffected.UWP)]
public class Issue40414 : Shell
{
	public Issue40414()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;

		Items.Add(new ShellContent
		{
			Title = "Flyout1Content",
			ContentTemplate = new DataTemplate(() => new ContentPage
			{
				Content = new Label
				{
					Text = "Open Flyout2Content, return to Flyout1Content, then open Flyout2Content again. The grouped items should be visible.",
					AutomationId = "Flyout1Label"
				}
			})
		});

		Items.Add(new ShellContent
		{
			Title = "Flyout2Content",
			ContentTemplate = new DataTemplate(() => new Issue40414CollectionPage())
		});
	}
}

class Issue40414CollectionPage : ContentPage
{
	readonly CollectionView _collectionView;

	public Issue40414CollectionPage()
	{
		_collectionView = new CollectionView
		{
			IsGrouped = true,
			ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
			SelectionMode = SelectionMode.None,
			AutomationId = "GroupedCollectionView",
			ItemsLayout = new GridItemsLayout(5, ItemsLayoutOrientation.Vertical),
			// A layout as the template root is required to reproduce the issue.
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				return new HorizontalStackLayout { Children = { label } };
			}),
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, nameof(Issue40414Group.GroupName));
				return new HorizontalStackLayout { Children = { new Label { Text = "Group: " }, label } };
			})
		};

		Padding = 16;
		Content = _collectionView;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		_collectionView.ItemsSource = new List<Issue40414Group>
		{
			new Issue40414Group("1", ["A", "B", "C", "D", "E", "F", "G"]),
			new Issue40414Group("2", ["A", "B", "C", "D", "E", "F", "G"]),
			new Issue40414Group("3", ["A", "B", "C", "D", "E", "F", "G"]),
		};
	}
}

public class Issue40414Group : List<string>
{
	public Issue40414Group(string groupName, List<string> items) : base(items)
	{
		GroupName = groupName;
	}

	public string GroupName { get; }
}
