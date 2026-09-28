#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.LifecycleEvents;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category("AppActions")]
	public class AppActionsHostConfigurationTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ConfigureTestBuilderDeliversAppActionAndCompletesOnce()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var lifecycleService = ApplicationServices.GetRequiredService<ILifecycleEventService>();
				var forwarder = Assert.Single(
					lifecycleService.GetEventDelegates<iOSLifecycle.PerformActionForShortcutItem>(
						nameof(iOSLifecycle.PerformActionForShortcutItem)));
				var originalShortcutItems = UIApplication.SharedApplication.ShortcutItems;
				var activationCount = 0;
				var completionCount = 0;
				bool? completionResult = null;
				string? receivedId = null;
				EventHandler<AppActionEventArgs> activated = (_, args) =>
				{
					activationCount++;
					receivedId = args.AppAction.Id;
				};
				AppActions.OnAppAction += activated;

				try
				{
					await AppActions.SetAsync(new AppAction("configured-host-action", "Configured Host Action"));
					var shortcutItems = UIApplication.SharedApplication.ShortcutItems;
					Assert.NotNull(shortcutItems);
					var shortcutItem = Assert.Single(shortcutItems);

					forwarder(
						UIApplication.SharedApplication,
						shortcutItem,
						handled =>
						{
							completionCount++;
							completionResult = handled;
						});

					Assert.Equal(1, activationCount);
					Assert.Equal("configured-host-action", receivedId);
					Assert.Equal(1, completionCount);
					Assert.Equal(true, completionResult);
				}
				finally
				{
					AppActions.OnAppAction -= activated;
					UIApplication.SharedApplication.ShortcutItems = originalShortcutItems;
				}
			});
		}
	}
}
