using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ToolbarDrawerToggleTests : ShellTestBase
	{
		/// <summary>
		/// Stands in for an out-of-tree platform backend. It is deliberately written against
		/// <see cref="IToolbar"/> only and never references <see cref="Toolbar"/>, so anything it
		/// can do here is something an external backend assembly can also do.
		/// </summary>
		class ExternalToolbarBackendHandler : ElementHandler<IToolbar, object>
		{
			public static readonly IPropertyMapper<IToolbar, ExternalToolbarBackendHandler> ExternalMapper =
				new PropertyMapper<IToolbar, ExternalToolbarBackendHandler>(ElementMapper)
				{
					[nameof(IToolbar.BackButtonVisible)] = MapBackButtonVisible,
					[nameof(IToolbarDrawerToggleVisible.DrawerToggleVisible)] = MapDrawerToggleVisible,
				};

			public ExternalToolbarBackendHandler()
				: base(ExternalMapper)
			{
			}

			public List<bool> DrawerToggleUpdates { get; } = new List<bool>();

			public List<bool> BackButtonUpdates { get; } = new List<bool>();

			public bool? DrawerToggleVisibleObservedFromBackButtonMapper { get; private set; }

			public void ClearRecordedUpdates()
			{
				DrawerToggleUpdates.Clear();
				BackButtonUpdates.Clear();
				DrawerToggleVisibleObservedFromBackButtonMapper = null;
			}

			protected override object CreatePlatformElement() => new object();

			/// <summary>
			/// The only way an external backend reads drawer state: pattern match the optional capability
			/// interface off <see cref="IToolbar"/>. A toolbar that does not implement it has no drawer toggle.
			/// </summary>
			internal static bool ReadDrawerToggleVisible(IToolbar toolbar) =>
				toolbar is IToolbarDrawerToggleVisible { DrawerToggleVisible: true };

			static void MapDrawerToggleVisible(ExternalToolbarBackendHandler handler, IToolbar toolbar) =>
				handler.DrawerToggleUpdates.Add(ReadDrawerToggleVisible(toolbar));

			static void MapBackButtonVisible(ExternalToolbarBackendHandler handler, IToolbar toolbar)
			{
				handler.BackButtonUpdates.Add(toolbar.BackButtonVisible);
				handler.DrawerToggleVisibleObservedFromBackButtonMapper = ReadDrawerToggleVisible(toolbar);
			}
		}

		/// <summary>
		/// Creates a Shell whose effective <see cref="FlyoutBehavior"/> is <see cref="FlyoutBehavior.Flyout"/>.
		/// A Shell built from a single implicit <see cref="ContentPage"/> resolves to
		/// <see cref="FlyoutBehavior.Disabled"/> instead, which would hide the drawer toggle.
		/// </summary>
		TestShell CreateFlyoutShell()
		{
			var shell = new TestShell();
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			return shell;
		}

		static ExternalToolbarBackendHandler AttachExternalBackend(IToolbar toolbar)
		{
			var handler = new ExternalToolbarBackendHandler();
			handler.SetVirtualView(toolbar);
			handler.ClearRecordedUpdates();
			return handler;
		}

		[Fact]
		public void DrawerToggleVisibleIsReachableFromIToolbar()
		{
			var shell = CreateFlyoutShell();

			// An external backend only ever gets handed the IToolbar contract.
			IToolbar toolbar = shell.Toolbar;

			var capability = Assert.IsAssignableFrom<IToolbarDrawerToggleVisible>(toolbar);
			Assert.True(capability.DrawerToggleVisible);

			shell.FlyoutBehavior = FlyoutBehavior.Disabled;
			Assert.False(capability.DrawerToggleVisible);
		}

		/// <summary>
		/// The capability interface is optional, so an existing external <see cref="IToolbar"/>
		/// implementation that predates it keeps compiling and simply reports no drawer toggle.
		/// This type deliberately does not implement <see cref="IToolbarDrawerToggleVisible"/>.
		/// </summary>
		[Fact]
		public void ToolbarWithoutCapabilityInterfaceReportsNoDrawerToggle()
		{
			IToolbar toolbar = new LegacyExternalToolbar();

			Assert.False(toolbar is IToolbarDrawerToggleVisible);
			Assert.False(ExternalToolbarBackendHandler.ReadDrawerToggleVisible(toolbar));
		}

		[Fact]
		public void ToolbarImplementingCapabilityInterfaceReportsDrawerToggle()
		{
			IToolbar toolbar = new CapableExternalToolbar { DrawerToggleVisible = true };

			Assert.True(ExternalToolbarBackendHandler.ReadDrawerToggleVisible(toolbar));
		}

		[Fact]
		public void ExternalBackendIsNotifiedWhenFlyoutBehaviorChanges()
		{
			var shell = CreateFlyoutShell();
			var handler = AttachExternalBackend(shell.Toolbar);

			shell.FlyoutBehavior = FlyoutBehavior.Disabled;
			Assert.Equal(new[] { false }, handler.DrawerToggleUpdates);

			shell.FlyoutBehavior = FlyoutBehavior.Flyout;
			Assert.Equal(new[] { false, true }, handler.DrawerToggleUpdates);
		}

		[Fact]
		public void ExternalBackendIsNotNotifiedWhenDrawerToggleIsUnchanged()
		{
			var shell = CreateFlyoutShell();
			var handler = AttachExternalBackend(shell.Toolbar);

			// Neither Disabled nor Locked shows the drawer toggle, so no update should be pushed.
			shell.FlyoutBehavior = FlyoutBehavior.Disabled;
			handler.ClearRecordedUpdates();

			shell.FlyoutBehavior = FlyoutBehavior.Locked;

			Assert.Empty(handler.DrawerToggleUpdates);
		}

		[Fact]
		public void TizenNavigationSlotRestoresTitleIconWhenDrawerToggleIsHidden()
		{
			var shell = CreateFlyoutShell();
			var toolbar = shell.Toolbar;
			toolbar.TitleIcon = new FileImageSource { File = "title.png" };

			Assert.Equal(ToolbarNavigationIconKind.DrawerToggle, toolbar.NavigationIconKind);

			shell.FlyoutBehavior = FlyoutBehavior.Disabled;

			Assert.False(toolbar.BackButtonVisible);
			Assert.False(toolbar.DrawerToggleVisible);
			Assert.Equal(ToolbarNavigationIconKind.TitleIcon, toolbar.NavigationIconKind);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ForwardingNavigationSlotAvoidsInvalidIntermediateState(bool transitionToBackButton)
		{
			var source = new Toolbar(null)
			{
				TitleIcon = new FileImageSource { File = "custom.png" },
				BackButtonVisible = transitionToBackButton,
				DrawerToggleVisible = !transitionToBackButton,
			};
			var destination = new Toolbar(null)
			{
				TitleIcon = source.TitleIcon,
				BackButtonVisible = !transitionToBackButton,
				DrawerToggleVisible = transitionToBackButton,
			};
			var observedKinds = new List<ToolbarNavigationIconKind>();
			destination.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(Toolbar.BackButtonVisible) ||
					e.PropertyName == nameof(Toolbar.DrawerToggleVisible))
				{
					observedKinds.Add(destination.NavigationIconKind);
				}
			};

			source.ForwardNavigationIconStateTo(destination);

			Assert.NotEmpty(observedKinds);
			Assert.DoesNotContain(ToolbarNavigationIconKind.None, observedKinds);
			Assert.DoesNotContain(ToolbarNavigationIconKind.TitleIcon, observedKinds);
			Assert.Equal(source.NavigationIconKind, destination.NavigationIconKind);
		}

		[Fact]
		public async Task StaleTitleIconLoadIsRejectedAfterNavigationSlotChanges()
		{
			var toolbar = new Toolbar(null)
			{
				TitleIcon = new StreamImageSource(),
			};
			var source = toolbar.TitleIcon;
			var generation = toolbar.BeginNavigationIconUpdate();
			var releaseLoad = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

			Assert.True(toolbar.IsCurrentTitleIconUpdate(generation, source));
			Assert.False(toolbar.IsCurrentTitleIconUpdate(generation, new StreamImageSource()));

			var delayedLoad = Task.Run(async () =>
			{
				await releaseLoad.Task;
				return toolbar.IsCurrentTitleIconUpdate(generation, source);
			});

			toolbar.DrawerToggleVisible = true;
			toolbar.BeginNavigationIconUpdate();
			releaseLoad.SetResult();

			Assert.False(await delayedLoad);
			Assert.Equal(ToolbarNavigationIconKind.DrawerToggle, toolbar.NavigationIconKind);
		}

		[Fact]
		public async Task BackButtonTakesPrecedenceOverDrawerToggleInShell()
		{
			var shell = CreateFlyoutShell();
			var toolbar = shell.Toolbar;

			Assert.True(toolbar.DrawerToggleVisible);
			Assert.False(toolbar.BackButtonVisible);

			await shell.Navigation.PushAsync(new ContentPage());

			Assert.True(toolbar.BackButtonVisible);
			Assert.False(toolbar.DrawerToggleVisible);

			await shell.Navigation.PopAsync();

			Assert.False(toolbar.BackButtonVisible);
			Assert.True(toolbar.DrawerToggleVisible);
		}

		/// <summary>
		/// <c>BackButtonVisible</c> and <c>DrawerToggleVisible</c> are not mutually exclusive — on Windows
		/// both can be true at once. What the framework does guarantee is ordering: the drawer backing value
		/// is already current when <c>BackButtonVisible</c> notifies, so a backend that renders the shared
		/// navigation slot from either mapper always sees a settled pair and can apply back-button precedence.
		/// </summary>
		[Fact]
		public async Task DrawerToggleValueIsCurrentWhenBackButtonMapperRuns()
		{
			var shell = CreateFlyoutShell();
			var handler = AttachExternalBackend(shell.Toolbar);

			await shell.Navigation.PushAsync(new ContentPage());

			// On this (non-Windows) configuration ShellToolbar hides the drawer toggle once a page is
			// pushed, and the back button mapper observes that settled value rather than a stale true.
			Assert.False(handler.DrawerToggleVisibleObservedFromBackButtonMapper);
			Assert.Equal(new[] { false }, handler.DrawerToggleUpdates);
			Assert.Equal(new[] { true }, handler.BackButtonUpdates);
		}

		[Fact]
		public async Task DrawerToggleTransitionsForNavigationPageInsideFlyoutPage()
		{
			DeviceInfo.SetCurrent(new MockDeviceInfo(idiom: DeviceIdiom.Phone));

			var navigationPage = new NavigationPage(new ContentPage());
			var flyoutPage = new FlyoutPage
			{
				Flyout = new ContentPage { Title = "Flyout" },
				Detail = navigationPage
			};

			_ = new TestWindow(flyoutPage);

			var toolbar = flyoutPage.Toolbar;
			Assert.NotNull(toolbar);
			Assert.True(toolbar.DrawerToggleVisible);

			await navigationPage.Navigation.PushAsync(new ContentPage());
			Assert.True(toolbar.BackButtonVisible);
			Assert.False(toolbar.DrawerToggleVisible);

			await navigationPage.Navigation.PopAsync();
			Assert.False(toolbar.BackButtonVisible);
			Assert.True(toolbar.DrawerToggleVisible);
		}

		[Fact]
		public async Task ExternalBackendIsNotifiedForNavigationPageInsideFlyoutPage()
		{
			DeviceInfo.SetCurrent(new MockDeviceInfo(idiom: DeviceIdiom.Phone));

			var navigationPage = new NavigationPage(new ContentPage());
			var flyoutPage = new FlyoutPage
			{
				Flyout = new ContentPage { Title = "Flyout" },
				Detail = navigationPage
			};

			_ = new TestWindow(flyoutPage);

			var handler = AttachExternalBackend(flyoutPage.Toolbar);

			await navigationPage.Navigation.PushAsync(new ContentPage());
			Assert.Equal(new[] { false }, handler.DrawerToggleUpdates);

			await navigationPage.Navigation.PopAsync();
			Assert.Equal(new[] { false, true }, handler.DrawerToggleUpdates);
		}

		[Fact]
		public void DrawerToggleIsHiddenWhenFlyoutPageIsSplit()
		{
			DeviceInfo.SetCurrent(new MockDeviceInfo(idiom: DeviceIdiom.Tablet));
			DeviceDisplay.SetCurrent(new MockDeviceDisplay());

			var navigationPage = new NavigationPage(new ContentPage());
			var flyoutPage = new FlyoutPage
			{
				FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split,
				Flyout = new ContentPage { Title = "Flyout" },
				Detail = navigationPage
			};

			_ = new TestWindow(flyoutPage);

			var toolbar = flyoutPage.Toolbar;
			Assert.False(toolbar.DrawerToggleVisible);

			flyoutPage.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
			Assert.True(toolbar.DrawerToggleVisible);
		}

		[Fact]
		public void DrawerToggleStateIsTrackedPerWindow()
		{
			var firstShell = CreateFlyoutShell();
			var secondShell = CreateFlyoutShell();

			var firstHandler = AttachExternalBackend(firstShell.Toolbar);
			var secondHandler = AttachExternalBackend(secondShell.Toolbar);

			firstShell.FlyoutBehavior = FlyoutBehavior.Disabled;

			Assert.NotSame(firstShell.Toolbar, secondShell.Toolbar);
			Assert.False(firstShell.Toolbar.DrawerToggleVisible);
			Assert.True(secondShell.Toolbar.DrawerToggleVisible);
			Assert.Equal(new[] { false }, firstHandler.DrawerToggleUpdates);
			Assert.Empty(secondHandler.DrawerToggleUpdates);
		}

		/// <summary>Models an external toolbar written before the capability interface existed.</summary>
		class LegacyExternalToolbar : IToolbar
		{
			public bool BackButtonVisible { get; set; }

			public bool IsVisible { get; set; }

			public string Title => string.Empty;

			public IElement Parent => null;

			public IElementHandler Handler { get; set; }
		}

		/// <summary>Models an external toolbar that opts into the capability interface.</summary>
		class CapableExternalToolbar : IToolbar, IToolbarDrawerToggleVisible
		{
			public bool BackButtonVisible { get; set; }

			public bool IsVisible { get; set; }

			public bool DrawerToggleVisible { get; set; }

			public string Title => string.Empty;

			public IElement Parent => null;

			public IElementHandler Handler { get; set; }
		}
	}
}
