using Xamarin.Forms.CustomAttributes;
using System.Linq;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 59580, "Raising Command.CanExecutChanged causes crash on Android",
		PlatformAffected.Android)]
	public class Bugzilla59580 : TestContentPage
	{
		protected override void Init()
		{
			var tableView = new TableView();
			var tableSection = new TableSection();
			var switchCell = new TextCell
			{
				AutomationId = "Cell",
				Text = "Cell"
			};

			var menuItem = new MenuItem
			{
				AutomationId = "Fire CanExecuteChanged",
				Text = "Fire CanExecuteChanged",
				Command = new DelegateCommand(_ =>
					((DelegateCommand)switchCell.ContextActions.Single().Command).RaiseCanExecuteChanged()),
				IsDestructive = true
			};
			switchCell.ContextActions.Add(menuItem);
			tableSection.Add(switchCell);
			tableView.Root.Add(tableSection);
			Content = tableView;
		}

#if UITEST
		[Test]
		public void Issue59580Test()
		{
			RunningApp.WaitForElement(c => c.Marked("Cell"));

			RunningApp.ActivateContextMenu("Cell");

			RunningApp.WaitForElement(c => c.Marked("Fire CanExecuteChanged"));
			RunningApp.Tap(c => c.Marked("Fire CanExecuteChanged"));
			RunningApp.WaitForElement("Cell");
		}
#endif
	}
}
