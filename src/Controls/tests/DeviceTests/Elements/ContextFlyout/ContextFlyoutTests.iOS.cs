#if IOS || MACCATALYST
using System;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
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

			await CreateHandlerAndAddToWindow<LabelHandler>(label, async handler =>
			{
				if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
					return;

				var presentingController = GetContextFlyoutPresentingController(handler.PlatformView);
				try
				{
					FlyoutBase.ShowContextFlyout(label);
					if (OperatingSystem.IsIOSVersionAtLeast(17, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
					{
						var button = Assert.Single(handler.PlatformView.Subviews.OfType<UIButton>());
						var section = Assert.IsType<UIMenu>(Assert.Single(button.Menu.Children));
						var nativeSubmenu = Assert.IsType<UIMenu>(Assert.Single(section.Children));
						Assert.NotNull(nativeSubmenu.Image);
						var nestedSection = Assert.IsType<UIMenu>(Assert.Single(nativeSubmenu.Children));
						var action = Assert.IsType<UIAction>(Assert.Single(nestedSection.Children));
						Assert.NotNull(action.Image);
						return;
					}

					await AssertEventually(() => presentingController.PresentedViewController is UINavigationController);
					var navigation = Assert.IsType<UINavigationController>(presentingController.PresentedViewController);
					navigation.View.LayoutIfNeeded();
					var root = Assert.IsAssignableFrom<UITableViewController>(navigation.TopViewController);
					await AssertCompactIconRow(root);

					using var path = NSIndexPath.FromRowSection(0, 0);
					root.RowSelected(root.TableView, path);
					navigation.View.LayoutIfNeeded();
					var nested = Assert.IsAssignableFrom<UITableViewController>(navigation.TopViewController);
					Assert.NotSame(root, nested);
					await AssertCompactIconRow(nested);
				}
				finally
				{
					ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
				}
			});
		}

		[Fact]
		public async Task ProgrammaticNativeMenuHonorsItemMapperCustomizations()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(17, 4) && !OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
				return;

			var originalMapper = MenuFlyoutItemHandler.Mapper;
			var clicks = 0;
			var mappings = 0;
			MenuFlyoutItemHandler.Mapper = new PropertyMapper<IMenuFlyoutItem, IMenuFlyoutItemHandler>(originalMapper)
			{
				["ContextFlyoutTestCustomization"] = (handler, _) =>
				{
					var action = Assert.IsType<UIAction>(handler.PlatformView);
					action.Title = "Customized action";
					action.Attributes |= UIMenuElementAttributes.Disabled;
					mappings++;
				}
			};
			try
			{
				SetupBuilder();
				var label = CreateContextFlyoutTarget();
				var item = new MenuFlyoutItem { Text = "Action" };
				item.Clicked += (_, _) => clicks++;
				FlyoutBase.SetContextFlyout(label, new MenuFlyout { item });
				await CreateHandlerAndAddToWindow<LabelHandler>(label, handler =>
				{
					try
					{
						FlyoutBase.ShowContextFlyout(label);
						var button = Assert.Single(handler.PlatformView.Subviews.OfType<UIButton>());
						var section = Assert.IsType<UIMenu>(Assert.Single(button.Menu.Children));
						var action = Assert.IsType<UIAction>(Assert.Single(section.Children));
						Assert.Equal(1, mappings);
						Assert.Equal("Customized action", action.Title);
						Assert.True(action.Attributes.HasFlag(UIMenuElementAttributes.Disabled));
						button.SendAction(action);
						Assert.Equal(0, clicks);
					}
					finally
					{
						ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
					}
				});
			}
			finally
			{
				MenuFlyoutItemHandler.Mapper = originalMapper;
			}
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ProgrammaticPresenterRemovedWhenPlainNativeTargetUnloads(bool buttonTarget)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
				return;

			SetupBuilder(handlers =>
			{
				handlers.AddHandler<Slider, SliderHandler>();
				handlers.AddHandler<Button, ButtonHandler>();
			});
			View target = buttonTarget
				? new Button { Text = "Context menu", WidthRequest = 150, HeightRequest = 50 }
				: new Slider { WidthRequest = 150, HeightRequest = 50 };
			FlyoutBase.SetContextFlyout(target, CreateContextFlyout());
			var layout = new Grid { Children = { target } };
			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async _ =>
			{
				var handler = Assert.IsAssignableFrom<IViewHandler>(target.Handler);
				var platformView = Assert.IsAssignableFrom<UIView>(handler.PlatformView);
				Assert.False(platformView is IUIViewLifeCycleEvents);
				var originalSubviewCount = platformView.Subviews.Length;
				var presentingController = GetContextFlyoutPresentingController(platformView);
				try
				{
					FlyoutBase.ShowContextFlyout(target);
					Assert.Single(platformView.Interactions.OfType<ProgrammaticContextFlyoutPresenter>());
					await Task.Yield();
					layout.Remove(target);
					await AssertEventually(() => !platformView.Interactions.OfType<ProgrammaticContextFlyoutPresenter>().Any(),
						message: "The presenter interaction must be removed after target unload.");
					await AssertEventually(() => platformView.Subviews.Length == originalSubviewCount,
						message: "The menu host and unload observer must be removed after target unload.");
					await AssertEventually(() => presentingController.PresentedViewController is null);
					Assert.Same(handler, target.Handler);
				}
				finally
				{
					ProgrammaticContextFlyoutPresenter.Remove(platformView);
				}
			});
		}

		static UIViewController GetContextFlyoutPresentingController(UIView target)
		{
			var controller = target.Window.RootViewController;
			Assert.NotNull(controller);
			while (controller.PresentedViewController is { } presented)
				controller = presented;
			return controller;
		}

		[Fact]
		public async Task ProgrammaticNativeMenuPreservesSectionsAndGuardsActions()
		{
			SetupBuilder();
			if (!OperatingSystem.IsIOSVersionAtLeast(17, 4) && !OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
				return;

			var clicks = 0;
			var nested = new MenuFlyoutItem { Text = "Nested action" };
			nested.Clicked += (_, _) => clicks++;
			var disabled = new MenuFlyoutItem { Text = "Disabled action", IsEnabled = false };
			disabled.Clicked += (_, _) => clicks++;
			var submenu = new MenuFlyoutSubItem { Text = "Submenu" };
			submenu.Add(nested);
			var disabledBranch = new MenuFlyoutSubItem { Text = "Disabled branch", IsEnabled = false };
			var disabledChild = new MenuFlyoutItem { Text = "Must not execute" };
			disabledChild.Clicked += (_, _) => clicks++;
			disabledBranch.Add(disabledChild);
			var label = CreateContextFlyoutTarget();
			FlyoutBase.SetContextFlyout(label, new MenuFlyout { submenu, new MenuFlyoutSeparator(), disabled, disabledBranch });

			await CreateHandlerAndAddToWindow<LabelHandler>(label, handler =>
			{
				try
				{
					FlyoutBase.ShowContextFlyout(label);
					var button = Assert.Single(handler.PlatformView.Subviews.OfType<UIButton>());
					Assert.Equal(UIContextMenuConfigurationElementOrder.Fixed, button.PreferredMenuElementOrder);
					Assert.False(button.UserInteractionEnabled);
					Assert.True(button.AccessibilityElementsHidden);
					FlyoutBase.ShowContextFlyout(label);
					Assert.Same(button, Assert.Single(handler.PlatformView.Subviews.OfType<UIButton>()));

					var sections = button.Menu.Children;
					Assert.Equal(2, sections.Length);
					var first = Assert.IsType<UIMenu>(sections[0]);
					var nativeSubmenu = Assert.IsType<UIMenu>(Assert.Single(first.Children));
					var nestedSection = Assert.IsType<UIMenu>(Assert.Single(nativeSubmenu.Children));
					var action = Assert.IsType<UIAction>(Assert.Single(nestedSection.Children));
					var last = Assert.IsType<UIMenu>(sections[1]);
					Assert.Equal(2, last.Children.Length);
					var disabledAction = Assert.IsType<UIAction>(last.Children[0]);
					Assert.True(disabledAction.Attributes.HasFlag(UIMenuElementAttributes.Disabled));
					var nativeDisabledBranch = Assert.IsType<UIMenu>(last.Children[1]);
					Assert.Equal("Disabled branch", nativeDisabledBranch.Title);
					var disabledSection = Assert.IsType<UIMenu>(Assert.Single(nativeDisabledBranch.Children));
					var disabledChildAction = Assert.IsType<UIAction>(Assert.Single(disabledSection.Children));
					Assert.True(disabledChildAction.Attributes.HasFlag(UIMenuElementAttributes.Disabled));

					button.SendAction(disabledAction);
					button.SendAction(disabledChildAction);
					submenu.IsEnabled = false;
					button.SendAction(action);
					Assert.Equal(0, clicks);

					submenu.IsEnabled = true;
					button.SendAction(action);
					button.SendAction(action);
					Assert.Equal(1, clicks);
				}
				finally
				{
					ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
				}
			});
		}

#if MACCATALYST
		[Fact]
		public async Task ProgrammaticMenuHostDoesNotExpandWithWideTarget()
		{
			SetupBuilder();
			if (!OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
				return;

			var label = CreateContextFlyoutTarget();
			await CreateHandlerAndAddToWindow<LabelHandler>(label, handler =>
			{
				var target = handler.PlatformView;
				var originalBounds = target.Bounds;
				try
				{
					target.Bounds = new CoreGraphics.CGRect(0, 0, 800, 100);
					FlyoutBase.ShowContextFlyout(label);
					var button = Assert.Single(target.Subviews.OfType<UIButton>());
					Assert.Equal(2d, (double)button.Bounds.Width);
					Assert.Equal(2d, (double)button.Bounds.Height);
					Assert.Equal((double)target.Bounds.GetMidX(), (double)button.Frame.GetMidX(), 1);
					Assert.Equal((double)target.Bounds.GetMaxY(), (double)button.Frame.GetMaxY(), 1);

					target.Bounds = new CoreGraphics.CGRect(0, 0, 1000, 150);
					Assert.Equal(2d, (double)button.Bounds.Width);
					Assert.Equal(2d, (double)button.Bounds.Height);
					Assert.Equal((double)target.Bounds.GetMidX(), (double)button.Frame.GetMidX(), 1);
					Assert.Equal((double)target.Bounds.GetMaxY(), (double)button.Frame.GetMaxY(), 1);
				}
				finally
				{
					ProgrammaticContextFlyoutPresenter.Remove(target);
					target.Bounds = originalBounds;
				}
			});
		}
#endif

		static async Task AssertCompactIconRow(UITableViewController page)
		{
			using var path = NSIndexPath.FromRowSection(0, 0);
			await AssertEventually(() =>
			{
				page.NavigationController?.View.LayoutIfNeeded();
				page.TableView.LayoutIfNeeded();
				var visibleCell = page.TableView.CellAt(path);
				if (visibleCell?.Window is null)
					return false;

				visibleCell.LayoutIfNeeded();
				var renderedContent = visibleCell.ContentView as UIListContentView
					?? visibleCell.ContentView.FindDescendantView<UIListContentView>();
				renderedContent?.LayoutIfNeeded();
				return renderedContent?.ImageLayoutGuide?.LayoutFrame.Width > 0 &&
					renderedContent?.TextLayoutGuide?.LayoutFrame.Width > 0;
			});
			var cell = page.TableView.CellAt(path);
			Assert.NotNull(cell);
			var content = cell.ContentView as UIListContentView
				?? cell.ContentView.FindDescendantView<UIListContentView>();
			Assert.NotNull(content);
			Assert.NotNull(content.ImageLayoutGuide);
			Assert.NotNull(content.TextLayoutGuide);
			var imageFrame = content.ImageLayoutGuide.LayoutFrame;
			Assert.InRange((double)imageFrame.Width, 0.1, 24.5);
			Assert.InRange((double)imageFrame.Height, 0.1, 24.5);
			Assert.InRange((double)cell.Bounds.Height, 1, 80);
			Assert.True(content.TextLayoutGuide.LayoutFrame.Width > 0);
		}

		[Fact]
		public async Task ProgrammaticPresentationPreservesNativeContextMenu()
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
