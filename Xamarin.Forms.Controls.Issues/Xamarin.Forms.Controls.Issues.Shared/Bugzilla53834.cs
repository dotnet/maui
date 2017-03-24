using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 53834, "incorrect row heights on ios when using groupheadertemplate in Xamarin.Forms 2.3.4.214-pre5", PlatformAffected.iOS)]
	public class Bugzilla53834 : TestContentPage
	{
		const string Instructions = "";
		ObservableCollection<GroupedItem> grouped { get; set; }
		ListView lstView;

		class MyViewCell : ViewCell
		{
			public MyViewCell()
			{
				var label = new Label { HeightRequest = 66, VerticalOptions = LayoutOptions.Start };
				label.SetBinding(Label.TextProperty, ".");
				View = new StackLayout { Padding = 10, Children = { label } };
			}
		}

		class MyHeaderViewCell : ViewCell
		{
			public MyHeaderViewCell()
			{
				Height = 25;
				var label = new Label { VerticalOptions = LayoutOptions.Center };
				label.SetBinding(Label.TextProperty, nameof(GroupedItem.LongName));
				View = label;
			}
		}

		class GroupedItem : ObservableCollection<string>
		{
			public string LongName { get; set; }
			public string ShortName { get; set; }
		}

		protected override void Init()
		{
			var label = new Label { Text = Instructions };
			grouped = new ObservableCollection<GroupedItem>();
			lstView = new ListView()
			{
				IsGroupingEnabled = true,
				HasUnevenRows = true,
				ItemTemplate = new DataTemplate(typeof(MyViewCell)),
				GroupHeaderTemplate = new DataTemplate(typeof(MyHeaderViewCell)),
				ItemsSource = grouped,
			};

			var grp1 = new GroupedItem() { LongName = "Group 1", ShortName = "1" };
			var grp2 = new GroupedItem() { LongName = "Group 2", ShortName = "2" };

			for (int i = 1; i < 4; i++)
			{
				grp1.Add($"I am a short text #{i}");
				grp1.Add($"I am a long text that should cause the line to wrap, and I should not be cut off or overlapping in any way. #{i}");
				grp2.Add($"I am a short text #{i}");
				grp2.Add($"I am a long text that should cause the line to wrap, and I should not be cut off or overlapping in any way. #{i}");
			}

			grouped.Add(grp1);
			grouped.Add(grp2);

			Content = new StackLayout
			{
				Children = {
					label,
					lstView
				}
			};
		}

#if (UITEST && __IOS__)
		[Test]
		[Category(UITestCategories.ManualReview)]
		public void Bugzilla53834Test()
		{
			RunningApp.Screenshot("incorrect row heights test");
		}
#endif
	}
}