using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 55555, "Header problem")]
public class Issue55555 : TestContentPage
{
	protected override void Init()
	{
		var lstView = new ListView(ListViewCachingStrategy.RecycleElement);
		ObservableCollection<GroupedVeggieModel> grouped = CreateData();

		lstView.ItemsSource = grouped;
		lstView.HasUnevenRows = true;
		lstView.IsGroupingEnabled = true;
		lstView.GroupDisplayBinding = new Binding("LongName");
#if !WINDOWS // Getting exception while running the sample with GroupShortNameBinding on windows, Issue: https://github.com/dotnet/maui/issues/26534.
		lstView.GroupShortNameBinding = new Binding("ShortName");
#endif

		lstView.ItemTemplate = new DataTemplate(typeof(DemoTextCell));
		lstView.ItemTemplate.SetBinding(DemoTextCell.TextProperty, "Name");

		Content = lstView;
	}

	static ObservableCollection<GroupedVeggieModel> CreateData()
	{
		var grouped = new ObservableCollection<GroupedVeggieModel>();

		var veggieGroup = new GroupedVeggieModel() { LongName = "vegetables", ShortName = "v" };
		veggieGroup.Add(new VeggieModel() { Name = "celery", IsReallyAVeggie = true, Comment = "try ants on a log" });
		veggieGroup.Add(new VeggieModel() { Name = "tomato", IsReallyAVeggie = false, Comment = "pairs well with basil" });
		veggieGroup.Add(new VeggieModel() { Name = "zucchini", IsReallyAVeggie = true, Comment = "zucchini bread > bannana bread" });
		veggieGroup.Add(new VeggieModel() { Name = "peas", IsReallyAVeggie = true, Comment = "like peas in a pod" });

		var fruitGroup = new GroupedVeggieModel() { LongName = "fruit", ShortName = "f" };
		fruitGroup.Add(new VeggieModel() { Name = "banana", IsReallyAVeggie = false, Comment = "available in chip form factor" });
		fruitGroup.Add(new VeggieModel() { Name = "strawberry", IsReallyAVeggie = false, Comment = "spring plant" });
		fruitGroup.Add(new VeggieModel() { Name = "cherry", IsReallyAVeggie = false, Comment = "topper for icecream" });

		grouped.Add(veggieGroup);
		grouped.Add(fruitGroup);

		return grouped;
	}

	public class VeggieModel
	{
		public string Name { get; set; }
		public string Comment { get; set; }
		public bool IsReallyAVeggie { get; set; }
		public string Image { get; set; }
	}

	public class GroupedVeggieModel : ObservableCollection<VeggieModel>
	{
		public string LongName { get; set; }
		public string ShortName { get; set; }
	}

	public class DemoTextCell : TextCell
	{
		public DemoTextCell()
		{
			Height = 150;
		}
	}
}
