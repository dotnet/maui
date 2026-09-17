#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Xunit;

namespace Microsoft.Maui.UnitTests.LifecycleEvents
{
	[Category(TestCategory.Core, TestCategory.Lifecycle)]
	public class LifecycleEventCompletionTests
	{
		[Fact]
		public void MissingServicesCompletesFalseOnce()
		{
			IServiceProvider? services = null;
			var completion = new CompletionRecorder();

			Invoke(services, completion);

			completion.AssertCompletedOnce(false);
		}

		[Fact]
		public void ZeroHandlersCompletesFalseOnce()
		{
			using var app = CreateApp();
			var completion = new CompletionRecorder();

			Invoke(app.Services, completion);

			completion.AssertCompletedOnce(false);
		}

		[Fact]
		public void SingleTrueHandlerCompletesTrueOnce()
		{
			using var app = CreateApp(callback => callback(true));
			var completion = new CompletionRecorder();

			Invoke(app.Services, completion);

			completion.AssertCompletedOnce(true);
		}

		[Fact]
		public void TrueCompletesAfterInvocationPassWithoutWaitingForLaterHandler()
		{
			var laterHandlerInvoked = false;
			var completionObservedAfterLaterHandler = false;
			Action<bool>? laterResponse = null;
			using var app = CreateApp(
				callback => callback(true),
				callback =>
				{
					laterHandlerInvoked = true;
					laterResponse = callback;
				});
			var completion = new CompletionRecorder(
				() => completionObservedAfterLaterHandler = laterHandlerInvoked);

			Invoke(app.Services, completion);

			Assert.True(laterHandlerInvoked);
			Assert.True(completionObservedAfterLaterHandler);
			completion.AssertCompletedOnce(true);

			Assert.NotNull(laterResponse);
			laterResponse!(false);

			completion.AssertCompletedOnce(true);
		}

		[Fact]
		public void InPassTrueDoesNotCompleteWhileLaterHandlerIsRunning()
		{
			var laterHandlerRunning = false;
			var completionObservedDuringLaterHandler = false;
			using var app = CreateApp(
				callback => callback(true),
				_ =>
				{
					laterHandlerRunning = true;
					Thread.Sleep(10);
					laterHandlerRunning = false;
				});
			var completion = new CompletionRecorder(
				() => completionObservedDuringLaterHandler = laterHandlerRunning);

			Invoke(app.Services, completion);

			Assert.False(completionObservedDuringLaterHandler);
			completion.AssertCompletedOnce(true);
		}

		[Fact]
		public async Task AsyncTrueCompletesWithOutstandingHandlerAndLateRepliesAreIgnored()
		{
			Action<bool>? asyncTrue = null;
			Action<bool>? outstanding = null;
			using var app = CreateApp(
				callback => asyncTrue = callback,
				callback => outstanding = callback);
			var completion = new CompletionRecorder();

			Invoke(app.Services, completion);

			Assert.Equal(0, completion.Count);
			Assert.NotNull(asyncTrue);
			Assert.NotNull(outstanding);

			await Task.Run(() => asyncTrue!(true));

			Assert.True(await completion.WaitAsync());
			completion.AssertCompletedOnce(true);

			outstanding!(false);
			outstanding!(true);

			completion.AssertCompletedOnce(true);
		}

		[Fact]
		public void AllFalseHandlersCompleteFalseOnce()
		{
			using var app = CreateApp(
				callback => callback(false),
				callback => callback(false));
			var completion = new CompletionRecorder();

			Invoke(app.Services, completion);

			completion.AssertCompletedOnce(false);
		}

		[Fact]
		public void AsyncTrueHandlerWinsAfterSynchronousFalse()
		{
			Action<bool>? delayedCompletion = null;
			using var app = CreateApp(
				callback => callback(false),
				callback => delayedCompletion = callback);
			var completion = new CompletionRecorder();

			Invoke(app.Services, completion);

			Assert.Equal(0, completion.Count);

			Assert.NotNull(delayedCompletion);
			delayedCompletion!(true);

			completion.AssertCompletedOnce(true);
		}

		[Fact]
		public void MultipleTrueHandlersCompleteTrueOnce()
		{
			using var app = CreateApp(
				callback => callback(true),
				callback => callback(true));
			var completion = new CompletionRecorder();

			Invoke(app.Services, completion);

			completion.AssertCompletedOnce(true);
		}

		[Fact]
		public void DuplicateHandlerResponseIsIgnoredAndLogged()
		{
			var loggerFactory = new RecordingLoggerFactory();
			using var app = CreateApp(
				loggerFactory,
				callback =>
				{
					callback(true);
					callback(false);
				});
			var completion = new CompletionRecorder();

			Invoke(app.Services, completion);

			completion.AssertCompletedOnce(true);
			Assert.Equal(1, loggerFactory.WarningCount);
		}

