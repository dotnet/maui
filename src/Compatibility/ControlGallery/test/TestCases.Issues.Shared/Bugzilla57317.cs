using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest.iOS;
using Xamarin.UITest;
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
	[Issue(IssueTracker.Bugzilla, 57317, "Modifying Cell.ContextActions can crash on Android", PlatformAffected.Android)]
	public class Bugzilla57317 : TestContentPage
	{
		protected override void Init()
		{
			var tableView = new TableView();
			var tableSection = new TableSection();
			var switchCell = new TextCell
			{
				Text = "Cell",
				AutomationId = "Cell"
			};

			var menuItem = new MenuItem
			{
				Text = "Self-Deleting item",
				Command = new Command(() => switchCell.ContextActions.RemoveAt(0)),
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
		public void Bugzilla57317Test()
		{
			RunningApp.WaitForFirstElement("Cell");

			RunningApp.ActivateContextMenu("Cell");

			RunningApp.WaitForFirstElement("Self-Deleting item");
			RunningApp.Tap(c => c.Marked("Self-Deleting item"));
		}
#endif
	}
}
