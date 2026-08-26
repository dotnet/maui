using System.Collections.Generic;
using System.Linq;
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
		/// platform default view.
		/// </summary>
		class FakeExternalShellFlyoutBackend
		{
			readonly Shell _shell;

			public FakeExternalShellFlyoutBackend(Shell shell) => _shell = shell;

			public View Header => ((IShellController)_shell).FlyoutHeader;

			public View Footer => ((IShellController)_shell).FlyoutFooter;

			public List<List<Element>> GetFlyoutGroups() => ((IShellController)_shell).GenerateFlyoutGrouping();

			public IEnumerable<Element> GetFlyoutItems() => GetFlyoutGroups().SelectMany(g => g);

			public DataTemplate SelectTemplate(BindableObject flyoutItem)
			{
				if (!Shell.IsFlyoutItemTemplateSet(_shell, flyoutItem))
					return PlatformDefaultTemplate;

				return ((IShellController)_shell).GetFlyoutItemDataTemplate(flyoutItem);
			}

			public View CreateFlyoutItemView(BindableObject flyoutItem)
			{
				var template = SelectTemplate(flyoutItem).SelectDataTemplate(flyoutItem, _shell);
				var view = (View)template.CreateContent();

				// Menu items are backed by two objects; the binding context has to come from the same object
				// that supplies the template, otherwise bindings inside the template resolve against the wrong item.
				view.BindingContext = Shell.GetFlyoutItemTemplateSource(flyoutItem);
				view.Parent = _shell;
				return view;
			}

			public static DataTemplate PlatformDefaultTemplate { get; } =
				new DataTemplate(() => new Label { Text = PlatformDefaultText });

			public const string PlatformDefaultText = "PlatformDefault";
		}

		static MenuItem AddMenuShellItem(Shell shell, MenuItem menuItem)
		{
			// This is the only public way to get at the internal MenuShellItem wrapper.
			shell.Items.Add(menuItem);
			return menuItem;
		}

		static Element FindFlyoutItem(FakeExternalShellFlyoutBackend backend, MenuItem menuItem) =>
			backend.GetFlyoutItems().Single(e => e == menuItem || e == menuItem.Parent);

		[Fact]
		public void ShellItemWithoutTemplateFallsBackToBackendDefault()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem();
			shell.Items.Add(shellItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.False(Shell.IsFlyoutItemTemplateSet(shell, shellItem));
			Assert.Same(FakeExternalShellFlyoutBackend.PlatformDefaultTemplate, backend.SelectTemplate(shellItem));
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

			Assert.True(Shell.IsFlyoutItemTemplateSet(shell, shellItem));
			Assert.Equal("ItemTemplate", ((Label)backend.CreateFlyoutItemView(shellItem)).Text);
		}

		[Fact]
		public void ItemLevelItemTemplateIsUsedForShellItems()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem();
			Shell.SetItemTemplate(shellItem, new DataTemplate(() => new Label { Text = "ItemLevel" }));
			shell.Items.Add(shellItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.True(Shell.IsFlyoutItemTemplateSet(shell, shellItem));
			Assert.Equal("ItemLevel", ((Label)backend.CreateFlyoutItemView(shellItem)).Text);
		}

		[Fact]
		public void MenuItemWithoutTemplateFallsBackToBackendDefault()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem());
			var menuItem = AddMenuShellItem(shell, new MenuItem { Text = "Menu" });

			var backend = new FakeExternalShellFlyoutBackend(shell);
			var flyoutItem = FindFlyoutItem(backend, menuItem);

			Assert.False(Shell.IsFlyoutItemTemplateSet(shell, flyoutItem));
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

		// This is the branch an external backend cannot reproduce without GetFlyoutItemTemplateSource:
		// the template is set on the MenuItem, but the flyout item handed to the backend is the internal
		// MenuShellItem wrapper, which does not have the property set.
		[Fact]
		public void MenuItemTemplateSetOnMenuItemIsFoundThroughMenuShellItem()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem());

			var menuItem = new MenuItem { Text = "Menu" };
			Shell.SetMenuItemTemplate(menuItem, new DataTemplate(() => new Label { Text = "OnMenuItem" }));
			AddMenuShellItem(shell, menuItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);
			var flyoutItem = FindFlyoutItem(backend, menuItem);

			Assert.NotSame(menuItem, flyoutItem);
			Assert.Same(menuItem, Shell.GetFlyoutItemTemplateSource(flyoutItem));
			Assert.True(Shell.IsFlyoutItemTemplateSet(shell, flyoutItem));
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
			Shell.SetMenuItemTemplate(shellContent, new DataTemplate(() => new Label { Text = "OnParent" }));

			shell.Items.Add(shellContent);

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.Contains(menuItem, backend.GetFlyoutItems());
			Assert.Same(shellContent, Shell.GetFlyoutItemTemplateSource(menuItem));
			Assert.True(Shell.IsFlyoutItemTemplateSet(shell, menuItem));
			Assert.Equal("OnParent", ((Label)backend.CreateFlyoutItemView(menuItem)).Text);
		}

		// Documents the failure mode this API exists to fix: an external backend that only inspects the flyout
		// item itself concludes that no template is set and incorrectly falls back to its own default view.
		[Fact]
		public void InspectingTheFlyoutItemDirectlyMissesMenuItemTemplates()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem());

			var menuItem = new MenuItem { Text = "Menu" };
			Shell.SetMenuItemTemplate(menuItem, new DataTemplate(() => new Label { Text = "OnMenuItem" }));
			AddMenuShellItem(shell, menuItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);
			var flyoutItem = FindFlyoutItem(backend, menuItem);

			Assert.False(flyoutItem.IsSet(Shell.MenuItemTemplateProperty));
			Assert.True(Shell.IsFlyoutItemTemplateSet(shell, flyoutItem));
		}

		[Fact]
		public void MenuItemTemplateSourceProvidesBindingContextForTemplate()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem());

			var menuItem = new MenuItem { Text = "Menu" };
			Shell.SetMenuItemTemplate(menuItem, new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, static (MenuItem item) => item.Text);
				return label;
			}));

			AddMenuShellItem(shell, menuItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);
			var flyoutItem = FindFlyoutItem(backend, menuItem);

			Assert.Equal("Menu", ((Label)backend.CreateFlyoutItemView(flyoutItem)).Text);
		}

		[Fact]
		public void TemplateSourceCarriesStyleClassForMenuItems()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem());

			var menuItem = new MenuItem { Text = "Menu", StyleClass = new[] { "fooClass" } };
			Shell.SetMenuItemTemplate(menuItem, new DataTemplate(() => new Label()));
			AddMenuShellItem(shell, menuItem);

			var backend = new FakeExternalShellFlyoutBackend(shell);
			var flyoutItem = FindFlyoutItem(backend, menuItem);

			var styleSource = Assert.IsType<MenuItem>(Shell.GetFlyoutItemTemplateSource(flyoutItem));

			Assert.Contains("fooClass", styleSource.StyleClass);
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
		public void FlyoutHeaderTemplateIsAvailableToExternalBackends()
		{
			var shell = new Shell
			{
				FlyoutHeaderTemplate = new DataTemplate(() => new Label { Text = "HeaderTemplate" })
			};

			shell.Items.Add(CreateShellItem());

			var backend = new FakeExternalShellFlyoutBackend(shell);

			Assert.Equal("HeaderTemplate", Assert.IsType<Label>(backend.Header).Text);
		}

		[Fact]
		public void TemplatePropertyMatchesItemKind()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem();
			shell.Items.Add(shellItem);
			var menuItem = AddMenuShellItem(shell, new MenuItem { Text = "Menu" });

			var backend = new FakeExternalShellFlyoutBackend(shell);
			var flyoutItem = FindFlyoutItem(backend, menuItem);

			Assert.Same(Shell.ItemTemplateProperty, Shell.GetFlyoutItemTemplateProperty(shellItem));
			Assert.Same(Shell.MenuItemTemplateProperty, Shell.GetFlyoutItemTemplateProperty(flyoutItem));
			Assert.Same(Shell.MenuItemTemplateProperty, Shell.GetFlyoutItemTemplateProperty(menuItem));
		}

		[Fact]
		public void TemplateSourceReturnsItemWhenNoTemplateIsSet()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem();
			shell.Items.Add(shellItem);
			var menuItem = AddMenuShellItem(shell, new MenuItem { Text = "Menu" });

			var backend = new FakeExternalShellFlyoutBackend(shell);
			var flyoutItem = FindFlyoutItem(backend, menuItem);

			Assert.Same(shellItem, Shell.GetFlyoutItemTemplateSource(shellItem));
			Assert.Same(flyoutItem, Shell.GetFlyoutItemTemplateSource(flyoutItem));
		}

		[Fact]
		public void NullFlyoutItemThrows()
		{
			Assert.Throws<System.ArgumentNullException>(() => Shell.GetFlyoutItemTemplateSource(null));
			Assert.Throws<System.ArgumentNullException>(() => Shell.GetFlyoutItemTemplateProperty(null));
			Assert.Throws<System.ArgumentNullException>(() => Shell.IsFlyoutItemTemplateSet(new Shell(), null));
		}

		[Fact]
		public void NullShellOnlyConsidersTemplatesSetOnTheItem()
		{
			var shell = new Shell
			{
				ItemTemplate = new DataTemplate(() => new Label { Text = "ItemTemplate" })
			};

			var shellItem = CreateShellItem();
			shell.Items.Add(shellItem);

			Assert.True(Shell.IsFlyoutItemTemplateSet(shell, shellItem));
			Assert.False(Shell.IsFlyoutItemTemplateSet(null, shellItem));
		}
	}
}
