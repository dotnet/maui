using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TableViewMemoryLeakTests : BaseTestFixture
	{
		/// <summary>
		/// Verifies that assigning a long-lived (shared / reused) <see cref="TableRoot"/> to a
		/// <see cref="TableView"/> does not keep the control alive. Reproduces issue #36355:
		/// the TableView subscribed to the root's SectionCollectionChanged / CollectionChanged /
		/// PropertyChanged events with plain (strong) delegates and only detached on Root
		/// reassignment, so a shared root rooted the whole control.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task TableViewDoesNotLeakWhenRootIsShared()
		{
			// A TableRoot that outlives the TableView (simulates a shared/reused root).
			var sharedRoot = new TableRoot("Root")
			{
				new TableSection("Section")
				{
					new TextCell { Text = "Cell" }
				}
			};

			WeakReference CreateTableViewReference()
			{
				var tableView = new TableView { Root = sharedRoot };
				return new WeakReference(tableView);
			}

			var reference = CreateTableViewReference();

			Assert.False(await reference.WaitForCollect(), "TableView should not be alive!");

			// Keep the shared root alive for the duration of the test.
			GC.KeepAlive(sharedRoot);
		}
	}
}
