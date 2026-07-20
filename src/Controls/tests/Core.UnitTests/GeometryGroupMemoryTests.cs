using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class GeometryGroupMemoryTests : BaseTestFixture
	{
		/// <summary>
		/// Verifies that a shared/long-lived <see cref="GeometryCollection"/> assigned to
		/// <see cref="GeometryGroup.Children"/> does not keep the <see cref="GeometryGroup"/>
		/// alive via the CollectionChanged / child PropertyChanged subscriptions. Reproduces issue #36365.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task GeometryGroupDoesNotLeakWhenChildrenOutliveIt()
		{
			// A long-lived / shared collection (e.g. stored in a static or ViewModel) that outlives the group.
			var sharedChildren = new GeometryCollection
			{
				new EllipseGeometry()
			};

			WeakReference CreateGeometryGroupReference()
			{
				var group = new GeometryGroup
				{
					Children = sharedChildren
				};

				return new WeakReference(group);
			}

			var reference = CreateGeometryGroupReference();

			Assert.False(await reference.WaitForCollect(), "GeometryGroup should not be alive!");

			GC.KeepAlive(sharedChildren);
		}

		/// <summary>
		/// Verifies that the group still responds to child collection changes (invalidation) after
		/// forced GC, ensuring the weak subscription survives while the group is alive.
		/// </summary>
		[Fact]
		public void GeometryGroupStillInvalidatesAfterGc()
		{
			var children = new GeometryCollection();
			var group = new GeometryGroup { Children = children };

			int invalidateCount = 0;
			group.InvalidateGeometryRequested += (_, _) => invalidateCount++;

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			children.Add(new EllipseGeometry());

			Assert.True(invalidateCount > 0, "GeometryGroup should still invalidate when its children change.");
		}
	}
}
