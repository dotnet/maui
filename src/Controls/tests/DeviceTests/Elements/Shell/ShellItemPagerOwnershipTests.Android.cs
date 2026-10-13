using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Activity;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.BottomSheet;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using AContext = Android.Content.Context;
using AView = Android.Views.View;
using NativeShellHandler = Microsoft.Maui.Controls.Handlers.ShellHandler;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Trait(RendererHandlerVariant.TraitName, RendererHandlerVariant.AndroidShellHandler)]
	public class ShellItemPagerOwnershipTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		public Task HandlerAndFragmentUseTheSamePager(int selectedIndex) =>
			RunWithShell(fixture =>
			{
				var pager = fixture.GetPager();
				fixture.AssertPagerIdentity(pager);
				Assert.Same(fixture.Fragment.View, pager.Parent);
				Assert.IsType<FrameLayout.LayoutParams>(fixture.Fragment.View.LayoutParameters);
				Assert.Equal(Resource.Id.shellitem_viewpager, pager.Id);
				Assert.Equal(OverScrollMode.Never, pager.OverScrollMode);
				Assert.Equal(selectedIndex, pager.CurrentItem);
				Assert.False(pager.UserInputEnabled);
				Assert.False(((AView)pager).SaveEnabled);
				Assert.Single(fixture.ItemHandler.ConnectedPagers);
				Assert.Same(pager, fixture.ItemHandler.ConnectedPagers[0]);
				Assert.Single(fixture.ItemHandler.MappedPagers);
				Assert.Same(pager, fixture.ItemHandler.MappedPagers[0]);
				Assert.Equal(PagerTrackingShellItemHandler.Customization, pager.ContentDescription);
				return Task.CompletedTask;
			}, selectedIndex: selectedIndex);

		[Fact]
		public Task InitialPresentationCreatesOneNativePager() =>
			RunWithShell(fixture =>
			{
				var pager = fixture.GetPager();
				var observedPagers = fixture.ItemHandler.CreatedPagers.Append(pager).Distinct().ToList();
				Assert.Single(observedPagers);
				Assert.Single(fixture.ShellHandler.InflatedPagers);
				Assert.Same(pager, fixture.ShellHandler.InflatedPagers[0]);
				Assert.Single(fixture.ItemHandler.CreatedPagers);
				return Task.CompletedTask;
			});

		[Theory]
		[InlineData(typeof(ProgrammaticPagerShellItemHandler))]
		[InlineData(typeof(ReplacingPagerShellItemHandler))]
		public Task CustomCreatedPagerIsHostedWithoutReplacement(Type itemHandlerType) =>
			RunWithShell(fixture =>
			{
				var pager = fixture.GetPager();
				fixture.AssertPagerIdentity(pager);
				Assert.Same(pager, fixture.ItemHandler.CreatedPagers.Last());
				Assert.Same(pager, Assert.Single(fixture.ItemHandler.ConnectedPagers));
				Assert.Same(pager, Assert.Single(fixture.ItemHandler.MappedPagers));
				Assert.Equal(PagerTrackingShellItemHandler.Customization, pager.ContentDescription);
				Assert.IsType<FrameLayout.LayoutParams>(fixture.Fragment.View.LayoutParameters);
				if (itemHandlerType == typeof(ProgrammaticPagerShellItemHandler))
				{
					Assert.Single(fixture.ItemHandler.CreatedPagers);
					Assert.Empty(fixture.ShellHandler.InflatedPagers);
				}
				return Task.CompletedTask;
			}, itemHandlerType: itemHandlerType);

		[Fact]
		public Task ViewRecreationReleasesAndReconnectsThePagerOnce() =>
			RunWithShell(async fixture =>
			{
				var oldPager = fixture.GetPager();
				fixture.AssertPagerIdentity(oldPager);
				var oldRoot = fixture.Fragment.View;
				var oldManager = fixture.ItemHandler._tabbedViewManager;
				var oldAdapter = fixture.ItemHandler._adapter;
				var oldCallback = fixture.ItemHandler._pageChangeCallback;
				var observedPagers = new HashSet<ViewPager2> { oldPager };
				var originalSection = fixture.Item.Items[0];
				var retainedPage = new ContentPage { Title = "Retained navigation" };
				await fixture.Shell.Navigation.PushAsync(retainedPage, false);
				await OnLoadedAsync(retainedPage);

				for (var presentation = 2; presentation <= 3; presentation++)
				{
					var sections = fixture.Item.Items.ToArray();
					await AssertEventually(() => sections.All(section => section.Handler is PagerTrackingShellSectionHandler));
					var sectionHandlers = sections.Select(section => Assert.IsType<PagerTrackingShellSectionHandler>(section.Handler)).ToArray();
					var renderers = Enumerable.Range(0, sections.Length)
						.Select(index => Assert.IsType<ShellSectionHandlerAdapter>(oldAdapter.GetRenderer(index))).ToArray();
					var sectionIds = Enumerable.Range(0, sections.Length).Select(oldAdapter.GetItemId).ToArray();
					var rendererDisposals = new Dictionary<IShellSectionRenderer, int>();
					foreach (var renderer in renderers)
					{
						Assert.NotNull(renderer);
						rendererDisposals.Add(renderer, 0);
						renderer.Destroyed += (_, _) => rendererDisposals[renderer]++;
					}
					for (var index = 0; index < sections.Length; index++)
					{
						AssertSectionSubscriptions(sections[index], sectionHandlers[index], subscribed: true);
					}

					fixture.Detach();

					Assert.Null(fixture.ItemHandler._viewPager);
					Assert.Null(((IElementHandler)fixture.ItemHandler).PlatformView);
					Assert.Null(fixture.ItemHandler._adapter);
					Assert.Null(fixture.ItemHandler._pageChangeCallback);
					Assert.Null(fixture.ItemHandler._tabbedViewManager);
					Assert.Null(fixture.ItemHandler._shellItemAdapter);
					Assert.Null(fixture.ItemHandler._registeredShell);
					Assert.Null(fixture.ItemHandler._shellSection);
					Assert.Null(fixture.ItemHandler._toolbar);
					Assert.Null(oldPager.Adapter);
					Assert.Null(oldRoot.Parent);
					Assert.Same(fixture.Item, ((IElementHandler)fixture.ItemHandler).VirtualView);
					Assert.Same(fixture.ItemHandler, fixture.Item.Handler);
					Assert.False(((IShellSectionController)fixture.Item.CurrentItem)
						.RemoveDisplayedPageObserver(fixture.ItemHandler));
					Assert.Equal(presentation - 1, fixture.ItemHandler.DisconnectedPagers.Count);
					Assert.Same(oldPager, fixture.ItemHandler.DisconnectedPagers.Last());
					for (var index = 0; index < sections.Length; index++)
					{
						Assert.Null(((IElementHandler)sectionHandlers[index]).VirtualView);
						Assert.Null(((IElementHandler)sectionHandlers[index]).PlatformView);
						Assert.Null(sections[index].Handler);
						Assert.Equal(1, sectionHandlers[index].DisconnectCount);
						AssertSectionSubscriptions(sections[index], sectionHandlers[index], subscribed: false);
						Assert.Equal(1, rendererDisposals[renderers[index]]);
						Assert.Null(oldAdapter.GetRenderer(index));
						Assert.False(oldAdapter.ContainsItem(sectionIds[index]));
					}
					Assert.Equal(0, oldAdapter.ItemCount);
					Assert.Null(fixture.ItemHandler.PublishedPlatformViewsOnDisconnect.Last());

					fixture.Item.CurrentItem = fixture.Item.Items[1];
					fixture.Item.Items[0].BadgeText = presentation.ToString();
					fixture.Attach();
					await OnLoadedAsync(fixture.Shell.CurrentPage);
					await AssertEventually(() => sections.All(section => section.Handler is PagerTrackingShellSectionHandler));

					var pager = fixture.GetPager();
					fixture.AssertPagerIdentity(pager);
					Assert.NotSame(oldPager, pager);
					Assert.NotSame(oldRoot, fixture.Fragment.View);
					Assert.NotSame(oldManager, fixture.ItemHandler._tabbedViewManager);
					Assert.NotSame(oldAdapter, fixture.ItemHandler._adapter);
					Assert.NotSame(oldCallback, fixture.ItemHandler._pageChangeCallback);
					Assert.Equal(1, pager.CurrentItem);
					Assert.Equal(presentation.ToString(), fixture.ItemHandler.BottomNavigationView.GetBadge(0).Text);
					Assert.Equal(presentation, fixture.ItemHandler.CreatedPagers.Count);
					Assert.Equal(presentation, fixture.ItemHandler.ConnectedPagers.Count);
					Assert.Equal(presentation, fixture.ItemHandler.MappedPagers.Count);
					Assert.Equal(PagerTrackingShellItemHandler.Customization, pager.ContentDescription);
					for (var index = 0; index < sections.Length; index++)
					{
						Assert.NotSame(sectionHandlers[index], sections[index].Handler);
						Assert.Equal(1, sectionHandlers[index].DisconnectCount);
						Assert.Equal(1, rendererDisposals[renderers[index]]);
						AssertSectionSubscriptions(sections[index], sectionHandlers[index], subscribed: false);
						AssertSectionSubscriptions(sections[index], sections[index].Handler, subscribed: true);
					}
					observedPagers.Add(pager);
					Assert.Equal(presentation, observedPagers.Count);

					var sectionController = (IShellSectionController)fixture.Item.CurrentItem;
					Assert.True(sectionController.RemoveDisplayedPageObserver(fixture.ItemHandler));
					Assert.False(sectionController.RemoveDisplayedPageObserver(fixture.ItemHandler));
					fixture.ItemHandler.UpdateValue(nameof(ShellItem.CurrentItem));

					fixture.Item.CurrentItem = originalSection;
					await OnLoadedAsync(retainedPage);
					Assert.Same(retainedPage, fixture.Shell.CurrentPage);
					Assert.Same(retainedPage, fixture.ItemHandler._toolbarTracker.Page);
					Assert.Contains(retainedPage, originalSection.Navigation.NavigationStack);
					fixture.AssertPagerIdentity(pager);
					oldPager = pager;
					oldRoot = fixture.Fragment.View;
					oldManager = fixture.ItemHandler._tabbedViewManager;
					oldAdapter = fixture.ItemHandler._adapter;
					oldCallback = fixture.ItemHandler._pageChangeCallback;
				}
			});

		[Fact]
		public Task DisconnectAfterViewDestructionDoesNotRepeatNativeCleanup() =>
			RunWithShell(fixture =>
			{
				fixture.AssertPagerIdentity(fixture.GetPager());
				var pager = fixture.GetPager();
				fixture.Detach();
				((IElementHandler)fixture.ItemHandler).DisconnectHandler();
				((IElementHandler)fixture.ItemHandler).DisconnectHandler();

				Assert.Null(((IElementHandler)fixture.ItemHandler).VirtualView);
				Assert.Null(((IElementHandler)fixture.ItemHandler).PlatformView);
				Assert.Null(fixture.Item.Handler);
				Assert.Single(fixture.ItemHandler.DisconnectedPagers);
				Assert.Same(pager, fixture.ItemHandler.DisconnectedPagers[0]);
				Assert.Null(Assert.Single(fixture.ItemHandler.PublishedPlatformViewsOnDisconnect));

				fixture.Attach();
				Assert.IsType<FrameLayout>(fixture.Fragment.View);
				Assert.Single(fixture.ItemHandler.CreatedPagers);
				Assert.Single(fixture.ItemHandler.ConnectedPagers);
				return Task.CompletedTask;
			});

		[Fact]
		public Task HandlerReconnectUsesANewLayoutWithoutCreatingAnotherFragmentPager() =>
			RunWithShell(async fixture =>
			{
				var oldPager = fixture.GetPager();
				fixture.AssertPagerIdentity(oldPager);
				fixture.Detach();
				((IElementHandler)fixture.ItemHandler).DisconnectHandler();
				fixture.ItemHandler.SetVirtualView(fixture.Item);
				var reconnectedPager = fixture.ItemHandler.PlatformView;
				fixture.Attach();
				await OnLoadedAsync(fixture.Shell.CurrentPage);

				fixture.AssertPagerIdentity(reconnectedPager);
				Assert.NotSame(oldPager, reconnectedPager);
				Assert.Equal(2, fixture.ItemHandler.CreatedPagers.Count);
				Assert.Equal(2, fixture.ItemHandler.ConnectedPagers.Count);
				Assert.Equal(2, fixture.ItemHandler.MappedPagers.Count);
				Assert.Equal(2, fixture.ShellHandler.InflatedPagers.Count);
				Assert.Single(fixture.ItemHandler.DisconnectedPagers);
			});

		[Fact]
		public Task SwitchingShellItemsKeepsTheOwnedPagerAndAdapter() =>
			RunWithShell(async fixture =>
			{
				var pager = fixture.GetPager();
				var adapter = fixture.ItemHandler._adapter;
				var firstSectionId = adapter.GetItemId(0);
				var secondItem = new PagerTestItem
				{
					Title = "Second item",
					Items =
					{
						new ShellSection { Title = "Other section", Items = { new ContentPage { Title = "Other page" } } }
					}
				};
				fixture.Shell.Items.Add(secondItem);
				fixture.Shell.CurrentItem = secondItem;
				await OnLoadedAsync(fixture.Shell.CurrentPage);

				fixture.AssertPagerIdentity(pager);
				Assert.Same(fixture.ItemHandler, secondItem.Handler);
				Assert.Same(adapter, fixture.ItemHandler._adapter);
				Assert.False(adapter.ContainsItem(firstSectionId));
				var secondSectionId = adapter.GetItemId(0);
				Assert.NotEqual(firstSectionId, secondSectionId);

				fixture.Shell.CurrentItem = fixture.Item;
				await OnLoadedAsync(fixture.Shell.CurrentPage);
				fixture.AssertPagerIdentity(pager);
				Assert.Same(adapter, fixture.ItemHandler._adapter);
				Assert.False(adapter.ContainsItem(secondSectionId));
				Assert.Single(fixture.ItemHandler.CreatedPagers);
				Assert.Single(fixture.ItemHandler.ConnectedPagers);
				Assert.Empty(fixture.ItemHandler.DisconnectedPagers);
				Assert.Single(fixture.ShellHandler.InflatedPagers);
				Assert.Equal(2, pager.OffscreenPageLimit);
			});

		[Fact]
		public Task SwitchingShellItemsWhileTheViewIsDestroyedConnectsTheNewPagerOnce() =>
			RunWithShell(async fixture =>
			{
				var oldPager = fixture.GetPager();
				fixture.AssertPagerIdentity(oldPager);
				fixture.Detach();
				var secondItem = new PagerTestItem
				{
					Title = "Second item",
					Items =
					{
						new ShellSection { Title = "Other section", Items = { new ContentPage { Title = "Other page" } } }
					}
				};
				fixture.Shell.Items.Add(secondItem);
				fixture.Shell.CurrentItem = secondItem;
				var pendingPager = fixture.ItemHandler.PlatformView;
				fixture.Attach();
				await OnLoadedAsync(fixture.Shell.CurrentPage);

				Assert.NotSame(oldPager, pendingPager);
				fixture.AssertPagerIdentity(pendingPager);
				Assert.Same(fixture.ItemHandler, secondItem.Handler);
				Assert.Null(fixture.Item.Handler);
				Assert.Equal(2, fixture.ItemHandler.CreatedPagers.Count);
				Assert.Equal(2, fixture.ItemHandler.ConnectedPagers.Count);
				Assert.Equal(2, fixture.ItemHandler.MappedPagers.Count);
				Assert.Single(fixture.ItemHandler.DisconnectedPagers);
			});

		[Fact]
		public Task RecreatedPagerSupportsMoreBadgesAppearanceAndBackNavigation() =>
			RunWithShell(async fixture =>
			{
				fixture.AssertPagerIdentity(fixture.GetPager());
				fixture.Detach();
				fixture.Item.Items[0].BadgeText = "9";
				Shell.SetTabBarBackgroundColor(fixture.Item, Colors.Blue);
				fixture.Attach();
				await OnLoadedAsync(fixture.Shell.CurrentPage);

				var pager = fixture.GetPager();
				var bottomNavigation = fixture.ItemHandler.BottomNavigationView;
				Assert.Equal(5, bottomNavigation.Menu.Size());
				Assert.Equal("9", bottomNavigation.GetBadge(0).Text);
				var background = Assert.IsType<ColorChangeRevealDrawable>(bottomNavigation.Background);
				Assert.Equal(Colors.Blue.ToPlatform(), background.EndColor);

				try
				{
					bottomNavigation.SelectedItemId = BottomNavigationViewUtils.MoreTabId;
					Assert.NotNull(fixture.ItemHandler.MoreDialog);
					Assert.True(fixture.ItemHandler.MoreDialog.IsShowing);
					fixture.ItemHandler.SelectMoreItem(5);
					await OnLoadedAsync(fixture.Shell.CurrentPage);
					await AssertEventually(() => pager.CurrentItem == 5);
					Assert.Same(fixture.Item.Items[5], fixture.Item.CurrentItem);
					Assert.True(bottomNavigation.Menu.GetItem(4).IsChecked);

					var page = new ContentPage { Title = "Pushed page" };
					await fixture.Shell.Navigation.PushAsync(page, false);
					await OnLoadedAsync(page);
					Assert.True(fixture.ItemHandler.OnBackButtonPressed());
					await AssertEventually(() => fixture.Shell.Navigation.NavigationStack.Count == 1);
					fixture.AssertPagerIdentity(pager);
					Assert.Equal(2, fixture.ItemHandler.CreatedPagers.Count);
				}
				finally
				{
					fixture.ItemHandler.MoreDialog?.Dismiss();
					fixture.ItemHandler.MoreDialog?.Dispose();
				}
			}, sectionCount: 6);

		[Fact]
		public Task BackDispatcherDropsTheDestroyedViewsCallback() =>
			RunWithShell(async fixture =>
			{
				fixture.AssertPagerIdentity(fixture.GetPager());
				var backCount = 0;
				fixture.Shell.Navigating += (_, navigation) =>
				{
					if (navigation.Source == ShellNavigationSource.Pop)
					{
						backCount++;
						navigation.Cancel();
					}
				};
				fixture.Detach();
				var dispatcher = fixture.ItemHandler.MauiContext.GetActivity().OnBackPressedDispatcher;
				using var fallback = new CountingBackPressedCallback();
				dispatcher.AddCallback(fallback);

				try
				{
					fixture.Attach();
					await AssertEventually(() => fixture.Fragment.IsResumed);
					dispatcher.OnBackPressed();
					Assert.Equal(1, backCount);
					Assert.Equal(0, fallback.InvocationCount);

					fixture.Detach();
					dispatcher.OnBackPressed();
					Assert.Equal(1, backCount);
					Assert.Equal(1, fallback.InvocationCount);

					fixture.Attach();
					await AssertEventually(() => fixture.Fragment.IsResumed);
					dispatcher.OnBackPressed();
					Assert.Equal(2, backCount);
					Assert.Equal(1, fallback.InvocationCount);
					fixture.AssertPagerIdentity(fixture.GetPager());
				}
				finally
				{
					fallback.Remove();
				}
			});

		[Fact]
		public async Task ParameterlessRestorationDoesNotCreateAPager()
		{
			SetupBuilder();
			await InvokeOnMainThreadAsync(() =>
			{
				using var fragment = new ShellItemWrapperFragment();
				using var state = new Bundle();
				using var view = fragment.OnCreateView(MauiContext.GetLayoutInflater(), null, state);
				Assert.IsType<FrameLayout>(view);
				fragment.OnViewCreated(view, null);
				fragment.OnDestroyView();
			});
		}

		void SetupBuilder(Type itemHandlerType = null) =>
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
			{
				SetupShellHandlers(handlers);
				handlers.AddHandler(typeof(Shell), typeof(PagerTrackingShellHandler));
				handlers.AddHandler(typeof(ShellItem), itemHandlerType ?? typeof(PagerTrackingShellItemHandler));
				handlers.AddHandler(typeof(ShellSection), typeof(PagerTrackingShellSectionHandler));
				handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
			}));

		async Task RunWithShell(Func<Fixture, Task> action, int sectionCount = 2, int selectedIndex = 0,
			Type itemHandlerType = null)
		{
			SetupBuilder(itemHandlerType);
			var shell = await InvokeOnMainThreadAsync(() =>
			{
				var item = new PagerTestItem { Title = "Pager ownership" };
				for (var index = 0; index < sectionCount; index++)
				{
					item.Items.Add(new ShellSection
					{
						Title = $"Section {index}",
						Items = { new ContentPage { Title = $"Page {index}", Content = new Label { Text = $"Content {index}" } } }
					});
				}
				item.CurrentItem = item.Items[selectedIndex];
				return new Shell { FlyoutBehavior = FlyoutBehavior.Disabled, Items = { item } };
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				var shellHandler = Assert.IsType<PagerTrackingShellHandler>(shell.Handler);
				var item = Assert.IsType<PagerTestItem>(shell.CurrentItem);
				var itemHandler = Assert.IsAssignableFrom<PagerTrackingShellItemHandler>(item.Handler);
				var fragment = Assert.IsType<ShellItemWrapperFragment>(shellHandler.ItemRenderer.Fragment);
				await action(new Fixture(shell, item, shellHandler, itemHandler, fragment));
			});
		}

		static void AssertSectionSubscriptions(ShellSection section, IElementHandler handler, bool subscribed)
		{
			Assert.Equal(subscribed ? 1 : 0,
				GetEventSubscribers(section.Items, typeof(ShellElementCollection), "VisibleItemsChanged")
					.Count(subscriber => ReferenceEquals(subscriber.Target, handler)));
			foreach (var content in section.Items)
			{
				Assert.Equal(subscribed ? 1 : 0,
					GetEventSubscribers(content, typeof(BindableObject), nameof(BindableObject.PropertyChanged))
						.Count(subscriber => ReferenceEquals(subscriber.Target, handler)));
			}
		}

		static Delegate[] GetEventSubscribers(object source, Type declaringType, string eventName)
		{
			var field = declaringType.GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(field);
			return (field.GetValue(source) as MulticastDelegate)?.GetInvocationList() ?? Array.Empty<Delegate>();
		}

		public class ProgrammaticPagerShellItemHandler : PagerTrackingShellItemHandler
		{
			protected override ViewPager2 CreatePlatformElement()
			{
				var pager = new ViewPager2(MauiContext.Context);
				CreatedPagers.Add(pager);
				return pager;
			}
		}

		public class PagerTrackingShellSectionHandler : ShellSectionHandler
		{
			public int DisconnectCount { get; private set; }

			protected override void DisconnectHandler(AView platformView)
			{
				DisconnectCount++;
				base.DisconnectHandler(platformView);
			}
		}

		public class ReplacingPagerShellItemHandler : PagerTrackingShellItemHandler
		{
			protected override ViewPager2 CreatePlatformElement()
			{
				base.CreatePlatformElement();
				var pager = new ViewPager2(MauiContext.Context);
				CreatedPagers.Add(pager);
				return pager;
			}
		}

		sealed class Fixture
		{
			public Fixture(Shell shell, PagerTestItem item, PagerTrackingShellHandler shellHandler,
				PagerTrackingShellItemHandler itemHandler, ShellItemWrapperFragment fragment)
			{
				Shell = shell;
				Item = item;
				ShellHandler = shellHandler;
				ItemHandler = itemHandler;
				Fragment = fragment;
			}

			public Shell Shell { get; }
			public PagerTestItem Item { get; }
			public PagerTrackingShellHandler ShellHandler { get; }
			public PagerTrackingShellItemHandler ItemHandler { get; }
			public ShellItemWrapperFragment Fragment { get; }

			public ViewPager2 GetPager()
			{
				Assert.IsType<CoordinatorLayout>(Fragment.View);
				return Assert.IsType<ViewPager2>(Fragment.View.FindViewById(Resource.Id.shellitem_viewpager));
			}

			public void AssertPagerIdentity(ViewPager2 pager)
			{
				Assert.Same(pager, GetPager());
				Assert.Same(pager, ItemHandler.PlatformView);
				Assert.Same(pager, ((ElementHandler)ItemHandler).PlatformView);
				Assert.Same(pager, ((IElementHandler)ItemHandler).PlatformView);
				Assert.Same(pager, ItemHandler._viewPager);
				if (ItemHandler._tabbedViewManager is not null)
					Assert.Same(pager, ItemHandler._tabbedViewManager.ViewPager);
			}

			public void Detach() =>
				Fragment.ParentFragmentManager.BeginTransaction().Detach(Fragment).CommitNow();

			public void Attach() =>
				Fragment.ParentFragmentManager.BeginTransaction().Attach(Fragment).CommitNow();
		}

		sealed class PagerTestItem : FlyoutItem, IPropertyMapperView
		{
			readonly PropertyMapper<ShellItem, ShellItemHandler> _mapper = new()
			{
				["PagerCustomization"] = (handler, _) =>
				{
					var trackingHandler = (PagerTrackingShellItemHandler)handler;
					trackingHandler.MappedPagers.Add(handler.PlatformView);
					handler.PlatformView.ContentDescription = PagerTrackingShellItemHandler.Customization;
				}
			};

			PropertyMapper IPropertyMapperView.GetPropertyMapperOverrides() => _mapper;
		}

		sealed class CountingBackPressedCallback : OnBackPressedCallback
		{
			public CountingBackPressedCallback() : base(true)
			{
			}

			public int InvocationCount { get; private set; }

			public override void HandleOnBackPressed() => InvocationCount++;
		}
	}

	public class PagerTrackingShellItemHandler : ShellItemHandler
	{
		public const string Customization = "Customized Shell item pager";
		public List<ViewPager2> CreatedPagers { get; } = new();
		public List<ViewPager2> ConnectedPagers { get; } = new();
		public List<ViewPager2> DisconnectedPagers { get; } = new();
		public List<object> PublishedPlatformViewsOnDisconnect { get; } = new();
		public List<ViewPager2> MappedPagers { get; } = new();
		public BottomSheetDialog MoreDialog { get; private set; }
		Action<int, BottomSheetDialog> _selectMoreItem;

		protected override ViewPager2 CreatePlatformElement()
		{
			var pager = base.CreatePlatformElement();
			CreatedPagers.Add(pager);
			return pager;
		}

		protected override void ConnectHandler(ViewPager2 platformView)
		{
			ConnectedPagers.Add(platformView);
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(ViewPager2 platformView)
		{
			PublishedPlatformViewsOnDisconnect.Add(((IElementHandler)this).PlatformView);
			DisconnectedPagers.Add(platformView);
			base.DisconnectHandler(platformView);
		}

		protected override BottomSheetDialog CreateMoreBottomSheet(
			Action<int, BottomSheetDialog> selectCallback,
			List<(string title, ImageSource icon, bool tabEnabled)> items)
		{
			_selectMoreItem = selectCallback;
			return MoreDialog = base.CreateMoreBottomSheet(selectCallback, items);
		}

		public void SelectMoreItem(int index)
		{
			_selectMoreItem(index, MoreDialog);
			MoreDialog = null;
		}
	}

	public class PagerTrackingShellHandler : NativeShellHandler
	{
		LayoutInflater _inflater;
		public List<ViewPager2> InflatedPagers { get; } = new();
		internal IShellItemRenderer ItemRenderer { get; private set; }

		protected override IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
		{
			var context = MauiContext;
			var inflater = context.GetLayoutInflater();
			_inflater = inflater.CloneInContext(inflater.Context)
				?? throw new InvalidOperationException("LayoutInflater cloning failed");
			_inflater.Factory2 = new PagerCountingFactory(inflater.Factory2, InflatedPagers);
			SetMauiContext(context.MakeScoped(layoutInflater: _inflater));
			try
			{
				return ItemRenderer = base.CreateShellItemRenderer(shellItem);
			}
			finally
			{
				SetMauiContext(context);
			}
		}

		sealed class PagerCountingFactory : Java.Lang.Object, LayoutInflater.IFactory2
		{
			readonly LayoutInflater.IFactory2 _inner;
			readonly List<ViewPager2> _pagers;

			public PagerCountingFactory(LayoutInflater.IFactory2 inner, List<ViewPager2> pagers)
			{
				_inner = inner;
				_pagers = pagers;
			}

			public AView OnCreateView(AView parent, string name, AContext context, IAttributeSet attrs)
			{
				if (name == "androidx.viewpager2.widget.ViewPager2" &&
					attrs.GetAttributeResourceValue("http://schemas.android.com/apk/res/android", "id", 0) == Resource.Id.shellitem_viewpager)
				{
					var pager = new ViewPager2(context, attrs);
					_pagers.Add(pager);
					return pager;
				}
				return _inner?.OnCreateView(parent, name, context, attrs);
			}

			public AView OnCreateView(string name, AContext context, IAttributeSet attrs) =>
				OnCreateView(null, name, context, attrs);
		}
	}
}
