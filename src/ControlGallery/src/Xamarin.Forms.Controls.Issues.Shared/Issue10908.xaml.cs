using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10908, "[Bug] [iOS]SwipeView not working on Grouped ListView", PlatformAffected.iOS)]
	public partial class Issue10908 : ContentPage
	{
		public Issue10908()
		{
#if APP
			InitializeComponent();

			Grouped = new ObservableCollection<GroupedIssue10908Model>();

			var veggieGroup = new GroupedIssue10908Model() { LongName = "vegetables", ShortName = "v" };
			var fruitGroup = new GroupedIssue10908Model() { LongName = "fruit", ShortName = "f" };
			var veggie1Group = new GroupedIssue10908Model() { LongName = "vegetables1", ShortName = "v1" };
			var fruit1Group = new GroupedIssue10908Model() { LongName = "fruit1", ShortName = "f1" };
			var veggie2Group = new GroupedIssue10908Model() { LongName = "vegetables2", ShortName = "v2" };
			var fruit2Group = new GroupedIssue10908Model() { LongName = "fruit2", ShortName = "f2" };
			veggieGroup.Add(new Issue10908Model() { Name = "celery", IsReallyAVeggie = true, Comment = "try ants on a log" });
			veggieGroup.Add(new Issue10908Model() { Name = "tomato", IsReallyAVeggie = false, Comment = "pairs well with basil" });
			veggieGroup.Add(new Issue10908Model() { Name = "zucchini", IsReallyAVeggie = true, Comment = "zucchini bread > bannana bread" });
			veggieGroup.Add(new Issue10908Model() { Name = "peas", IsReallyAVeggie = true, Comment = "like peas in a pod" });
			fruitGroup.Add(new Issue10908Model() { Name = "banana", IsReallyAVeggie = false, Comment = "available in chip form factor" });
			fruitGroup.Add(new Issue10908Model() { Name = "strawberry", IsReallyAVeggie = false, Comment = "spring plant" });
			fruitGroup.Add(new Issue10908Model() { Name = "cherry", IsReallyAVeggie = false, Comment = "topper for icecream" });

			veggie1Group.Add(new Issue10908Model() { Name = "celery", IsReallyAVeggie = true, Comment = "try ants on a log" });
			veggie1Group.Add(new Issue10908Model() { Name = "tomato", IsReallyAVeggie = false, Comment = "pairs well with basil" });
			veggie1Group.Add(new Issue10908Model() { Name = "zucchini", IsReallyAVeggie = true, Comment = "zucchini bread > bannana bread" });
			veggie1Group.Add(new Issue10908Model() { Name = "peas", IsReallyAVeggie = true, Comment = "like peas in a pod" });
			fruit1Group.Add(new Issue10908Model() { Name = "banana", IsReallyAVeggie = false, Comment = "available in chip form factor" });
			fruit1Group.Add(new Issue10908Model() { Name = "strawberry", IsReallyAVeggie = false, Comment = "spring plant" });
			fruit1Group.Add(new Issue10908Model() { Name = "cherry", IsReallyAVeggie = false, Comment = "topper for icecream" });

			veggie2Group.Add(new Issue10908Model() { Name = "celery", IsReallyAVeggie = true, Comment = "try ants on a log" });
			veggie2Group.Add(new Issue10908Model() { Name = "tomato", IsReallyAVeggie = false, Comment = "pairs well with basil" });
			veggie2Group.Add(new Issue10908Model() { Name = "zucchini", IsReallyAVeggie = true, Comment = "zucchini bread > bannana bread" });
			veggie2Group.Add(new Issue10908Model() { Name = "peas", IsReallyAVeggie = true, Comment = "like peas in a pod" });
			fruit2Group.Add(new Issue10908Model() { Name = "banana", IsReallyAVeggie = false, Comment = "available in chip form factor" });
			fruit2Group.Add(new Issue10908Model() { Name = "strawberry", IsReallyAVeggie = false, Comment = "spring plant" });
			fruit2Group.Add(new Issue10908Model() { Name = "cherry", IsReallyAVeggie = false, Comment = "topper for icecream" });

			Grouped.Add(veggieGroup);
			Grouped.Add(fruitGroup);
			Grouped.Add(veggie1Group);
			Grouped.Add(fruit1Group);
			Grouped.Add(veggie2Group);
			Grouped.Add(fruit2Group);

			lstView.ItemsSource = Grouped;
#endif
		}

		ObservableCollection<GroupedIssue10908Model> Grouped { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue10908Model
	{
		public string Name { get; set; }
		public string Comment { get; set; }
		public bool IsReallyAVeggie { get; set; }
		public string Image { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class GroupedIssue10908Model : ObservableCollection<Issue10908Model>
	{
		public string LongName { get; set; }
		public string ShortName { get; set; }
	}
}