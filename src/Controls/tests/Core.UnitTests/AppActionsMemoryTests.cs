using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class AppActionsMemoryTests : IDisposable
	{
		readonly IAppActions previous = AppActions.Current;

		public void Dispose() => AppActions.SetCurrent(previous);

		[Fact]
		public async Task OnAppActionDoesNotRetainSubscriber()
		{
			var publisher = AppActions.Current;
			var reference = SubscribeAndDrop();

			try
			{
				Assert.False(await reference.WaitForCollect(), "AppActions.OnAppAction should not retain its subscriber!");
				GC.KeepAlive(publisher);
			}
			finally
			{
				if (reference.Target is Subscriber subscriber)
					AppActions.OnAppAction -= subscriber.OnAppAction;
			}
		}

		[Fact]
		public async Task LiveSubscriberReceivesEventAfterCollection()
		{
			var publisher = new AppActionsStub();
			AppActions.SetCurrent(publisher);
			var subscriber = new Subscriber();
			AppActions.OnAppAction += subscriber.OnAppAction;

			await TestHelpers.Collect();
			var args = new AppActionEventArgs(new AppAction("id", "title"));
			publisher.Raise(args);

			Assert.Equal(1, subscriber.Count);
			Assert.Same(publisher, subscriber.Sender);
			Assert.Same(args, subscriber.Args);
			GC.KeepAlive(subscriber);
		}

		[Theory]
		[InlineData(1, 0, 1)]
		[InlineData(1, 1, 0)]
		[InlineData(2, 1, 1)]
		[InlineData(1, 2, 0)]
		public void RemovingHandlerPreservesDuplicateSubscriptions(int additions, int removals, int expected)
		{
			var publisher = new AppActionsStub();
			AppActions.SetCurrent(publisher);
			var subscriber = new Subscriber();
			AppActions.OnAppAction += null;
			AppActions.OnAppAction -= null;

			for (int i = 0; i < additions; i++)
				AppActions.OnAppAction += subscriber.OnAppAction;
			for (int i = 0; i < removals; i++)
				AppActions.OnAppAction -= subscriber.OnAppAction;

			publisher.Raise(new AppActionEventArgs(new AppAction("id", "title")));

			Assert.Equal(expected, subscriber.Count);
		}

		[Fact]
		public void MulticastRemovalRemovesLastMatchingSequence()
		{
			var publisher = new AppActionsStub();
			AppActions.SetCurrent(publisher);
			var first = new Subscriber();
			var second = new Subscriber();
			EventHandler<AppActionEventArgs> handlers = first.OnAppAction;
			handlers += second.OnAppAction;
			AppActions.OnAppAction += handlers;
			AppActions.OnAppAction += handlers;
			AppActions.OnAppAction -= second.OnAppAction;
			AppActions.OnAppAction -= handlers;

			publisher.Raise(new AppActionEventArgs(new AppAction("id", "title")));

			Assert.Equal(1, first.Count);
			Assert.Equal(0, second.Count);
		}

		[Fact]
		public async Task StaticHandlerSurvivesCollectionAndCanBeRemoved()
		{
			var publisher = new AppActionsStub();
			AppActions.SetCurrent(publisher);
			AppActions.OnAppAction += OnStaticAppAction;
			await TestHelpers.Collect();
			var args = new AppActionEventArgs(new AppAction("id", "title"));

			publisher.Raise(args);
			Assert.Equal("handled", args.AppAction.Title);

			AppActions.OnAppAction -= OnStaticAppAction;
			args.AppAction.Title = "removed";
			publisher.Raise(args);
			Assert.Equal("removed", args.AppAction.Title);
		}

		[Fact]
		public async Task ConfiguredHandlerSurvivesCollectionWhileAppIsAlive()
		{
			var publisher = new AppActionsStub();
			var subscriber = new Subscriber();
			var builder = MauiApp.CreateBuilder()
				.ConfigureEssentials(essentials => essentials.OnAppAction(action => subscriber.OnAppAction(null, new AppActionEventArgs(action))));
			builder.Services.AddSingleton<IAppActions>(publisher);
			using var app = builder.Build();

			await TestHelpers.Collect();
			publisher.Raise(new AppActionEventArgs(new AppAction("id", "title")));

			Assert.Equal(1, subscriber.Count);
			GC.KeepAlive(app);
		}

		[Fact]
		public void SubscriberExceptionPropagatesUnwrapped()
		{
			var publisher = new AppActionsStub();
			AppActions.SetCurrent(publisher);
			var failure = new InvalidOperationException("Subscriber failed");
			AppActions.OnAppAction += (_, _) => throw failure;

			var thrown = Assert.Throws<InvalidOperationException>(() =>
				publisher.Raise(new AppActionEventArgs(new AppAction("id", "title"))));

			Assert.Same(failure, thrown);
		}

		[Fact]
		public void SubscriptionsBelongToTheirPublisher()
		{
			var first = new AppActionsStub();
			var second = new AppActionsStub();
			var subscriber = new Subscriber();
			AppActions.SetCurrent(first);
			AppActions.OnAppAction += subscriber.OnAppAction;
			AppActions.SetCurrent(second);
			AppActions.OnAppAction -= subscriber.OnAppAction;
			AppActions.OnAppAction += subscriber.OnAppAction;
			AppActions.OnAppAction -= subscriber.OnAppAction;
			var args = new AppActionEventArgs(new AppAction("id", "title"));

			second.Raise(args);
			Assert.Equal(0, subscriber.Count);
			first.Raise(args);
			Assert.Equal(1, subscriber.Count);
		}

		static void OnStaticAppAction(object sender, AppActionEventArgs args) =>
			args.AppAction.Title = "handled";

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference SubscribeAndDrop()
		{
			var subscriber = new Subscriber();
			AppActions.OnAppAction += subscriber.OnAppAction;
			return new WeakReference(subscriber);
		}

		sealed class Subscriber
		{
			public int Count { get; private set; }
			public object Sender { get; private set; }
			public AppActionEventArgs Args { get; private set; }

			public void OnAppAction(object sender, AppActionEventArgs args)
			{
				Count++;
				Sender = sender;
				Args = args;
			}
		}

		sealed class AppActionsStub : IAppActions
		{
			public bool IsSupported => true;
			public event EventHandler<AppActionEventArgs> AppActionActivated;
			public Task<IEnumerable<AppAction>> GetAsync() => throw new NotImplementedException();
			public Task SetAsync(IEnumerable<AppAction> actions) => throw new NotImplementedException();
			public void Raise(AppActionEventArgs args) => AppActionActivated?.Invoke(this, args);
		}
	}
}
