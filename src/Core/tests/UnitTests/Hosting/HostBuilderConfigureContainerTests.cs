using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderConfigureContainerTests
	{
		[Fact]
		public void CreatesServiceProviderByDefault()
		{
			var builder = MauiApp.CreateBuilder();
			var mauiApp = builder.Build();

			Assert.IsType<ServiceProvider>(mauiApp.Services);
		}

		[Fact]
		public void ConfigureContainerThrowArgNull()
		{
			var builder = MauiApp.CreateBuilder();
			Assert.Throws<ArgumentNullException>(() => builder.ConfigureContainer<IServiceCollection>(null));
		}

		[Fact]
		public void ConfigureContainerCanReplaceIServiceProvider()
		{
			var builder = MauiApp.CreateBuilder(useDefaults: false);

			builder.ConfigureContainer(
				new MyServiceProviderFactory(),
				builder => builder.Configured = true);

			var mauiApp = builder.Build();
			Assert.IsType<MyServiceProvider>(mauiApp.Services);
			Assert.True(((MyServiceProvider)mauiApp.Services).Builder.Configured);
		}

		[Fact]
		public void AppCleanupRunsBeforeConfigurationIsDisposed()
		{
			var (app, configuration, cleanup) = BuildAppWithTrackedConfiguration();

			app.Dispose();

			Assert.True(cleanup.WasCalled);
			Assert.True(configuration.IsDisposed);
		}

		[Fact]
		public async Task AppCleanupRunsBeforeConfigurationIsDisposedAsync()
		{
			var (app, configuration, cleanup) = BuildAppWithTrackedConfiguration();

			await app.DisposeAsync();

			Assert.True(cleanup.WasCalled);
			Assert.True(configuration.IsDisposed);
		}

		[Fact]
		public void PostProviderCleanupRunsAfterProviderDisposal()
		{
			var probe = new ProviderDisposalProbe();
			var postCleanup = new CallbackPostProviderCleanup(() => Assert.True(probe.IsDisposed));
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton(_ => probe);
			builder.Services.AddSingleton<IMauiAppPostProviderCleanupService>(postCleanup);
			var app = builder.Build();
			_ = app.Services.GetRequiredService<ProviderDisposalProbe>();

			app.Dispose();

			Assert.True(postCleanup.WasCalled);
		}

		[Fact]
		public async Task PostProviderCleanupRunsAfterProviderDisposalAsync()
		{
			var probe = new ProviderDisposalProbe();
			var postCleanup = new CallbackPostProviderCleanup(() => Assert.True(probe.IsDisposed));
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton(_ => probe);
			builder.Services.AddSingleton<IMauiAppPostProviderCleanupService>(postCleanup);
			var app = builder.Build();
			_ = app.Services.GetRequiredService<ProviderDisposalProbe>();

			await app.DisposeAsync();

			Assert.True(postCleanup.WasCalled);
		}

		[Fact]
		public void AppCleanupRunsAllServicesAndAggregatesFailures()
		{
			var executionOrder = new List<int>();
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() =>
				{
					executionOrder.Add(1);
					throw new InvalidOperationException("first cleanup failed");
				}));
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() => executionOrder.Add(2)));
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() =>
				{
					executionOrder.Add(3);
					throw new InvalidOperationException("third cleanup failed");
				}));

			var aggregate = Assert.Throws<AggregateException>(() => builder.Build().Dispose());

			Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
			Assert.Collection(
				aggregate.InnerExceptions,
				ex => Assert.Equal("first cleanup failed", ex.Message),
				ex => Assert.Equal("third cleanup failed", ex.Message));
		}

		[Fact]
		public void RepeatedDisposeRunsCleanupOnce()
		{
			var cleanupCount = 0;
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() => cleanupCount++));
			var app = builder.Build();

			app.Dispose();
			app.Dispose();

			Assert.Equal(1, cleanupCount);
		}

		[Fact]
		public async Task MixedDisposeAndDisposeAsyncRunsCleanupOnce()
		{
			var cleanupCount = 0;
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() => cleanupCount++));
			var app = builder.Build();

			app.Dispose();
			await app.DisposeAsync();

			Assert.Equal(1, cleanupCount);
		}

		[Fact]
		public async Task RepeatedDisposeAsyncRunsCleanupOnce()
		{
			var cleanupCount = 0;
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() => cleanupCount++));
			var app = builder.Build();

			await app.DisposeAsync();
			await app.DisposeAsync();

			Assert.Equal(1, cleanupCount);
		}

		[Fact]
		public async Task MixedDisposeAsyncAndDisposeRunsCleanupOnce()
		{
			var cleanupCount = 0;
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() => cleanupCount++));
			var app = builder.Build();

			await app.DisposeAsync();
			app.Dispose();

			Assert.Equal(1, cleanupCount);
		}

		[Fact]
		public async Task ParallelDisposeCallersAllWaitForCleanupCompletion()
		{
			// A genuinely parallel race (Task.WhenAll) of sync and async disposers on the same
			// instance: the winner runs cleanup once and every "losing" caller must wait until that
			// teardown finishes rather than returning early.
			var timeout = TimeSpan.FromSeconds(30);
			var cleanupStarted = 0;
			var cleanupFinished = 0;
			var enteredCleanup = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var releaseCleanup = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() =>
				{
					Interlocked.Increment(ref cleanupStarted);
					enteredCleanup.TrySetResult(true);
					Assert.True(releaseCleanup.Task.Wait(timeout));
					Interlocked.Increment(ref cleanupFinished);
				}));
			var app = builder.Build();

			const int callers = 6;
			var start = new Barrier(callers);
			var tasks = new Task[callers];
			for (int i = 0; i < callers; i++)
			{
				var useSync = i % 2 == 0;
				tasks[i] = Task.Run(async () =>
				{
					start.SignalAndWait(timeout);
					if (useSync)
						app.Dispose();
					else
						await app.DisposeAsync();
				});
			}

			await enteredCleanup.Task.WaitAsync(timeout);

			// While the winner is still inside cleanup, no caller (winner or loser) may have returned.
			await Task.Delay(150);
			Assert.All(tasks, t => Assert.False(t.IsCompleted));

			releaseCleanup.TrySetResult(true);
			await Task.WhenAll(tasks).WaitAsync(timeout);

			Assert.Equal(1, cleanupStarted);
			Assert.Equal(1, cleanupFinished);
		}

		[Fact]
		public async Task ParallelDisposeCallersAllObserveCleanupException()
		{
			// Every concurrent caller (sync Dispose and async DisposeAsync) must observe the winner's
			// teardown exception, not silently return.
			var timeout = TimeSpan.FromSeconds(30);
			var cleanupCount = 0;
			var releaseCleanup = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() =>
				{
					Interlocked.Increment(ref cleanupCount);
					Assert.True(releaseCleanup.Task.Wait(timeout));
					throw new InvalidOperationException("cleanup boom");
				}));
			var app = builder.Build();

			const int callers = 6;
			var start = new Barrier(callers);
			var tasks = new Task[callers];
			for (int i = 0; i < callers; i++)
			{
				var useSync = i % 2 == 0;
				tasks[i] = Task.Run(async () =>
				{
					start.SignalAndWait(timeout);
					if (useSync)
						app.Dispose();
					else
						await app.DisposeAsync();
				});
			}

			// Give every caller time to reach the one-shot gate before the winner throws.
			await Task.Delay(150);
			releaseCleanup.TrySetResult(true);

			foreach (var task in tasks)
			{
				var exception = await Record.ExceptionAsync(() => task);
				var invalidOperation = Assert.IsType<InvalidOperationException>(exception);
				Assert.Equal("cleanup boom", invalidOperation.Message);
			}

			Assert.Equal(1, cleanupCount);
		}

		[Fact]
		public void RepeatedDisposeAfterCleanupFailureRethrowsSameException()
		{
			var cleanupCount = 0;
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() =>
				{
					cleanupCount++;
					throw new InvalidOperationException("cleanup boom");
				}));
			var app = builder.Build();

			var first = Assert.Throws<InvalidOperationException>(app.Dispose);
			var second = Assert.Throws<InvalidOperationException>(app.Dispose);

			Assert.Same(first, second);
			Assert.Equal(1, cleanupCount);
		}

		[Fact]
		public void InitializeAppServicesAfterDisposeThrowsObjectDisposedException()
		{
			var app = MauiApp.CreateBuilder(useDefaults: false).Build();
			app.Dispose();

			Assert.Throws<ObjectDisposedException>(app.InitializeAppServices);
		}

		[Fact]
		public async Task DisposeWaitsForInFlightInitializeAppServices()
		{
			var timeout = TimeSpan.FromSeconds(30);
			var initializer = new BlockingRepeatedInitializeService(timeout);
			var cleanupRan = false;
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiInitializeService>(initializer);
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() =>
				{
					Assert.True(initializer.SecondCallExited);
					cleanupRan = true;
				}));
			var app = builder.Build();

			var initialization = Task.Run(app.InitializeAppServices);
			await initializer.SecondCallEntered.Task.WaitAsync(timeout);

			var disposal = Task.Run(app.Dispose);
			Assert.True(
				SpinWait.SpinUntil(() => GetDisposeCompletion(app) is not null, timeout),
				"Disposal did not begin while app-service initialization was in flight.");
			Assert.False(disposal.IsCompleted);

			initializer.ReleaseSecondCall.TrySetResult(true);
			await Task.WhenAll(initialization, disposal).WaitAsync(timeout);

			Assert.True(cleanupRan);
		}

		[Fact]
		public async Task DisposeAsyncWaitsAsynchronouslyForInFlightInitializeAppServices()
		{
			var timeout = TimeSpan.FromSeconds(30);
			var returnTimeout = TimeSpan.FromSeconds(5);
			var initializer = new BlockingRepeatedInitializeService(timeout);
			var cleanupRan = false;
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiInitializeService>(initializer);
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() =>
				{
					Assert.True(initializer.SecondCallExited);
					cleanupRan = true;
				}));
			var app = builder.Build();

			var initialization = Task.Run(app.InitializeAppServices);
			await initializer.SecondCallEntered.Task.WaitAsync(timeout);

			var disposeCall = Task.Factory.StartNew(
				() => app.DisposeAsync().AsTask(),
				CancellationToken.None,
				TaskCreationOptions.None,
				TaskScheduler.Default);
			var returnedBeforeRelease = await Task.WhenAny(disposeCall, Task.Delay(returnTimeout)) == disposeCall;
			if (!returnedBeforeRelease)
			{
				initializer.ReleaseSecondCall.TrySetResult(true);
				await Task.WhenAll(initialization, disposeCall).WaitAsync(timeout);
			}

			Assert.True(
				returnedBeforeRelease,
				"DisposeAsync blocked synchronously while app-service initialization was in flight.");

			var disposal = await disposeCall;
			Assert.False(disposal.IsCompleted);

			initializer.ReleaseSecondCall.TrySetResult(true);
			await Task.WhenAll(initialization, disposal).WaitAsync(timeout);

			Assert.True(cleanupRan);
		}

		[Fact]
		public async Task DisposeWaitsForChildFlowInitializationAfterParentScopeExits()
		{
			var timeout = TimeSpan.FromSeconds(30);
			var childEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var releaseChild = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			using var app = MauiApp.CreateBuilder(useDefaults: false).Build();

			app.EnterInitializeAppServices();
			Task childInitialization;
			try
			{
				childInitialization = Task.Run(async () =>
				{
					app.EnterInitializeAppServices();
					childEntered.TrySetResult(true);
					try
					{
						await releaseChild.Task.WaitAsync(timeout);
					}
					finally
					{
						app.ExitInitializeAppServices();
					}
				});

				await childEntered.Task.WaitAsync(timeout);
			}
			finally
			{
				app.ExitInitializeAppServices();
			}

			var disposal = Task.Run(app.Dispose);
			Assert.True(
				SpinWait.SpinUntil(
					() => GetDisposeCompletion(app) is not null || disposal.IsCompleted,
					timeout),
				"Disposal did not begin while child-flow initialization was in flight.");

			var disposeCompletion = GetDisposeCompletion(app);
			var disposalCompletedBeforeRelease = disposal.IsCompleted;

			releaseChild.TrySetResult(true);
			await childInitialization.WaitAsync(timeout);
			var disposeException = await Record.ExceptionAsync(() => disposal.WaitAsync(timeout));

			Assert.NotNull(disposeCompletion);
			Assert.False(
				disposalCompletedBeforeRelease,
				"Disposal should wait for initialization still running in the inherited child flow.");
			Assert.Null(disposeException);
		}

		[Fact]
		public async Task DisposeFromChildFlowThrowsWhileInheritedParentScopeRemainsActive()
		{
			var timeout = TimeSpan.FromSeconds(30);
			var runChildDispose = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			using var app = MauiApp.CreateBuilder(useDefaults: false).Build();

			app.EnterInitializeAppServices();
			Task<Exception> childDisposal;
			bool observedDisposeAttempt;
			bool completedWhileParentScopeActive;
			object disposeCompletion;
			try
			{
				app.EnterInitializeAppServices();
				try
				{
					childDisposal = Task.Run(async () =>
					{
						await runChildDispose.Task.WaitAsync(timeout);
						return Record.Exception(app.Dispose);
					});
				}
				finally
				{
					app.ExitInitializeAppServices();
				}

				runChildDispose.TrySetResult(true);
				observedDisposeAttempt = SpinWait.SpinUntil(
					() => childDisposal.IsCompleted || GetDisposeCompletion(app) is not null,
					timeout);
				completedWhileParentScopeActive = childDisposal.IsCompleted;
				disposeCompletion = GetDisposeCompletion(app);
			}
			finally
			{
				app.ExitInitializeAppServices();
			}

			var exception = await childDisposal.WaitAsync(timeout);

			Assert.True(observedDisposeAttempt, "Child-flow disposal did not start.");
			Assert.True(
				completedWhileParentScopeActive,
				"Child-flow disposal should be rejected while an inherited parent scope remains active.");
			Assert.Null(disposeCompletion);
			Assert.IsType<InvalidOperationException>(exception);
		}

		[Fact]
		public async Task InitializeAppServicesDuringDisposeThrowsObjectDisposedException()
		{
			var timeout = TimeSpan.FromSeconds(30);
			var cleanupEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var releaseCleanup = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() =>
				{
					cleanupEntered.TrySetResult(true);
					Assert.True(releaseCleanup.Task.Wait(timeout));
				}));
			var app = builder.Build();
			var disposal = Task.Run(app.Dispose);

			await cleanupEntered.Task.WaitAsync(timeout);
			Assert.Throws<ObjectDisposedException>(app.InitializeAppServices);

			releaseCleanup.TrySetResult(true);
			await disposal.WaitAsync(timeout);
		}

		[Fact]
		public void DisposeFromInitializeAppServicesThrowsInsteadOfDeadlocking()
		{
			var initializer = new ReentrantDisposeInitializeService();
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiInitializeService>(initializer);
			var app = builder.Build();
			initializer.Target = app;

			app.InitializeAppServices();

			Assert.IsType<InvalidOperationException>(initializer.CapturedException);
			app.Dispose();
		}

		[Fact]
		public void DisposeAsyncFromInitializeAppServicesThrowsInsteadOfDeadlocking()
		{
			var initializer = new ReentrantDisposeAsyncInitializeService();
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiInitializeService>(initializer);
			var app = builder.Build();
			initializer.Target = app;

			app.InitializeAppServices();

			Assert.IsType<InvalidOperationException>(initializer.CapturedException);
			app.Dispose();
		}

		[Fact]
		public void ReentrantDisposeFromCleanupOnAnotherThreadDoesNotDeadlock()
		{
			// The disposal scope flows through Task.Run, so cleanup can hand work to another thread
			// and wait for it without that reentrant caller waiting on the outer disposal.
			var timeout = TimeSpan.FromSeconds(30);
			var cleanupCount = 0;
			MauiApp app = null!;
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() =>
				{
					Interlocked.Increment(ref cleanupCount);
					Assert.True(Task.Run(app.Dispose).Wait(timeout));
					Assert.True(Task.Run(async () => await app.DisposeAsync()).Wait(timeout));
				}));
			app = builder.Build();

			var disposal = Task.Run(() => app.Dispose());

			Assert.True(disposal.Wait(timeout), "Reentrant dispose from a cleanup service deadlocked.");
			Assert.Equal(1, cleanupCount);
		}

		[Fact]
		public async Task DisposeFromChildFlowForkedDuringTeardownRethrowsRecordedException()
		{
			var timeout = TimeSpan.FromSeconds(30);
			var childForked = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var releaseChild = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var failure = new InvalidOperationException("cleanup failed");
			Task<Exception> childDisposal = null!;
			MauiApp app = null!;
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() =>
				{
					childDisposal = Task.Run(async () =>
					{
						childForked.TrySetResult(true);
						await releaseChild.Task.WaitAsync(timeout);
						return Record.Exception(app.Dispose);
					});
					throw failure;
				}));
			app = builder.Build();

			var outerException = Record.Exception(app.Dispose);
			await childForked.Task.WaitAsync(timeout);
			releaseChild.TrySetResult(true);
			var childException = await childDisposal.WaitAsync(timeout);

			Assert.Same(failure, outerException);
			Assert.Same(failure, childException);
		}

		[Fact]
		public void MauiAppDisposeDisposesAsyncOnlyServiceProvider()
		{
			var factory = new AsyncOnlyServiceProviderFactory();
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.ConfigureContainer(factory);

			var app = builder.Build();
			app.Dispose();

			Assert.True(factory.Provider.IsDisposed);
		}

		[Fact]
		public void MauiAppDisposeDoesNotCaptureCurrentSynchronizationContextForAsyncOnlyServiceProvider()
		{
			var factory = new AsyncOnlyServiceProviderFactory(useYieldingProvider: true);
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.ConfigureContainer(factory);
			var app = builder.Build();
			var originalContext = SynchronizationContext.Current;

			try
			{
				SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

				app.Dispose();
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(originalContext);
			}

			var provider = Assert.IsType<YieldingAsyncOnlyServiceProvider>(factory.Provider);
			Assert.True(provider.IsDisposed);
			Assert.False(provider.DisposeStartedWithSynchronizationContext);
		}

		[Fact]
		public void BuildFailureDisposesAsyncOnlyServiceProvider()
		{
			var factory = new AsyncOnlyServiceProviderFactory();
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.ConfigureContainer(factory);
			builder.Services.AddSingleton<IMauiInitializeService, ThrowingInitializeService>();

			var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

			Assert.Equal("initialization failed", exception.Message);
			Assert.True(factory.Provider.IsDisposed);
		}

		[Fact]
		public async Task BuildFailureDoesNotBlockUIThreadForAsyncOnlyServiceProvider()
		{
			var timeout = TimeSpan.FromSeconds(30);
			var returnTimeout = TimeSpan.FromSeconds(1);
			var context = new QueuedSynchronizationContext();
			var factory = new UIThreadAsyncOnlyServiceProviderFactory(context);
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.ConfigureContainer(factory);
			builder.Services.AddSingleton<IMauiInitializeService, ThrowingInitializeService>();
			var buildReturned = new TaskCompletionSource<Exception>(
				TaskCreationOptions.RunContinuationsAsynchronously);

			var build = Task.Run(() =>
			{
				var originalContext = SynchronizationContext.Current;
				try
				{
					SynchronizationContext.SetSynchronizationContext(context);
					buildReturned.TrySetResult(Record.Exception(builder.Build));
				}
				finally
				{
					SynchronizationContext.SetSynchronizationContext(originalContext);
				}
			});

			await factory.DisposeStarted.Task.WaitAsync(timeout);
			var returnedBeforePump =
				await Task.WhenAny(buildReturned.Task, Task.Delay(returnTimeout)) == buildReturned.Task;

			context.RunAll();
			var exception = await buildReturned.Task.WaitAsync(timeout);
			await Task.WhenAll(build, factory.DisposeCompleted.Task).WaitAsync(timeout);

			Assert.True(
				returnedBeforePump,
				"Build blocked the UI thread while the async-only provider awaited dispatched work.");
			Assert.IsType<InvalidOperationException>(exception);
			Assert.Equal("initialization failed", exception.Message);
			Assert.True(factory.Provider.IsDisposed);
		}

		[Fact]
		public async Task BuildFailureRestoresEssentialsFacadeAfterAsyncOnlyServiceProviderDisposalCompletes()
		{
			var timeout = TimeSpan.FromSeconds(30);
			var returnTimeout = TimeSpan.FromSeconds(1);
			var originalPreferences = Preferences.Default;
			var bridgedPreferences = new StubPreferences();
			PreferencesReadingThrowingInitializeService initializer = null;
			var context = new QueuedSynchronizationContext();
			var factory = new UIThreadAsyncOnlyServiceProviderFactory(context);
			var builder = MauiApp.CreateBuilder();
			builder.ConfigureContainer(factory);
			builder.Services.AddSingleton<IPreferences>(bridgedPreferences);
			builder.Services.AddSingleton<IMauiInitializeService>(_ =>
				initializer = new PreferencesReadingThrowingInitializeService());
			var buildReturned = new TaskCompletionSource<Exception>(
				TaskCreationOptions.RunContinuationsAsynchronously);

			try
			{
				var build = Task.Run(() =>
				{
					var originalContext = SynchronizationContext.Current;
					try
					{
						SynchronizationContext.SetSynchronizationContext(context);
						buildReturned.TrySetResult(Record.Exception(builder.Build));
					}
					finally
					{
						SynchronizationContext.SetSynchronizationContext(originalContext);
					}
				});

				await factory.DisposeStarted.Task.WaitAsync(timeout);
				var returnedBeforePump =
					await Task.WhenAny(buildReturned.Task, Task.Delay(returnTimeout)) == buildReturned.Task;
				var exception = await buildReturned.Task.WaitAsync(timeout);

				Assert.True(
					returnedBeforePump,
					"Build blocked while the async-only provider awaited dispatched disposal work.");
				Assert.IsType<InvalidOperationException>(exception);
				Assert.Equal("initialization failed", exception.Message);
				Assert.Same(bridgedPreferences, Preferences.Default);
				Assert.False(factory.Provider.IsDisposed);

				context.RunAll();
				await Task.WhenAll(build, factory.DisposeCompleted.Task).WaitAsync(timeout);

				Assert.NotNull(initializer);
				Assert.Same(bridgedPreferences, initializer!.PreferencesDuringDispose);
				Assert.True(factory.Provider.IsDisposed);
				Assert.True(
					SpinWait.SpinUntil(
						() => ReferenceEquals(originalPreferences, Preferences.Default),
						timeout),
					"Essentials facade was not restored after provider disposal completed.");
			}
			finally
			{
				context.RunAll();
				Preferences.SetDefault(originalPreferences);
			}
		}

		static (MauiApp App, DisposableConfigurationProvider Configuration, ConfigurationReadingCleanup Cleanup)
			BuildAppWithTrackedConfiguration()
		{
			var configuration = new DisposableConfigurationProvider();
			var cleanup = new ConfigurationReadingCleanup(configuration);
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			((IConfigurationBuilder)builder.Configuration).Add(new DisposableConfigurationSource(configuration));
			builder.Services.AddSingleton<IMauiAppCleanupService>(cleanup);

			return (builder.Build(), configuration, cleanup);
		}

		static object GetDisposeCompletion(MauiApp app)
		{
			var field = typeof(MauiApp).GetField(
				"_disposeCompletion",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
				?? throw new InvalidOperationException("MauiApp disposal completion field was not found.");
			return field.GetValue(app);
		}

		private class MyServiceProviderFactory : IServiceProviderFactory<MyServiceBuilder>
		{
			public MyServiceBuilder CreateBuilder(IServiceCollection services) => new MyServiceBuilder(services);

			public IServiceProvider CreateServiceProvider(MyServiceBuilder containerBuilder) => new MyServiceProvider(containerBuilder);
		}

		private class MyServiceBuilder
		{
			public bool Configured { get; set; }
			public IServiceCollection Services { get; }

			public MyServiceBuilder(IServiceCollection services)
			{
				Services = services;
			}
		}

		private class MyServiceProvider : IServiceProvider
		{
			private readonly ServiceProvider _serviceProvider;

			public MyServiceBuilder Builder { get; set; }

			public MyServiceProvider(MyServiceBuilder builder)
			{
				Builder = builder;
				_serviceProvider = builder.Services.BuildServiceProvider();
			}

			public object GetService(Type serviceType) => _serviceProvider.GetService(serviceType);
		}

		sealed class AsyncOnlyServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
		{
			readonly bool _useYieldingProvider;

			public AsyncOnlyServiceProviderFactory(bool useYieldingProvider = false)
			{
				_useYieldingProvider = useYieldingProvider;
			}

			public AsyncOnlyServiceProvider Provider { get; private set; } = null!;

			public IServiceCollection CreateBuilder(IServiceCollection services) => services;

			public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
			{
				Provider = _useYieldingProvider
					? new YieldingAsyncOnlyServiceProvider(containerBuilder.BuildServiceProvider())
					: new AsyncOnlyServiceProvider(containerBuilder.BuildServiceProvider());
				return Provider;
			}
		}

		class AsyncOnlyServiceProvider : IServiceProvider, IAsyncDisposable
		{
			protected readonly ServiceProvider InnerProvider;

			public AsyncOnlyServiceProvider(ServiceProvider innerProvider)
			{
				InnerProvider = innerProvider;
			}

			public bool IsDisposed { get; private set; }

			public object GetService(Type serviceType) =>
				InnerProvider.GetService(serviceType);

			public virtual ValueTask DisposeAsync()
			{
				IsDisposed = true;
				return InnerProvider.DisposeAsync();
			}
		}

		sealed class YieldingAsyncOnlyServiceProvider : AsyncOnlyServiceProvider
		{
			public YieldingAsyncOnlyServiceProvider(ServiceProvider innerProvider)
				: base(innerProvider)
			{
			}

			public bool DisposeStartedWithSynchronizationContext { get; private set; }

			public override async ValueTask DisposeAsync()
			{
				DisposeStartedWithSynchronizationContext = SynchronizationContext.Current is not null;
				await Task.Yield();
				await base.DisposeAsync();
			}
		}

		sealed class UIThreadAsyncOnlyServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
		{
			readonly QueuedSynchronizationContext _context;

			public UIThreadAsyncOnlyServiceProviderFactory(QueuedSynchronizationContext context)
			{
				_context = context;
			}

			public TaskCompletionSource<bool> DisposeStarted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public TaskCompletionSource<bool> DisposeCompleted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public UIThreadAsyncOnlyServiceProvider Provider { get; private set; } = null!;

			public IServiceCollection CreateBuilder(IServiceCollection services) => services;

			public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
			{
				Provider = new UIThreadAsyncOnlyServiceProvider(
					containerBuilder.BuildServiceProvider(),
					_context,
					DisposeStarted,
					DisposeCompleted);
				return Provider;
			}
		}

		sealed class UIThreadAsyncOnlyServiceProvider : AsyncOnlyServiceProvider
		{
			readonly QueuedSynchronizationContext _context;
			readonly TaskCompletionSource<bool> _disposeStarted;
			readonly TaskCompletionSource<bool> _disposeCompleted;

			public UIThreadAsyncOnlyServiceProvider(
				ServiceProvider innerProvider,
				QueuedSynchronizationContext context,
				TaskCompletionSource<bool> disposeStarted,
				TaskCompletionSource<bool> disposeCompleted)
				: base(innerProvider)
			{
				_context = context;
				_disposeStarted = disposeStarted;
				_disposeCompleted = disposeCompleted;
			}

			public override async ValueTask DisposeAsync()
			{
				_disposeStarted.TrySetResult(true);
				try
				{
					await _context.InvokeAsync();
					await base.DisposeAsync();
				}
				finally
				{
					_disposeCompleted.TrySetResult(true);
				}
			}
		}

		sealed class ThrowingInitializeService : IMauiInitializeService
		{
			public void Initialize(IServiceProvider services)
			{
				throw new InvalidOperationException("initialization failed");
			}
		}

		sealed class PreferencesReadingThrowingInitializeService : IMauiInitializeService, IAsyncDisposable
		{
			public IPreferences PreferencesDuringDispose { get; private set; }

			public void Initialize(IServiceProvider services)
			{
				throw new InvalidOperationException("initialization failed");
			}

			public ValueTask DisposeAsync()
			{
				PreferencesDuringDispose = Preferences.Default;
				return ValueTask.CompletedTask;
			}
		}

		sealed class StubPreferences : IPreferences
		{
			public bool ContainsKey(string key, string sharedName = null) => false;
			public void Remove(string key, string sharedName = null) { }
			public void Clear(string sharedName = null) { }
			public void Set<T>(string key, T value, string sharedName = null) { }
			public T Get<T>(string key, T defaultValue, string sharedName = null) => defaultValue;
		}

		sealed class BlockingRepeatedInitializeService : IMauiInitializeService
		{
			readonly TimeSpan _timeout;
			int _callCount;

			public BlockingRepeatedInitializeService(TimeSpan timeout)
			{
				_timeout = timeout;
			}

			public TaskCompletionSource<bool> SecondCallEntered { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public TaskCompletionSource<bool> ReleaseSecondCall { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public bool SecondCallExited { get; private set; }

			public void Initialize(IServiceProvider services)
			{
				if (Interlocked.Increment(ref _callCount) != 2)
					return;

				SecondCallEntered.TrySetResult(true);
				Assert.True(ReleaseSecondCall.Task.Wait(_timeout));
				SecondCallExited = true;
			}
		}

		sealed class ReentrantDisposeInitializeService : IMauiInitializeService
		{
			int _callCount;

			public MauiApp Target { get; set; }

			public Exception CapturedException { get; private set; }

			public void Initialize(IServiceProvider services)
			{
				if (Interlocked.Increment(ref _callCount) == 1)
					return;

				CapturedException = Record.Exception(() => Target?.Dispose());
			}
		}

		sealed class ReentrantDisposeAsyncInitializeService : IMauiInitializeService
		{
			int _callCount;

			public MauiApp Target { get; set; }

			public Exception CapturedException { get; private set; }

			public void Initialize(IServiceProvider services)
			{
				if (Interlocked.Increment(ref _callCount) == 1)
					return;

				CapturedException = Record.Exception(() => Target.DisposeAsync().AsTask().GetAwaiter().GetResult());
			}
		}

		sealed class ConfigurationReadingCleanup : IMauiAppCleanupService
		{
			readonly DisposableConfigurationProvider _configuration;

			public ConfigurationReadingCleanup(DisposableConfigurationProvider configuration)
			{
				_configuration = configuration;
			}

			public bool WasCalled { get; private set; }

			public void Cleanup()
			{
				if (_configuration.IsDisposed)
					throw new InvalidOperationException("Configuration was disposed before app cleanup.");

				WasCalled = true;
			}
		}

		sealed class CallbackCleanup : IMauiAppCleanupService
		{
			readonly Action _cleanup;

			public CallbackCleanup(Action cleanup)
			{
				_cleanup = cleanup;
			}

			public void Cleanup() => _cleanup();
		}

		sealed class CallbackPostProviderCleanup : IMauiAppPostProviderCleanupService
		{
			readonly Action _cleanup;

			public CallbackPostProviderCleanup(Action cleanup)
			{
				_cleanup = cleanup;
			}

			public bool WasCalled { get; private set; }

			public void Cleanup()
			{
				_cleanup();
				WasCalled = true;
			}
		}

		sealed class ProviderDisposalProbe : IDisposable
		{
			public bool IsDisposed { get; private set; }

			public void Dispose()
			{
				IsDisposed = true;
			}
		}

		sealed class QueuedSynchronizationContext : SynchronizationContext
		{
			readonly object _sync = new();
			readonly Queue<(SendOrPostCallback Callback, object State)> _callbacks = new();

			public override void Post(SendOrPostCallback d, object state)
			{
				lock (_sync)
					_callbacks.Enqueue((d, state));
			}

			public Task InvokeAsync()
			{
				var completion = new TaskCompletionSource<bool>(
					TaskCreationOptions.RunContinuationsAsynchronously);
				Post(_ => completion.TrySetResult(true), null);
				return completion.Task;
			}

			public void RunAll()
			{
				while (true)
				{
					(SendOrPostCallback Callback, object State) callback;
					lock (_sync)
					{
						if (_callbacks.Count == 0)
							return;

						callback = _callbacks.Dequeue();
					}

					callback.Callback(callback.State);
				}
			}
		}

		sealed class DisposableConfigurationSource : IConfigurationSource
		{
			readonly DisposableConfigurationProvider _provider;

			public DisposableConfigurationSource(DisposableConfigurationProvider provider)
			{
				_provider = provider;
			}

			public IConfigurationProvider Build(IConfigurationBuilder builder) =>
				_provider;
		}

		sealed class DisposableConfigurationProvider : ConfigurationProvider, IDisposable
		{
			public bool IsDisposed { get; private set; }

			public void Dispose()
			{
				IsDisposed = true;
			}
		}
	}
}