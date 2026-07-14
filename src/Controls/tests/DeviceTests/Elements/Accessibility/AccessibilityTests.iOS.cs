using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	// Regression tests for https://github.com/dotnet/maui/issues/34380
	// [iOS] VoiceOver does not correctly describe a View with GestureRecognizers when it has
	// SemanticProperties.Hint and child Labels — should synthesize an accessibility label from
	// children, promote the layout to an accessibility element, and route VoiceOver activation
	// to the layout's TapGestureRecognizer.
	public partial class AccessibilityTests
	{
		[Category(TestCategory.Accessibility)]
		[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
		public class Issue34380Tests : ControlsHandlerTestBase
		{
			void SetupBuilder()
			{
				EnsureHandlerCreated(builder =>
				{
					builder.ConfigureMauiHandlers(handlers =>
					{
						handlers.AddMauiControlsHandlers();
						handlers.AddHandler(typeof(Window), typeof(WindowHandlerStub));
					});
				});
			}

			[Fact("Layout with Hint and child Labels synthesizes a combined AccessibilityLabel")]
			public async Task LayoutWithHintAndChildLabels_SynthesizesAccessibilityLabel()
			{
				SetupBuilder();

				var layout = new VerticalStackLayout();
				layout.Add(new Label { Text = "First line" });
				layout.Add(new Label { Text = "Second line" });
				SemanticProperties.SetHint(layout, "Activates the action");

				var page = new ContentPage { Content = layout };

				await CreateHandlerAndAddToWindow<IWindowHandler>(page, async (handler) =>
				{
					await Task.Delay(100);

					var platformView = (UIView)layout.Handler.PlatformView;

					Assert.True(platformView.IsAccessibilityElement,
						"Layout with Hint should be promoted to an accessibility element.");

					Assert.False(string.IsNullOrEmpty(platformView.AccessibilityLabel),
						"AccessibilityLabel should be synthesized from child Labels.");

					Assert.Contains("First line", platformView.AccessibilityLabel, StringComparison.Ordinal);
					Assert.Contains("Second line", platformView.AccessibilityLabel, StringComparison.Ordinal);

					Assert.Equal("Activates the action", platformView.AccessibilityHint);
				});
			}

			[Fact("Layout with TapGestureRecognizer (no Hint) sets ShouldGroupAccessibilityChildren")]
			public async Task LayoutWithTapGesture_SetsShouldGroupAccessibilityChildren()
			{
				SetupBuilder();

				var layout = new VerticalStackLayout();
				layout.Add(new Label { Text = "Tap me" });

				var tap = new TapGestureRecognizer();
				layout.GestureRecognizers.Add(tap);

				var page = new ContentPage { Content = layout };

				await CreateHandlerAndAddToWindow<IWindowHandler>(page, async (handler) =>
				{
					await Task.Delay(100);

					var platformView = (UIView)layout.Handler.PlatformView;

					Assert.True(platformView.ShouldGroupAccessibilityChildren,
						"Layout with TapGestureRecognizer should group accessibility children so the tap target is reachable by VoiceOver.");
				});
			}

			[Fact("AccessibilityActivate on a layout with TapGestureRecognizer fires the gesture")]
			public async Task AccessibilityActivate_InvokesTapGesture()
			{
				SetupBuilder();

				var layout = new VerticalStackLayout();
				layout.Add(new Label { Text = "Tap" });
				SemanticProperties.SetHint(layout, "Tap to act");

				bool tapped = false;
				var tap = new TapGestureRecognizer();
				tap.Tapped += (s, e) => tapped = true;
				layout.GestureRecognizers.Add(tap);

				var page = new ContentPage { Content = layout };

				await CreateHandlerAndAddToWindow<IWindowHandler>(page, async (handler) =>
				{
					await Task.Delay(100);

					var platformView = (UIView)layout.Handler.PlatformView;
					var activated = platformView.AccessibilityActivate();

					Assert.True(activated, "AccessibilityActivate should report that the activation was handled.");
					Assert.True(tapped, "TapGestureRecognizer.Tapped should fire when VoiceOver activates the layout.");
				});
			}
		}
	}
}

