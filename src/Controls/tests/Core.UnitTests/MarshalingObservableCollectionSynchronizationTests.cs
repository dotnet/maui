using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

		[Fact]
		public void ChangesDuringResetDeliveryScheduleAnotherRefresh()
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
				snapshot = new MarshalingObservableCollection(source, CreateDispatcher(queued));
				bool changed = false;
				snapshot.CollectionChanged += (_, _) =>
				{
					if (!changed)
					{
						changed = true;
						lock (context)
							source.Add(2);
					}
				};
				lock (context)
					source.Add(1);
				queued.Dequeue()();
				Assert.Single(queued);
				queued.Dequeue()();
				Assert.Equal(new object[] { 1, 2 }, snapshot);
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
		public void UIThreadChangesQueueOneRefreshAfterNonRecursiveWriteLockIsReleased()
		{
			var source = new ObservableCollection<int> { 1, 2, 3, 4 };
			var gate = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			BindingBase.EnableCollectionSynchronization(source, gate, (_, _, access, writeAccess) =>
			{
				if (writeAccess)
					gate.EnterWriteLock();
				else
					gate.EnterReadLock();
				try
				{
					access();
				}
				finally
				{
					if (writeAccess)
						gate.ExitWriteLock();
					else
						gate.ExitReadLock();
				}
			});
			var queued = new Queue<Action>();
			var dispatcher = CreateDispatcher(queued);
			dispatcher.IsDispatchRequired.Returns(false);
			MarshalingObservableCollection snapshot = null;
			try
			{
				snapshot = new MarshalingObservableCollection(source, dispatcher);
				int changes = 0;
				snapshot.CollectionChanged += (_, _) => changes++;
				gate.EnterWriteLock();
				try
				{
					source.Move(2, 0);
					source.Add(5);
					source.Clear();
					source.Add(6);
					Assert.Equal(new object[] { 1, 2, 3, 4 }, snapshot);
				}
				finally
				{
					gate.ExitWriteLock();
				}

				Assert.Equal(0, changes);
				Assert.Single(queued);
				queued.Dequeue()();
				Assert.Equal(new object[] { 6 }, snapshot);
				Assert.Equal(1, changes);
				Assert.Empty(queued);
			}
			finally
			{
				snapshot?.Dispose();
				BindingBase.DisableCollectionSynchronization(source);
				gate.Dispose();
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
