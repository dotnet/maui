using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TransformGroupMemoryTests : BaseTestFixture
	{
		/// <summary>
		/// Verifies that a shared/long-lived <see cref="TransformCollection"/> assigned to
		/// <see cref="TransformGroup.Children"/> does not keep the <see cref="TransformGroup"/>
		/// alive via the CollectionChanged / child PropertyChanged subscriptions. Reproduces issue #36367.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task TransformGroupDoesNotLeakWhenChildrenOutliveIt()
		{
			// A long-lived / shared collection (e.g. stored in a static or ViewModel) that outlives the group.
			var sharedChildren = new TransformCollection
			{
				new ScaleTransform()
			};

			WeakReference CreateTransformGroupReference()
			{
				var group = new TransformGroup
				{
					Children = sharedChildren
				};

				return new WeakReference(group);
			}

			var reference = CreateTransformGroupReference();

			Assert.False(await reference.WaitForCollect(), "TransformGroup should not be alive!");

			GC.KeepAlive(sharedChildren);
		}

		/// <summary>
		/// Verifies that the group still recomputes its matrix when its children change after a
		/// forced GC, ensuring the weak subscription survives while the group is alive.
		/// </summary>
		[Fact]
		public void TransformGroupStillUpdatesAfterGc()
		{
			var children = new TransformCollection();
			var group = new TransformGroup { Children = children };

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			children.Add(new ScaleTransform { ScaleX = 2, ScaleY = 3 });

			Assert.Equal(2, group.Value.M11);
			Assert.Equal(3, group.Value.M22);
		}
	}
}
