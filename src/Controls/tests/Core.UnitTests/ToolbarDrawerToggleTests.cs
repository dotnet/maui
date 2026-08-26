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
					[nameof(IToolbar.DrawerToggleVisible)] = MapDrawerToggleVisible,
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

			static void MapDrawerToggleVisible(ExternalToolbarBackendHandler handler, IToolbar toolbar) =>
				handler.DrawerToggleUpdates.Add(toolbar.DrawerToggleVisible);

			static void MapBackButtonVisible(ExternalToolbarBackendHandler handler, IToolbar toolbar)
			{
				handler.BackButtonUpdates.Add(toolbar.BackButtonVisible);
				handler.DrawerToggleVisibleObservedFromBackButtonMapper = toolbar.DrawerToggleVisible;
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
		public void IToolbarExposesDrawerToggleVisible()
		{
			var shell = CreateFlyoutShell();

			// An external backend only ever gets handed the IToolbar contract.
			IToolbar toolbar = shell.Toolbar;

			Assert.True(toolbar.DrawerToggleVisible);

			shell.FlyoutBehavior = FlyoutBehavior.Disabled;
			Assert.False(toolbar.DrawerToggleVisible);
		}

		[Fact]
		public void IToolbarDrawerToggleVisibleDefaultsToFalseForCustomImplementations()
		{
			IToolbar toolbar = new MinimalToolbar();

			Assert.False(toolbar.DrawerToggleVisible);
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

		[Fact]
		public async Task ExternalBackendSeesConsistentStateWhileBackButtonMapperRuns()
		{
			var shell = CreateFlyoutShell();
			var handler = AttachExternalBackend(shell.Toolbar);

			await shell.Navigation.PushAsync(new ContentPage());

			// The drawer toggle backing value is updated before BackButtonVisible notifies, so a backend
			// that renders the shared navigation slot from the back button mapper never sees a stale
			// "back button and drawer toggle are both visible" state.
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

		class MinimalToolbar : IToolbar
		{
			public bool BackButtonVisible { get; set; }

			public bool IsVisible { get; set; }

			public string Title => string.Empty;

			public IElement Parent => null;

			public IElementHandler Handler { get; set; }
		}
	}
}
