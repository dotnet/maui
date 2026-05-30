#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Layouts;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	[Category("Layout")]
	public class LayoutTests : BaseTestFixture
	{
		[Fact]
		public void UsingIndexUpdatesParent()
		{
			var layout = new VerticalStackLayout();

			var child0 = new Button();
			var child1 = new Button();

			Assert.Null(child0.Parent);
			Assert.Null(child1.Parent);

			layout.Add(child0);

			Assert.Equal(layout, child0.Parent);
			Assert.Null(child1.Parent);

			layout[0] = child1;

			Assert.Null(child0.Parent);
			Assert.Equal(layout, child1.Parent);
		}

		[Fact]
		public void ClearUpdatesParent()
		{
			var layout = new VerticalStackLayout();

			var child0 = new Button();
			var child1 = new Button();

			layout.Add(child0);
			layout.Add(child1);

			Assert.Equal(layout, child0.Parent);
			Assert.Equal(layout, child1.Parent);

			layout.Clear();

			Assert.Null(child0.Parent);
			Assert.Null(child1.Parent);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
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

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
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

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
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

		IMauiContext SetupContext(ILayoutManagerFactory layoutManagerFactory)
		{
			var services = Substitute.For<IServiceProvider>();
			services.GetService(Arg.Any<Type>()).Returns(layoutManagerFactory);
			var context = Substitute.For<IMauiContext>();
			context.Services.Returns(services);

			return context;
		}

		class AlternateLayoutManager : ILayoutManager
		{
			readonly double _width;
			readonly double _height;

			public AlternateLayoutManager(double width, double height)
			{
				_width = width;
				_height = height;
			}

			public Size ArrangeChildren(Rect bounds)
			{
				throw new NotImplementedException();
			}

			public Size Measure(double widthConstraint, double heightConstraint)
			{
				return new Size(_width, _height);
			}
		}

		[Fact]
		public void CanUseFactoryForAlternateManager()
		{
			var layoutManagerFactory = Substitute.For<Controls.ILayoutManagerFactory>();
			layoutManagerFactory.CreateLayoutManager(Arg.Any<Layout>()).Returns(new AlternateLayoutManager(8765, 4321));

			var context = SetupContext(layoutManagerFactory);

			var handler = Substitute.For<IViewHandler>();
			handler.MauiContext.Returns(context);

			var layout = new Grid
			{
				Handler = handler
			};

			var result = layout.CrossPlatformMeasure(100, 100);

			Assert.Equal(8765, result.Width);
			Assert.Equal(4321, result.Height);
		}

		class NullLayoutManagerFactory : Controls.ILayoutManagerFactory
		{
			public ILayoutManager? CreateLayoutManager(Layout layout)
			{
				return null;
			}
		}

		[Fact]
		public void FactoryCanPuntAndUseOriginalType()
		{
			var layoutManagerFactory = new NullLayoutManagerFactory();
			var context = SetupContext(layoutManagerFactory);

			var handler = Substitute.For<IViewHandler>();
			handler.MauiContext.Returns(context);

			var layout = new VerticalStackLayout();
			layout.Handler = handler;

			var view = new Label { Text = "a", WidthRequest = 100, HeightRequest = 100 };
			layout.Add(view);

			var result = layout.CrossPlatformMeasure(100, 100);

			Assert.Equal(0, result.Width);
			Assert.Equal(0, result.Height);
		}

		class ChoosyLayoutManagerFactory : Controls.ILayoutManagerFactory
		{
			public ILayoutManager? CreateLayoutManager(Layout layout)
			{
				if (layout is AbsoluteLayout)
				{
					return new AlternateLayoutManager(1234, 1234);
				}

				return new AlternateLayoutManager(4567, 4567);
			}
		}

		[Fact]
		public void FactoryCanCustomizeBasedOnLayoutType()
		{
			var layoutManagerFactory = new ChoosyLayoutManagerFactory();
			var context = SetupContext(layoutManagerFactory);

			var handler = Substitute.For<IViewHandler>();
			handler.MauiContext.Returns(context);

			var absLayout = new AbsoluteLayout
			{
				Handler = handler
			};

			var view = new Label { Text = "a", WidthRequest = 100, HeightRequest = 100 };
			absLayout.Add(view);

			var absResult = absLayout.CrossPlatformMeasure(100, 100);

			Assert.Equal(1234, absResult.Width);
			Assert.Equal(1234, absResult.Height);

			var vsl = new VerticalStackLayout
			{
				Handler = handler // Obviously this wouldn't be okay in a real app, but it works well enough for this test
			};

			var vslResult = vsl.CrossPlatformMeasure(100, 100);

			Assert.Equal(4567, vslResult.Width);
			Assert.Equal(4567, vslResult.Height);
		}
	}
}
