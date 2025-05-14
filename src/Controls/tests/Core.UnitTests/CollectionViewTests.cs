using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class CollectionViewTests : BaseTestFixture
	{
		[Fact, Category(TestCategory.Memory)]
		public async Task CanCollectEmptyView()
		{
			var collectionView = new CollectionView { IsPlatformEnabled = true };
			collectionView.EmptyView = new View();

			var weakReference = new WeakReference(collectionView.EmptyView);

			collectionView.EmptyView = null;

			await Task.Yield();
			GC.Collect(2);
			GC.WaitForPendingFinalizers();

			Assert.False(weakReference.IsAlive);
		}
	}
}