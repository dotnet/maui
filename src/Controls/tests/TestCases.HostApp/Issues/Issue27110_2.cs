namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27110_2, "ScrollTo a grouped CollectionView item by object", PlatformAffected.UWP)]
public sealed class Issue27110_2 : Issue27110ObjectScrollSamplePage
{
	readonly IList<Issue27110MonkeyGroup> _groups;

	public Issue27110_2()
		: this(Issue27110ObjectScrollSampleData.CreateGroups())
	{
	}

	Issue27110_2(IList<Issue27110MonkeyGroup> groups)
		: base("Scroll to object (grouped data)", groups, isGrouped: true)
	{
		_groups = groups;
	}

	protected override void ScrollToItem(ScrollToPosition position, bool animate)
	{
		var group = _groups.FirstOrDefault(item => item.Name == "Monkeys")
			?? throw new InvalidOperationException("The Monkeys group is missing from the sample data.");
		var monkey = group.FirstOrDefault(item => item.Name == "Proboscis Monkey")
			?? throw new InvalidOperationException("The Proboscis Monkey item is missing from the Monkeys group.");
		CollectionView.ScrollTo(monkey, group, position, animate);
	}
}
