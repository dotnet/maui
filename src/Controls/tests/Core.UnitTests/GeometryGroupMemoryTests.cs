using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class GeometryGroupMemoryTests : BaseTestFixture
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference AssignSharedChildrenAndDrop(GeometryCollection sharedChildren)
		{
			var group = new GeometryGroup { Children = sharedChildren };
			return new WeakReference(group);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference AssignSharedChildAndDrop(Geometry sharedChild)
		{
			var group = new GeometryGroup
			{
				Children = new GeometryCollection { sharedChild }
			};

			return new WeakReference(group);
		}

		[Fact]
		public async Task GeometryGroupDoesNotLeakWhenSharingChildren()
		{
			// A long-lived/shared GeometryCollection, exactly as the issue describes.
			var sharedChildren = new GeometryCollection
			{
				new RectangleGeometry()
			};
			var weakGroup = AssignSharedChildrenAndDrop(sharedChildren);

			Assert.False(await weakGroup.WaitForCollect(), "GeometryGroup should not be alive!");
			GC.KeepAlive(sharedChildren);
		}

		[Fact]
		public async Task GeometryGroupDoesNotLeakWhenSharingChild()
		{
			var sharedChild = new RectangleGeometry();
			var weakGroup = AssignSharedChildAndDrop(sharedChild);

			Assert.False(await weakGroup.WaitForCollect(), "GeometryGroup should not be alive!");
			GC.KeepAlive(sharedChild);
		}

		[Fact]
		public async Task ChildGeometryChangesStillInvalidateAfterGc()
		{
			var child = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var group = new GeometryGroup
			{
				Children = new GeometryCollection { child }
			};
			bool invalidated = false;
			group.InvalidateGeometryRequested += (_, __) => invalidated = true;

			await TestHelpers.Collect();

			child.Rect = new Rect(0, 0, 20, 20);

			Assert.True(invalidated);
			GC.KeepAlive(group);
		}

		[Fact]
		public void AssigningChildrenWithNullEntrySubscribesValidChildren()
		{
			var child = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var group = new GeometryGroup
			{
				Children = new GeometryCollection { null, child }
			};
			bool invalidated = false;
			group.InvalidateGeometryRequested += (_, __) => invalidated = true;

			child.Rect = new Rect(0, 0, 20, 20);

			Assert.True(invalidated);
		}

		[Fact]
		public void AppendPathSkipsNullChildren()
		{
			var group = new GeometryGroup
			{
				Children = new GeometryCollection
				{
					null,
					new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) }
				}
			};
			var path = new PathF();

			group.AppendPath(path);

			Assert.Equal(1, path.SubPathCount);
		}

		[Fact]
		public void AppendPathAllowsNullChildrenCollection()
		{
			var group = new GeometryGroup { Children = null };
			var path = new PathF();

			group.AppendPath(path);

			Assert.Equal(0, path.SubPathCount);
		}

		[Fact]
		public void FlattenGeometrySkipsNullChildren()
		{
			var group = new GeometryGroup
			{
				Children = new GeometryCollection
				{
					null,
					new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) }
				}
			};

			var flattened = GeometryHelper.FlattenGeometry(group, tolerance: 0.1);

			Assert.Single(flattened.Figures);
		}

		[Fact]
		public void FlattenGeometryAllowsNullChildrenCollection()
		{
			var group = new GeometryGroup { Children = null };

			var flattened = GeometryHelper.FlattenGeometry(group, tolerance: 0.1);

			Assert.Empty(flattened.Figures);
		}

		[Fact]
		public async Task SharedChildrenInvalidateEachLiveGroupAfterGc()
		{
			var child = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var sharedChildren = new GeometryCollection { child };
			var firstGroup = new GeometryGroup { Children = sharedChildren };
			var secondGroup = new GeometryGroup { Children = sharedChildren };
			int firstInvalidationCount = 0;
			int secondInvalidationCount = 0;
			firstGroup.InvalidateGeometryRequested += (_, __) => firstInvalidationCount++;
			secondGroup.InvalidateGeometryRequested += (_, __) => secondInvalidationCount++;

			await TestHelpers.Collect();

			child.Rect = new Rect(0, 0, 20, 20);

			Assert.Equal(1, firstInvalidationCount);
			Assert.Equal(1, secondInvalidationCount);
			GC.KeepAlive(firstGroup);
			GC.KeepAlive(secondGroup);
		}

		[Fact]
		public void RemovingAndReplacingChildrenMovesSubscriptions()
		{
			var removedChild = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var retainedChild = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var children = new GeometryCollection { removedChild, retainedChild };
			var group = new GeometryGroup { Children = children };
			int invalidationCount = 0;
			group.InvalidateGeometryRequested += (_, __) => invalidationCount++;

			children.Remove(removedChild);
			invalidationCount = 0;

			removedChild.Rect = new Rect(0, 0, 20, 20);
			Assert.Equal(0, invalidationCount);

			retainedChild.Rect = new Rect(0, 0, 20, 20);
			Assert.Equal(1, invalidationCount);

			var replacementChild = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			children[0] = replacementChild;
			invalidationCount = 0;

			retainedChild.Rect = new Rect(0, 0, 30, 30);
			Assert.Equal(0, invalidationCount);

			replacementChild.Rect = new Rect(0, 0, 20, 20);
			Assert.Equal(1, invalidationCount);
		}

		[Fact]
		public void ReplacingChildrenCollectionMovesSubscriptions()
		{
			var oldChild = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var oldChildren = new GeometryCollection { oldChild };
			var newChild = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var newChildren = new GeometryCollection { newChild };
			var group = new GeometryGroup { Children = oldChildren };
			int invalidationCount = 0;
			group.InvalidateGeometryRequested += (_, __) => invalidationCount++;

			group.Children = newChildren;

			oldChild.Rect = new Rect(0, 0, 20, 20);
			var addedOldChild = new RectangleGeometry();
			oldChildren.Add(addedOldChild);
			addedOldChild.Rect = new Rect(0, 0, 20, 20);

			Assert.Equal(0, invalidationCount);

			newChild.Rect = new Rect(0, 0, 20, 20);
			Assert.Equal(1, invalidationCount);

			var addedNewChild = new RectangleGeometry();
			newChildren.Add(addedNewChild);
			invalidationCount = 0;
			addedNewChild.Rect = new Rect(0, 0, 20, 20);

			Assert.Equal(1, invalidationCount);
		}

		[Fact]
		public void ReplacingChildrenThroughNullMovesSubscriptions()
		{
			var oldChild = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var oldChildren = new GeometryCollection { oldChild };
			var newChild = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var newChildren = new GeometryCollection { newChild };
			var group = new GeometryGroup { Children = oldChildren };
			int invalidationCount = 0;
			group.InvalidateGeometryRequested += (_, __) => invalidationCount++;

			group.Children = null;

			oldChild.Rect = new Rect(0, 0, 20, 20);
			var addedOldChild = new RectangleGeometry();
			oldChildren.Add(addedOldChild);
			addedOldChild.Rect = new Rect(0, 0, 20, 20);

			Assert.Equal(0, invalidationCount);

			group.Children = newChildren;
			newChild.Rect = new Rect(0, 0, 20, 20);

			Assert.Equal(1, invalidationCount);

			var addedNewChild = new RectangleGeometry();
			newChildren.Add(addedNewChild);
			invalidationCount = 0;
			addedNewChild.Rect = new Rect(0, 0, 20, 20);

			Assert.Equal(1, invalidationCount);
		}

		[Fact]
		public void MovingChildrenPreservesSubscriptions()
		{
			var firstChild = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var secondChild = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var children = new GeometryCollection { firstChild, secondChild };
			var group = new GeometryGroup { Children = children };
			int invalidationCount = 0;
			group.InvalidateGeometryRequested += (_, __) => invalidationCount++;

			children.Move(0, 1);
			invalidationCount = 0;

			firstChild.Rect = new Rect(0, 0, 20, 20);
			Assert.Equal(1, invalidationCount);

			secondChild.Rect = new Rect(0, 0, 20, 20);
			Assert.Equal(2, invalidationCount);
		}

		[Fact]
		public void DuplicateChildrenPreserveOccurrenceSubscriptions()
		{
			var child = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var children = new GeometryCollection { child, child };
			var group = new GeometryGroup { Children = children };
			int invalidationCount = 0;
			group.InvalidateGeometryRequested += (_, __) => invalidationCount++;

			child.Rect = new Rect(0, 0, 20, 20);

			Assert.Equal(2, invalidationCount);

			children.Remove(child);
			invalidationCount = 0;
			child.Rect = new Rect(0, 0, 30, 30);

			Assert.Equal(1, invalidationCount);

			children.Remove(child);
			invalidationCount = 0;
			child.Rect = new Rect(0, 0, 40, 40);

			Assert.Equal(0, invalidationCount);
		}

		[Fact]
		public void ResetChildrenResubscribesLaterAdditions()
		{
			var oldChild = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var newChild = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var group = new GeometryGroup
			{
				Children = new GeometryCollection { oldChild }
			};
			bool invalidated = false;
			group.InvalidateGeometryRequested += (_, __) => invalidated = true;

			group.Children.Clear();
			group.Children.Add(newChild);
			invalidated = false;

			oldChild.Rect = new Rect(0, 0, 20, 20);
			Assert.False(invalidated);

			newChild.Rect = new Rect(0, 0, 20, 20);
			Assert.True(invalidated);
		}
	}
}
