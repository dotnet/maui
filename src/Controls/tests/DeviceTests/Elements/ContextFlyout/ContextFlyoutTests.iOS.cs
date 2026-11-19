#if IOS || MACCATALYST
using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using ProgrammaticContextFlyoutPresenter = Microsoft.Maui.Platform.MauiContextFlyoutPopover;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ContextFlyoutTests
	{
		[Theory]
		[InlineData("big_white_horizontal.png")]
		[InlineData("blue.png")]
		public async Task ProgrammaticIconsDoNotExpandRootOrSubmenuRows(string image)
		{
			SetupBuilder();
			var submenu = new MenuFlyoutSubItem { Text = "Submenu", IconImageSource = image };
			submenu.Add(new MenuFlyoutItem { Text = "Nested action", IconImageSource = image });
			var label = CreateContextFlyoutTarget();
			FlyoutBase.SetContextFlyout(label, new MenuFlyout { submenu });

			await CreateHandlerAndAddToWindow<LabelHandler>(label, handler =>
			{
				if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
					return;

				try
				{
					FlyoutBase.ShowContextFlyout(label);
					var navigation = Assert.IsType<UINavigationController>(
						handler.PlatformView.Window.RootViewController.PresentedViewController);
					navigation.View.LayoutIfNeeded();
					var root = Assert.IsAssignableFrom<UITableViewController>(navigation.TopViewController);
					AssertCompactIconRow(root);

					using var path = NSIndexPath.FromRowSection(0, 0);
					root.RowSelected(root.TableView, path);
					navigation.View.LayoutIfNeeded();
					var nested = Assert.IsAssignableFrom<UITableViewController>(navigation.TopViewController);
					Assert.NotSame(root, nested);
					AssertCompactIconRow(nested);
				}
				finally
				{
					ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
				}
			});
		}

		static void AssertCompactIconRow(UITableViewController page)
		{
			page.TableView.LayoutIfNeeded();
			using var path = NSIndexPath.FromRowSection(0, 0);
			var cell = page.TableView.CellAt(path);
			Assert.NotNull(cell);
			cell.LayoutIfNeeded();
			var content = Assert.Single(cell.ContentView.Subviews.OfType<UIListContentView>());
			Assert.NotNull(content.ImageLayoutGuide);
			Assert.NotNull(content.TextLayoutGuide);
			var imageFrame = content.ImageLayoutGuide.LayoutFrame;
			Assert.InRange((double)imageFrame.Width, 0.1, 24.5);
			Assert.InRange((double)imageFrame.Height, 0.1, 24.5);
			Assert.InRange((double)cell.Bounds.Height, 1, 80);
			Assert.True(content.TextLayoutGuide.LayoutFrame.Width > 0);
		}

		[Fact]
		public async Task ProgrammaticPopoverPreservesNativeContextMenu()
		{
			SetupBuilder();
			var label = CreateContextFlyoutTarget();
			await CreateHandlerAndAddToWindow<LabelHandler>(label, handler =>
			{
				if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
					return;

				var platformView = handler.PlatformView;
				var nativeMenus = platformView.Interactions.OfType<MauiUIContextMenuInteraction>().ToArray();
#if MACCATALYST
				Assert.Single(nativeMenus);
#else
				Assert.Empty(nativeMenus);
#endif
				try
				{
					FlyoutBase.ShowContextFlyout(label);

					Assert.Single(platformView.Interactions.OfType<ProgrammaticContextFlyoutPresenter>());
					Assert.Equal(nativeMenus, platformView.Interactions.OfType<MauiUIContextMenuInteraction>().ToArray());
					Assert.Empty(platformView.Interactions.OfType<UIKit.UIEditMenuInteraction>());
				}
				finally
				{
					ProgrammaticContextFlyoutPresenter.Remove(platformView);
				}

				Assert.Equal(nativeMenus, platformView.Interactions.OfType<MauiUIContextMenuInteraction>().ToArray());
#if MACCATALYST
				Assert.NotNull(nativeMenus[0].GetConfigurationForMenu());
#endif
			});
		}

		[Fact]
		public async Task ShowContextFlyoutRequiresLoadedTarget()
		{
			SetupBuilder();
			await InvokeOnMainThreadAsync(() =>
			{
				var label = CreateContextFlyoutTarget();
				CreateHandler<LabelHandler>(label);
				if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsMacCatalystVersionAtLeast(15))
					Assert.Throws<InvalidOperationException>(() => FlyoutBase.ShowContextFlyout(label));
				else
					Assert.Throws<PlatformNotSupportedException>(() => FlyoutBase.ShowContextFlyout(label));
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ProgrammaticPresenterRemovedOnFlyoutChange(bool remove)
		{
			SetupBuilder();
			var label = CreateContextFlyoutTarget();
			await CreateHandlerAndAddToWindow<LabelHandler>(label, handler =>
			{
				if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
				{
					Assert.Throws<PlatformNotSupportedException>(() => FlyoutBase.ShowContextFlyout(label));
					return;
				}

				FlyoutBase.ShowContextFlyout(label);
				Assert.Single(handler.PlatformView.Interactions.OfType<ProgrammaticContextFlyoutPresenter>());

				FlyoutBase.SetContextFlyout(label, remove ? null : CreateContextFlyout());
				Assert.Empty(handler.PlatformView.Interactions.OfType<ProgrammaticContextFlyoutPresenter>());
				Assert.Equal(!remove && OperatingSystem.IsMacCatalyst() ? 1 : 0,
					handler.PlatformView.Interactions.OfType<MauiUIContextMenuInteraction>().Count());

				if (!remove)
				{
					FlyoutBase.ShowContextFlyout(label);
					Assert.Single(handler.PlatformView.Interactions.OfType<ProgrammaticContextFlyoutPresenter>());
				}
			});
		}

		[Fact]
		public async Task ProgrammaticPresenterRemovedOnHandlerDisconnect()
		{
			SetupBuilder();
			var label = CreateContextFlyoutTarget();
			await CreateHandlerAndAddToWindow<LabelHandler>(label, handler =>
			{
				if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
				{
					Assert.Throws<PlatformNotSupportedException>(() => FlyoutBase.ShowContextFlyout(label));
					return;
				}

				FlyoutBase.ShowContextFlyout(label);
				var platformView = handler.PlatformView;
				Assert.Single(platformView.Interactions.OfType<ProgrammaticContextFlyoutPresenter>());

				((IElementHandler)handler).DisconnectHandler();

				Assert.Empty(platformView.Interactions.OfType<ProgrammaticContextFlyoutPresenter>());
			});
		}

		[Fact]
		public async Task ProgrammaticPresenterRemovedOnTargetUnload()
		{
			SetupBuilder();
			var label = CreateContextFlyoutTarget();
			var layout = new Grid { Children = { label } };
			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async _ =>
			{
				if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
				{
					Assert.Throws<PlatformNotSupportedException>(() => FlyoutBase.ShowContextFlyout(label));
					return;
				}

				FlyoutBase.ShowContextFlyout(label);
				var platformView = ((LabelHandler)label.Handler).PlatformView;
				Assert.Single(platformView.Interactions.OfType<ProgrammaticContextFlyoutPresenter>());

				layout.Remove(label);

				await AssertEventually(() => !platformView.Interactions.OfType<ProgrammaticContextFlyoutPresenter>().Any());
			});
		}

		static Label CreateContextFlyoutTarget()
		{
			var label = new Label { Text = "Context menu", WidthRequest = 150, HeightRequest = 50 };
			FlyoutBase.SetContextFlyout(label, CreateContextFlyout());
			return label;
		}

		static MenuFlyout CreateContextFlyout() => new MenuFlyout
		{
			new MenuFlyoutItem { Text = "Action" }
		};
	}
}
#endif
