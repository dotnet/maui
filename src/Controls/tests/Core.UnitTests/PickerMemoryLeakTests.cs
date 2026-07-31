using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PickerMemoryLeakTests : BaseTestFixture
	{
		/// <summary>
		/// Verifies that a Picker bound to a long-lived ObservableCollection via ItemsSource
		/// does not leak when the Picker is torn down without reassigning ItemsSource.
		/// Reproduces issue #36346: Picker subscribed to the collection's CollectionChanged
		/// with a strong delegate, so the shared collection rooted the Picker.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task PickerDoesNotLeakWhenItemsSourceIsLongLivedCollection()
		{
			// A shared/long-lived collection that outlives the Picker.
			var sharedRoot = new ObservableCollection<string> { "a", "b", "c" };

			WeakReference CreatePickerReference()
			{
				var picker = new Picker
				{
					ItemsSource = sharedRoot
				};

				return new WeakReference(picker);
			}

			var reference = CreatePickerReference();

			Assert.False(await reference.WaitForCollect(), "Picker should be collected, but it was retained by the shared ObservableCollection's CollectionChanged subscription.");

			GC.KeepAlive(sharedRoot);
		}
	}
}
