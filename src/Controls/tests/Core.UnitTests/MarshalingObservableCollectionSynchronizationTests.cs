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
