using System;
using System.Maui.CustomAttributes;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Maui.Internals;

#if UITEST
using Xamarin.UITest.iOS;
using Xamarin.UITest;
using NUnit.Framework;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 57317, "Modifying Cell.ContextActions can crash on Android", PlatformAffected.Android)]
	public class Bugzilla57317 : TestContentPage
	{
		protected override void Init ()
		{
			var tableView = new TableView();
			var tableSection = new TableSection();
			var switchCell = new TextCell
			{
				Text = "Cell"
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
		public void Bugzilla57317Test ()
		{
			RunningApp.WaitForElement (c => c.Marked ("Cell"));

			RunningApp.ActivateContextMenu("Cell");

			RunningApp.WaitForElement (c => c.Marked ("Self-Deleting item"));
			RunningApp.Tap (c => c.Marked ("Self-Deleting item"));
		}
#endif
	}
}
