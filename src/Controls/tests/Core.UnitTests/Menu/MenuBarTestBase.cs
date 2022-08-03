#nullable enable
using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests.Menu
{
	public abstract class MenuBarTestBase<TTestType, TIChildType, TChildType, THandlerUpdate> : BaseTestFixture
		where TChildType : Element, TIChildType, new()
		where TTestType : class, Maui.IElement, IList<TIChildType>, new()
	{
		protected abstract void SetHandler(
			Maui.IElement element,
			List<(string Name, THandlerUpdate? Args)> events);

		protected abstract TIChildType GetItem(THandlerUpdate handlerUpdate);

		protected abstract int GetIndex(THandlerUpdate handlerUpdate);

		[Test]
		public void UsingIndexUpdatesParent()
		{
			var menuBar = new TTestType();

			var child0 = new TChildType();
			var child1 = new TChildType();

			Assert.Null(child0.Parent);
			Assert.Null(child1.Parent);

			menuBar.Add(child0);

			Assert.AreEqual(menuBar, child0.Parent);
			Assert.Null(child1.Parent);

			menuBar[0] = child1;

			Assert.Null(child0.Parent);
			Assert.AreEqual(menuBar, child1.Parent);
		}

		[Test]
		public void ClearUpdatesParent()
		{
			var menuBar = new TTestType();

			var child0 = new TChildType();
			var child1 = new TChildType();

			menuBar.Add(child0);
			menuBar.Add(child1);

			Assert.AreEqual(menuBar, child0.Parent);
			Assert.AreEqual(menuBar, child1.Parent);

			menuBar.Clear();

			Assert.Null(child0.Parent);
			Assert.Null(child1.Parent);
		}

		[Test]
		public void AddCallsCorrectHandlerMethod()
		{
			var events = new List<(string Name, THandlerUpdate? Args)>();

			var menuBar = new TTestType();
			SetHandler(menuBar, events);

			events.Clear();

			var child0 = new TChildType();

			menuBar.Add(child0);

			Assert.AreEqual(1, events.Count);
			var (name, args) = events[0];
			Assert.AreEqual(nameof(IMenuBarHandler.Add), name);
			Assert.AreEqual(0, GetIndex(args!));
			Assert.AreEqual(child0, GetItem(args!));
		}

		[Test]
		public void RemoveCallsCorrectHandlerMethod()
		{
			var events = new List<(string Name, THandlerUpdate? Args)>();

			var menuBar = new TTestType();
			SetHandler(menuBar, events);

			var child0 = new TChildType();
			menuBar.Add(child0);

			events.Clear();

			menuBar.Remove(child0);

			Assert.AreEqual(1, events.Count);
			var (name, args) = events[0];
			Assert.AreEqual(nameof(IMenuBarHandler.Remove), name);
			Assert.AreEqual(0, GetIndex(args!));
			Assert.AreEqual(child0, GetItem(args!));
		}

		[Test]
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

			Assert.AreEqual(1, events.Count);
			var (name, args) = events[0];
			Assert.AreEqual(nameof(IMenuBarHandler.Insert), name);
			Assert.AreEqual(1, GetIndex(args!));
			Assert.AreEqual(child1, GetItem(args!));
		}
	}
}
