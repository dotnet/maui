#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Handlers;
using Xunit;
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue37187 : ControlsHandlerTestBase
	{
		const string IssueNumber = "37187";

		static string? GetReplicationIssue()
		{
#if ANDROID
			return global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
				.MauiTestInstrumentation.Current?.Arguments?.GetString("MAUI_REPRODUCTION_ISSUE");
#elif IOS || MACCATALYST
			return global::Foundation.NSProcessInfo.ProcessInfo.Environment["MAUI_REPRODUCTION_ISSUE"]?.ToString();
#else
			return Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE");
#endif
		}

		[Fact]
		public async Task RemovedFlyoutFooterDoesNotMeasureReplacement()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					SetupShellHandlers(handlers);
					handlers.AddHandler(typeof(ContentView), typeof(ContentViewHandler));
				});
			});

			var footerA = new MeasureProbeView();
			footerA.Initialize("Footer A");
			var footerB = new MeasureProbeView();
			footerB.Initialize("Footer B");
			var shell = new Shell
			{
				FlyoutBehavior = FlyoutBehavior.Flyout,
				FlyoutFooter = footerA,
				CurrentItem = new FlyoutItem
				{
					Items =
					{
						new ContentPage()
					}
				}
			};

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async _ =>
			{
				await OnNavigatedToAsync(shell.CurrentPage);

				shell.FlyoutIsPresented = true;
				await footerA.WaitForMeasureAsync();
				shell.FlyoutIsPresented = false;

				shell.FlyoutFooter = footerB;
				await footerB.WaitForMeasureAsync();
				footerB.ResetMeasureCount();

				footerA.TriggerMeasureInvalidation();

				Assert.True(
					footerB.MeasureCount == 0,
					"Removed footer A should not measure footer B.");
			});
		}

		sealed class MeasureProbeView : ContentView
		{
			TaskCompletionSource<bool>? _measured;

			public void Initialize(string text)
			{
				Content = new Label
				{
					Padding = 12,
					Text = text
				};
				_measured = new(TaskCreationOptions.RunContinuationsAsynchronously);
			}

			public int MeasureCount { get; private set; }

			public void ResetMeasureCount() => MeasureCount = 0;

			public void TriggerMeasureInvalidation() => InvalidateMeasure();

			public Task WaitForMeasureAsync() =>
				_measured!.Task.WaitAsync(TimeSpan.FromSeconds(2));

			protected override Graphics.Size MeasureOverride(
				double widthConstraint,
				double heightConstraint)
			{
				if (!string.Equals(
					GetReplicationIssue(),
					IssueNumber,
					StringComparison.Ordinal))
				{
					return base.MeasureOverride(widthConstraint, heightConstraint);
				}

				MeasureCount++;
				var size = base.MeasureOverride(widthConstraint, heightConstraint);
				_measured!.TrySetResult(true);
				return size;
			}
		}
	}
}
