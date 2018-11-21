using System;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 55555, "Header problem")]
	public class Issue55555 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			var lstView = new ListView(ListViewCachingStrategy.RecycleElement);
			ObservableCollection<GroupedVeggieModel> grouped = CreateData();

			lstView.ItemsSource = grouped;
			lstView.HasUnevenRows = true;
			lstView.IsGroupingEnabled = true;
			lstView.GroupDisplayBinding = new Binding("LongName");
			lstView.GroupShortNameBinding = new Binding("ShortName");

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

		[Preserve(AllMembers = true)]
		public class VeggieModel
		{
			public string Name { get; set; }
			public string Comment { get; set; }
			public bool IsReallyAVeggie { get; set; }
			public string Image { get; set; }
		}

		[Preserve(AllMembers = true)]
		public class GroupedVeggieModel : ObservableCollection<VeggieModel>
		{
			public string LongName { get; set; }
			public string ShortName { get; set; }
		}

		[Preserve(AllMembers = true)]
		public class DemoTextCell : TextCell
		{
			public DemoTextCell()
			{
				Height = 150;
			}
		}

#if UITEST
		[Test]
		public void TGroupDisplayBindingPresentRecycleElementTest()
		{
			RunningApp.WaitForElement(q => q.Marked("vegetables"));
		}
#endif
	}
}
