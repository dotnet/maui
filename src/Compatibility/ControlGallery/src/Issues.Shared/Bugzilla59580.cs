using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TableView)]
#endif
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
		[Compatibility.UITests.FailsOnMauiIOS]
		public void RaisingCommandCanExecuteChangedCausesCrashOnAndroid()
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
