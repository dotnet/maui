using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class SelectionListMemoryLeakTests : BaseTestFixture
	{
		/// <summary>
		/// Verifies that a CollectionView bound to a retained ObservableCollection via SelectedItems
		/// does not leak after handler disconnect. Reproduces issue #35497.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task CollectionViewDoesNotLeakWhenSelectedItemsBoundToRetainedObservableCollection()
		{
			// The retained ObservableCollection that outlives the CollectionView (simulates long-lived state)
			var retainedCollection = new ObservableCollection<object>();

			WeakReference CreateCollectionViewReference()
			{
				var cv = new CollectionView
				{
					SelectionMode = SelectionMode.Multiple,
					ItemsSource = new List<string> { "Item1", "Item2", "Item3" }
				};

				// Bind SelectedItems to the retained collection — this is the leak trigger
				cv.SetBinding(SelectableItemsView.SelectedItemsProperty, new Binding(".")
				{
					Source = retainedCollection
				});

				// Simulate handler connect (page appearing)
				cv.Handler = new CollectionViewHandlerStub();

				// Simulate handler disconnect (page popped)
				cv.Handler = null;

				return new WeakReference(cv);
			}

			var reference = CreateCollectionViewReference();

			Assert.False(await reference.WaitForCollect(), "CollectionView should be collected after handler disconnect, but it was retained by SelectionList's CollectionChanged subscription on the ObservableCollection.");

			// Keep the retained collection alive for the duration of the test
			GC.KeepAlive(retainedCollection);
		}

		/// <summary>
		/// Verifies that reassigning SelectedItems detaches the old SelectionList
		/// so it doesn't accumulate leaked subscriptions.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task CollectionViewDoesNotLeakWhenSelectedItemsReassigned()
		{
			var retainedCollection1 = new ObservableCollection<object>();
			var retainedCollection2 = new ObservableCollection<object>();

			WeakReference CreateCollectionViewReference()
			{
				var cv = new CollectionView
				{
					SelectionMode = SelectionMode.Multiple,
					ItemsSource = new List<string> { "A", "B", "C" }
				};

				// Bind to first collection
				cv.SetBinding(SelectableItemsView.SelectedItemsProperty, new Binding(".")
				{
					Source = retainedCollection1
				});

				// Reassign to second collection — old SelectionList should detach
				cv.SetBinding(SelectableItemsView.SelectedItemsProperty, new Binding(".")
				{
					Source = retainedCollection2
				});

				// Simulate handler lifecycle
				cv.Handler = new CollectionViewHandlerStub();
				cv.Handler = null;

				return new WeakReference(cv);
			}

			var reference = CreateCollectionViewReference();

			Assert.False(await reference.WaitForCollect(), "CollectionView should be collected after SelectedItems reassignment and handler disconnect.");

			GC.KeepAlive(retainedCollection1);
			GC.KeepAlive(retainedCollection2);
		}

		/// <summary>
		/// Verifies that SelectionChanged still fires after handler disconnect and reconnect.
		/// Ensures the weak proxy subscription survives handler lifecycle changes.
		/// </summary>
		[Fact]
		public void SelectionChangedFiresAfterHandlerReconnect()
		{
			var selectedItems = new ObservableCollection<object>();
			var cv = new CollectionView
			{
				SelectionMode = SelectionMode.Multiple,
				ItemsSource = new List<string> { "Item1", "Item2", "Item3" },
				SelectedItems = selectedItems
			};

			// Connect handler
			cv.Handler = new CollectionViewHandlerStub();

			// Disconnect handler (simulates page being removed from visual tree)
			cv.Handler = null;

			// Reconnect handler (simulates page being re-added)
			cv.Handler = new CollectionViewHandlerStub();

			// Mutate the source collection — SelectionChanged should still fire
			int selectionChangedCount = 0;
			cv.SelectionChanged += (_, _) => selectionChangedCount++;

			selectedItems.Add("Item1");

			Assert.Equal(1, selectionChangedCount);
		}

		/// <summary>
		/// Minimal handler stub for CollectionView to enable handler lifecycle in unit tests.
		/// </summary>
		class CollectionViewHandlerStub : ViewHandler<CollectionView, object>
		{
			public CollectionViewHandlerStub() : base(new PropertyMapper<IView>())
			{
			}

			protected override object CreatePlatformView() => new object();
		}
	}
}