		[Fact]
		public async Task CrossThreadResponsesCompleteOnce()
		{
			Action<bool>? firstCompletion = null;
			Action<bool>? secondCompletion = null;
			using var app = CreateApp(
				callback => firstCompletion = callback,
				callback => secondCompletion = callback);
			var completion = new CompletionRecorder();

			Invoke(app.Services, completion);

			Assert.NotNull(firstCompletion);
			Assert.NotNull(secondCompletion);
			await Task.WhenAll(
				Task.Run(() => firstCompletion!(false)),
				Task.Run(() => secondCompletion!(true)));

			Assert.True(await completion.WaitAsync());
			completion.AssertCompletedOnce(true);
		}

		[Fact]
		public void ThrowBeforeResponseCompletesFalseOnceAndPropagates()
		{
			using var app = CreateApp(_ => throw new InvalidOperationException("Failure"));
			var completion = new CompletionRecorder();

			Assert.Throws<InvalidOperationException>(() => Invoke(app.Services, completion));

			completion.AssertCompletedOnce(false);
		}

		[Fact]
		public void ServiceResolutionFailureCompletesFalseOnceAndPropagates()
		{
			var services = new ThrowingServiceProvider();
			var completion = new CompletionRecorder();

			Assert.Throws<InvalidOperationException>(() => Invoke(services, completion));

			completion.AssertCompletedOnce(false);
		}

		[Fact]
		public void TrueThenThrowCompletesFalseOnceAndPropagates()
		{
			using var app = CreateApp(callback =>
			{
				callback(true);
				throw new InvalidOperationException("Failure");
			});
			var completion = new CompletionRecorder();

			Assert.Throws<InvalidOperationException>(() => Invoke(app.Services, completion));

			completion.AssertCompletedOnce(false);
		}

		[Fact]
		public void TrueThenThrowAndThrowingCompletionReportsBothFailures()
		{
			var handlerFailure = new InvalidOperationException("Handler failed.");
			var completionFailure = new ApplicationException("Completion failed.");
			Action<bool>? handlerCompletion = null;
			var completionCount = 0;
			using var app = CreateApp(callback =>
			{
				handlerCompletion = callback;
				callback(true);
				throw handlerFailure;
			});

			var exception = Assert.Throws<AggregateException>(() =>
				app.Services.InvokeLifecycleEventsWithCompletion<CompletionEvent>(
					(handler, callback) => handler(callback),
					_ =>
					{
						Interlocked.Increment(ref completionCount);
						throw completionFailure;
					}));

			Assert.Collection(
				exception.InnerExceptions,
				error => Assert.Same(handlerFailure, error),
				error => Assert.Same(completionFailure, error));
			Assert.Equal(1, completionCount);

			Assert.NotNull(handlerCompletion);
			handlerCompletion!(false);

			Assert.Equal(1, completionCount);
		}

		[Fact]
		public void CancellationCompletesFalseOnceAndPropagates()
		{
			using var app = CreateApp(_ => throw new OperationCanceledException());
			var completion = new CompletionRecorder();

			Assert.Throws<OperationCanceledException>(() => Invoke(app.Services, completion));

			completion.AssertCompletedOnce(false);
		}

		[Fact]
		public void CancellationAndThrowingCompletionReportsBothFailures()
		{
			var cancellation = new OperationCanceledException("Handler canceled.");
			var completionFailure = new ApplicationException("Completion failed.");
			var completionCount = 0;
			using var app = CreateApp(_ => throw cancellation);

			var exception = Assert.Throws<AggregateException>(() =>
				app.Services.InvokeLifecycleEventsWithCompletion<CompletionEvent>(
					(handler, callback) => handler(callback),
					_ =>
					{
						Interlocked.Increment(ref completionCount);
						throw completionFailure;
					}));

			Assert.Collection(
				exception.InnerExceptions,
				error => Assert.Same(cancellation, error),
				error => Assert.Same(completionFailure, error));
			Assert.Equal(1, completionCount);
		}

