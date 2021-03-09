using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ShellFlyoutItemGroupTests : ShellTestBase
	{
		[Test]
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

			Assert.AreEqual(groups.Count, 2);
			Assert.AreEqual(groups[0].Count, 2);
			Assert.AreEqual(groups[1].Count, 1);
		}

		[Test]
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

			Assert.AreEqual(2, groups.Count);
			Assert.AreEqual(groups[0].Count, 2);
			Assert.AreEqual(groups[1].Count, 2);
		}

		[Test]
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

			Assert.AreEqual(groups.Count, 1);
			Assert.AreEqual(groups[0].Count, 2);
		}

		[Test]
		public void MenuItemGeneratesForShellContent()
		{
			var shell = new TestShell();

			var shellContent = CreateShellContent();
			shellContent.MenuItems.Add(new MenuItem());
			shell.Items.Add(shellContent);

			IShellController shellController = (IShellController)shell;
			var groups = shellController.GenerateFlyoutGrouping();
			Assert.AreEqual(groups.SelectMany(x => x.OfType<IMenuItemController>()).Count(), 1);
		}


		[Test]
		public void MenuItemGeneratesForShellSection()
		{
			var shell = new TestShell();

			var shellSection = CreateShellSection<Tab>();
			shellSection.CurrentItem.MenuItems.Add(new MenuItem());
			shellSection.FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;
			shell.Items.Add(shellSection);

			IShellController shellController = (IShellController)shell;
			var groups = shellController.GenerateFlyoutGrouping();
			Assert.AreEqual(1, groups.SelectMany(x => x.OfType<IMenuItemController>()).Count());
		}


		[Test]
		public void FlyoutItemVisibleWorksForMenuItemsAddedAsShellItem()
		{
			var shell = new TestShell();
			var item = new MenuShellItem(CreateNonVisibleMenuItem());
			shell.Items.Add(item);

			var itemsAreEquals = item.Equals(shell.Items[0]);
			Assert.IsTrue(itemsAreEquals);

			IShellController shellController = (IShellController)shell;
			var groups = shellController.GenerateFlyoutGrouping();

			var r = groups.SelectMany(x => x.OfType<IMenuItemController>());

			Assert.AreEqual(r.Count(), 0);
		}

		[Test]
		public void FlyoutItemVisibleWorksForMenuItemsAddedAsTab()
		{
			var shell = new TestShell();

			var shellSection = CreateShellSection<Tab>();
			shellSection.Items[0].MenuItems.Add(CreateNonVisibleMenuItem());
			shell.Items.Add(shellSection);

			IShellController shellController = (IShellController)shell;
			var groups = shellController.GenerateFlyoutGrouping();
			Assert.AreEqual(groups.SelectMany(x => x.OfType<IMenuItemController>()).Count(), 0);
		}

		[Test]
		public void FlyoutItemVisibleWorksForMenuItemsAddedAsShellContent()
		{
			var shell = new TestShell();

			var shellContent = CreateShellContent();
			shellContent.MenuItems.Add(CreateNonVisibleMenuItem());
			shell.Items.Add(shellContent);

			IShellController shellController = (IShellController)shell;
			var groups = shellController.GenerateFlyoutGrouping();
			Assert.AreEqual(groups.SelectMany(x => x.OfType<IMenuItemController>()).Count(), 0);
		}

		[Test]
		public void FlyoutItemVisibleWorksForMenuItemsFlyoutItemAsMultipleItems()
		{
			var shell = new TestShell();

			var flyoutItem = CreateShellItem<FlyoutItem>();
			flyoutItem.FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;
			flyoutItem.CurrentItem.CurrentItem.MenuItems.Add(CreateNonVisibleMenuItem());
			shell.Items.Add(flyoutItem);

			var groups = shell.Controller.GenerateFlyoutGrouping();
			Assert.AreEqual(groups.SelectMany(x => x.OfType<IMenuItemController>()).Count(), 0);
		}

		[Test]
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
			Assert.AreEqual(0, groups.SelectMany(x => x.OfType<IMenuItemController>()).Count());
		}

		[Test]
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
			Assert.AreEqual(1, groups.Count);
			Assert.AreEqual(1, groups[0].Count);
		}

		[Test]
		public void ReturnTheSameGroupingInstanceIfStructureHasntChanged()
		{
			var shell = new TestShell();

			shell.Items.Add(CreateShellItem<FlyoutItem>());

			var flyoutItems = shell.Controller.GenerateFlyoutGrouping();
			var flyoutItems2 = shell.Controller.GenerateFlyoutGrouping();

			Assert.AreSame(flyoutItems, flyoutItems2);

			shell.Items.Add(CreateShellItem<FlyoutItem>());
			flyoutItems2 = shell.Controller.GenerateFlyoutGrouping();

			Assert.AreNotSame(flyoutItems, flyoutItems2);
		}

		[Test]
		public void FlyoutItemsBasicSyncTest()
		{
			var shell = new TestShell();
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			shell.Items.Add(CreateShellItem<FlyoutItem>());
			shell.Items[3].IsVisible = false;

			var flyoutItems = shell.GenerateTestFlyoutItems();
			Assert.AreEqual(shell.Items[0], flyoutItems[0][0]);
			Assert.AreEqual(shell.Items[1], flyoutItems[0][1]);
			Assert.AreEqual(shell.Items[2], flyoutItems[0][2]);
			Assert.AreEqual(3, flyoutItems[0].Count);
			Assert.AreEqual(1, flyoutItems.Count);
		}

		[Test]
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
			Assert.AreEqual(sec1, flyoutItems[0][0]);
			Assert.AreEqual(sec2, flyoutItems[0][1]);
			Assert.AreEqual(sec3, flyoutItems[0][2]);
			Assert.AreEqual(shell.Items[1], flyoutItems[1][0]);
		}

		[Test]
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
			Assert.AreEqual(shell.Items[0], flyoutItems[0][0]);
			Assert.AreEqual(1, flyoutItems.Count);
		}

		[Test]
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
			Assert.AreEqual(sec1, flyoutItems[1][0]);
			Assert.AreEqual(sec2, flyoutItems[1][1]);
			Assert.AreEqual(sec3, flyoutItems[1][2]);
			Assert.AreEqual(shell.Items[0], flyoutItems[0][0]);
		}

		MenuItem CreateNonVisibleMenuItem()
		{
			MenuItem item = new MenuItem();
			Shell.SetFlyoutItemIsVisible(item, false);
			return item;
		}
	}
}
