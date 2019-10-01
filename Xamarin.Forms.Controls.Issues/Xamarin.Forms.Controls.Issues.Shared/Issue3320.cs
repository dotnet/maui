using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Collections.ObjectModel;
using System;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{

#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3320, "[iOS] Cells aren't highlighted on touch-down when the ListView's CachingStrategy is set to RecycleElement", PlatformAffected.iOS)]
	public class Issue3320 : TestContentPage
	{

		protected override void Init()
		{
			Title = "List Page";
			ObservableCollection<string> source = new ObservableCollection<string>();
			for (int i = 0; i < 50; i++)
			{

				source.Add($"ListItem:{i}");
			}

			var listview = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				ItemsSource = source
			};

			this.Content = listview;
		}

#if UITEST
		[Test]
		[Description("Verify that SelectedItem is selected on PressAndHold")]
		[UiTest(typeof(ViewCell))]
		[UiTest(typeof(ListView))]
		[UiTest(typeof(ListView), "TapAndHoldSelectedItem")]
		public void Issue3320TextTapAndHoldSelectsRow()
		{
			RunningApp.TouchAndHold(q => q.Marked("ListItem:5"));
			RunningApp.Screenshot("TouchAndHold-ItemIsSelected");
		}
#endif

	}
}
