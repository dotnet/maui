using System;
using System.Threading.Tasks;
using Android.Content;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ContextFlyoutTests
	{
		[Fact]
		public async Task ShowContextFlyoutRequiresLoadedTarget()
		{
			SetupBuilder();
			await InvokeOnMainThreadAsync(() =>
			{
				var label = CreateContextFlyoutTarget();
				CreateHandler<LabelHandler>(label);
				Assert.Throws<InvalidOperationException>(() => FlyoutBase.ShowContextFlyout(label));
			});
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ProgrammaticPopupDoesNotChangeNativeGestures(bool longClickable)
		{
			SetupBuilder(handlers => handlers.AddHandler<Label, ContextMenuLabelHandler>());
			var label = CreateContextFlyoutTarget();
			await CreateHandlerAndAddToWindow<ContextMenuLabelHandler>(label, handler =>
			{
				var target = Assert.IsType<ContextMenuTextView>(handler.PlatformView);
				target.LongClickable = longClickable;
				try
				{
					FlyoutBase.SetContextFlyout(label, new MenuFlyout { new MenuFlyoutItem { Text = "Replacement action" } });
					FlyoutBase.ShowContextFlyout(label);
					Assert.Equal(longClickable, target.LongClickable);
					Assert.Equal(0, target.PresentationRequests);

					FlyoutBase.SetContextFlyout(label, null);
					Assert.Equal(longClickable, target.LongClickable);
					Assert.Equal(0, target.PresentationRequests);
				}
				finally
				{
					MauiContextMenu.Remove(target);
				}
			});
		}

		static Label CreateContextFlyoutTarget()
		{
			var label = new Label { Text = "Context menu", WidthRequest = 150, HeightRequest = 50 };
			FlyoutBase.SetContextFlyout(label, new MenuFlyout { new MenuFlyoutItem { Text = "Action" } });
			return label;
		}

		class ContextMenuLabelHandler : LabelHandler
		{
			protected override AppCompatTextView CreatePlatformView() => new ContextMenuTextView(Context);
		}

		class ContextMenuTextView : MauiTextView
		{
			public ContextMenuTextView(Context context) : base(context)
			{
			}

			internal int PresentationRequests { get; private set; }

			public override bool ShowContextMenu()
			{
				PresentationRequests++;
				throw new InvalidOperationException("Programmatic opening must not invoke native context-menu gestures.");
			}

			public override bool ShowContextMenu(float x, float y)
			{
				PresentationRequests++;
				throw new InvalidOperationException("Programmatic opening must not invoke native context-menu gestures.");
			}
		}
	}
}
