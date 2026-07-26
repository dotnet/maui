using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes
{
	public class TransformGroupTests : BaseTestFixture
	{
		[Fact]
		public async Task ReplacingChildrenUnsubscribesFromOldChildPropertyChanged()
		{
			var sharedTransform = new ScaleTransform { ScaleX = 1.0, ScaleY = 1.0 };
			WeakReference weakGroup;

			{
				var group = new TransformGroup();
				group.Children.Add(sharedTransform);
				group.Children = new TransformCollection();
				weakGroup = new WeakReference(group);
			}

			Assert.False(await weakGroup.WaitForCollect(),
				"TransformGroup should be collected after Children replacement. " +
				"Shared child transform is keeping it alive via stale PropertyChanged subscription.");
		}

		[Fact]
		public void ReplacingChildrenSubscribesToNewChildPropertyChanged()
		{
			var group = new TransformGroup();
			var newCollection = new TransformCollection();
			var childTransform = new ScaleTransform { ScaleX = 1.0, ScaleY = 1.0 };
			newCollection.Add(childTransform);

			group.Children = newCollection;

			var matrixBefore = group.Value;
			childTransform.ScaleX = 2.0;
			var matrixAfter = group.Value;

			Assert.NotEqual(matrixBefore, matrixAfter);
		}

		[Fact]
		public async Task SharedTransformDoesNotRetainMultipleGroups()
		{
			var sharedTransform = new ScaleTransform { ScaleX = 1.0, ScaleY = 1.0 };
			var weakRefs = new WeakReference[10];

			{
				for (int i = 0; i < 10; i++)
				{
					var group = new TransformGroup();
					group.Children.Add(sharedTransform);
					group.Children = new TransformCollection();
					weakRefs[i] = new WeakReference(group);
				}
			}

			for (int i = 0; i < weakRefs.Length; i++)
			{
				Assert.False(await weakRefs[i].WaitForCollect(),
					$"TransformGroup #{i} should be collected. Shared transform is retaining it.");
			}
		}

		[Fact]
		public async Task ClearingChildrenUnsubscribesAllTransforms()
		{
			var sharedTransform = new ScaleTransform { ScaleX = 1.0, ScaleY = 1.0 };
			WeakReference weakGroup;

			{
				var group = new TransformGroup();
				group.Children.Add(sharedTransform);
				group.Children.Clear();
				weakGroup = new WeakReference(group);
			}

			Assert.False(await weakGroup.WaitForCollect(),
				"TransformGroup should be collected after Children.Clear(). " +
				"Shared child transform is keeping it alive via stale PropertyChanged subscription.");
		}
	}
}