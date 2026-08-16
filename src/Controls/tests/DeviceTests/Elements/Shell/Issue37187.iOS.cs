#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using NavigationViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer;
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
		public async Task ReplacingFlyoutFooterDetachesMeasureInvalidation()
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
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler(typeof(Button), typeof(ButtonHandler));
					handlers.AddHandler(typeof(Entry), typeof(EntryHandler));
					handlers.AddHandler(typeof(ContentView), typeof(ContentViewHandler));
					handlers.AddHandler(typeof(ScrollView), typeof(ScrollViewHandler));
					handlers.AddHandler(typeof(CollectionView), typeof(CollectionViewHandler));
				});
			});

			var oldFooter = new MeasureProbeView
			{
				Content = new Label { Text = "Footer A" }
			};
			var currentFooter = new MeasureProbeView
			{
				Content = new Label { Text = "Footer B" }
			};
			var shell = new Shell
			{
				CurrentItem = new ContentPage { Content = new Label { Text = "Shell content" } },
				FlyoutBehavior = FlyoutBehavior.Flyout,
				FlyoutFooter = oldFooter
			};

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async _ =>
			{
				await OnNavigatedToAsync(shell.CurrentPage);
				shell.FlyoutIsPresented = true;
				await OnLoadedAsync(oldFooter);
				shell.FlyoutIsPresented = false;

				shell.FlyoutFooter = currentFooter;
				currentFooter.ArmMeasureProbe();
				oldFooter.TriggerMeasureInvalidation();

				Assert.False(
					currentFooter.WasMeasuredWhileArmed,
					"Replacing FlyoutFooter must detach the removed footer's measure invalidation callback.");
			});
		}

		sealed class MeasureProbeView : ContentView
		{
			bool _measureProbeArmed;

			public bool WasMeasuredWhileArmed { get; private set; }

			public void ArmMeasureProbe()
			{
				WasMeasuredWhileArmed = false;
				_measureProbeArmed = true;
			}

			public void TriggerMeasureInvalidation()
			{
				InvalidateMeasure();
			}

			protected override Microsoft.Maui.Graphics.Size MeasureOverride(double widthConstraint, double heightConstraint)
			{
				var size = base.MeasureOverride(widthConstraint, heightConstraint);
				if (_measureProbeArmed)
				{
					_measureProbeArmed = false;
					WasMeasuredWhileArmed = true;
				}

				return size;
			}
		}
	}
}
