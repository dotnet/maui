using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
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


			IShellController shellController = (IShellController)shell;
			var groups = shellController.GenerateFlyoutGrouping();
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

		MenuItem CreateNonVisibleMenuItem()
		{
			MenuItem item = new MenuItem();
			FlyoutItem.SetIsVisible(item, false);
			return item;
		}
	}
}
