using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 3053, "Moving items around on an Observable Collection causes the last item to disappear", PlatformAffected.UWP)]

public class Issue3053 : TestContentPage
{
	const string _instructions = "Click me once. Item 2 should remain on bottom";


	public class Item
	{
		public string Name { get; set; }
	}

	protected override void Init()
	{
		var listView = new ListView
		{
			ItemsSource = new ObservableCollection<Item>(Enumerable.Range(0, 3).Select(x => new Item() { Name = $"Item {x}" })),
			ItemTemplate = new DataTemplate(() =>
			{
				Label nameLabel = new Label();
				nameLabel.SetBinding(Label.TextProperty, new Binding("Name"));
				nameLabel.SetBinding(Label.AutomationIdProperty, new Binding("Name"));
				var cell = new ViewCell
				{
					//Frame is obsolete in net9.0, so here changed to border
					View = new Border()
					{
						Content = nameLabel
					},
				};
				return cell;
			})
		};
		Content = new StackLayout
		{
			Children =
			{
				new Button()
				{
					Text = _instructions,
					AutomationId = "InstructionButton",
					Command = new Command(() =>
					{
						var collection = listView.ItemsSource as ObservableCollection<Item>;
						collection.Add(new Item(){ Name =  Guid.NewGuid().ToString() });
						collection.Move(3,1);
					})
				},
				listView
			}
		};
	}
}