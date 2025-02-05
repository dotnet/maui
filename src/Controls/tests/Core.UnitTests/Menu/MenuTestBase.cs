#nullable enable
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Menu
{
	public abstract class MenuTestBase<TTestType, TIChildType, TChildType, THandlerUpdate> : BaseTestFixture
		where TChildType : Element, TIChildType, new()
		where TTestType : class, Maui.IElement, IList<TIChildType>, new()
	{
		protected abstract void SetHandler(
			Maui.IElement element,
			List<(string Name, THandlerUpdate? Args)> events);

		protected abstract TIChildType GetItem(THandlerUpdate handlerUpdate);

		protected abstract int GetIndex(THandlerUpdate handlerUpdate);

		[Fact]
		public void IconImageSourceAddsCorrectlyToEachSetOfLogicalChildren()
		{
			var menuBar = new TTestType();

			var child0 = new TChildType();
			var child1 = new TChildType();

			menuBar.Add(child0);
			menuBar.Add(child1);

			if (menuBar is MenuItem mi0)
				mi0.IconImageSource = new FileImageSource { File = "coffee.png" };

			if (child0 is MenuItem mi1)
				mi1.IconImageSource = new FileImageSource { File = "coffee.png" };

			if (child1 is MenuItem mi2)
				mi2.IconImageSource = new FileImageSource { File = "coffee.png" };

			foreach (var item in menuBar)
			{
				Assert.NotNull(item);
				if (item is IList list)
				{
					foreach (var child in list)
					{
						Assert.NotNull(child);
					}
				}
			}
		}


		[Fact]
		public void UsingIndexUpdatesParent()
		{
			var menuBar = new TTestType();

			var child0 = new TChildType();
			var child1 = new TChildType();

			Assert.Null(child0.Parent);
			Assert.Null(child1.Parent);

			menuBar.Add(child0);

			// Menu Bar Items only get parented to pages currently
			if (typeof(TChildType) == typeof(MenuBarItem))
			{
				var cp = new ContentPage();
				cp.MenuBarItems.Add(child0 as MenuBarItem);
				Assert.Same(cp, child0.Parent);
			}
			else
			{
				Assert.Same(menuBar, child0.Parent);
			}

			Assert.Null(child1.Parent);

			// Menu Bar Items only get parented to pages currently
			if (typeof(TChildType) == typeof(MenuBarItem))
			{
				(child0.Parent as ContentPage)!.MenuBarItems[0] = child1 as MenuBarItem;
			}
			else
			{
				menuBar[0] = child1;
			}

			Assert.Null(child0.Parent);

			if (typeof(TChildType) == typeof(MenuBarItem))
			{
				Assert.True(child1.Parent is ContentPage);
			}
			else
			{
				Assert.Same(menuBar, child1.Parent);
			}
		}

		[Fact]
		public void ClearUpdatesParent()
		{
			var menuBar = new TTestType();

			var child0 = new TChildType();
			var child1 = new TChildType();

			// Menu Bar Items only get parented to pages currently
			if (typeof(TChildType) == typeof(MenuBarItem))
			{
				// this sets up the MenuBarTracker
				var cp = new ContentPage();
				_ = new Window()
				{
					Page = cp
				};

				cp.MenuBarItems.Add(child0 as MenuBarItem);
				cp.MenuBarItems.Add(child1 as MenuBarItem);

				Assert.Same(cp, child0.Parent);
				Assert.Same(cp, child1.Parent);
			}
			else
			{
				menuBar.Add(child0);
				menuBar.Add(child1);

				Assert.Same(menuBar, child0.Parent);
				Assert.Same(menuBar, child1.Parent);
			}

			if (typeof(TChildType) == typeof(MenuBarItem))
			{
				(child0.Parent as ContentPage)!.MenuBarItems.Clear();
			}
			else
			{
				menuBar.Clear();
			}

			Assert.Null(child0.Parent);
			Assert.Null(child1.Parent);
		}

		[Fact]
		public void AddCallsCorrectHandlerMethod()
		{
			var events = new List<(string Name, THandlerUpdate? Args)>();

			var menuBar = new TTestType();
			SetHandler(menuBar, events);

			events.Clear();

			var child0 = new TChildType();

			menuBar.Add(child0);

			Assert.Single(events);
			var (name, args) = events[0];
			Assert.Equal(nameof(IMenuBarHandler.Add), name);
			Assert.Equal(0, GetIndex(args!));
			Assert.Equal(child0, GetItem(args!));
		}

		[Fact]
		public void RemoveCallsCorrectHandlerMethod()
		{
			var events = new List<(string Name, THandlerUpdate? Args)>();

			var menuBar = new TTestType();
			SetHandler(menuBar, events);

			var child0 = new TChildType();
			menuBar.Add(child0);

			events.Clear();

			menuBar.Remove(child0);

			Assert.Single(events);
			var (name, args) = events[0];
			Assert.Equal(nameof(IMenuBarHandler.Remove), name);
			Assert.Equal(0, GetIndex(args!));
			Assert.Equal(child0, GetItem(args!));
		}

		[Fact]
		public void InsertCallsCorrectHandlerMethod()
		{
			var events = new List<(string Name, THandlerUpdate? Args)>();

			var menuBar = new TTestType();
			SetHandler(menuBar, events);

			var child0 = new TChildType();
			var child1 = new TChildType();
			var child2 = new TChildType();

			menuBar.Add(child0);
			menuBar.Add(child2);

			events.Clear();

			menuBar.Insert(1, child1);

			Assert.Single(events);
			var (name, args) = events[0];
			Assert.Equal(nameof(IMenuBarHandler.Insert), name);
			Assert.Equal(1, GetIndex(args!));
			Assert.Equal(child1, GetItem(args!));
		}
	}
}