		[Fact]
		public void CollisionClearsFinalSinkBeforeEarlierAsyncResponse()
		{
			var handlerFailure = new InvalidOperationException("Handler failed.");
			var completionFailure = new ApplicationException("Completion failed.");
			Action<bool>? earlierResponse = null;
			var completionCount = 0;
			using var app = CreateApp(
				callback => earlierResponse = callback,
				_ => throw handlerFailure);

			var exception = Assert.Throws<AggregateException>(() =>
				app.Services.InvokeLifecycleEventsWithCompletion<CompletionEvent>(
					(handler, callback) => handler(callback),
					_ =>
					{
						Interlocked.Increment(ref completionCount);
						throw completionFailure;
					}));

			Assert.Collection(
				exception.InnerExceptions,
				error => Assert.Same(handlerFailure, error),
				error => Assert.Same(completionFailure, error));
			Assert.Equal(1, completionCount);

			Assert.NotNull(earlierResponse);
			earlierResponse!(true);

			Assert.Equal(1, completionCount);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void NormalFinalSinkFailurePropagatesOnce(bool handled)
		{
			var completionFailure = new ApplicationException("Completion failed.");
			var completionCount = 0;
			using var app = handled
				? CreateApp(callback => callback(true))
				: CreateApp(
					callback => callback(false),
					callback => callback(false));

			var exception = Assert.Throws<ApplicationException>(() =>
				app.Services.InvokeLifecycleEventsWithCompletion<CompletionEvent>(
					(handler, callback) => handler(callback),
					_ =>
					{
						Interlocked.Increment(ref completionCount);
						throw completionFailure;
					}));

			Assert.Same(completionFailure, exception);
			Assert.Equal(1, completionCount);
		}

		[Fact]
		public void LateResponseAfterFailureIsIgnored()
		{
			Action<bool>? lateCompletion = null;
			using var app = CreateApp(callback =>
			{
				lateCompletion = callback;
				throw new InvalidOperationException("Failure");
			});
			var completion = new CompletionRecorder();

			Assert.Throws<InvalidOperationException>(() => Invoke(app.Services, completion));
			completion.AssertCompletedOnce(false);

			Assert.NotNull(lateCompletion);
			lateCompletion!(true);

			completion.AssertCompletedOnce(false);
		}

		static void Invoke(IServiceProvider? services, CompletionRecorder completion) =>
			services.InvokeLifecycleEventsWithCompletion<CompletionEvent>(
				(handler, callback) => handler(callback),
				completion.Complete);

		static MauiApp CreateApp(params CompletionEvent[] handlers) =>
			CreateApp(null, handlers);

		static MauiApp CreateApp(ILoggerFactory? loggerFactory, params CompletionEvent[] handlers)
		{
			var builder = MauiApp.CreateBuilder()
				.ConfigureLifecycleEvents(lifecycle =>
				{
					foreach (var handler in handlers)
						lifecycle.AddEvent(nameof(CompletionEvent), handler);
				});

			if (loggerFactory is not null)
				builder.Services.AddSingleton(loggerFactory);

			return builder.Build();
		}

		delegate void CompletionEvent(Action<bool> completion);

		sealed class CompletionRecorder
		{
			readonly TaskCompletionSource<bool> _completion =
				new(TaskCreationOptions.RunContinuationsAsynchronously);
			readonly Action? _beforeCompletion;
			int _count;
			bool? _result;

			public CompletionRecorder(Action? beforeCompletion = null)
			{
				_beforeCompletion = beforeCompletion;
			}

			public int Count => Volatile.Read(ref _count);

			public void Complete(bool result)
			{
				_beforeCompletion?.Invoke();
				_result = result;
				Interlocked.Increment(ref _count);
				_completion.TrySetResult(result);
			}

			public async Task<bool> WaitAsync() =>
				await _completion.Task.WaitAsync(TimeSpan.FromSeconds(5));

			public void AssertCompletedOnce(bool expected)
			{
				Assert.Equal(1, Count);
				Assert.Equal(expected, _result);
			}
		}

		sealed class RecordingLoggerFactory : ILoggerFactory
		{
			readonly ILogger _logger;
			int _warningCount;

			public RecordingLoggerFactory()
			{
				_logger = new RecordingLogger(this);
			}

			public int WarningCount => Volatile.Read(ref _warningCount);

			public void AddProvider(ILoggerProvider provider)
			{
			}

			public ILogger CreateLogger(string categoryName) => _logger;

			public void Dispose()
			{
			}

			sealed class RecordingLogger : ILogger
			{
				readonly RecordingLoggerFactory _factory;

				public RecordingLogger(RecordingLoggerFactory factory)
				{
					_factory = factory;
				}

				public IDisposable? BeginScope<TState>(TState state)
					where TState : notnull =>
					null;

				public bool IsEnabled(LogLevel logLevel) => true;

				public void Log<TState>(
					LogLevel logLevel,
					EventId eventId,
					TState state,
					Exception? exception,
					Func<TState, Exception?, string> formatter)
				{
					if (logLevel == LogLevel.Warning)
						Interlocked.Increment(ref _factory._warningCount);
				}
			}
		}

		sealed class ThrowingServiceProvider : IServiceProvider
		{
			public object? GetService(Type serviceType) =>
				throw new InvalidOperationException("Service resolution failed.");
		}
	}
}
