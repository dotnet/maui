using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ContextFlyoutTests
	{
		[Fact]
		public async Task ContextFlyoutUpdatesNativeInteraction()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var label = new Label();
				var handler = CreateHandler<LabelHandler>(label);
				var platformView = handler.PlatformView;

				Assert.Empty(platformView.Interactions.OfType<MauiUIContextMenuInteraction>());

				var flyout = new MenuFlyout
				{
					new MenuFlyoutItem { Text = "Action" }
				};
				FlyoutBase.SetContextFlyout(label, flyout);

				var interaction = Assert.Single(platformView.Interactions.OfType<MauiUIContextMenuInteraction>());
				Assert.NotNull(interaction.GetConfigurationForMenu());

				FlyoutBase.SetContextFlyout(label, new MenuFlyout());
				Assert.Same(interaction, Assert.Single(platformView.Interactions.OfType<MauiUIContextMenuInteraction>()));

				FlyoutBase.SetContextFlyout(label, null);
				Assert.Empty(platformView.Interactions.OfType<MauiUIContextMenuInteraction>());
			});
		}
	}
}
