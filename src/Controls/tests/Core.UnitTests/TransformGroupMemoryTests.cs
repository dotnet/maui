using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TransformGroupMemoryTests : BaseTestFixture
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference GetChildProxyReference(TransformGroup group, int index)
		{
			return new WeakReference(GetChildProxies(group)[index]);
		}

		static IList GetChildProxies(TransformGroup group)
		{
			var flags = BindingFlags.NonPublic | BindingFlags.Instance;
			var subscriptionsField = typeof(TransformGroup).GetField("_childrenSubscriptions", flags);
			Assert.NotNull(subscriptionsField);

			var subscriptions = subscriptionsField.GetValue(group);
			Assert.NotNull(subscriptions);

			var childProxiesField = subscriptions.GetType().GetField("_childProxies", flags);
			Assert.NotNull(childProxiesField);

			return Assert.IsAssignableFrom<IList>(childProxiesField.GetValue(subscriptions));
		}

		[Fact, Category(TestCategory.Memory)]
		public async Task TransformGroupDoesNotLeakWhenSharingChildren()
		{
			// A long-lived/shared TransformCollection, exactly as the issue describes.
			var sharedChildren = new TransformCollection
			{
				new ScaleTransform(2, 2)
			};

			WeakReference weakGroup;
			{
				var group = new TransformGroup();
				group.Children = sharedChildren;
				weakGroup = new WeakReference(group);
			}

			Assert.False(await weakGroup.WaitForCollect(), "TransformGroup should not be alive!");
			GC.KeepAlive(sharedChildren);
		}

		[Fact, Category(TestCategory.Memory)]
		public async Task ChildTransformChangesStillInvalidateAfterGc()
		{
			var child = new ScaleTransform(1, 1);
			var group = new TransformGroup();
			group.Children.Add(child);

			await TestHelpers.Collect();

			var before = group.Value;
			child.ScaleX = 3;
			var after = group.Value;

			Assert.NotEqual(before, after);
			GC.KeepAlive(group);
		}

		[Fact, Category(TestCategory.Memory)]
		public async Task ExistingChildTransformChangesStillInvalidateAfterGc()
		{
			var child = new ScaleTransform(1, 1);
			var group = new TransformGroup
			{
				Children = new TransformCollection { child }
			};

			await TestHelpers.Collect();

			var before = group.Value;
			child.ScaleX = 3;
			var after = group.Value;

			Assert.NotEqual(before, after);
			GC.KeepAlive(group);
		}

		[Fact]
		public void ReassigningChildrenMovesChangeSubscriptions()
		{
			var oldChild = new ScaleTransform(2, 2);
			var oldChildren = new TransformCollection { oldChild };
			var replacementChild = new ScaleTransform(3, 3);
			var replacementChildren = new TransformCollection { replacementChild };
			var group = new TransformGroup { Children = oldChildren };

			group.Children = replacementChildren;
			var replacementValue = group.Value;

			int valueChangeCount = 0;
			group.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == Transform.ValueProperty.PropertyName)
					valueChangeCount++;
			};

			var sentinel = new Matrix(7, 0, 0, 11, 13, 17);
			group.Value = sentinel;
			valueChangeCount = 0;

			oldChild.ScaleX = 4;
			Assert.Equal(sentinel, group.Value);
			Assert.Equal(0, valueChangeCount);

			oldChildren.Add(new TranslateTransform(10, 20));
			Assert.Equal(sentinel, group.Value);
			Assert.Equal(0, valueChangeCount);

			group.Value = replacementValue;
			valueChangeCount = 0;

			replacementChild.ScaleX = 5;
			replacementChildren.Add(new TranslateTransform(30, 40));

			Assert.Equal(2, valueChangeCount);
		}

		[Fact, Category(TestCategory.Memory)]
		public async Task RemovingChildTransformReleasesSubscription()
		{
			var removed = new ScaleTransform(1, 1);
			var retained = new ScaleTransform(1, 1);
			var group = new TransformGroup
			{
				Children = new TransformCollection { removed, retained }
			};
			var removedProxy = GetChildProxyReference(group, 0);

			group.Children.Remove(removed);

			Assert.False(await removedProxy.WaitForCollect(), "Removed child proxy should not be alive!");

			var before = group.Value;
			retained.ScaleX = 3;
			var after = group.Value;

			Assert.NotEqual(before, after);
			GC.KeepAlive(removed);
			GC.KeepAlive(group);
		}

		[Fact, Category(TestCategory.Memory)]
		public async Task ReplacingChildTransformReleasesOldSubscription()
		{
			var replaced = new ScaleTransform(1, 1);
			var replacement = new ScaleTransform(1, 1);
			var group = new TransformGroup
			{
				Children = new TransformCollection { replaced }
			};
			var replacedProxy = GetChildProxyReference(group, 0);

			group.Children[0] = replacement;

			Assert.False(await replacedProxy.WaitForCollect(), "Replaced child proxy should not be alive!");

			var before = group.Value;
			replacement.ScaleX = 3;
			var after = group.Value;

			Assert.NotEqual(before, after);
			GC.KeepAlive(replaced);
			GC.KeepAlive(group);
		}

		[Fact, Category(TestCategory.Memory)]
		public async Task MovingChildTransformsReusesSubscriptions()
		{
			var scale = new ScaleTransform(2, 2);
			var translate = new TranslateTransform(10, 20);
			var group = new TransformGroup
			{
				Children = new TransformCollection { scale, translate }
			};
			var proxies = GetChildProxies(group);
			var scaleProxy = proxies[0];
			var translateProxy = proxies[1];
			var beforeMove = group.Value;

			group.Children.Move(0, 1);

			var afterMove = group.Value;
			var movedProxies = GetChildProxies(group);
			Assert.NotEqual(beforeMove, afterMove);
			Assert.True(ReferenceEquals(scaleProxy, movedProxies[0]) || ReferenceEquals(scaleProxy, movedProxies[1]));
			Assert.True(ReferenceEquals(translateProxy, movedProxies[0]) || ReferenceEquals(translateProxy, movedProxies[1]));

			await TestHelpers.Collect();

			var beforeChange = group.Value;
			scale.ScaleX = 3;
			var afterChange = group.Value;

			Assert.NotEqual(beforeChange, afterChange);
			GC.KeepAlive(group);
		}

		[Fact, Category(TestCategory.Memory)]
		public async Task ClearingChildrenReleasesSubscriptionsAndAllowsReuse()
		{
			var first = new ScaleTransform(1, 1);
			var second = new ScaleTransform(1, 1);
			var group = new TransformGroup
			{
				Children = new TransformCollection { first, second }
			};
			var firstProxy = GetChildProxyReference(group, 0);
			var secondProxy = GetChildProxyReference(group, 1);

			group.Children.Clear();

			Assert.False(await firstProxy.WaitForCollect(), "Cleared child proxy should not be alive!");
			Assert.False(await secondProxy.WaitForCollect(), "Cleared child proxy should not be alive!");

			var added = new ScaleTransform(1, 1);
			group.Children.Add(added);
			await TestHelpers.Collect();

			var before = group.Value;
			added.ScaleX = 3;
			var after = group.Value;

			Assert.NotEqual(before, after);
			GC.KeepAlive(first);
			GC.KeepAlive(second);
			GC.KeepAlive(group);
		}

		[Fact]
		public void NullChildrenUseIdentityMatrix()
		{
			var group = new TransformGroup
			{
				Children = null
			};

			Assert.Equal(new Matrix(), group.Value);
		}

		[Fact]
		public void NullChildIsIgnoredWhenUpdatingMatrix()
		{
			var transform = new TranslateTransform(10, 20);
			var group = new TransformGroup
			{
				Children = new TransformCollection { null, transform }
			};

			Assert.Equal(transform.Value, group.Value);
		}
	}
}
