#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using NUnit.Framework;

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
			var events = new List<(string Name, LayoutHandlerUpdate? Args)>();

			var layout = CreateLayout(TLayout);
			layout.Handler = CreateLayoutHandler((n, h, l, a) => events.Add((n, a)));

			events.Clear();

			var child0 = new Button();

			layout.Add(child0);

			Assert.AreEqual(1, events.Count);
			var (name, args) = events[0];
			Assert.AreEqual(nameof(ILayoutHandler.Add), name);
			Assert.AreEqual(0, args!.Index);
			Assert.AreEqual(child0, args!.View);
		}

		[TestCase(typeof(VerticalStackLayout))]
		[TestCase(typeof(HorizontalStackLayout))]
		[TestCase(typeof(Grid))]
		[TestCase(typeof(StackLayout))]
		public void RemoveCallsCorrectHandlerMethod(Type TLayout)
		{
			var events = new List<(string Name, LayoutHandlerUpdate? Args)>();

			var layout = CreateLayout(TLayout);
			layout.Handler = CreateLayoutHandler((n, h, l, a) => events.Add((n, a)));

			var child0 = new Button();
			layout.Add(child0);

			events.Clear();

			layout.Remove(child0);

			Assert.AreEqual(1, events.Count);
			var (name, args) = events[0];
			Assert.AreEqual(nameof(ILayoutHandler.Remove), name);
			Assert.AreEqual(0, args!.Index);
			Assert.AreEqual(child0, args!.View);
		}

		[TestCase(typeof(VerticalStackLayout))]
		[TestCase(typeof(HorizontalStackLayout))]
		[TestCase(typeof(Grid))]
		[TestCase(typeof(StackLayout))]
		public void InsertCallsCorrectHandlerMethod(Type TLayout)
		{
			var events = new List<(string Name, LayoutHandlerUpdate? Args)>();

			var layout = CreateLayout(TLayout);
			layout.Handler = CreateLayoutHandler((n, h, l, a) => events.Add((n, a)));

			var child0 = new Button();
			var child1 = new Button();
			var child2 = new Button();

			layout.Add(child0);
			layout.Add(child2);

			events.Clear();

			layout.Insert(1, child1);

			Assert.AreEqual(1, events.Count);
			var (name, args) = events[0];
			Assert.AreEqual(nameof(ILayoutHandler.Insert), name);
			Assert.AreEqual(1, args!.Index);
			Assert.AreEqual(child1, args!.View);
		}

		Layout CreateLayout(Type TLayout)
		{
			var layout = (Layout)Activator.CreateInstance(TLayout)!;
			layout.IsPlatformEnabled = true;
			return layout;
		}

		LayoutHandler CreateLayoutHandler(Action<string, ILayoutHandler, Maui.ILayout, LayoutHandlerUpdate?>? action)
		{
			var handler = new NonThrowingLayoutHandler(
				LayoutHandler.LayoutMapper,
				new CommandMapper<Maui.ILayout, ILayoutHandler>(LayoutHandler.LayoutCommandMapper)
				{
					[nameof(ILayoutHandler.Add)] = (h, l, a) => action?.Invoke(nameof(ILayoutHandler.Add), h, l, (LayoutHandlerUpdate?)a),
					[nameof(ILayoutHandler.Remove)] = (h, l, a) => action?.Invoke(nameof(ILayoutHandler.Remove), h, l, (LayoutHandlerUpdate?)a),
					[nameof(ILayoutHandler.Clear)] = (h, l, a) => action?.Invoke(nameof(ILayoutHandler.Clear), h, l, (LayoutHandlerUpdate?)a),
					[nameof(ILayoutHandler.Insert)] = (h, l, a) => action?.Invoke(nameof(ILayoutHandler.Insert), h, l, (LayoutHandlerUpdate?)a),
					[nameof(ILayoutHandler.Update)] = (h, l, a) => action?.Invoke(nameof(ILayoutHandler.Update), h, l, (LayoutHandlerUpdate?)a),
				});

			return handler;
		}

		class NonThrowingLayoutHandler : LayoutHandler
		{
			public NonThrowingLayoutHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
				: base(mapper, commandMapper)
			{
			}

			protected override object CreatePlatformView() => new object();
		}
	}
}
