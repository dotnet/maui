using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

// Apply the default category of "Issues" to all of the tests in this assembly
// We use this as a catch-all for tests which haven't been individually categorized
#if UITEST
[assembly: NUnit.Framework.Category("Issues")]
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "ListView crashes when disposed on ItemSelected", PlatformAffected.iOS)]
	public class ListViewNRE : TestContentPage
	{
		const string Success = "Success";

		protected override void Init()
		{
			var listView = new ListView
			{
				ItemsSource = Enumerable.Range(0, 10)
			};

			listView.ItemSelected += ListView_ItemSelected;

			Content = listView;
		}

		void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			Content = new Label { Text = Success };
		}

#if UITEST
		[Test]
		public void ListViewNRETest()
		{
			RunningApp.WaitForElement(q => q.Marked("1"));
			RunningApp.Tap(q => q.Marked("1"));
			RunningApp.WaitForElement(q => q.Marked(Success));
		}
#endif
	}
}