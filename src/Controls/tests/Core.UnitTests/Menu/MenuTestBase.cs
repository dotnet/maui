#nullable enable
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
		public void UsingIndexUpdatesParent()
		{
			var menuBar = new TTestType();

			var child0 = new TChildType();
			var child1 = new TChildType();

			Assert.Null(child0.Parent);
			Assert.Null(child1.Parent);

			menuBar.Add(child0);

			Assert.Same(menuBar, child0.Parent);
			Assert.Null(child1.Parent);

			menuBar[0] = child1;

			Assert.Null(child0.Parent);
			Assert.Same(menuBar, child1.Parent);
		}

		[Fact]
		public void ClearUpdatesParent()
		{
			var menuBar = new TTestType();

			var child0 = new TChildType();
			var child1 = new TChildType();

			menuBar.Add(child0);
			menuBar.Add(child1);

			Assert.Same(menuBar, child0.Parent);
			Assert.Same(menuBar, child1.Parent);

			menuBar.Clear();

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
