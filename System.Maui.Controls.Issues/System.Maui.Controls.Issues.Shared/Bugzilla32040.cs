using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Cells)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 32040, "EntryCell.Tapped or SwitchCell.Tapped does not fire when within a TableView ")]
	public class Bugzilla32040 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init ()
		{
			var switchCell = new SwitchCell { Text = "blahblah" };
			switchCell.Tapped += (s, e) =>
			{
				switchCell.Text = "Tapped";
			};
			switchCell.OnChanged += (sender, e) => {
				switchCell.Text = "Switched";
			};

			var entryCell = new EntryCell { Text = "yaddayadda" };
#pragma warning disable 618
			entryCell.XAlign = TextAlignment.End;
#pragma warning restore 618
			entryCell.Label = "Click Here";
			entryCell.Tapped += (s, e) =>
			{
				entryCell.Text = "Tapped";
			};
			entryCell.Completed += (sender, e) => {
				entryCell.Text = "Completed";
			};

			// The root page of your application
			Content = new TableView {
				Intent = TableIntent.Form,
				Root = new TableRoot ("Table Title") {
					new TableSection ("Section 1 Title") {
						switchCell,
						entryCell
					}
				}
			};
		}
#if UITEST
		[Test]
		public void TappedWorksForEntryAndSwithCellTest ()
		{
			RunningApp.Tap (q => q.Marked ("blahblah"));
			RunningApp.Tap (q => q.Marked ("Click Here"));
			Assert.GreaterOrEqual (RunningApp.Query (q => q.Marked ("Tapped")).Length, 2);
		}
#endif
	}
}
