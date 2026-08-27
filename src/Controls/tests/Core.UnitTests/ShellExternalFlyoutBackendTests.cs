using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	/// <summary>
	/// Covers the public contract an out-of-tree Shell backend needs in order to resolve the flyout item
	/// <see cref="DataTemplate"/> exactly like the in-box backends do.
	/// Everything in <see cref="FakeExternalShellFlyoutBackend"/> must be expressible with public API only.
	/// </summary>
	public class ShellExternalFlyoutBackendTests : ShellTestBase
	{
		/// <summary>
		/// Stand-in for a platform flyout adaptor living outside of this repository. It mirrors what the in-box
		/// backends do: use the application supplied template when there is one, otherwise use the backend's own
		/// platform-native flyout item presentation.
		/// </summary>
		class FakeExternalShellFlyoutBackend
		{
			readonly Shell _shell;

			public FakeExternalShellFlyoutBackend(Shell shell) => _shell = shell;

			public View Header => ((IShellController)_shell).FlyoutHeader;

			public View Footer => ((IShellController)_shell).FlyoutFooter;

			public List<List<Element>> GetFlyoutGroups() => ((IShellController)_shell).GenerateFlyoutGrouping();

			public IEnumerable<Element> GetFlyoutItems() => GetFlyoutGroups().SelectMany(g => g);

			public bool UsesPlatformDefault(BindableObject flyoutItem) =>
				Shell.ResolveFlyoutItemTemplate(_shell, flyoutItem) is null;

			public View CreateFlyoutItemView(BindableObject flyoutItem)
			{
				var template = Shell.ResolveFlyoutItemTemplate(_shell, flyoutItem);

				if (template is null)
					return CreatePlatformDefaultView(flyoutItem);

				var view = (View)template.SelectDataTemplate(flyoutItem, _shell).CreateContent();

				// Matches what the built-in Android, iOS, and Windows backends do.
				view.BindingContext = flyoutItem;
				view.Parent = _shell;
				return view;
			}

			static View CreatePlatformDefaultView(BindableObject flyoutItem) =>
				new Label { Text = PlatformDefaultText, BindingContext = flyoutItem };

			public const string PlatformDefaultText = "PlatformDefault";
		}

		static MenuItem AddMenuShellItem(Shell shell, MenuItem menuItem)
		{
			// Adding a MenuItem to Shell.Items wraps it in the internal MenuShellItem.
			shell.Items.Add(menuItem);
			return menuItem;
		}

		static Element FindFlyoutItem(FakeExternalShellFlyoutBackend backend, MenuItem menuItem) =>
			backend.GetFlyoutItems().Single(e => e == menuItem || e == menuItem.Parent);

		[Fact]
		public void ShellItemWithoutTemplateResolvesToNull()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem();
			shell.Items.Add(shellItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.Null(Shell.ResolveFlyoutItemTemplate(shell, shellItem));
			Assert.Equal(FakeExternalShellFlyoutBackend.PlatformDefaultText, ((Label)backend.CreateFlyoutItemView(shellItem)).Text);
		}

		[Fact]
		public void ShellLevelItemTemplateIsUsedForShellItems()
		{
			var shell = new Shell
			{
				ItemTemplate = new DataTemplate(() => new Label { Text = "ItemTemplate" })
			};

			var shellItem = CreateShellItem();
			shell.Items.Add(shellItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.Same(shell.ItemTemplate, Shell.ResolveFlyoutItemTemplate(shell, shellItem));
			Assert.Equal("ItemTemplate", ((Label)backend.CreateFlyoutItemView(shellItem)).Text);
		}

		[Fact]
		public void ItemLevelItemTemplateWinsOverShellLevelItemTemplate()
		{
			var shell = new Shell
			{
				ItemTemplate = new DataTemplate(() => new Label { Text = "ShellLevel" })
			};

			var shellItem = CreateShellItem();
			Shell.SetItemTemplate(shellItem, new DataTemplate(() => new Label { Text = "ItemLevel" }));
			shell.Items.Add(shellItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.Equal("ItemLevel", ((Label)backend.CreateFlyoutItemView(shellItem)).Text);
		}

		[Fact]
		public void MenuItemWithoutTemplateResolvesToNull()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem());
			var menuItem = AddMenuShellItem(shell, new MenuItem { Text = "Menu" });

			var backend = new FakeExternalShellFlyoutBackend(shell);
			var flyoutItem = FindFlyoutItem(backend, menuItem);

			Assert.Null(Shell.ResolveFlyoutItemTemplate(shell, flyoutItem));
			Assert.Equal(FakeExternalShellFlyoutBackend.PlatformDefaultText, ((Label)backend.CreateFlyoutItemView(flyoutItem)).Text);
		}

		[Fact]
		public void ShellLevelMenuItemTemplateIsUsedForMenuItems()
		{
			var shell = new Shell
			{
				MenuItemTemplate = new DataTemplate(() => new Label { Text = "MenuItemTemplate" }),
				ItemTemplate = new DataTemplate(() => new Label { Text = "ItemTemplate" })
			};

			var shellItem = CreateShellItem();
			shell.Items.Add(shellItem);
			var menuItem = AddMenuShellItem(shell, new MenuItem { Text = "Menu" });

			var backend = new FakeExternalShellFlyoutBackend(shell);
			var flyoutItem = FindFlyoutItem(backend, menuItem);

			Assert.Equal("MenuItemTemplate", ((Label)backend.CreateFlyoutItemView(flyoutItem)).Text);
			Assert.Equal("ItemTemplate", ((Label)backend.CreateFlyoutItemView(shellItem)).Text);
		}

		// This is the branch an external backend cannot reproduce on its own: the template is set on the
		// MenuItem, but the flyout item handed to the backend is the internal MenuShellItem wrapper, which does
		// not have the property set.
		[Fact]
		public void MenuItemTemplateSetOnMenuItemIsFoundThroughMenuShellItem()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem());

			var menuItem = new MenuItem { Text = "Menu" };
			var template = new DataTemplate(() => new Label { Text = "OnMenuItem" });
			Shell.SetMenuItemTemplate(menuItem, template);
			AddMenuShellItem(shell, menuItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);
			var flyoutItem = FindFlyoutItem(backend, menuItem);

			Assert.NotSame(menuItem, flyoutItem);
			Assert.Same(template, Shell.ResolveFlyoutItemTemplate(shell, flyoutItem));
			Assert.Equal("OnMenuItem", ((Label)backend.CreateFlyoutItemView(flyoutItem)).Text);
		}

		// Same gap seen from the other side: the flyout item is the MenuItem itself and the template lives on
		// its parent.
		[Fact]
		public void MenuItemTemplateSetOnParentIsFoundThroughMenuItem()
		{
			var shell = new Shell();
			var shellContent = CreateShellContent();

			var menuItem = new MenuItem { Text = "Menu" };
			shellContent.MenuItems.Add(menuItem);

			var template = new DataTemplate(() => new Label { Text = "OnParent" });
			Shell.SetMenuItemTemplate(shellContent, template);

			shell.Items.Add(shellContent);

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.Contains(menuItem, backend.GetFlyoutItems());
			Assert.False(menuItem.IsSet(Shell.MenuItemTemplateProperty));
			Assert.Same(template, Shell.ResolveFlyoutItemTemplate(shell, menuItem));
			Assert.Equal("OnParent", ((Label)backend.CreateFlyoutItemView(menuItem)).Text);
		}

		// Direct parent-resolution unit coverage. GenerateFlyoutGrouping hands a backend the MenuShellItem
		// wrapper for items added to Shell.Items, not the bare MenuItem, so this exercises the resolver's
		// parent lookup directly rather than a shape a backend receives from grouping.
		[Fact]
		public void ResolverFindsTemplateOnTheParentOfAMenuItem()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem());

			var menuItem = new MenuItem { Text = "Menu" };
			AddMenuShellItem(shell, menuItem);

			var template = new DataTemplate(() => new Label { Text = "OnParent" });
			Shell.SetMenuItemTemplate(menuItem.Parent, template);

			Assert.False(menuItem.IsSet(Shell.MenuItemTemplateProperty));
			Assert.Same(template, Shell.ResolveFlyoutItemTemplate(shell, menuItem));
		}

		// Direct resolver coverage, same caveat as above.
		[Fact]
		public void ResolverReturnsNullForAMenuItemWithNoTemplateAnywhere()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem());

			var menuItem = new MenuItem { Text = "Menu" };
			AddMenuShellItem(shell, menuItem);

			Assert.Null(Shell.ResolveFlyoutItemTemplate(shell, menuItem));
		}

		// Unlike Shell.Items, ShellContent.MenuItems puts the bare MenuItem in the flyout grouping, so a
		// template set directly on the MenuItem is a shape backends really do receive.
		[Fact]
		public void MenuItemTemplateSetOnTheMenuItemIsUsedForShellContentMenuItems()
		{
			var shell = new Shell();
			var shellContent = CreateShellContent();

			var menuItem = new MenuItem { Text = "Menu" };
			var template = new DataTemplate(() => new Label { Text = "OnMenuItem" });
			Shell.SetMenuItemTemplate(menuItem, template);

			shellContent.MenuItems.Add(menuItem);
			shell.Items.Add(shellContent);

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.Contains(menuItem, backend.GetFlyoutItems());
			Assert.Same(template, Shell.ResolveFlyoutItemTemplate(shell, menuItem));
			Assert.Equal("OnMenuItem", ((Label)backend.CreateFlyoutItemView(menuItem)).Text);
		}

		// Pins pre-existing behavior: when both the MenuItem and its parent set a template, the parent wins.
		// This is long-standing Shell behavior that the public resolver preserves verbatim.
		[Fact]
		public void ParentTemplateWinsOverMenuItemTemplateForShellContentMenuItems()
		{
			var shell = new Shell();
			var shellContent = CreateShellContent();

			var menuItem = new MenuItem { Text = "Menu" };
			Shell.SetMenuItemTemplate(menuItem, new DataTemplate(() => new Label { Text = "OnMenuItem" }));

			var parentTemplate = new DataTemplate(() => new Label { Text = "OnParent" });
			Shell.SetMenuItemTemplate(shellContent, parentTemplate);

			shellContent.MenuItems.Add(menuItem);
			shell.Items.Add(shellContent);

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.Same(parentTemplate, Shell.ResolveFlyoutItemTemplate(shell, menuItem));
			Assert.Equal("OnParent", ((Label)backend.CreateFlyoutItemView(menuItem)).Text);
		}

		[Fact]
		public void MenuItemTemplateBindsTextAndCommandForShellContentMenuItems()
		{
			bool invoked = false;

			var shell = new Shell();
			var shellContent = CreateShellContent();

			var menuItem = new MenuItem
			{
				Text = "Menu",
				Command = new Command(() => invoked = true)
			};

			shellContent.MenuItems.Add(menuItem);

			Shell.SetMenuItemTemplate(shellContent, new DataTemplate(() =>
			{
				var button = new Button();
				button.SetBinding(Button.TextProperty, static (MenuItem item) => item.Text);
				button.SetBinding(Button.CommandProperty, static (MenuItem item) => item.Command);
				return button;
			}));

			shell.Items.Add(shellContent);

			var backend = new FakeExternalShellFlyoutBackend(shell);
			var button = Assert.IsType<Button>(backend.CreateFlyoutItemView(menuItem));

			Assert.Equal("Menu", button.Text);

			Assert.NotNull(button.Command);
			button.Command.Execute(null);
			Assert.True(invoked);
		}

		// Shell level MenuItemTemplate is applied to the MenuShellItem wrapper, which mirrors Text/Title from
		// its MenuItem but does not surface Command. Selection is what activates the command; see
		// MenuItemFlyoutSelectionInvokesTheCommand.
		[Fact]
		public void MenuItemTemplateBindsTextForShellLevelMenuItems()
		{
			var shell = new Shell
			{
				MenuItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, "Text");
					return label;
				})
			};

			shell.Items.Add(CreateShellItem());

			var menuItem = new MenuItem { Text = "Menu" };
			AddMenuShellItem(shell, menuItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);
			var flyoutItem = FindFlyoutItem(backend, menuItem);
			var label = Assert.IsType<Label>(backend.CreateFlyoutItemView(flyoutItem));

			Assert.Equal("Menu", label.Text);
		}

		[Fact]
		public void MenuItemFlyoutSelectionInvokesTheCommand()
		{
			bool invoked = false;

			var shell = new Shell();
			shell.Items.Add(CreateShellItem());

			var menuItem = new MenuItem
			{
				Text = "Menu",
				Command = new Command(() => invoked = true)
			};

			AddMenuShellItem(shell, menuItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);
			var flyoutItem = FindFlyoutItem(backend, menuItem);

			((IShellController)shell).OnFlyoutItemSelected(flyoutItem);

			Assert.True(invoked);
		}

		[Fact]
		public void ItemTemplateBindsTitleForShellItems()
		{
			var shell = new Shell
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, static (BaseShellItem item) => item.Title);
					return label;
				})
			};

			var shellItem = CreateShellItem();
			shellItem.Title = "Cat";
			shell.Items.Add(shellItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.Equal("Cat", ((Label)backend.CreateFlyoutItemView(shellItem)).Text);
		}

		[Fact]
		public void ExplicitNullItemTemplateOnItemOptsOutOfShellTemplate()
		{
			var shell = new Shell
			{
				ItemTemplate = new DataTemplate(() => new Label { Text = "ShellLevel" })
			};

			var shellItem = CreateShellItem();
			Shell.SetItemTemplate(shellItem, null);
			shell.Items.Add(shellItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.True(shellItem.IsSet(Shell.ItemTemplateProperty));
			Assert.Null(Shell.ResolveFlyoutItemTemplate(shell, shellItem));
			Assert.True(backend.UsesPlatformDefault(shellItem));
			Assert.Equal(FakeExternalShellFlyoutBackend.PlatformDefaultText, ((Label)backend.CreateFlyoutItemView(shellItem)).Text);
		}

		[Fact]
		public void ExplicitNullTemplateStillProducesDefaultCellForBuiltInBackends()
		{
			var shell = new Shell
			{
				ItemTemplate = new DataTemplate(() => new Label { Text = "ShellLevel" })
			};

			var shellItem = CreateShellItem();
			Shell.SetItemTemplate(shellItem, null);
			shell.Items.Add(shellItem);

			// The in-box path must never hand a null template to platform code.
			Assert.NotNull(((IShellController)shell).GetFlyoutItemDataTemplate(shellItem));
		}

		[Fact]
		public void ExplicitNullMenuItemTemplateOptsOutOfShellTemplate()
		{
			var shell = new Shell
			{
				MenuItemTemplate = new DataTemplate(() => new Label { Text = "ShellLevel" })
			};

			shell.Items.Add(CreateShellItem());

			var menuItem = new MenuItem { Text = "Menu" };
			Shell.SetMenuItemTemplate(menuItem, null);
			AddMenuShellItem(shell, menuItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);
			var flyoutItem = FindFlyoutItem(backend, menuItem);

			Assert.Null(Shell.ResolveFlyoutItemTemplate(shell, flyoutItem));
			Assert.Equal(FakeExternalShellFlyoutBackend.PlatformDefaultText, ((Label)backend.CreateFlyoutItemView(flyoutItem)).Text);
		}

		[Fact]
		public void NullBoundTemplateResolvesToNull()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem();

			// A binding that resolves to null must behave like no template rather than throwing.
			shellItem.BindingContext = new object();
			shellItem.SetBinding(Shell.ItemTemplateProperty, "MissingTemplate");

			shell.Items.Add(shellItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.Null(Shell.ResolveFlyoutItemTemplate(shell, shellItem));
			Assert.Equal(FakeExternalShellFlyoutBackend.PlatformDefaultText, ((Label)backend.CreateFlyoutItemView(shellItem)).Text);
		}

		[Fact]
		public void DataTemplateSelectorIsReturnedAndResolvedByTheCaller()
		{
			var shell = new Shell
			{
				ItemTemplate = new TestFlyoutItemTemplateSelector()
			};

			var withTitle = CreateShellItem();
			withTitle.Title = "Cat";

			var withoutTitle = CreateShellItem();
			withoutTitle.Title = null;

			shell.Items.Add(withTitle);
			shell.Items.Add(withoutTitle);

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.IsType<TestFlyoutItemTemplateSelector>(Shell.ResolveFlyoutItemTemplate(shell, withTitle));
			Assert.Equal("HasTitle", ((Label)backend.CreateFlyoutItemView(withTitle)).Text);
			Assert.Equal("NoTitle", ((Label)backend.CreateFlyoutItemView(withoutTitle)).Text);
		}

		class TestFlyoutItemTemplateSelector : DataTemplateSelector
		{
			readonly DataTemplate _hasTitle = new DataTemplate(() => new Label { Text = "HasTitle" });
			readonly DataTemplate _noTitle = new DataTemplate(() => new Label { Text = "NoTitle" });

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
				item is BaseShellItem bsi && !string.IsNullOrEmpty(bsi.Title) ? _hasTitle : _noTitle;
		}

		[Fact]
		public void FlyoutHeaderAndFooterAreAvailableToExternalBackends()
		{
			var header = new Label { Text = "Header" };
			var footer = new Label { Text = "Footer" };

			var shell = new Shell
			{
				FlyoutHeader = header,
				FlyoutFooter = footer
			};

			shell.Items.Add(CreateShellItem());

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.Same(header, backend.Header);
			Assert.Same(footer, backend.Footer);
		}

		[Fact]
		public void FlyoutHeaderAndFooterTemplatesAreAvailableToExternalBackends()
		{
			var shell = new Shell
			{
				FlyoutHeaderTemplate = new DataTemplate(() => new Label { Text = "HeaderTemplate" }),
				FlyoutFooterTemplate = new DataTemplate(() => new Label { Text = "FooterTemplate" })
			};

			shell.Items.Add(CreateShellItem());

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.Equal("HeaderTemplate", Assert.IsType<Label>(backend.Header).Text);
			Assert.Equal("FooterTemplate", Assert.IsType<Label>(backend.Footer).Text);
		}

		[Fact]
		public void HeadersAreNotFlyoutItems()
		{
			var shell = new Shell
			{
				FlyoutHeader = new Label { Text = "Header" },
				ItemTemplate = new DataTemplate(() => new Label { Text = "ItemTemplate" })
			};

			shell.Items.Add(CreateShellItem());

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.DoesNotContain(backend.Header, backend.GetFlyoutItems());
		}

		[Fact]
		public void OmittingShellResolvesItThroughTheFlyoutItem()
		{
			var shell = new Shell
			{
				ItemTemplate = new DataTemplate(() => new Label { Text = "ItemTemplate" })
			};

			var shellItem = CreateShellItem();
			shell.Items.Add(shellItem);

			Assert.Same(shell.ItemTemplate, Shell.ResolveFlyoutItemTemplate(null, shellItem));
		}

		[Fact]
		public void UnparentedItemWithoutTemplateResolvesToNull()
		{
			var shellItem = CreateShellItem();

			Assert.Null(Shell.ResolveFlyoutItemTemplate(null, shellItem));
		}

		[Fact]
		public void UnparentedItemWithItemTemplateStillResolves()
		{
			var shellItem = CreateShellItem();
			var template = new DataTemplate(() => new Label { Text = "ItemLevel" });
			Shell.SetItemTemplate(shellItem, template);

			Assert.Same(template, Shell.ResolveFlyoutItemTemplate(null, shellItem));
		}

		[Fact]
		public void NullFlyoutItemThrows()
		{
			Assert.Throws<System.ArgumentNullException>(() => Shell.ResolveFlyoutItemTemplate(new Shell(), null));
			Assert.Throws<System.ArgumentNullException>(() => Shell.ResolveFlyoutItemTemplate(null, null));
		}
	}
}
