#if IOS || MACCATALYST
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue37187 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task RemovedFooterInvalidationDoesNotMeasureActiveFooter()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					SetupShellHandlers(handlers);
					handlers.AddHandler(typeof(Controls.ContentView), typeof(ContentViewHandler));
				});
			});

			var removedFooter = new MeasureProbeView
			{
				Content = new Label { Text = "Footer A" }
			};
			var activeFooter = new MeasureProbeView
			{
				Content = new Label { Text = "Footer B" }
			};
			var shell = new Shell
			{
				FlyoutBehavior = FlyoutBehavior.Flyout,
				FlyoutFooter = removedFooter,
				CurrentItem = new ContentPage
				{
					Content = new Label { Text = "Shell content" }
				}
			};

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async _ =>
			{
				shell.FlyoutIsPresented = true;
				await AssertEventually(() => removedFooter.MeasureCount > 0);
				shell.FlyoutIsPresented = false;

				shell.FlyoutFooter = activeFooter;
				Assert.NotNull(activeFooter.Handler);
				var measureCount = activeFooter.MeasureCount;

				removedFooter.TriggerMeasureInvalidation();

				Assert.True(
					activeFooter.MeasureCount == measureCount,
					"Removed footer invalidation must not measure the active footer.");
			});
		}

		sealed class MeasureProbeView : ContentView
		{
			public int MeasureCount { get; private set; }

			public void TriggerMeasureInvalidation() => InvalidateMeasure();

			protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
			{
				MeasureCount++;
				return base.MeasureOverride(widthConstraint, heightConstraint);
			}
		}
	}
}
#endif
