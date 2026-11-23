using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Android.Content;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using AView = Android.Views.View;

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

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		public async Task ProgrammaticPopupShowsAndRejectsRemovedItems(bool clear, bool nested)
		{
			SetupBuilder();
			var clicks = 0;
			var commandCalls = 0;
			var item = new MenuFlyoutItem
			{
				Text = "Action",
				Command = new Command(() => commandCalls++)
			};
			item.Clicked += (_, _) => clicks++;
			var submenu = new MenuFlyoutSubItem { Text = "Submenu" };
			submenu.Add(item);
			var flyout = new MenuFlyout();
			flyout.Add(nested ? submenu : item);
			var label = CreateContextFlyoutTarget();
			FlyoutBase.SetContextFlyout(label, flyout);
			await CreateHandlerAndAddToWindow<LabelHandler>(label, async handler =>
			{
				try
				{
					FlyoutBase.ShowContextFlyout(label);
					var popup = GetProgrammaticPopup(handler.PlatformView);
					await AssertEventually(() => IsPopupShowing(popup),
						message: "The native Android popup must actually be showing.");
					var menu = nested ? popup.Menu.GetItem(0).SubMenu : popup.Menu;
					Assert.NotNull(menu);
					var nativeItem = menu.GetItem(0);
					Assert.NotNull(nativeItem);
					Assert.True(menu.PerformIdentifierAction(nativeItem.ItemId, 0));
					Assert.Equal(1, clicks);
					Assert.Equal(1, commandCalls);

					MauiContextMenu.Remove(handler.PlatformView);
					FlyoutBase.ShowContextFlyout(label);
					popup = GetProgrammaticPopup(handler.PlatformView);
					await AssertEventually(() => IsPopupShowing(popup));
					menu = nested ? popup.Menu.GetItem(0).SubMenu : popup.Menu;
					nativeItem = menu.GetItem(0);
					if (clear)
						flyout.Clear();
					else if (nested)
						flyout.Remove(submenu);
					else
						flyout.Remove(item);

					menu.PerformIdentifierAction(nativeItem.ItemId, 0);
					Assert.Equal(1, clicks);
					Assert.Equal(1, commandCalls);
				}
				finally
				{
					MauiContextMenu.Remove(handler.PlatformView);
				}
			});
		}

		static PopupMenu GetProgrammaticPopup(AView target)
		{
			var registryField = typeof(MauiContextMenu).GetField("s_menus", BindingFlags.Static | BindingFlags.NonPublic);
			Assert.NotNull(registryField);
			var registry = Assert.IsType<ConditionalWeakTable<AView, MauiContextMenu>>(registryField.GetValue(null));
			Assert.True(registry.TryGetValue(target, out var presenter));
			var popupField = typeof(MauiContextMenu).GetField("_popup", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(popupField);
			return Assert.IsType<PopupMenu>(popupField.GetValue(presenter));
		}

		static bool IsPopupShowing(PopupMenu popup)
		{
			// PopupMenu has no public visibility API; observe AppCompat's actual popup helper.
			using var field = popup.Class.GetDeclaredField("mPopup");
			field.Accessible = true;
			using var helper = field.Get(popup);
			Assert.NotNull(helper);
			using var method = helper.Class.GetMethod("isShowing");
			using var result = method.Invoke(helper);
			return Assert.IsType<Java.Lang.Boolean>(result).BooleanValue();
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
