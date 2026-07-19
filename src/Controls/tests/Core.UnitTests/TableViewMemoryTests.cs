using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TableViewMemoryTests : BaseTestFixture
	{
		[Fact]
		public async Task TableViewRootDoesNotLeak()
		{
			// A shared / long-lived TableRoot that outlives the TableView, exactly as the
			// issue describes (e.g. a TableRoot reused across pages).
			var sharedRoot = new TableRoot();

			WeakReference weakTableView;
			{
				var tableView = new TableView();
				tableView.Root = sharedRoot;   // installs the CollectionChanged/SectionCollectionChanged/PropertyChanged subscriptions
				weakTableView = new WeakReference(tableView);
				// drop the only strong ref to `tableView`; `sharedRoot` stays alive.
			}

			Assert.False(await weakTableView.WaitForCollect(), "TableView should not be alive!");
			GC.KeepAlive(sharedRoot);
		}
	}
}
