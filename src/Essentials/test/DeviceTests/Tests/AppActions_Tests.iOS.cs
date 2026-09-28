using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using UIKit;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public partial class AppActions_Tests
	{
		[Fact]
		public Task PerformActionForShortcutItem_CompletesTrueOnceForSubscriber() =>
			Utils.OnMainThread(() =>
		{
			var appActions = new AppActionsImplementation();
			var received = 0;
			var completion = new CompletionRecorder();
			using var shortcutItem = new AppAction("app-action", "App Action").ToShortcutItem();

			appActions.AppActionActivated += (_, args) =>
			{
				received++;
				Assert.Equal("app-action", args.AppAction.Id);
			};

			appActions.PerformActionForShortcutItem(
				UIApplication.SharedApplication,
				shortcutItem,
				completion.Handler);

			Assert.Equal(1, received);
			completion.AssertCompletedOnce(true);
		});

		[Fact]
		public Task PerformActionForShortcutItem_CompletesFalseOnceForUnknownType() =>
			Utils.OnMainThread(() =>
		{
			var appActions = new AppActionsImplementation();
			var received = 0;
			var completion = new CompletionRecorder();
			using var shortcutItem = new UIApplicationShortcutItem(
				"custom-action",
				"Custom Action",
				null,
				null,
				null);

			appActions.AppActionActivated += (_, _) => received++;

			appActions.PerformActionForShortcutItem(
				UIApplication.SharedApplication,
				shortcutItem,
				completion.Handler);

			Assert.Equal(0, received);
			completion.AssertCompletedOnce(false);
		});

		[Fact]
		public Task PerformActionForShortcutItem_CompletesFalseOnceWithoutSubscriber() =>
			Utils.OnMainThread(() =>
		{
			var appActions = new AppActionsImplementation();
			var completion = new CompletionRecorder();
			using var shortcutItem = new AppAction("app-action", "App Action").ToShortcutItem();

			appActions.PerformActionForShortcutItem(
				UIApplication.SharedApplication,
				shortcutItem,
				completion.Handler);

			completion.AssertCompletedOnce(false);
		});

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task PerformActionForShortcutItem_CompletesFalseOnceAndPropagatesSubscriberFailure(bool cancellation)
		{
			var appActions = new AppActionsImplementation();
			var completion = new CompletionRecorder();

			appActions.AppActionActivated += (_, _) =>
			{
				if (cancellation)
					throw new OperationCanceledException();

				throw new InvalidOperationException();
			};

			if (cancellation)
			{
				await Assert.ThrowsAsync<OperationCanceledException>(() =>
					Utils.OnMainThread(() =>
					{
						using var shortcutItem = new AppAction("app-action", "App Action").ToShortcutItem();
						appActions.PerformActionForShortcutItem(
							UIApplication.SharedApplication,
							shortcutItem,
							completion.Handler);
					}));
			}
			else
			{
				await Assert.ThrowsAsync<InvalidOperationException>(() =>
					Utils.OnMainThread(() =>
					{
						using var shortcutItem = new AppAction("app-action", "App Action").ToShortcutItem();
						appActions.PerformActionForShortcutItem(
							UIApplication.SharedApplication,
							shortcutItem,
							completion.Handler);
					}));
			}

			completion.AssertCompletedOnce(false);
		}

		[Fact]
		public async Task PerformActionForShortcutItem_CompletesFalseOnceAndPropagatesConversionFailure()
		{
			var appActions = new AppActionsImplementation();
			var completion = new CompletionRecorder();
			appActions.AppActionActivated += (_, _) => { };

			await Assert.ThrowsAnyAsync<Exception>(() =>
				Utils.OnMainThread(() =>
				{
					using var shortcutItem = new UIApplicationShortcutItem(
						AppActionsImplementation.ShortcutType,
						"Malformed Action",
						null,
						null,
						new NSDictionary<NSString, NSObject>());
					appActions.PerformActionForShortcutItem(
						UIApplication.SharedApplication,
						shortcutItem,
						completion.Handler);
				}));

			completion.AssertCompletedOnce(false);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task PerformActionForShortcutItem_ReportsSubscriberAndCompletionFailures(bool cancellation)
		{
			var appActions = new AppActionsImplementation();
			Exception handlerFailure = cancellation
				? new OperationCanceledException("Handler canceled.")
				: new InvalidOperationException("Handler failed.");
			var completionFailure = new ApplicationException("Completion failed.");
			var completionCount = 0;

			appActions.AppActionActivated += (_, _) => throw handlerFailure;

			var exception = await Assert.ThrowsAsync<AggregateException>(() =>
				Utils.OnMainThread(() =>
				{
					using var shortcutItem = new AppAction("app-action", "App Action").ToShortcutItem();
					appActions.PerformActionForShortcutItem(
						UIApplication.SharedApplication,
						shortcutItem,
						_ =>
						{
							completionCount++;
							throw completionFailure;
						});
				}));

			Assert.Collection(
				exception.InnerExceptions,
				error => Assert.Same(handlerFailure, error),
				error => Assert.Same(completionFailure, error));
			Assert.Equal(1, completionCount);
		}

		[Fact]
		public async Task PerformActionForShortcutItem_PropagatesSuccessfulCompletionFailureOnce()
		{
			var appActions = new AppActionsImplementation();
			var completionFailure = new ApplicationException("Completion failed.");
			var completionCount = 0;
			appActions.AppActionActivated += (_, _) => { };

			var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
				Utils.OnMainThread(() =>
				{
					using var shortcutItem = new AppAction("app-action", "App Action").ToShortcutItem();
					appActions.PerformActionForShortcutItem(
						UIApplication.SharedApplication,
						shortcutItem,
						_ =>
						{
							completionCount++;
							throw completionFailure;
						});
				}));

			Assert.Same(completionFailure, exception);
			Assert.Equal(1, completionCount);
		}

		sealed class CompletionRecorder
		{
			public int Count { get; private set; }

			public bool? Result { get; private set; }

			public UIOperationHandler Handler => handled =>
			{
				Count++;
				Result = handled;
			};

			public void AssertCompletedOnce(bool expected)
			{
				Assert.Equal(1, Count);
				Assert.Equal(expected, Result);
			}
		}
	}
}
