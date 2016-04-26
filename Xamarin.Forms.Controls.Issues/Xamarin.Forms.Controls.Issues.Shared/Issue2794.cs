using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2794, "TableView does not react on underlying collection change", PlatformAffected.Android)]
	public class Issue2794 : ContentPage
	{
		TableSection _dataSection;

		public Issue2794 ()
		{

			var tableView = new TableView ();
			_dataSection = new TableSection ();
			var cell1 = new TextCell { Text = "Cell1" };
			cell1.ContextActions.Add (new MenuItem {
				Text = "Delete me after",
				IsDestructive = true,
				Command = new Command (Delete),
				CommandParameter = 0
			});

			var cell2 = new TextCell { Text = "Cell2" };
			cell2.ContextActions.Add (new MenuItem {
				Text = "Delete me first",
				IsDestructive = true,
				Command = new Command (Delete),
				CommandParameter = 1
			});

			_dataSection.Add (cell1);
			_dataSection.Add (cell2);
			tableView.Root.Add (_dataSection);

			Content = tableView;
		}

		protected void Delete(object parameters)
		{
			int rowId = (int)parameters;
			_dataSection.RemoveAt (rowId);
		}
	}
}


