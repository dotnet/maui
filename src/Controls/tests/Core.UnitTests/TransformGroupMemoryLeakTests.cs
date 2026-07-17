using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TransformGroupMemoryLeakTests : BaseTestFixture
	{
		/// <summary>
		/// Verifies that assigning a long-lived (shared) <see cref="TransformCollection"/> to
		/// <see cref="TransformGroup.Children"/> does not keep the <see cref="TransformGroup"/>
		/// alive via the collection's CollectionChanged subscription. Reproduces issue #36367.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task TransformGroupDoesNotLeakWhenChildrenBoundToSharedCollection()
		{
			// A long-lived collection that outlives the TransformGroup (simulates shared/reused state).
			var sharedChildren = new TransformCollection();

			WeakReference CreateTransformGroupReference()
			{
				var group = new TransformGroup();

				// Assigning the shared collection installs the CollectionChanged subscription;
				// with a strong subscription this roots the TransformGroup for the collection's lifetime.
				group.Children = sharedChildren;

				return new WeakReference(group);
			}

			var reference = CreateTransformGroupReference();

			Assert.False(await reference.WaitForCollect(), "TransformGroup should not be alive!");

			// Keep the shared collection alive for the duration of the test.
			GC.KeepAlive(sharedChildren);
		}
	}
}
