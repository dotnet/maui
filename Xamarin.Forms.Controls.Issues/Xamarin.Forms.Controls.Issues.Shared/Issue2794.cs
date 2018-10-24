using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2794, "TableView does not react on underlying collection change", PlatformAffected.Android)]
	public class Issue2794 : TestContentPage
	{
		TableSection _dataSection;

		protected override void Init()
		{
			var tableView = new TableView();
			_dataSection = new TableSection();
			var cell1 = new TextCell { Text = "Cell1" };
			cell1.ContextActions.Add (new MenuItem {
				Text = "Delete me after",
				IsDestructive = true,
				Command = new Command(Delete),
				CommandParameter = 0
			});

			var cell2 = new TextCell { Text = "Cell2" };
			cell2.ContextActions.Add(new MenuItem {
				Text = "Delete me first",
				IsDestructive = true,
				Command = new Command(Delete),
				CommandParameter = 1
			});

			_dataSection.Add(cell1);
			_dataSection.Add(cell2);
			tableView.Root.Add(_dataSection);
			var step1Label = new Label { Text = "• Tap and hold 'Cell2'" };
			var step2Label = new Label { Text = "• Tap 'Delete me first'" };
			var step3Label = new Label { Text = "• Tap and hold 'Cell1'" };
			var step4Label = new Label { Text = "• Tap 'Delete me after'" };
			var expectedLabel = new Label { Text = "Expected: 'Cell1' and 'Cell2' was deleted" };
			Content = new StackLayout
			{
				Padding = new Thickness(15, 15, 0, 0),
				Children =
				{
					step1Label,
					step2Label,
					step3Label,
					step4Label,
					expectedLabel,
					tableView
				}
			};
		}

		protected void Delete(object parameters)
		{
			var rowId = (int)parameters;
			_dataSection.RemoveAt(rowId);
		}

#if UITEST && __ANDROID__
		[Test]
		public void Issue2794Test()
		{
			RunningApp.TouchAndHold(x => x.Marked("Cell2"));
			RunningApp.Tap(x => x.Text("Delete me first"));
			RunningApp.WaitForNoElement(q => q.Marked("Cell2"));

			RunningApp.TouchAndHold(x => x.Marked("Cell1"));
			RunningApp.Tap(x => x.Text("Delete me after"));
			RunningApp.WaitForNoElement(q => q.Marked("Cell1"));
		}
#endif
	}
}
