using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes
{
	public class GeometryGroupMemoryTests : BaseTestFixture
	{
		/// <summary>
		/// A shared/long-lived <see cref="GeometryCollection"/> must not root the
		/// <see cref="GeometryGroup"/> it is assigned to. Reproduces issue #36365 where the
		/// group's non-weak CollectionChanged subscription kept the group (and any Path using it)
		/// alive for the collection's lifetime.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task GeometryGroupDoesNotLeakWhenChildrenSharedWithLongLivedCollection()
		{
			var sharedChildren = new GeometryCollection();

			WeakReference weakGroup;
			{
				var group = new GeometryGroup();
				group.Children = sharedChildren;
				weakGroup = new WeakReference(group);
			}

			Assert.False(await weakGroup.WaitForCollect(), "GeometryGroup should not be alive!");
			GC.KeepAlive(sharedChildren);
		}
	}
}
