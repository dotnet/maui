using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class AppActionsMemoryTests : IDisposable
	{
		readonly IAppActions _original = AppActions.Current;

		public void Dispose() => AppActions.SetCurrent(_original);

		[Fact]
		public async Task OnAppActionDoesNotRetainSubscriber()
		{
			var publisher = AppActions.Current;
			var reference = Subscribe();

			Assert.False(await reference.WaitForCollect(), "AppActions.OnAppAction should not retain its subscriber!");
			GC.KeepAlive(publisher);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference Subscribe()
		{
			var subscriber = new Subscriber();
			AppActions.OnAppAction += subscriber.OnAppAction;
			return new WeakReference(subscriber);
		}

		[Fact]
		public async Task LiveSubscriberReceivesEventsAfterCollection()
		{
			var publisher = UsePublisher();
			var subscriber = new Subscriber();
			AppActions.OnAppAction += subscriber.OnAppAction;

			await TestHelpers.Collect();
			publisher.Raise();

			Assert.Equal(1, subscriber.Calls);
			Assert.Same(publisher, subscriber.Sender);
			GC.KeepAlive(subscriber);
		}

		[Fact]
		public void RemovingMulticastHandlerRemovesLastMatchingSequence()
		{
			var publisher = UsePublisher();
			var calls = new List<int>();
			EventHandler<AppActionEventArgs> first = (s, e) => calls.Add(1);
			EventHandler<AppActionEventArgs> second = (s, e) => calls.Add(2);
			AppActions.OnAppAction += first + second + first + second + first;

			AppActions.OnAppAction -= first + second;
			publisher.Raise();

			Assert.Equal(new[] { 1, 2, 1 }, calls);
			GC.KeepAlive(first);
			GC.KeepAlive(second);
		}

		[Fact]
		public void RemovingDuplicateHandlerLeavesEarlierSubscription()
		{
			var publisher = UsePublisher();
			var subscriber = new Subscriber();
			AppActions.OnAppAction += subscriber.OnAppAction;
			AppActions.OnAppAction += subscriber.OnAppAction;
			AppActions.OnAppAction -= subscriber.OnAppAction;

			publisher.Raise();

			Assert.Equal(1, subscriber.Calls);
			AppActions.OnAppAction -= subscriber.OnAppAction;
			publisher.Raise();
			Assert.Equal(1, subscriber.Calls);
		}

		[Fact]
		public void DirectAndFacadeSubscriptionsPreserveOrder()
		{
			var publisher = UsePublisher();
			var calls = new List<int>();
			EventHandler<AppActionEventArgs> first = (s, e) => calls.Add(1);
			EventHandler<AppActionEventArgs> last = (s, e) => calls.Add(3);
			AppActions.OnAppAction += first;
			publisher.AppActionActivated += (s, e) => calls.Add(2);
			AppActions.OnAppAction += last;

			publisher.Raise();

			Assert.Equal(new[] { 1, 2, 3 }, calls);
			GC.KeepAlive(first);
			GC.KeepAlive(last);
		}

		[Fact]
		public void SubscriberExceptionIsNotWrapped()
		{
			var publisher = UsePublisher();
			var expected = new InvalidOperationException("Subscriber failure");
			EventHandler<AppActionEventArgs> handler = (s, e) => throw expected;
			AppActions.OnAppAction += handler;

			Assert.Same(expected, Assert.Throws<InvalidOperationException>(publisher.Raise));
			GC.KeepAlive(handler);
		}

		[Fact]
		public void NullHandlersAreIgnored()
		{
			var publisher = UsePublisher();
			AppActions.OnAppAction += null;
			AppActions.OnAppAction -= null;

			Assert.Equal(0, publisher.HandlerCount);
		}

		[Fact]
		public async Task StaticHandlerSurvivesCollectionAndCanBeRemoved()
		{
			var publisher = UsePublisher();
			AppActions.OnAppAction += StaticHandler;

			await TestHelpers.Collect();
			Assert.Throws<NotSupportedException>(publisher.Raise);

			AppActions.OnAppAction -= StaticHandler;
			publisher.Raise();
			Assert.Equal(0, publisher.HandlerCount);
		}

		static void StaticHandler(object sender, AppActionEventArgs args) => throw new NotSupportedException();

		[Fact]
		public void PublishersHaveIndependentSubscriptions()
		{
			var first = UsePublisher();
			var subscriber = new Subscriber();
			AppActions.OnAppAction += subscriber.OnAppAction;
			var second = UsePublisher();
			AppActions.OnAppAction += subscriber.OnAppAction;
			AppActions.OnAppAction -= subscriber.OnAppAction;

			second.Raise();
			Assert.Equal(0, subscriber.Calls);
			first.Raise();
			Assert.Equal(1, subscriber.Calls);
		}

		[Fact]
		public async Task DeadSubscriptionsAreDetachedWhenRaised()
		{
			var publisher = UsePublisher();
			var reference = Subscribe();
			Assert.False(await reference.WaitForCollect());
			Assert.Equal(1, publisher.HandlerCount);

			publisher.Raise();

			Assert.Equal(0, publisher.HandlerCount);
		}

		[Fact]
		public async Task ConfiguredAppActionCallbackSurvivesCollection()
		{
			var publisher = UsePublisher();
			var subscriber = new Subscriber();
			using var app = BuildApp(subscriber);

			await TestHelpers.Collect();
			publisher.Raise();

			Assert.Equal(1, subscriber.Calls);
			GC.KeepAlive(app);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static MauiApp BuildApp(Subscriber subscriber)
		{
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.ConfigureEssentials(essentials => essentials.OnAppAction(action => subscriber.Calls++));
			return builder.Build();
		}

		static Publisher UsePublisher()
		{
			var publisher = new Publisher();
			AppActions.SetCurrent(publisher);
			return publisher;
		}

		sealed class Publisher : IAppActions
		{
			public bool IsSupported => true;
			public event EventHandler<AppActionEventArgs> AppActionActivated;
			public int HandlerCount => AppActionActivated?.GetInvocationList().Length ?? 0;
			public Task<IEnumerable<AppAction>> GetAsync() => Task.FromResult<IEnumerable<AppAction>>(Array.Empty<AppAction>());
			public Task SetAsync(IEnumerable<AppAction> actions) => Task.CompletedTask;
			public void Raise() => AppActionActivated?.Invoke(this, new AppActionEventArgs(new AppAction("test", "Test")));
		}

		sealed class Subscriber
		{
			public int Calls;
			public object Sender;
			public void OnAppAction(object sender, AppActionEventArgs args)
			{
				Calls++;
				Sender = sender;
			}
		}
	}
}
