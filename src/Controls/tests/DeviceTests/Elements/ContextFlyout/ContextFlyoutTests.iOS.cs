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

		[Fact(DisplayName = "Disabled or input transparent views don't open a context menu on iOS")]
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
				AssertMenuOnlyOnMacCatalyst(interaction.CreateMenu());

				label.IsEnabled = true;
				label.InputTransparent = true;
				AssertMenuOnlyOnMacCatalyst(interaction.CreateMenu());

				label.InputTransparent = false;
				Assert.NotNull(interaction.CreateMenu());

				// Mac Catalyst keeps its existing behavior (before iOS support) of not checking the view's state
				static void AssertMenuOnlyOnMacCatalyst(UIMenu? menu)
				{
					if (OperatingSystem.IsMacCatalyst())
						Assert.NotNull(menu);
					else
						Assert.Null(menu);
				}
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

		// UIKit gives the context menu priority over long-press gestures, like in native apps
		public static TheoryData<string> ExistingLongPressBehaviors => new() { "None", "LongPressGesture" };

		[Theory(DisplayName = "The context menu opens like in native apps, also with other long-press behavior")]
		[MemberData(nameof(ExistingLongPressBehaviors))]
		public async Task ExistingLongPressBehaviorKeepsPriority(string behavior)
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				View target = new Label { Text = "Target" };
				if (behavior == "LongPressGesture")
					target.GestureRecognizers.Add(new LongPressGestureRecognizer());

				FlyoutBase.SetContextFlyout(target, CreateFlyout(out _));
				var handler = (IPlatformViewHandler)target.ToHandler(MauiContext);
				var interaction = GetInteraction(handler.PlatformView!)!;

				var configuration = interaction.GetConfigurationForMenu(CoreGraphics.CGPoint.Empty);

				Assert.NotNull(configuration);
			});
		}

		[Fact(DisplayName = "CollectionView reordering keeps priority over context menus of its items")]
		public async Task ReorderingKeepsPriority()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var label = new Label { Text = "Target" };
				FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));
				var handler = CreateHandler<LabelHandler>(label);
				var interaction = GetInteraction(handler.PlatformView)!;

				using var reorderingParent = new UIView();
				using var reorderRecognizer = new UILongPressGestureRecognizer();
				MauiUIContextMenuInteraction.MarkLongPressRecognizer(reorderRecognizer);
				reorderingParent.AddGestureRecognizer(reorderRecognizer);
				reorderingParent.AddSubview(handler.PlatformView);

				var configuration = interaction.GetConfigurationForMenu(CoreGraphics.CGPoint.Empty);

				// Mac Catalyst opens context menus with a secondary click, which doesn't compete with reordering
				if (OperatingSystem.IsMacCatalyst())
					Assert.NotNull(configuration);
				else
					Assert.Null(configuration);

				handler.PlatformView.RemoveFromSuperview();
			});
		}

		[Fact(DisplayName = "CollectionView reordering keeps priority inside a view with a context menu")]
		public async Task ReorderingInsideViewWithMenuKeepsPriority()
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var grid = new Grid();
				FlyoutBase.SetContextFlyout(grid, CreateFlyout(out _));
				var handler = CreateHandler<LayoutHandler>(grid);
				var interaction = GetInteraction(handler.PlatformView)!;
				handler.PlatformView.Frame = new CoreGraphics.CGRect(0, 0, 200, 100);

				using var reordering = new UIView(new CoreGraphics.CGRect(0, 0, 100, 100));
				using var reorderRecognizer = new UILongPressGestureRecognizer();
				MauiUIContextMenuInteraction.MarkLongPressRecognizer(reorderRecognizer);
				reordering.AddGestureRecognizer(reorderRecognizer);
				handler.PlatformView.AddSubview(reordering);

				var onReordering = interaction.GetConfigurationForMenu(new CoreGraphics.CGPoint(50, 50));
				var besideReordering = interaction.GetConfigurationForMenu(new CoreGraphics.CGPoint(150, 50));

				Assert.NotNull(besideReordering);
				if (OperatingSystem.IsMacCatalyst())
					Assert.NotNull(onReordering);
				else
					Assert.Null(onReordering);

				reordering.RemoveFromSuperview();
			});
		}

		// The text input itself, or a text input inside a layout with the flyout
		public static TheoryData<string, bool> TextInputs => new()
		{
			{ "Entry", false },
			{ "Editor", false },
			{ "SearchBar", false },
			{ "Entry", true },
			{ "Editor", true },
		};

		[Theory(DisplayName = "Text inputs keep selecting text on long press; the flyout items join their edit menu")]
		[MemberData(nameof(TextInputs))]
		public async Task TextInputsAddItemsToEditMenu(string kind, bool flyoutOnParent)
		{
			SetupBuilder();

			View input = kind switch
			{
				"Editor" => new Editor { Text = "Text", HeightRequest = 80 },
				"SearchBar" => new SearchBar { Text = "Text", WidthRequest = 300 },
				_ => new Entry { Text = "Text" },
			};

			var label = new Label { Text = "Label" };
			var layout = new VerticalStackLayout { WidthRequest = 300, Children = { input, label } };
			var withFlyout = flyoutOnParent ? (View)layout : input;
			FlyoutBase.SetContextFlyout(withFlyout, CreateFlyout(out _));

			await AttachAndRun(layout, _ =>
			{
				var interactionView = (UIView)withFlyout.Handler!.PlatformView!;
				var interaction = GetInteraction(interactionView)!;
				var inputView = (UIView)input.Handler!.PlatformView!;
				UIView textView = inputView is MauiSearchBar searchBar ? searchBar.GetSearchTextField()! : inputView;
				interactionView.LayoutIfNeeded();
				inputView.LayoutIfNeeded();
				Assert.True(textView.Bounds.Width > 0 && textView.Bounds.Height > 0);

				// A long press on the text keeps selecting text
				var onText = textView.ConvertPointToView(new CoreGraphics.CGPoint(textView.Bounds.Width / 2, textView.Bounds.Height / 2), interactionView);
				var configuration = interaction.GetConfigurationForMenu(onText);

				if (!OperatingSystem.IsIOSVersionAtLeast(16) && !OperatingSystem.IsMacCatalystVersionAtLeast(16))
				{
					// The edit menu can only be extended from iOS 16; before that the context menu is the only way to the items
					Assert.NotNull(configuration);
					return;
				}

				Assert.Null(configuration);

				// Other views keep opening the context menu
				if (flyoutOnParent)
				{
					var labelView = (UIView)label.Handler!.PlatformView!;
					var onLabel = labelView.ConvertPointToView(new CoreGraphics.CGPoint(labelView.Bounds.Width / 2, labelView.Bounds.Height / 2), interactionView);
					Assert.NotNull(interaction.GetConfigurationForMenu(onLabel));
				}

				// The edit menu keeps the system's actions and adds the flyout items after them
				var suggested = new UIMenuElement[] { UIAction.Create("System", null, null, _ => { }) };
				var menu = GetEditMenu(textView, suggested);
				Assert.NotNull(menu);
				Assert.Equal("System", menu!.Children[0].Title);
				var items = Assert.IsType<UIMenu>(menu.Children[^1]);
				Assert.True(items.Options.HasFlag(UIMenuOptions.DisplayInline));
				Assert.Equal(new[] { "Copy", "Disabled", "Edit" }, GetAllTitles(items).Where(t => t is "Copy" or "Disabled" or "Edit").ToArray());

				// Without a flyout the edit menu is the system's
				FlyoutBase.SetContextFlyout(withFlyout, null);
				var systemMenu = GetEditMenu(textView, suggested);
				Assert.True(systemMenu is null || systemMenu.Children.All(e => e.Title == "System"));
			});
		}

		[Fact(DisplayName = "With mobile context flyouts turned off, iOS has no context menus and Mac Catalyst works like before")]
		public async Task MobileContextFlyoutsCanBeTurnedOff()
		{
			SetupBuilder();
			var label = new Label { Text = "Target" };
			var entry = new Entry { Text = "Text" };
			FlyoutBase.SetContextFlyout(label, CreateFlyout(out _));
			FlyoutBase.SetContextFlyout(entry, CreateFlyout(out _));
			var root = new VerticalStackLayout { WidthRequest = 300, Children = { label, entry } };

			await WithMobileContextFlyoutsDisabled(() => AttachAndRun(root, _ =>
			{
				var labelView = (UIView)label.Handler!.PlatformView!;
				var textField = (UIView)entry.Handler!.PlatformView!;

				if (!OperatingSystem.IsMacCatalyst())
				{
					Assert.Null(GetInteraction(labelView));
					Assert.Null(GetInteraction(textField));
					return;
				}

				// Mac Catalyst: the text input opens its context menu, and its edit menu is the system's
				var interaction = GetInteraction(textField)!;
				textField.LayoutIfNeeded();
				Assert.NotNull(interaction.GetConfigurationForMenu(new CoreGraphics.CGPoint(textField.Bounds.Width / 2, textField.Bounds.Height / 2)));

				var suggested = new UIMenuElement[] { UIAction.Create("System", null, null, _ => { }) };
				var menu = GetEditMenu(textField, suggested);
				Assert.True(menu is null || menu.Children.All(e => e.Title == "System"));
			}));
		}

		[Fact(DisplayName = "Actions of an edit menu keep working after the menu was requested again")]
		public async Task EarlierEditMenuActionsKeepWorking()
		{
			SetupBuilder();

			var clicks = 0;
			var flyout = CreateFlyout(out _);
			((MenuFlyoutItem)flyout[0]).Clicked += (_, _) => clicks++;
			var entry = new Entry { Text = "Text" };
			FlyoutBase.SetContextFlyout(entry, flyout);

			await AttachAndRun(entry, handler =>
			{
				if (!OperatingSystem.IsIOSVersionAtLeast(16) && !OperatingSystem.IsMacCatalystVersionAtLeast(16))
					return;

				var textView = (UIView)handler.PlatformView!;
				var suggested = Array.Empty<UIMenuElement>();

				// UIKit asks for the edit menu again (e.g. when the selection changes) while it can still show the first one
				var first = GetEditMenu(textView, suggested)!;
				GetEditMenu(textView, suggested);

				var copy = FindAction(first, "Copy");
				using var sender = new UIButton();
				sender.SendAction(copy);
				Assert.Equal(1, clicks);

				// An item disabled since the menu was built doesn't run
				((MenuFlyoutItem)flyout[0]).IsEnabled = false;
				sender.SendAction(copy);
				Assert.Equal(1, clicks);
			});
		}

		static UIAction FindAction(UIMenu menu, string title)
		{
			foreach (var element in menu.Children)
			{
				if (element is UIAction action && action.Title == title)
					return action;

				if (element is UIMenu child && FindActionOrNull(child, title) is UIAction found)
					return found;
			}

			throw new InvalidOperationException($"No action {title}");

			static UIAction? FindActionOrNull(UIMenu menu, string title)
			{
				try
				{
					return FindAction(menu, title);
				}
				catch (InvalidOperationException)
				{
					return null;
				}
			}
		}

		[Fact(DisplayName = "Read-only text that can't be selected keeps opening the context menu")]
		public async Task NonSelectableEditorOpensContextMenu()
		{
			SetupBuilder();

			var editor = new Editor { Text = "Text", IsReadOnly = true, HeightRequest = 80 };
			FlyoutBase.SetContextFlyout(editor, CreateFlyout(out _));

			await AttachAndRun(editor, handler =>
			{
				var textView = (MauiTextView)handler.PlatformView!;
				textView.Selectable = false;
				var interaction = GetInteraction(textView)!;

				Assert.NotNull(interaction.GetConfigurationForMenu(new CoreGraphics.CGPoint(textView.Bounds.Width / 2, textView.Bounds.Height / 2)));
			});
		}

		static UIMenu? GetEditMenu(UIView textView, UIMenuElement[] suggested)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(16) && !OperatingSystem.IsMacCatalystVersionAtLeast(16))
				return null;

			return textView switch
			{
				MauiTextField textField => textField.GetEditMenu(textField.SelectedTextRange ?? textField.GetTextRange(textField.BeginningOfDocument, textField.EndOfDocument)!, suggested),
				MauiTextView editor => editor.GetEditMenu(editor.SelectedTextRange ?? editor.GetTextRange(editor.BeginningOfDocument, editor.EndOfDocument)!, suggested),
				UITextField searchField => searchField.Delegate.GetEditMenu(searchField, new Foundation.NSRange(0, 0), suggested),
				_ => null,
			};
		}

		static MauiUIContextMenuInteraction? GetInteraction(UIView view) =>
			view.Interactions.OfType<MauiUIContextMenuInteraction>().SingleOrDefault();

		static string[] GetAllTitles(UIMenu menu) =>
			menu.Children.SelectMany(e => e is UIMenu child ? GetAllTitles(child).Prepend(child.Title) : new[] { e.Title }).ToArray();
	}
}
