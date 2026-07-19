using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
#pragma warning disable CS0618 // Type or member is obsolete
	public class ListViewMemoryLeakTests : BaseTestFixture
	{
		// A minimal ICommand whose CanExecuteChanged is a plain CLR event (strong subscription),
		// as is common for custom/ViewModel commands. MAUI's own Command raises CanExecuteChanged
		// through a WeakEventManager, so it would not reproduce the leak.
		sealed class StrongEventCommand : ICommand
		{
			public event EventHandler CanExecuteChanged;
			public bool CanExecute(object parameter) => true;
			public void Execute(object parameter) { }
			public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Verifies that assigning a long-lived <see cref="ICommand"/> to
		/// <see cref="ListView.RefreshCommand"/> does not root the <see cref="ListView"/>.
		/// The ListView subscribes to <c>ICommand.CanExecuteChanged</c>; if that subscription
		/// is not weak, a shared/long-lived command keeps the ListView (and its page) alive.
		/// Reproduces issue #36539.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task ListViewDoesNotLeakWhenRefreshCommandIsLongLived()
		{
			// A shared/long-lived command (e.g. one exposed by a ViewModel) that outlives the ListView.
			var sharedCommand = new StrongEventCommand();

			WeakReference weakListView;
			{
				var listView = new ListView
				{
					RefreshCommand = sharedCommand
				};

				weakListView = new WeakReference(listView);
				// Drop the only strong reference to the ListView; sharedCommand stays alive.
			}

			Assert.False(
				await weakListView.WaitForCollect(),
				"ListView should not be alive! It is being rooted by the RefreshCommand's CanExecuteChanged subscription.");

			GC.KeepAlive(sharedCommand);
		}
	}
#pragma warning restore CS0618 // Type or member is obsolete
}
