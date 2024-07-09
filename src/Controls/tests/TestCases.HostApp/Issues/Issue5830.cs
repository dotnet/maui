using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5830, "[Enhancement] EntryCellTableViewCell should be public", PlatformAffected.iOS)]
	public class Issue5830 : TestContentPage
	{
		const string Instructions = "On iOS, if the below Entry Cell displays 'Text from Custom Entry Cell' in 'Red' color, this test has passed.";
		ListView lstView;

		public class ExtendedEntryCell : EntryCell
		{
			public ExtendedEntryCell()
			{
				base.Text = "Text from Custom Entry Cell";
			}
		}

		protected override void Init()
		{
			var label = new Label { Text = Instructions, AutomationId = "TestReady" };
			lstView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				ItemTemplate = new DataTemplate(typeof(ExtendedEntryCell)),
				ItemsSource = new[] { "item1" }
			};

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
		[Compatibility.UITests.FailsOnMauiIOS]
        public void Issue5830Test()
        {
			RunningApp.WaitForElement("TestReady");
            RunningApp.Screenshot("EntryTableViewCell Test with custom Text and TextColor");
        }
#endif
	}
}