using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ShellFlyoutItemGroupTests : ShellTestBase
	{
		[Fact]
		public void FlyoutCreatesCorrectNumberOfGroupsForAsMultipleItems()
		{
			var shell = new Shell();
			var shellItem = new ShellItem() { FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems, };
			var shellItem2 = new ShellItem();

			shellItem.Items.Add(CreateShellSection());
			shellItem.Items.Add(CreateShellSection());
			shellItem2.Items.Add(CreateShellContent());
			shellItem2.Items.Add(CreateShellSection());

			shell.Items.Add(shellItem);
			shell.Items.Add(shellItem2);
			IShellController shellController = shell;
			var groups = shellController.GenerateFlyoutGrouping();

			Assert.Equal(2, groups.Count);
			Assert.Equal(2, groups[0].Count);
			Assert.Single(groups[1]);
		}

		[Fact]
		public void FlyoutCreatesCorrectNumberOfGroupsForNestedAsMultipleItems()
		{
			var shell = new Shell();
			var shellItem = new ShellItem() { FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems, };
			var shellItem2 = new ShellItem() { FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems, };

			shellItem.Items.Add(CreateShellSection());
			shellItem.Items.Add(CreateShellSection());

			shellItem2.Items.Add(CreateShellContent());
			shellItem2.Items.Add(CreateShellSection());

			shell.Items.Add(shellItem);
			shell.Items.Add(shellItem2);

			IShellController shellController = shell;
			var groups = shellController.GenerateFlyoutGrouping();

			Assert.Equal(2, groups.Count);
			Assert.Equal(2, groups[0].Count);
			Assert.Equal(2, groups[1].Count);
		}

		[Fact]
		public void FlyoutCreatesCorrectNumberOfGroupsForAsSingleItem()
		{
			var shell = new Shell();
			var shellItem = new ShellItem() { FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem, };
			var shellItem2 = new ShellItem() { FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem, };

			shellItem.Items.Add(CreateShellSection());
			shellItem.Items.Add(CreateShellSection());
			shellItem2.Items.Add(CreateShellContent());
			shellItem2.Items.Add(CreateShellSection());


			shell.Items.Add(shellItem);
			shell.Items.Add(shellItem2);
			IShellController shellController = shell;
			var groups = shellController.GenerateFlyoutGrouping();

			Assert.Single(groups);
			Assert.Equal(2, groups[0].Count);
		}

		[Fact]
		public void MenuItemGeneratesForShellContent()
		{
			var shell = new TestShell();

			var shellContent = CreateShellContent();
			shellContent.MenuItems.Add(new MenuItem());
			shell.Items.Add(shellContent);

			IShellController shellController = (IShellController)shell;
			var groups = shellController.GenerateFlyoutGrouping();
			Assert.Single(groups.SelectMany(x => x.OfType<IMenuItemController>()));
		}


		[Fact]
		public void MenuItemGeneratesForShellSection()
		{
			var shell = new TestShell();

			var shellSection = CreateShellSection<Tab>();
			shellSection.CurrentItem.MenuItems.Add(new MenuItem());
			shellSection.FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;
			shell.Items.Add(shellSection);

			IShellController shellController = (IShellController)shell;
			var groups = shellController.GenerateFlyoutGrouping();
			Assert.Single(groups.SelectMany(x => x.OfType<IMenuItemController>()));
		}


		[Fact]
		public void FlyoutItemVisibleWorksForMenuItemsAddedAsShellItem()
		{
			var shell = new TestShell();
			var item = new MenuShellItem(CreateNonVisibleMenuItem());
			shell.Items.Add(item);

			var itemsAreEquals = item.Equals(shell.Items[0]);
			Assert.True(itemsAreEquals);

			IShellController shellController = (IShellController)shell;
			var groups = shellController.GenerateFlyoutGrouping();

			var r = groups.SelectMany(x => x.OfType<IMenuItemController>());

			Assert.Empty(r);
		}

		[Fact]
		public void FlyoutItemVisibleWorksForMenuItemsAddedAsTab()
		{
			var shell = new TestShell();

			var shellSection = CreateShellSection<Tab>();
			shellSection.Items[0].MenuItems.Add(CreateNonVisibleMenuItem());
			shell.Items.Add(shellSection);

			IShellController shellController = (IShellController)shell;
			var groups = shellController.GenerateFlyoutGrouping();
			Assert.Empty(groups.SelectMany(x => x.OfType<IMenuItemController>()));
		}

		[Fact]
		public void FlyoutItemVisibleWorksForMenuItemsAddedAsShellContent()
		{
			var shell = new TestShell();

			var shellContent = CreateShellContent();
			shellContent.MenuItems.Add(CreateNonVisibleMenuItem());
			shell.Items.Add(shellContent);

			IShellController shellController = (IShellController)shell;
			var groups = shellController.GenerateFlyoutGrouping();
			Assert.Empty(groups.SelectMany(x => x.OfType<IMenuItemController>()));
		}

		[Fact]
		public void FlyoutItemVisibleWorksForMenuItemsFlyoutItemAsMultipleItems()
		{
			var shell = new TestShell();

			var flyoutItem = CreateShellItem<FlyoutItem>();
			flyoutItem.FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;
			flyoutItem.CurrentItem.CurrentItem.MenuItems.Add(CreateNonVisibleMenuItem());
			shell.Items.Add(flyoutItem);

			var groups = shell.Controller.GenerateFlyoutGrouping();
			Assert.Empty(groups.SelectMany(x => x.OfType<IMenuItemController>()));
		}

		[Fact]
		public void FlyoutItemVisibleWorksForMenuItemsTabAsMultipleItems()
		{
			var shell = new TestShell();

			var flyoutItem = CreateShellItem<FlyoutItem>();
			flyoutItem.FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;
			flyoutItem.CurrentItem.FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;
			flyoutItem.CurrentItem.CurrentItem.MenuItems.Add(CreateNonVisibleMenuItem());
			shell.Items.Add(flyoutItem);

			IShellController shellController = (IShellController)shell;
			var groups = shellController.GenerateFlyoutGrouping();
			Assert.Empty(groups.SelectMany(x => x.OfType<IMenuItemController>()));
		}

		[Fact]
		public void FlyoutItemNotVisibleWhenShellContentSetToNotVisible()
		{
			var shell = new TestShell();
			var shellSection = CreateShellSection();
			shellSection.FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;
			shellSection.Items.Add(CreateShellContent());
			shellSection.Items[0].IsVisible = false;
			shell.Items.Add(shellSection);

			IShellController shellController = (IShellController)shell;
			var groups = shellController.GenerateFlyoutGrouping();
			Assert.Single(groups);
			Assert.Single(groups[0]);
		}

		[Fact]
		public void ReturnTheSameGroupingInstanceIfStructureHasntChanged()
		{
			var shell = new TestShell();

			shell.Items.Add(CreateShellItem<FlyoutItem>());

			var flyoutItems = shell.Controller.GenerateFlyoutGrouping();
			var flyoutItems2 = shell.Controller.GenerateFlyoutGrouping();

			Assert.Same(flyoutItems, flyoutItems2);

			shell.Items.Add(CreateShellItem<FlyoutItem>());
			flyoutItems2 = shell.Controller.GenerateFlyoutGrouping();

			Assert.NotSame(flyoutItems, flyoutItems2);
		}

		[Fact]
		public void FlyoutItemsBasicSyncTest()
		{
			var shell = new TestShell();
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			shell.Items[3].IsVisible = false;

			var flyoutItems = shell.GenerateTestFlyoutItems();
			Assert.Equal(shell.Items[0], flyoutItems[0][0]);
			Assert.Equal(shell.Items[1], flyoutItems[0][1]);
			Assert.Equal(shell.Items[2], flyoutItems[0][2]);
			Assert.Equal(3, flyoutItems[0].Count);
			Assert.Single(flyoutItems);
		}

		[Fact]
		public void FlyoutItemsGroupTest()
		{
			var shell = new TestShell();
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			var sec1 = shell.Items[0].Items[0];
			var sec2 = CreateShellSection<Tab>();
			var sec3 = CreateShellSection<Tab>();

			shell.Items[0].FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;
			shell.Items[0].Items.Add(sec2);
			shell.Items[0].Items.Add(sec3);

			var flyoutItems = shell.GenerateTestFlyoutItems();
			Assert.Equal(sec1, flyoutItems[0][0]);
			Assert.Equal(sec2, flyoutItems[0][1]);
			Assert.Equal(sec3, flyoutItems[0][2]);
			Assert.Equal(shell.Items[1], flyoutItems[1][0]);
		}

		[Fact]
		public void FlyoutItemsGroupTestWithRemove()
		{
			var shell = new TestShell();
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			var sec1 = shell.Items[0].Items[0];
			var sec2 = CreateShellSection<Tab>();
			var sec3 = CreateShellSection<Tab>();

			shell.Items[0].FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;
			shell.Items[0].Items.Add(sec2);
			shell.Items[0].Items.Add(sec3);
			shell.Items.RemoveAt(0);

			var flyoutItems = shell.GenerateTestFlyoutItems();
			Assert.Equal(shell.Items[0], flyoutItems[0][0]);
			Assert.Single(flyoutItems);
		}

		[Fact]
		public void FlyoutItemsGroupTestMoveGroup()
		{
			var shell = new TestShell();
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			var sec1 = shell.Items[0].Items[0];
			var sec2 = CreateShellSection<Tab>();
			var sec3 = CreateShellSection<Tab>();

			shell.Items[0].FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;
			shell.Items[0].Items.Add(sec2);
			shell.Items[0].Items.Add(sec3);

			var item1 = shell.Items[0];
			shell.Items.RemoveAt(0);
			shell.Items.Add(item1);
			var flyoutItems = shell.GenerateTestFlyoutItems();
			Assert.Equal(sec1, flyoutItems[1][0]);
			Assert.Equal(sec2, flyoutItems[1][1]);
			Assert.Equal(sec3, flyoutItems[1][2]);
			Assert.Equal(shell.Items[0], flyoutItems[0][0]);
		}

		MenuItem CreateNonVisibleMenuItem()
		{
			MenuItem item = new MenuItem();
			Shell.SetFlyoutItemIsVisible(item, false);
			return item;
		}
	}
}
