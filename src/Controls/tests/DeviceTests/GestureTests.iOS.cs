using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Gesture)]
	public class GestureTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task UserInteractionEnabledTrueWhenInitializedWithGestureRecognizer()
		{
			var label = new Label();
			label.GestureRecognizers.Add(new TapGestureRecognizer() { NumberOfTapsRequired = 1 });

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);
				Assert.True(handler.PlatformView.UserInteractionEnabled);
			});
		}

#if MACCATALYST
		[Fact]
		public async Task InteractionsAreRemovedWhenGestureIsRemoved()
		{
			var label = new Label();
			label.GestureRecognizers.Add(new TapGestureRecognizer() { Buttons = ButtonsMask.Secondary });

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);

				var interactions =
					handler.PlatformView.Interactions.OfType<GestureManager.FakeRightClickContextMenuInteraction>()
						.ToList();

				Assert.Single(interactions);

				label.GestureRecognizers.RemoveAt(0);

				interactions =
					handler.PlatformView.Interactions.OfType<GestureManager.FakeRightClickContextMenuInteraction>()
						.ToList();

				Assert.Empty(interactions);
			});
		}

		[Fact]
		public async Task InteractionsAreRemovedWhenGestureButtonMaskChanged()
		{
			var label = new Label();
			label.GestureRecognizers.Add(new TapGestureRecognizer() { Buttons = ButtonsMask.Secondary });

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);

				var interactions =
					handler.PlatformView.Interactions.OfType<GestureManager.FakeRightClickContextMenuInteraction>()
						.ToList();

				Assert.Single(interactions);

				(label.GestureRecognizers[0] as TapGestureRecognizer).Buttons = ButtonsMask.Primary;

				interactions =
					handler.PlatformView.Interactions.OfType<GestureManager.FakeRightClickContextMenuInteraction>()
						.ToList();

				Assert.Empty(interactions);
			});
		}
#endif
	}
}
