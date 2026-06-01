namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23038, "[Windows] GroupHeaderTemplate width smaller than ItemTemplate", PlatformAffected.UWP)]
public class Issue23038 : ContentPage
{
	public Issue23038()
	{
		var teams = new List<Issue23038Team>
		{
			new("TeamA", "Team A", new List<Issue23038Member>
			{
				new("Member 1"), new("Member 2"), new("Member 3"),
			}),
			new("TeamB", "Team B", new List<Issue23038Member>
			{
				new("Member 4"), new("Member 5"),
			}),
		};

		var collectionView = new CollectionView
		{
			AutomationId = "GroupedCollectionView",
			IsGrouped = true,
			ItemsSource = teams,
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					BackgroundColor = Colors.LightGreen,
					FontAttributes = FontAttributes.Bold,
					Padding = new Thickness(5),
				};
				label.SetBinding(Label.TextProperty, nameof(Issue23038Team.Name));
				label.SetBinding(Label.AutomationIdProperty, nameof(Issue23038Team.Key), stringFormat: "Header{0}");
				return label;
			}),
			GroupFooterTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					BackgroundColor = Colors.Orange,
					Padding = new Thickness(5),
				};
				label.SetBinding(Label.TextProperty, "Count", stringFormat: "Total: {0}");
				label.SetBinding(Label.AutomationIdProperty, nameof(Issue23038Team.Key), stringFormat: "Footer{0}");
				return label;
			}),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(5, 2),
				};
				label.SetBinding(Label.TextProperty, nameof(Issue23038Member.Name));
				return label;
			}),
		};

		Content = collectionView;
	}
}

public class Issue23038Team : List<Issue23038Member>
{
	public Issue23038Team(string key, string name, List<Issue23038Member> members) : base(members)
	{
		Key = key;
		Name = name;
	}

	public string Key { get; set; }
	public string Name { get; set; }
}

public class Issue23038Member
{
	public Issue23038Member(string name) => Name = name;
	public string Name { get; set; }
}
