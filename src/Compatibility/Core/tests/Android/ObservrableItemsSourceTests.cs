using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Android.OS;
using Java.Lang;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.UnitTests
{
	[TestFixture]
	public class ObservrableItemsSourceTests 
	{
		Handler _handler = new Handler(Looper.MainLooper);

		[Test, Category("CollectionView")]
		[Description("Off-main-thread modifications to the source should be reflected in the count when the main thread has processed them.")]
		public async Task ObservableSourceItemsCountConsistent()
		{
			var source = new ObservableCollection<int>();

			source.Add(1);
			source.Add(2);

			var ois = ItemsSourceFactory.Create(source, new MockCollectionChangedNotifier());

			Assert.That(ois.Count, Is.EqualTo(2));

			source.Add(3);

			var count = await Device.InvokeOnMainThreadAsync(() => ois.Count);

			Assert.That(ois.Count, Is.EqualTo(3));
		}

		[Test, Category("CollectionView")]
		[Description("Off-main-thread Adds should be reflected once the main thread has processed them.")]
		public async Task AddItemCountConsistentOnUIThread()
		{
			var notifier = new MockCollectionChangedNotifier();
			var source = new ObservableCollection<int>();
			IItemsViewSource ois = ItemsSourceFactory.Create(source, notifier);

			int countBeforeNotify = -1;

			// Add an item from a threadpool thread
			await Task.Run(() => { 
				source.Add(1);

				// Post a check ahead of the queued update on the main thread
				_handler.PostAtFrontOfQueue(() => {
					countBeforeNotify = ois.Count;
				});
			});

			// Check the result on the main thread
			var onMainThreadCount = await Device.InvokeOnMainThreadAsync(() => ois.Count);

			Assert.That(countBeforeNotify, Is.EqualTo(0), "Count should still be reporting no items before the notify resolves");
			Assert.That(onMainThreadCount, Is.EqualTo(1));
			Assert.That(notifier.InsertCount, Is.EqualTo(1), "Should have recorded exactly one Add");
		}

		[Test, Category("CollectionView")]
		[Description("????")]
		public async Task RemoveitemCountConsistentOnUIThread()
		{
			var notifier = new MockCollectionChangedNotifier();
			var source = new ObservableCollection<int> { 1 };
			IItemsViewSource ois = ItemsSourceFactory.Create(source, notifier);

			int countBeforeNotify = -1;

			// Remove an item from a threadpool thread
			await Task.Run(() =>
			{
				source.Remove(1);
				
				// Post a check ahead of the queued update on the main thread
				_handler.PostAtFrontOfQueue(() => countBeforeNotify = ois.Count);
			});

			// Check the result on the main thread
			var onMainThreadCount = await Device.InvokeOnMainThreadAsync(() => ois.Count);

			Assert.That(countBeforeNotify, Is.EqualTo(1));
			Assert.That(onMainThreadCount, Is.EqualTo(0));
			Assert.That(notifier.RemoveCount, Is.EqualTo(1));
		}

		[Test, Category("CollectionView")]
		[Description("????")]
		public async Task GetItemConsistentOnUIThread()
		{
			var notifier = new MockCollectionChangedNotifier();
			var source = new ObservableCollection<string>
			{
				"zero",
				"one",
				"two"
			};

			IItemsViewSource ois = ItemsSourceFactory.Create(source, notifier);

			string itemAtPosition2BeforeNotify = string.Empty;

			// Add an item from a threadpool thread
			await Task.Run(() => {
				source.Insert(0, "foo");

				// Post a check ahead of the queued update on the main thread
				_handler.PostAtFrontOfQueue(() => itemAtPosition2BeforeNotify = (string)ois.GetItem(2));
			});

			// Check the result on the main thread
			var onMainThreadGetItem = await Device.InvokeOnMainThreadAsync(() => (string)ois.GetItem(2));

			Assert.That(itemAtPosition2BeforeNotify, Is.EqualTo("two"));
			Assert.That(onMainThreadGetItem, Is.EqualTo("one"));
			Assert.That(notifier.InsertCount, Is.EqualTo(1));
		}

		[Test, Category("CollectionView")]
		[Description("????")]
		public async Task GetPositionConsistentOnUIThread()
		{
			var notifier = new MockCollectionChangedNotifier();
			var source = new ObservableCollection<string>
			{
				"zero",
				"one",
				"two"
			};

			IItemsViewSource ois = ItemsSourceFactory.Create(source, notifier);

			int positionBeforeNotify = -1;

			// Add an item from a threadpool thread
			await Task.Run(() => { 
				source.Insert(0, "foo");

				// Post a check ahead of the queued update on the main thread
				_handler.PostAtFrontOfQueue(() => positionBeforeNotify = ois.GetPosition("zero"));
			});

			// Check the result on the main thread
			var onMainThreadGetItem = await Device.InvokeOnMainThreadAsync(() => ois.GetPosition("zero"));

			Assert.That(positionBeforeNotify, Is.EqualTo(0));
			Assert.That(onMainThreadGetItem, Is.EqualTo(1));
			Assert.That(notifier.InsertCount, Is.EqualTo(1));
		}

		class MockCollectionChangedNotifier : ICollectionChangedNotifier
		{
			public int InsertCount;
			public int RemoveCount;

			public void NotifyDataSetChanged()
			{
			}

			public void NotifyItemChanged(IItemsViewSource source, int startIndex)
			{
			}

			public void NotifyItemInserted(IItemsViewSource source, int startIndex)
			{
				InsertCount += 1;
			}

			public void NotifyItemMoved(IItemsViewSource source, int fromPosition, int toPosition)
			{
			}

			public void NotifyItemRangeChanged(IItemsViewSource source, int start, int end)
			{
			}

			public void NotifyItemRangeInserted(IItemsViewSource source, int startIndex, int count)
			{
			}

			public void NotifyItemRangeRemoved(IItemsViewSource source, int startIndex, int count)
			{
			}

			public void NotifyItemRemoved(IItemsViewSource source, int startIndex)
			{
				RemoveCount += 1;
			}
		}
	}
}