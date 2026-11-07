using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38330, "CollectionView cells overlap after swiping a SwipeView item", PlatformAffected.iOS)]
public class Issue38330 : ContentPage
{
	public Issue38330()
	{
		var groups = new ObservableCollection<Issue38330Group>
		{
			new("Section A", Enumerable.Range(0, 3).Select(index => new Issue38330Item($"Row A {index}", false))),
			new("Section B", Enumerable.Range(0, 40).Select(index => new Issue38330Item($"Row B {index:D2}", true)))
		};

		var groupHeaderTemplate = new DataTemplate(() =>
		{
			var label = new Label
			{
				Padding = new Thickness(14, 10, 14, 4),
				Background = Colors.LightGray,
				FontAttributes = FontAttributes.Bold,
				FontSize = 16
			};
			label.SetBinding(Label.TextProperty, static (Issue38330Group group) => group.Name);
			return label;
		});

		Content = new CollectionView
		{
			AutomationId = "Issue38330CollectionView",
			IsGrouped = true,
			ItemsSource = groups,
			ItemTemplate = new Issue38330TemplateSelector(),
			GroupHeaderTemplate = groupHeaderTemplate,
			SelectionMode = SelectionMode.None
		};
	}

	sealed class Issue38330TemplateSelector : DataTemplateSelector
	{
		readonly DataTemplate _plainTemplate = new(CreateItemContent);
		readonly DataTemplate _swipeTemplate = new(CreateSwipeItem);

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
			((Issue38330Item)item).HasSwipeAction ? _swipeTemplate : _plainTemplate;

		static View CreateItemContent()
		{
			var label = new Label
			{
				VerticalOptions = LayoutOptions.Center
			};
			label.SetBinding(Label.TextProperty, static (Issue38330Item item) => item.Text);
			label.SetBinding(AutomationIdProperty, static (Issue38330Item item) => item.Text);

			return new Border
			{
				Margin = 6,
				Padding = 12,
				Background = Colors.White,
				HeightRequest = 64,
				Stroke = Colors.Gray,
				StrokeThickness = 1,
				Content = label
			};
		}

		static View CreateSwipeItem()
		{
			var swipeItem = new SwipeItemView
			{
				Background = Colors.Green,
				Content = new Label
				{
					Text = "read",
					TextColor = Colors.White,
					FontAttributes = FontAttributes.Bold,
					Padding = new Thickness(24, 0),
					VerticalOptions = LayoutOptions.Center
				}
			};
			var swipeItems = new SwipeItems
			{
				Mode = SwipeMode.Execute,
				SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close
			};
			swipeItems.Add(swipeItem);

			return new SwipeView
			{
				Threshold = 60,
				RightItems = swipeItems,
				Content = CreateItemContent()
			};
		}
	}
}

sealed class Issue38330Group : ObservableCollection<Issue38330Item>
{
	public Issue38330Group(string name, IEnumerable<Issue38330Item> items) : base(items)
	{
		Name = name;
	}

	public string Name { get; }
}

sealed record Issue38330Item(string Text, bool HasSwipeAction);