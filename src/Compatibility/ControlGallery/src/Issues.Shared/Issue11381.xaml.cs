using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11381, "[Bug] [iOS] NRE on grouped ListView when removing cells with gesture recognizers",
		PlatformAffected.iOS)]
	public partial class Issue11381 : TestContentPage
	{
		public Issue11381()
		{
#if APP
			InitializeComponent();

			grouped = new ObservableCollection<GroupedIssue11381Model>();

			var g1Group = new GroupedIssue11381Model() { LongName = "Group1", ShortName = "g1" };
			var g2Group = new GroupedIssue11381Model() { LongName = "Group2", ShortName = "g2" };

			g1Group.Add(new Issue11381Model() { Name = "G1I1", IsReallyAVeggie = true, Comment = "Lorem ipsum dolor sit amet" });
			g1Group.Add(new Issue11381Model() { Name = "G1I2", IsReallyAVeggie = false, Comment = "Lorem ipsum dolor sit amet" });
			g1Group.Add(new Issue11381Model() { Name = "G1I3", IsReallyAVeggie = true, Comment = "Lorem ipsum dolor sit amet" });
			g1Group.Add(new Issue11381Model() { Name = "G1I4", IsReallyAVeggie = true, Comment = "Lorem ipsum dolor sit amet" });

			g2Group.Add(new Issue11381Model() { Name = "G2I1", IsReallyAVeggie = false, Comment = "Lorem ipsum dolor sit amet" });
			g2Group.Add(new Issue11381Model() { Name = "G2I2", IsReallyAVeggie = false, Comment = "Lorem ipsum dolor sit amet" });

			grouped.Add(g1Group);
			grouped.Add(g2Group);

			Issue11381ListView.ItemsSource = grouped;
#endif
		}

		public ObservableCollection<GroupedIssue11381Model> grouped { get; set; }

		protected override void Init()
		{

		}

#if APP
		void OnTapGestureRecognizerTapped(object sender, EventArgs e)
		{
			if (sender is View view && view.BindingContext is Issue11381Model model)
			{
				var group = grouped.FirstOrDefault(g => g.Contains(model));

				if (group != null)
				{
					group.Remove(model);

					if (!group.Any())
					{
						grouped.Remove(group);
					}

					ItemsCount.Text = $"{grouped.Count} groups";
				}
			}
		}
#endif

#if UITEST && __IOS__
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue11381RemoveListViewGroups()
		{
			RunningApp.WaitForElement("ListViewId", "Timed out waiting for the ListView.");

			RunningApp.Tap(x => x.Marked("G1I1"));
			RunningApp.Tap(x => x.Marked("G1I2"));
			RunningApp.Tap(x => x.Marked("G1I3"));
			RunningApp.Tap(x => x.Marked("G1I4"));
			RunningApp.Tap(x => x.Marked("G2I1"));
			RunningApp.Tap(x => x.Marked("G2I2"));

			RunningApp.WaitForElement("ResultId");
			Assert.AreEqual("0 groups", RunningApp.WaitForElement("ResultId")[0].ReadText());
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class Issue11381Model
	{
		public string Name { get; set; }
		public string Comment { get; set; }
		public bool IsReallyAVeggie { get; set; }
		public string Image { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class GroupedIssue11381Model : ObservableCollection<Issue11381Model>
	{
		public string LongName { get; set; }
		public string ShortName { get; set; }
	}
}