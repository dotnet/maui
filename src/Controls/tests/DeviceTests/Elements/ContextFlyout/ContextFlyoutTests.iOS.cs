#if IOS || MACCATALYST
using System;
using System.Linq;
using System.Runtime.CompilerServices;
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

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ProgrammaticNativeMenuHonorsItemMapperCustomizations(bool initializationKey)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(17, 4) && !OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
				return;

			var originalMapper = MenuFlyoutItemHandler.Mapper;
			var clicks = 0;
			var mappings = 0;
			var mapper = new PropertyMapper<IMenuFlyoutItem, IMenuFlyoutItemHandler>(originalMapper);
			mapper.AppendToMapping(initializationKey ? "CustomTitle" : nameof(IMenuElement.Text), (handler, _) =>
			{
				var action = Assert.IsType<UIAction>(handler.PlatformView);
				action.Title = "Customized action";
				mappings++;
			});
			mapper.AppendToMapping(nameof(IMenuElement.IsEnabled), (handler, _) =>
				Assert.IsType<UIAction>(handler.PlatformView).Attributes |= UIMenuElementAttributes.Disabled);
			MenuFlyoutItemHandler.Mapper = mapper;
			try
			{
				SetupBuilder();
				var label = CreateContextFlyoutTarget();
				var item = new MenuFlyoutItem { Text = "Action" };
				item.Clicked += (_, _) => clicks++;
				FlyoutBase.SetContextFlyout(label, new MenuFlyout { item });
				await CreateHandlerAndAddToWindow<LabelHandler>(label, async handler =>
				{
					try
					{
						FlyoutBase.ShowContextFlyout(label);
						var button = Assert.Single(handler.PlatformView.Subviews.OfType<UIButton>());
						var section = Assert.IsType<UIMenu>(Assert.Single(button.Menu.Children));
						var action = Assert.IsType<UIAction>(Assert.Single(section.Children));
						Assert.True(mappings > 0);
						Assert.Equal("Customized action", action.Title);
						Assert.True(action.Attributes.HasFlag(UIMenuElementAttributes.Disabled));
						button.SendAction(action);
						Assert.Equal(0, clicks);

						ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
						await AssertEventually(() => !handler.PlatformView.Subviews.OfType<UIButton>().Any());
						var originalHandler = item.Handler;
						if (!initializationKey)
							item.Text = "Changed action";
						item.IconImageSource = "blue.png";
						FlyoutBase.ShowContextFlyout(label);
						button = Assert.Single(handler.PlatformView.Subviews.OfType<UIButton>());
						section = Assert.IsType<UIMenu>(Assert.Single(button.Menu.Children));
						action = Assert.IsType<UIAction>(Assert.Single(section.Children));
						Assert.Same(originalHandler, item.Handler);
						Assert.Equal("Customized action", action.Title);
						Assert.NotNull(action.Image);
						Assert.True(action.Attributes.HasFlag(UIMenuElementAttributes.Disabled));
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

		[Fact]
		public async Task ProgrammaticNativeMenuTextMappingDoesNotAccumulate()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(17, 4) && !OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
				return;

			var originalMapper = MenuFlyoutItemHandler.Mapper;
			var mapper = new PropertyMapper<IMenuFlyoutItem, IMenuFlyoutItemHandler>(originalMapper);
			mapper.AppendToMapping(nameof(IMenuElement.Text), (handler, _) =>
				Assert.IsType<UIAction>(handler.PlatformView).Title += "!");
			MenuFlyoutItemHandler.Mapper = mapper;
			try
			{
				SetupBuilder();
				var item = new MenuFlyoutItem { Text = "Action" };
				var label = CreateContextFlyoutTarget();
				FlyoutBase.SetContextFlyout(label, new MenuFlyout { item });
				await CreateHandlerAndAddToWindow<LabelHandler>(label, async handler =>
				{
					try
					{
						for (int opening = 0; opening < 2; opening++)
						{
							FlyoutBase.ShowContextFlyout(label);
							var button = Assert.Single(handler.PlatformView.Subviews.OfType<UIButton>());
							var section = Assert.IsType<UIMenu>(Assert.Single(button.Menu.Children));
							Assert.Equal("Action!", Assert.IsType<UIAction>(Assert.Single(section.Children)).Title);
							ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
							await AssertEventually(() => !handler.PlatformView.Subviews.OfType<UIButton>().Any());
						}
						item.Text = "Changed";
						Assert.Equal("Changed!", Assert.IsType<UIAction>(item.Handler.PlatformView).Title);
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

		[Fact]
		public async Task ProgrammaticNativeMenuRebindingUsesCurrentItem()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(17, 4) && !OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
				return;

			SetupBuilder();
			var originalClicks = 0;
			var currentClicks = 0;
			var original = new MenuFlyoutItem { Text = "Original" };
			original.Clicked += (_, _) => originalClicks++;
			var current = new MenuFlyoutItem { Text = "Current" };
			current.Clicked += (_, _) => currentClicks++;
			var label = CreateContextFlyoutTarget();
			FlyoutBase.SetContextFlyout(label, new MenuFlyout { original });
			await CreateHandlerAndAddToWindow<LabelHandler>(label, async handler =>
			{
				try
				{
					FlyoutBase.ShowContextFlyout(label);
					var itemHandler = original.Handler;
					var nativeAction = itemHandler.PlatformView;
					ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
					await AssertEventually(() => !handler.PlatformView.Subviews.OfType<UIButton>().Any());
					FlyoutBase.SetContextFlyout(label, new MenuFlyout { current });
					itemHandler.SetVirtualView(current);
					Assert.Same(nativeAction, itemHandler.PlatformView);
					FlyoutBase.ShowContextFlyout(label);
					var button = Assert.Single(handler.PlatformView.Subviews.OfType<UIButton>());
					var section = Assert.IsType<UIMenu>(Assert.Single(button.Menu.Children));
					button.SendAction(Assert.IsType<UIAction>(Assert.Single(section.Children)));
					Assert.Equal(0, originalClicks);
					Assert.Equal(1, currentClicks);
					GC.KeepAlive(original);
				}
				finally
				{
					ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
				}
			});
		}

		[Fact]
		public async Task ProgrammaticNativeMenuReopensAfterNativeDismissal()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(17, 4) && !OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
				return;

			SetupBuilder();
			var label = CreateContextFlyoutTarget();
			await CreateHandlerAndAddToWindow<LabelHandler>(label, async handler =>
			{
				try
				{
					for (int opening = 0; opening < 2; opening++)
					{
						FlyoutBase.ShowContextFlyout(label);
						var presenter = Assert.Single(handler.PlatformView.Interactions.OfType<ProgrammaticContextFlyoutPresenter>());
						await AssertEventually(() => presenter.IsNativeMenuPresented);
						var button = Assert.Single(handler.PlatformView.Subviews.OfType<UIButton>());
						Assert.NotNull(button.ContextMenuInteraction);
						button.ContextMenuInteraction.DismissMenu();
						await AssertEventually(() => !handler.PlatformView.Subviews.OfType<UIButton>().Any(),
							message: "Native dismissal must remove the menu host without calling Remove.");
						await AssertEventually(() => !handler.PlatformView.Interactions.OfType<ProgrammaticContextFlyoutPresenter>().Any());
					}
				}
				finally
				{
					ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
				}
			});
		}

		[Theory]
		[InlineData("File")]
		[InlineData("Edit")]
		public async Task ProgrammaticNativeMenuOpensReservedSubmenuAndRefreshesMetadata(string title)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(17, 4) && !OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
				return;

			SetupBuilder();
			var item = new MenuFlyoutItem { Text = "Before", IconImageSource = "blue.png" };
			var submenu = new MenuFlyoutSubItem { Text = title, IconImageSource = "blue.png" };
			submenu.Add(item);
			var label = CreateContextFlyoutTarget();
			FlyoutBase.SetContextFlyout(label, new MenuFlyout { submenu });
			await CreateHandlerAndAddToWindow<LabelHandler>(label, async handler =>
			{
				try
				{
					Assert.Null(submenu.Handler);
					FlyoutBase.ShowContextFlyout(label);
					var presenter = Assert.Single(handler.PlatformView.Interactions.OfType<ProgrammaticContextFlyoutPresenter>());
					await AssertEventually(() => presenter.IsNativeMenuPresented,
						message: "UIKit must report native menu presentation.");
					var button = Assert.Single(handler.PlatformView.Subviews.OfType<UIButton>());
					var section = Assert.IsType<UIMenu>(Assert.Single(button.Menu.Children));
					var nativeSubmenu = Assert.IsType<UIMenu>(Assert.Single(section.Children));
					Assert.Equal(title, nativeSubmenu.Title);
					Assert.NotNull(nativeSubmenu.Image);
					var nestedSection = Assert.IsType<UIMenu>(Assert.Single(nativeSubmenu.Children));
					var action = Assert.IsType<UIAction>(Assert.Single(nestedSection.Children));
					Assert.Equal("Before", action.Title);
					Assert.NotNull(action.Image);

					ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
					await AssertEventually(() => !handler.PlatformView.Subviews.OfType<UIButton>().Any(),
						message: "Explicit dismissal must remove the native menu host before reopening.");
					var itemHandler = item.Handler;
					var submenuHandler = submenu.Handler;
					item.Text = "After";
					item.IconImageSource = null;
					submenu.Text = "Renamed";
					submenu.IconImageSource = null;

					FlyoutBase.ShowContextFlyout(label);
					presenter = Assert.Single(handler.PlatformView.Interactions.OfType<ProgrammaticContextFlyoutPresenter>());
					await AssertEventually(() => presenter.IsNativeMenuPresented);
					Assert.Same(itemHandler, item.Handler);
					Assert.Same(submenuHandler, submenu.Handler);
					button = Assert.Single(handler.PlatformView.Subviews.OfType<UIButton>());
					section = Assert.IsType<UIMenu>(Assert.Single(button.Menu.Children));
					nativeSubmenu = Assert.IsType<UIMenu>(Assert.Single(section.Children));
					Assert.Equal("Renamed", nativeSubmenu.Title);
					Assert.Null(nativeSubmenu.Image);
					nestedSection = Assert.IsType<UIMenu>(Assert.Single(nativeSubmenu.Children));
					action = Assert.IsType<UIAction>(Assert.Single(nestedSection.Children));
					Assert.Equal("After", action.Title);
					Assert.Null(action.Image);
				}
				finally
				{
					ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
				}
			});
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		public async Task ProgrammaticAppleMenuRejectsRemovedItems(bool clear, bool nested)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
				return;

			SetupBuilder();
			var clicks = 0;
			var item = new MenuFlyoutItem { Text = "Action" };
			item.Clicked += (_, _) => clicks++;
			var submenu = new MenuFlyoutSubItem { Text = "Group" };
			submenu.Add(item);
			var flyout = new MenuFlyout();
			flyout.Add(nested ? submenu : item);
			var label = CreateContextFlyoutTarget();
			FlyoutBase.SetContextFlyout(label, flyout);
			await CreateHandlerAndAddToWindow<LabelHandler>(label, async handler =>
			{
				var controller = GetContextFlyoutPresentingController(handler.PlatformView);
				try
				{
					FlyoutBase.ShowContextFlyout(label);
					var button = handler.PlatformView.Subviews.OfType<UIButton>().SingleOrDefault();
					UITableViewController page = null;
					UIAction action = null;
					if (button is not null)
					{
						var section = Assert.IsType<UIMenu>(Assert.Single(button.Menu.Children));
						if (nested)
						{
							var branch = Assert.IsType<UIMenu>(Assert.Single(section.Children));
							section = Assert.IsType<UIMenu>(Assert.Single(branch.Children));
						}
						action = Assert.IsType<UIAction>(Assert.Single(section.Children));
					}
					else
					{
						await AssertEventually(() => controller.PresentedViewController is UINavigationController);
						var navigation = Assert.IsType<UINavigationController>(controller.PresentedViewController);
						page = Assert.IsAssignableFrom<UITableViewController>(navigation.TopViewController);
						if (nested)
						{
							using var branchPath = NSIndexPath.FromRowSection(0, 0);
							page.RowSelected(page.TableView, branchPath);
							page = Assert.IsAssignableFrom<UITableViewController>(navigation.TopViewController);
						}
					}

					if (clear)
						flyout.Clear();
					else
						flyout.Remove(nested ? submenu : item);
					if (button is not null)
						button.SendAction(action);
					else
					{
						using var path = NSIndexPath.FromRowSection(0, 0);
						page.RowSelected(page.TableView, path);
					}
					Assert.Equal(0, clicks);
				}
				finally
				{
					ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
				}
			});
		}

		[Fact]
		public async Task ProgrammaticFallbackHonorsMapperAndDispatchesLeaf()
		{
			if ((!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15)) ||
				OperatingSystem.IsIOSVersionAtLeast(17, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
				return;

			var originalMapper = MenuFlyoutItemHandler.Mapper;
			MenuFlyoutItemHandler.Mapper = new PropertyMapper<IMenuFlyoutItem, IMenuFlyoutItemHandler>(originalMapper)
			{
				["FallbackCustomization"] = (handler, view) =>
				{
					var action = Assert.IsType<UIAction>(handler.PlatformView);
					if (view.Text == "Mapper disabled")
						action.Attributes |= UIMenuElementAttributes.Disabled;
					else if (view.Text == "Mapper hidden")
						action.Attributes |= UIMenuElementAttributes.Hidden;
					else
						action.Title = "Custom enabled title";
				}
			};
			try
			{
				SetupBuilder();
				var clicks = 0;
				var commands = 0;
				var enabled = new MenuFlyoutItem { Text = "Enabled", Command = new Command(() => commands++) };
				var disabled = new MenuFlyoutItem { Text = "Mapper disabled" };
				var hidden = new MenuFlyoutItem { Text = "Mapper hidden" };
				enabled.Clicked += (_, _) => clicks++;
				disabled.Clicked += (_, _) => clicks++;
				hidden.Clicked += (_, _) => clicks++;
				var label = CreateContextFlyoutTarget();
				FlyoutBase.SetContextFlyout(label, new MenuFlyout
				{
					hidden, new MenuFlyoutSeparator(), enabled, disabled
				});
				await CreateHandlerAndAddToWindow<LabelHandler>(label, async handler =>
				{
					var controller = GetContextFlyoutPresentingController(handler.PlatformView);
					try
					{
						UITableViewController page = null;
						for (int opening = 0; opening < 2; opening++)
						{
							FlyoutBase.ShowContextFlyout(label);
							await AssertEventually(() => controller.PresentedViewController is UINavigationController);
							var navigation = Assert.IsType<UINavigationController>(controller.PresentedViewController);
							page = Assert.IsAssignableFrom<UITableViewController>(navigation.TopViewController);
							Assert.Equal((nint)1, page.NumberOfSections(page.TableView));
							Assert.Equal((nint)2, page.RowsInSection(page.TableView, 0));
							using var titlePath = NSIndexPath.FromRowSection(0, 0);
							using var cell = page.GetCell(page.TableView, titlePath);
							Assert.Equal("Custom enabled title", cell.AccessibilityLabel);
							if (opening == 0)
							{
								ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
								await AssertEventually(() => controller.PresentedViewController is null);
							}
						}
						Assert.NotNull(page);
						using var disabledPath = NSIndexPath.FromRowSection(1, 0);
						using var disabledCell = page.GetCell(page.TableView, disabledPath);
						Assert.False(disabledCell.UserInteractionEnabled);
						page.RowSelected(page.TableView, disabledPath);
						Assert.Equal(0, clicks);
						Assert.Equal(0, commands);
						using var enabledPath = NSIndexPath.FromRowSection(0, 0);
						page.RowSelected(page.TableView, enabledPath);
						await AssertEventually(() => clicks == 1 && commands == 1);
						await AssertEventually(() => controller.PresentedViewController is null);
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

		[Fact]
		public async Task ProgrammaticNativeMenuRefreshesMutatedFileSource()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(17, 4) && !OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
				return;

			SetupBuilder();
			var source = new FileImageSource { File = "blue.png" };
			var item = new MenuFlyoutItem { Text = "Action", IconImageSource = source };
			var submenu = new MenuFlyoutSubItem { Text = "Group", IconImageSource = source };
			submenu.Add(item);
			var label = CreateContextFlyoutTarget();
			FlyoutBase.SetContextFlyout(label, new MenuFlyout { submenu });
			await CreateHandlerAndAddToWindow<LabelHandler>(label, async handler =>
			{
				try
				{
					FlyoutBase.ShowContextFlyout(label);
					var originalImage = Assert.IsType<UIAction>(item.Handler.PlatformView).Image;
					Assert.NotNull(originalImage);
					var originalSize = originalImage.Size;
					ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
					await AssertEventually(() => !handler.PlatformView.Subviews.OfType<UIButton>().Any());
					source.File = "big_white_horizontal.png";
					FlyoutBase.ShowContextFlyout(label);
					var button = Assert.Single(handler.PlatformView.Subviews.OfType<UIButton>());
					var section = Assert.IsType<UIMenu>(Assert.Single(button.Menu.Children));
					var branch = Assert.IsType<UIMenu>(Assert.Single(section.Children));
					var children = Assert.IsType<UIMenu>(Assert.Single(branch.Children));
					var action = Assert.IsType<UIAction>(Assert.Single(children.Children));
					using var expected = UIImage.FromBundle(source.File);
					Assert.NotNull(expected);
					Assert.NotEqual(originalSize, expected.Size);
					Assert.NotNull(branch.Image);
					Assert.NotNull(action.Image);
					Assert.Equal(expected.Size, branch.Image.Size);
					Assert.Equal(expected.Size, action.Image.Size);
					Assert.Same(source, item.IconImageSource);
					Assert.Same(source, submenu.IconImageSource);
				}
				finally
				{
					ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
				}
			});
		}

		[Fact]
		public async Task ProgrammaticNativeMenuDoesNotRetainDetachedItems()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(17, 4) && !OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
				return;

			SetupBuilder();
			var label = CreateContextFlyoutTarget();
			await CreateHandlerAndAddToWindow<LabelHandler>(label, async handler =>
			{
				var probe = CreateNativeMenuRetentionProbe(label);
				try
				{
					FlyoutBase.SetContextFlyout(label, null);
					await AssertEventually(() => !handler.PlatformView.Subviews.OfType<UIButton>().Any());
					await AssertionExtensions.WaitForGC(probe.References);
					GC.KeepAlive(probe.Action);
				}
				finally
				{
					ProgrammaticContextFlyoutPresenter.Remove(handler.PlatformView);
					probe.Action.Dispose();
				}
			});
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static (UIAction Action, WeakReference[] References) CreateNativeMenuRetentionProbe(Label target)
		{
			var page = new ContentPage();
			var item = new MenuFlyoutItem { Text = "Action" };
			item.Clicked += (_, _) => page.Title = "Selected";
			var flyout = new MenuFlyout { item };
			FlyoutBase.SetContextFlyout(target, flyout);
			FlyoutBase.ShowContextFlyout(target);
			return (Assert.IsType<UIAction>(item.Handler.PlatformView),
				new[] { new WeakReference(item), new WeakReference(item.Handler), new WeakReference(page) });
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
					Assert.True(button.UserInteractionEnabled);
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
