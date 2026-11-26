using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using Microsoft.Maui.Dispatching;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class MarshalingObservableCollectionSynchronizationTests
	{
		[Fact]
		public void BackgroundBurstQueuesOneRefreshAndUsesSynchronizedSnapshot()
		{
			var context = new object();
			var source = new CheckingCollection(context) { 1, 2 };
			var queued = new Queue<Action>();
			var dispatcher = CreateDispatcher(queued);
			int reads = 0;
			BindingBase.EnableCollectionSynchronization(source, context, (collection, suppliedContext, access, writeAccess) =>
			{
				Assert.Same(source, collection);
				Assert.Same(context, suppliedContext);
				Assert.False(writeAccess);
				reads++;
				lock (context)
					access();
			});

			MarshalingObservableCollection snapshot = null;
			try
			{
				snapshot = new MarshalingObservableCollection(source, dispatcher);
				int resets = 0;
				snapshot.CollectionChanged += (_, args) =>
				{
					Assert.Equal(NotifyCollectionChangedAction.Reset, args.Action);
					resets++;
				};

				lock (context)
				{
					for (int index = 0; index < 10000; index++)
					{
						source.Add(index);
						source.RemoveAt(source.Count - 1);
					}
					source[0] = 3;
					source.Move(0, 1);
					source.Clear();
					source.Add(4);
				}

				Assert.Single(queued);
				Assert.Equal(new object[] { 1, 2 }, snapshot);
				Assert.Equal(1, reads);
				queued.Dequeue()();
				Assert.Empty(queued);
				Assert.Equal(new object[] { 4 }, snapshot);
				Assert.Equal(2, reads);
				Assert.Equal(1, resets);

				lock (context)
					source.Add(5);
				Assert.Single(queued);
				queued.Dequeue()();
				Assert.Equal(new object[] { 4, 5 }, snapshot);
			}
			finally
			{
				snapshot?.Dispose();
				BindingBase.DisableCollectionSynchronization(source);
			}
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ChangesDuringResetDeliveryScheduleAnotherRefresh(bool onUIThread)
		{
			var source = new ObservableCollection<int>();
			var context = new object();
			var queued = new Queue<Action>();
			BindingBase.EnableCollectionSynchronization(source, context, (_, suppliedContext, access, _) =>
			{
				lock (suppliedContext)
					access();
			});
			MarshalingObservableCollection snapshot = null;
			try
			{
				var dispatcher = CreateDispatcher(queued);
				snapshot = new MarshalingObservableCollection(source, dispatcher);
				bool changed = false;
				bool deliveringReset = false;
				int resets = 0;
				snapshot.CollectionChanged += (_, _) =>
				{
					Assert.False(deliveringReset, "A source mutation must not deliver nested collection notifications.");
					deliveringReset = true;
					try
					{
						resets++;
						if (!changed)
						{
							changed = true;
							lock (context)
								source.Add(2);
							Assert.Equal(new object[] { 1 }, snapshot);
						}
					}
					finally
					{
						deliveringReset = false;
					}
				};
				lock (context)
					source.Add(1);
				dispatcher.IsDispatchRequired.Returns(!onUIThread);
				queued.Dequeue()();
				Assert.Single(queued);
				queued.Dequeue()();
				Assert.Equal(new object[] { 1, 2 }, snapshot);
				Assert.Equal(2, resets);
				Assert.Empty(queued);
			}
			finally
			{
				snapshot?.Dispose();
				BindingBase.DisableCollectionSynchronization(source);
			}
		}

		[Fact]
		public void DisposalStopsPendingAndFutureRefreshes()
		{
			var source = new ObservableCollection<int>();
			var queued = new Queue<Action>();
			var snapshot = new MarshalingObservableCollection(source, CreateDispatcher(queued));
			int changes = 0;
			snapshot.CollectionChanged += (_, _) => changes++;
			source.Add(1);
			snapshot.Dispose();
			queued.Dequeue()();
			source.Add(2);
			Assert.Empty(snapshot);
			Assert.Empty(queued);
			Assert.Equal(0, changes);
		}

		[Fact]
		public void UIThreadChangesPreserveIncrementalNotifications()
		{
			var source = new ObservableCollection<int> { 1, 2 };
			var queued = new Queue<Action>();
			var dispatcher = CreateDispatcher(queued);
			dispatcher.IsDispatchRequired.Returns(false);
			var snapshot = new MarshalingObservableCollection(source, dispatcher);
			try
			{
				var actions = new List<NotifyCollectionChangedAction>();
				snapshot.CollectionChanged += (_, args) => actions.Add(args.Action);
				source.Add(3);
				source[0] = 4;
				source.Move(0, 2);
				source.RemoveAt(0);
				source.Clear();
				Assert.Single(queued);
				queued.Dequeue()();
				Assert.Equal(new[]
				{
					NotifyCollectionChangedAction.Add,
					NotifyCollectionChangedAction.Replace,
					NotifyCollectionChangedAction.Move,
					NotifyCollectionChangedAction.Remove,
					NotifyCollectionChangedAction.Reset
				}, actions);
				Assert.Empty(snapshot);
				Assert.Empty(queued);
			}
			finally
			{
				snapshot.Dispose();
			}
		}

		[Theory]
		[InlineData("Reset")]
		[InlineData("RangeMove")]
		[InlineData("AddWithoutIndex")]
		[InlineData("RemoveWithoutIndex")]
		[InlineData("UnequalReplace")]
		public void UIThreadSourceReadDefersUntilNonRecursiveWriteLockIsReleased(string action)
		{
			using var gate = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			var source = new RangeMoveCollection { 1, 2, 3, 4 };
			var queued = new Queue<Action>();
			var dispatcher = CreateDispatcher(queued);
			dispatcher.IsDispatchRequired.Returns(false);
			BindingBase.EnableCollectionSynchronization(source, gate, (_, _, access, writeAccess) =>
			{
				Assert.False(writeAccess);
				gate.EnterReadLock();
				try
				{
					access();
				}
				finally
				{
					gate.ExitReadLock();
				}
			});
			MarshalingObservableCollection snapshot = null;
			try
			{
				snapshot = new MarshalingObservableCollection(source, dispatcher);
				gate.EnterWriteLock();
				try
				{
					switch (action)
					{
						case "Reset":
							source.Clear();
							source.Add(3);
							break;
						case "RangeMove":
							source.MoveRange(2, 0, 2);
							break;
						case "AddWithoutIndex":
							source.AddWithoutIndex(5);
							break;
						case "RemoveWithoutIndex":
							source.RemoveWithoutIndex();
							break;
						case "UnequalReplace":
							source.ReplaceWithRange();
							break;
					}
					Assert.Equal(new object[] { 1, 2, 3, 4 }, snapshot);
					Assert.Single(queued);
				}
				finally
				{
					gate.ExitWriteLock();
				}
				queued.Dequeue()();
				Assert.Equal(source.Cast<object>(), snapshot);
			}
			finally
			{
				snapshot?.Dispose();
				BindingBase.DisableCollectionSynchronization(source);
			}
		}

		[Theory]
		[InlineData(2, 0)]
		[InlineData(0, 3)]
		public void UIThreadRangeMoveRefreshesInSourceOrder(int oldIndex, int newIndex)
		{
			var source = new RangeMoveCollection { 0, 1, 2, 3, 4 };
			var queued = new Queue<Action>();
			var dispatcher = CreateDispatcher(queued);
			dispatcher.IsDispatchRequired.Returns(false);
			var snapshot = new MarshalingObservableCollection(source, dispatcher);
			try
			{
				source.MoveRange(oldIndex, newIndex, 2);
				Assert.Equal(new object[] { 0, 1, 2, 3, 4 }, snapshot);
				Assert.Single(queued);
				queued.Dequeue()();
				Assert.Equal(source.Cast<object>(), snapshot);
			}
			finally
			{
				snapshot.Dispose();
			}
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void StaleReorderIsRejectedUnderWriteLockAndRefreshesDisplay(bool useRemoveAndInsert)
		{
			var source = new ObservableCollection<string> { "A", "B", "C" };
			var context = new object();
			var queued = new Queue<Action>();
			var dispatcher = CreateDispatcher(queued);
			BindingBase.EnableCollectionSynchronization(source, context, (_, _, access, _) =>
			{
				lock (context)
					access();
			});
			MarshalingObservableCollection snapshot = null;
			try
			{
				snapshot = new MarshalingObservableCollection(source, dispatcher);
				lock (context)
					source.Insert(0, "X");
				dispatcher.IsDispatchRequired.Returns(false);
				bool applied = false;
				snapshot.UpdateSource(() =>
				{
					applied = true;
					if (useRemoveAndInsert)
						source.RemoveAt(1);
					else
						source.Move(1, 2);
				});
				if (useRemoveAndInsert)
					snapshot.UpdateSource(() => source.Insert(2, "B"));
				Assert.False(applied);
				Assert.Equal(new[] { "X", "A", "B", "C" }, source);
				Assert.Single(queued);
				queued.Dequeue()();
				Assert.Equal(source.Cast<object>(), snapshot);

				snapshot.UpdateSource(() =>
				{
					Assert.True(Monitor.IsEntered(context));
					source.Move(2, 3);
					applied = true;
				});
				Assert.True(applied);
				Assert.Equal(new[] { "X", "A", "C", "B" }, source);
				Assert.Equal(source.Cast<object>(), snapshot);
			}
			finally
			{
				snapshot?.Dispose();
				BindingBase.DisableCollectionSynchronization(source);
			}
		}

		[Fact]
		public void WriteBackChecksLiveIdentityEvenWithoutPendingNotification()
		{
			var first = new object();
			var second = new object();
			var source = new ObservableCollection<object> { first, second };
			var queued = new Queue<Action>();
			var snapshot = new MarshalingObservableCollection(source, CreateDispatcher(queued));
			try
			{
				// Simulate a displayed snapshot differing from the source without a pending refresh.
				snapshot.Reverse();
				bool applied = false;
				snapshot.UpdateSource(() => applied = true);
				Assert.False(applied);
				Assert.Equal(new[] { first, second }, source);
				Assert.Single(queued);
				queued.Dequeue()();
				Assert.Equal(source, snapshot);
			}
			finally
			{
				snapshot.Dispose();
			}
		}

		[Fact]
		public void FailedDispatchIsReportedAndCanBeRetried()
		{
			var source = new ObservableCollection<int>();
			var queued = new Queue<Action>();
			var dispatcher = CreateDispatcher(queued);
			dispatcher.DispatchDelayed(Arg.Any<TimeSpan>(), Arg.Any<Action>()).Returns(false);
			var snapshot = new MarshalingObservableCollection(source, dispatcher);
			try
			{
				Assert.Throws<InvalidOperationException>(() => source.Add(1));
				Assert.Empty(queued);
				dispatcher.DispatchDelayed(Arg.Any<TimeSpan>(), Arg.Any<Action>()).Returns(call =>
				{
					queued.Enqueue(call.Arg<Action>());
					return true;
				});
				source.Add(2);
				Assert.Single(queued);
				queued.Dequeue()();
				Assert.Equal(new object[] { 1, 2 }, snapshot);
			}
			finally
			{
				snapshot.Dispose();
			}
		}

		[Fact]
		public void SourceWritesUseWriteAccessCallback()
		{
			var source = new ObservableCollection<int>();
			var context = new object();
			bool wrote = false;
			BindingBase.EnableCollectionSynchronization(source, context, (_, suppliedContext, access, writeAccess) =>
			{
				wrote |= writeAccess;
				lock (suppliedContext)
					access();
			});
			MarshalingObservableCollection snapshot = null;
			try
			{
				snapshot = new MarshalingObservableCollection(source, CreateDispatcher(new Queue<Action>()));
				Assert.False(wrote);
				snapshot.UpdateSource(() =>
				{
					Assert.True(Monitor.IsEntered(context));
					source.Add(1);
				});
				Assert.True(wrote);
			}
			finally
			{
				snapshot?.Dispose();
				BindingBase.DisableCollectionSynchronization(source);
			}
		}

		static IDispatcher CreateDispatcher(Queue<Action> queued)
		{
			var dispatcher = Substitute.For<IDispatcher>();
			dispatcher.IsDispatchRequired.Returns(true);
			dispatcher.DispatchDelayed(Arg.Any<TimeSpan>(), Arg.Any<Action>()).Returns(call =>
			{
				Assert.Equal(TimeSpan.FromMilliseconds(16), call.Arg<TimeSpan>());
				queued.Enqueue(call.Arg<Action>());
				return true;
			});
			return dispatcher;
		}

		class RangeMoveCollection : ObservableCollection<int>
		{
			public void AddWithoutIndex(int item)
			{
				Items.Add(item);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
			}

			public void RemoveWithoutIndex()
			{
				int item = Items[0];
				Items.RemoveAt(0);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
			}

			public void ReplaceWithRange()
			{
				int oldItem = Items[0];
				Items[0] = 5;
				Items.Insert(1, 6);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(
					NotifyCollectionChangedAction.Replace, new[] { 5, 6 }, new[] { oldItem }, 0));
			}

			public void MoveRange(int oldIndex, int newIndex, int count)
			{
				var moved = new List<int>();
				for (int index = 0; index < count; index++)
				{
					moved.Add(Items[oldIndex]);
					Items.RemoveAt(oldIndex);
				}
				for (int index = 0; index < count; index++)
					Items.Insert(newIndex + index, moved[index]);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(
					NotifyCollectionChangedAction.Move, moved, newIndex, oldIndex));
			}
		}

		class CheckingCollection : ObservableCollection<int>, IEnumerable, INotifyCollectionChanged
		{
			readonly object _context;
			public CheckingCollection(object context) => _context = context;

			IEnumerator IEnumerable.GetEnumerator()
			{
				Assert.True(Monitor.IsEntered(_context));
				return GetEnumerator();
			}

			event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
			{
				add
				{
					Assert.True(Monitor.IsEntered(_context));
					CollectionChanged += value;
				}
				remove => CollectionChanged -= value;
			}
		}
	}
}
