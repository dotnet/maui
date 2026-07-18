using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class GeometryGroupMemoryLeakTests : BaseTestFixture
	{
		/// <summary>
		/// Verifies that assigning a long-lived / shared <see cref="GeometryCollection"/> to
		/// <see cref="GeometryGroup.Children"/> does not root the <see cref="GeometryGroup"/>.
		/// Reproduces issue #36365.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task GeometryGroupDoesNotLeakWhenChildrenSharedWithLongLivedCollection()
		{
			// A shared collection that outlives the group (simulates a cached/shared geometry).
			var sharedChildren = new GeometryCollection();

			WeakReference CreateGroupReference()
			{
				var group = new GeometryGroup
				{
					Children = sharedChildren
				};

				return new WeakReference(group);
			}

			var reference = CreateGroupReference();

			Assert.False(await reference.WaitForCollect(), "GeometryGroup should not be alive!");

			GC.KeepAlive(sharedChildren);
		}

		/// <summary>
		/// Verifies the group still responds to collection changes from its active source after GC pressure.
		/// </summary>
		[Fact]
		public void GeometryGroupStillInvalidatesAfterChildrenChange()
		{
			var children = new GeometryCollection();
			var group = new GeometryGroup { Children = children };

			int invalidateCount = 0;
			group.InvalidateGeometryRequested += (_, _) => invalidateCount++;

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			children.Add(new EllipseGeometry());

			Assert.Equal(1, invalidateCount);
			GC.KeepAlive(group);
		}
	}
}
