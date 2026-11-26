using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using UIKit;
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

		[Fact]
		public async Task HandlerWithoutGesturesDoesNotReadInteractions()
		{
			var view = new CountingElement();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CountingHandler>(view);
				var platformView = (CountingPlatformView)handler.PlatformView;

				Assert.Equal(0, platformView.Reads);

				view.GestureRecognizers.Add(new TapGestureRecognizer());

				Assert.Equal(0, platformView.Reads);

				((IElementHandler)handler).DisconnectHandler();
			});
		}

		[Fact]
		public async Task DragInteractionIsRecreatedAfterNativeRemoval()
		{
			var label = new Label();
			label.GestureRecognizers.Add(new DragGestureRecognizer());

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);
				var platformView = handler.PlatformView;

				platformView.RemoveInteraction(Assert.Single(platformView.Interactions.OfType<UIDragInteraction>()));

				label.GestureRecognizers.Add(new TapGestureRecognizer());

				Assert.Single(platformView.Interactions.OfType<UIDragInteraction>());
			});
		}

		class CountingElement : View
		{
		}

		class CountingPlatformView : UIView
		{
			public int Reads { get; private set; }

			public override IUIInteraction[] Interactions
			{
				get
				{
					Reads++;
					return base.Interactions;
				}
			}
		}

		class CountingHandler : ViewHandler<CountingElement, CountingPlatformView>
		{
			public CountingHandler() : base(ViewMapper)
			{
			}

			protected override CountingPlatformView CreatePlatformView() => new CountingPlatformView();
		}

#if MACCATALYST
		[Fact]
		public async Task SecondaryTapInteractionIsRecreatedAfterNativeRemoval()
		{
			var label = new Label();
			var tap = new TapGestureRecognizer() { Buttons = ButtonsMask.Secondary };
			label.GestureRecognizers.Add(tap);

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);
				var first = Assert.Single(handler.PlatformView.Interactions.OfType<GesturePlatformManager.FakeRightClickContextMenuInteraction>());

				handler.PlatformView.RemoveInteraction((IUIInteraction)first);
				label.GestureRecognizers.Add(new TapGestureRecognizer());

				var recreated = Assert.Single(handler.PlatformView.Interactions.OfType<GesturePlatformManager.FakeRightClickContextMenuInteraction>());
				Assert.NotSame(first, recreated);

				tap.Buttons = ButtonsMask.Primary;

				Assert.Empty(handler.PlatformView.Interactions.OfType<GesturePlatformManager.FakeRightClickContextMenuInteraction>());
			});
		}

		[Fact(
#if MACCATALYST
			Skip = "Fails on Mac Catalyst, fixme"
#endif
			)]
		public async Task InteractionsAreRemovedWhenGestureIsRemoved()
		{
			var label = new Label();
			label.GestureRecognizers.Add(new TapGestureRecognizer() { Buttons = ButtonsMask.Secondary });

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);

				var interactions =
					handler.PlatformView.Interactions.OfType<GesturePlatformManager.FakeRightClickContextMenuInteraction>()
						.ToList();

				Assert.Single(interactions);

				label.GestureRecognizers.RemoveAt(0);

				interactions =
					handler.PlatformView.Interactions.OfType<GesturePlatformManager.FakeRightClickContextMenuInteraction>()
						.ToList();

				Assert.Empty(interactions);
			});
		}

		[Fact(
#if MACCATALYST
			Skip = "Fails on Mac Catalyst, fixme"
#endif
			)]
		public async Task InteractionsAreRemovedWhenGestureButtonMaskChanged()
		{
			var label = new Label();
			label.GestureRecognizers.Add(new TapGestureRecognizer() { Buttons = ButtonsMask.Secondary });

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);

				var interactions =
					handler.PlatformView.Interactions.OfType<GesturePlatformManager.FakeRightClickContextMenuInteraction>()
						.ToList();

				Assert.Single(interactions);

				(label.GestureRecognizers[0] as TapGestureRecognizer).Buttons = ButtonsMask.Primary;

				interactions =
					handler.PlatformView.Interactions.OfType<GesturePlatformManager.FakeRightClickContextMenuInteraction>()
						.ToList();

				Assert.Empty(interactions);
			});
		}
#endif

		[Theory]
		[InlineData(1, 2)]
		[InlineData(2, 3)]
		[InlineData(3, 1)]
		[InlineData(1, 3)]
		public async Task ChangingNumberOfTapsRequiredUpdatesNativeGestureRecognizerForSpan(int initialTaps, int updatedTaps)
		{
			var tapGestureRecognizer = new TapGestureRecognizer { NumberOfTapsRequired = initialTaps };

			var span = new Span { Text = "Tap me" };
			span.GestureRecognizers.Add(tapGestureRecognizer);

			var label = new Label
			{
				FormattedText = new FormattedString
				{
					Spans = { span }
				}
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);

				UITapGestureRecognizer GetNativeTapGestureRecognizer() =>
					handler.PlatformView.GestureRecognizers?.OfType<UITapGestureRecognizer>().FirstOrDefault();

				Assert.Equal((nuint)initialTaps, GetNativeTapGestureRecognizer().NumberOfTapsRequired);

				tapGestureRecognizer.NumberOfTapsRequired = updatedTaps;

				Assert.Equal((nuint)updatedTaps, GetNativeTapGestureRecognizer().NumberOfTapsRequired);
			});
		}
	}
}
