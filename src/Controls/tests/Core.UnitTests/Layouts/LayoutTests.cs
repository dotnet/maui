#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using NUnit.Framework;
using NSubstitute;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	[TestFixture, Category("Layout")]
	public class LayoutTests : BaseTestFixture
	{
		[Test]
		public void UsingIndexUpdatesParent()
		{
			var layout = new VerticalStackLayout();

			var child0 = new Button();
			var child1 = new Button();

			Assert.Null(child0.Parent);
			Assert.Null(child1.Parent);

			layout.Add(child0);

			Assert.AreEqual(layout, child0.Parent);
			Assert.Null(child1.Parent);

			layout[0] = child1;

			Assert.Null(child0.Parent);
			Assert.AreEqual(layout, child1.Parent);
		}

		[Test]
		public void ClearUpdatesParent()
		{
			var layout = new VerticalStackLayout();

			var child0 = new Button();
			var child1 = new Button();

			layout.Add(child0);
			layout.Add(child1);

			Assert.AreEqual(layout, child0.Parent);
			Assert.AreEqual(layout, child1.Parent);

			layout.Clear();

			Assert.Null(child0.Parent);
			Assert.Null(child1.Parent);
		}

		[TestCase(typeof(VerticalStackLayout))]
		[TestCase(typeof(HorizontalStackLayout))]
		[TestCase(typeof(Grid))]
		[TestCase(typeof(StackLayout))]
		public void AddCallsCorrectHandlerMethod(Type TLayout)
		{
			var layout = CreateLayout(TLayout);
			var handler = Substitute.For<ILayoutHandler>();
			layout.Handler = handler;

			var child0 = new Button();
			layout.Add(child0);

			var command = Arg.Is(nameof(ILayoutHandler.Add));
			var args = Arg.Is<LayoutHandlerUpdate>(lhu => lhu.Index == 0 && lhu.View == child0);

			handler.Received().Invoke(command, args); 
		}

		[TestCase(typeof(VerticalStackLayout))]
		[TestCase(typeof(HorizontalStackLayout))]
		[TestCase(typeof(Grid))]
		[TestCase(typeof(StackLayout))]
		public void RemoveCallsCorrectHandlerMethod(Type TLayout)
		{
			var layout = CreateLayout(TLayout);
			var handler = Substitute.For<ILayoutHandler>();
			layout.Handler = handler;

			var child0 = new Button();
			layout.Add(child0);
			layout.Remove(child0);

			var command = Arg.Is(nameof(ILayoutHandler.Remove));
			var args = Arg.Is<LayoutHandlerUpdate>(lhu => lhu.Index == 0 && lhu.View == child0);

			handler.Received().Invoke(command, args);
		}

		[TestCase(typeof(VerticalStackLayout))]
		[TestCase(typeof(HorizontalStackLayout))]
		[TestCase(typeof(Grid))]
		[TestCase(typeof(StackLayout))]
		public void InsertCallsCorrectHandlerMethod(Type TLayout)
		{
			var events = new List<(string Name, LayoutHandlerUpdate? Args)>();

			var layout = CreateLayout(TLayout);
			var handler = Substitute.For<ILayoutHandler>();
			layout.Handler = handler;

			var child0 = new Button();
			var child1 = new Button();
			var child2 = new Button();

			layout.Add(child0);
			layout.Add(child2);
			layout.Insert(1, child1);

			var command = Arg.Is(nameof(ILayoutHandler.Insert));
			var args = Arg.Is<LayoutHandlerUpdate>(lhu => lhu.Index == 1 && lhu.View == child1);

			handler.Received().Invoke(command, args);
		}

		static Layout CreateLayout(Type TLayout)
		{
			var layout = (Layout)Activator.CreateInstance(TLayout)!;
			return layout;
		}

		[Test]
		public void AddRespectsCascadeInputTransparent()
		{
			var layout = new VerticalStackLayout()
			{ 
				InputTransparent = true,
				CascadeInputTransparent = true
			};

			var handler = Substitute.For<IViewHandler>();
			layout.Handler = handler;

			var child0 = new Button() { InputTransparent = false };
			layout.Add(child0);

			handler.Received().UpdateValue(Arg.Is(nameof(Layout.CascadeInputTransparent)));
		}

		[Test]
		public void InsertRespectsCascadeInputTransparent()
		{
			var layout = new VerticalStackLayout()
			{
				InputTransparent = true,
				CascadeInputTransparent = true
			};

			var handler = Substitute.For<IViewHandler>();
			layout.Handler = handler;

			var child0 = new Button() { InputTransparent = false };
			layout.Insert(0, child0);

			handler.Received().UpdateValue(Arg.Is(nameof(Layout.CascadeInputTransparent)));
		}

		[Test]
		public void UpdateRespectsCascadeInputTransparent()
		{
			var layout = new VerticalStackLayout()
			{
				InputTransparent = true,
				CascadeInputTransparent = true
			};

			var handler = Substitute.For<IViewHandler>();
			layout.Handler = handler;

			var child0 = new Button() { InputTransparent = false };
			layout.Add(child0);

			var child1 = new Button() { InputTransparent = false };
			layout[0] = child1;

			handler.Received(2).UpdateValue(Arg.Is(nameof(Layout.CascadeInputTransparent)));
		}
	}
}
