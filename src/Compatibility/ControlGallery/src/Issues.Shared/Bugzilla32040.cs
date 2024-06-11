using System;
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
	[Category(UITestCategories.Bugzilla)]
	[Category(UITestCategories.Cells)]
	[Category(UITestCategories.UwpIgnore)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TableView)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 32040, "EntryCell.Tapped or SwitchCell.Tapped does not fire when within a TableView ")]
	public class Bugzilla32040 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			var switchCell = new SwitchCell { Text = "blahblah" };
			switchCell.Tapped += (s, e) =>
			{
				switchCell.Text = "Tapped";
			};
			switchCell.OnChanged += (sender, e) =>
			{
				switchCell.Text = "Switched";
			};

			var entryCell = new EntryCell { Text = "yaddayadda" };
			entryCell.HorizontalTextAlignment = TextAlignment.End;
			entryCell.Label = "Click Here";
			entryCell.Tapped += (s, e) =>
			{
				entryCell.Text = "Tapped";
			};
			entryCell.Completed += (sender, e) =>
			{
				entryCell.Text = "Completed";
			};

			// The root page of your application
			Content = new TableView
			{
				Intent = TableIntent.Form,
				Root = new TableRoot("Table Title") {
					new TableSection ("Section 1 Title") {
						switchCell,
						entryCell
					}
				}
			};
		}
#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void TappedWorksForEntryAndSwithCellTest()
		{
			RunningApp.Tap(q => q.Marked("blahblah"));
			RunningApp.Tap(q => q.Marked("Click Here"));
			Assert.GreaterOrEqual(RunningApp.Query(q => q.Marked("Tapped")).Length, 2);
		}
#endif
	}
}
