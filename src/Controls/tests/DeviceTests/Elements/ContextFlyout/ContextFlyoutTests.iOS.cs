#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ContextFlyoutTests
	{
		[Fact(DisplayName = "Context menu interaction is only added when a ContextFlyout is set")]
		public async Task InteractionIsAddedAndRemovedWithContextFlyout()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var label = new Label { Text = "Target" };
				var handler = CreateHandler<LabelHandler>(label);

				Assert.Null(GetInteraction(handler.PlatformView));

				FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));
				Assert.NotNull(GetInteraction(handler.PlatformView));

				FlyoutBase.SetContextFlyout(label, null);
				Assert.Null(GetInteraction(handler.PlatformView));
			});
		}

		[Fact(DisplayName = "Context menu contains items, sub menus, separators sections and disabled state")]
		public async Task ContextMenuIsBuiltFromFlyout()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var label = new Label { Text = "Target" };
				FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));
				var handler = CreateHandler<LabelHandler>(label);

				var menu = GetInteraction(handler.PlatformView)!.CreateMenu();
				Assert.NotNull(menu);

				// The separator splits the menu into two inline sections
				var sections = menu!.Children.Cast<UIMenu>().ToArray();
				Assert.Equal(2, sections.Length);
				Assert.All(sections, s => Assert.True(s.Options.HasFlag(UIMenuOptions.DisplayInline)));

				var first = sections[0].Children;
				Assert.Equal(new[] { "Copy", "Disabled" }, first.Select(e => e.Title));
				Assert.Equal((UIMenuElementAttributes)0, ((UIAction)first[0]).Attributes);
				Assert.Equal(UIMenuElementAttributes.Disabled, ((UIAction)first[1]).Attributes);

				// A sub item named like a main menu ("Edit") stays a regular sub menu of the context menu
				var subMenu = Assert.IsType<UIMenu>(sections[1].Children.Single());
				Assert.Equal("Edit", subMenu.Title);
				Assert.Equal(new[] { "Nested" }, subMenu.Children.Select(e => e.Title));
			});
		}

		[Fact(DisplayName = "Context menu reflects the current state of the items every time it opens")]
		public async Task ContextMenuIsRebuiltWithCurrentState()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var label = new Label { Text = "Target", BindingContext = new ContextMenuViewModel { Title = "Before" } };
				var flyout = CreateFlyout(out var nested);
				var bound = new MenuFlyoutItem();
				bound.SetBinding(MenuFlyoutItem.TextProperty, new Binding(nameof(ContextMenuViewModel.Title)));
				flyout.Add(bound);
				FlyoutBase.SetContextFlyout(label, flyout);
				var handler = CreateHandler<LabelHandler>(label);
				var interaction = GetInteraction(handler.PlatformView)!;

				Assert.Contains("Before", GetAllTitles(interaction.CreateMenu()!));

				((ContextMenuViewModel)label.BindingContext).Title = "After";
				nested.Text = "Nested renamed";

				var titles = GetAllTitles(interaction.CreateMenu()!);
				Assert.Contains("After", titles);
				Assert.Contains("Nested renamed", titles);
				Assert.DoesNotContain("Before", titles);
			});
		}

		[Fact(DisplayName = "Disabled or input transparent views don't open a context menu")]
		public async Task DisabledViewHasNoContextMenu()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var label = new Label { Text = "Target" };
				FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));
				var handler = CreateHandler<LabelHandler>(label);
				var interaction = GetInteraction(handler.PlatformView)!;

				label.IsEnabled = false;
				Assert.Null(interaction.CreateMenu());

				label.IsEnabled = true;
				label.InputTransparent = true;
				Assert.Null(interaction.CreateMenu());

				label.InputTransparent = false;
				Assert.NotNull(interaction.CreateMenu());
			});
		}

		[Fact(DisplayName = "Unsupported icon sources don't prevent the context menu from opening")]
		public async Task UnsupportedIconSourceIsIgnored()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var label = new Label { Text = "Target" };
				var flyout = new MenuFlyout
				{
					new MenuFlyoutItem { Text = "Remote icon", IconImageSource = new UriImageSource { Uri = new Uri("https://localhost/icon.png") } }
				};
				FlyoutBase.SetContextFlyout(label, flyout);
				var handler = CreateHandler<LabelHandler>(label);

				var menu = GetInteraction(handler.PlatformView)!.CreateMenu();

				var action = Assert.IsType<UIAction>(menu!.Children.Single());
				Assert.Equal("Remote icon", action.Title);
				Assert.Null(action.Image);
			});
		}

		[Fact(DisplayName = "Separator-only flyouts and empty icons don't break the context menu")]
		public async Task SeparatorOnlyFlyoutAndEmptyIconAreHandled()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var label = new Label { Text = "Target" };
				FlyoutBase.SetContextFlyout(label, new MenuFlyout { new MenuFlyoutSeparator() });
				var handler = CreateHandler<LabelHandler>(label);
				var interaction = GetInteraction(handler.PlatformView)!;

				Assert.Null(interaction.GetConfigurationForMenu());

				FlyoutBase.SetContextFlyout(label, new MenuFlyout { new MenuFlyoutItem { Text = "No icon", IconImageSource = new FileImageSource() } });

				var action = Assert.IsType<UIAction>(GetInteraction(handler.PlatformView)!.CreateMenu()!.Children.Single());
				Assert.Null(action.Image);
			});
		}

		static MauiUIContextMenuInteraction? GetInteraction(UIView view) =>
			view.Interactions.OfType<MauiUIContextMenuInteraction>().SingleOrDefault();

		static string[] GetAllTitles(UIMenu menu) =>
			menu.Children.SelectMany(e => e is UIMenu child ? GetAllTitles(child).Prepend(child.Title) : new[] { e.Title }).ToArray();
	}
}
