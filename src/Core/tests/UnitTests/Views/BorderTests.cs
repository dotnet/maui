using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.UnitTests.Views
{
	[Category(TestCategory.Core, TestCategory.View)]
	public class BorderTests
	{
		[Fact]
		public void TestDefaultBorderStrokeShape()
		{
			Border border = new Border();

			Assert.NotNull(border);
			Assert.NotNull(border.StrokeShape);
		}

		[Fact]
		public void TestBorderPropagateBindingContext()
		{
			Border border = new Border();

			var bindingContext = new object();
			border.BindingContext = bindingContext;

			var content = new ContentView();
			border.Content = content;

			Assert.True(border.BindingContext == bindingContext);
		}

		[Fact]
		public void TestStrokeShapeBindingContext()
		{
			var context = new object();

			var parent = new Border
			{
				BindingContext = context
			};

			var strokeShape = new Rectangle();

			parent.StrokeShape = strokeShape;

			Assert.Same(context, ((Rectangle)parent.StrokeShape).BindingContext);
		}

		[Fact]
		public async Task BorderStrokeShapeSubscribed()
		{
			var strokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) };
			var border = new Border { StrokeShape = strokeShape };

			bool fired = false;
			border.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(Border.StrokeShape))
					fired = true;
			};

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.KeepAlive(border);

			strokeShape.CornerRadius = new CornerRadius(24);

			Assert.True(fired, "PropertyChanged did not fire!");
		}

		[Fact]
		public async Task BorderStrokeSubscribed()
		{
			var stroke = new SolidColorBrush(Colors.Red);
			var border = new Border { Stroke = stroke };

			bool fired = false;
			border.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(Border.Stroke))
					fired = true;
			};

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.KeepAlive(border);

			stroke.Color = Colors.Green;
			Assert.True(fired, "PropertyChanged did not fire!");
		}

		[Theory]
		[InlineData(typeof(Rectangle))]
		[InlineData(typeof(RoundRectangle))]
		[InlineData(typeof(Ellipse))]
		public async Task BorderStrokeShapeDoesNotLeak(Type type)
		{
			var strokeShape = (Shape)Activator.CreateInstance(type);
			var reference = new WeakReference(new Border { StrokeShape = strokeShape });

			strokeShape = null;

			Assert.False(await reference.WaitForCollect(), "Border should not be alive!");
		}

		[Fact]
		public void TestSetChild()
		{
			Border border = new Border();

			var child1 = new Label();

			bool added = false;

			border.ChildAdded += (sender, e) => added = true;

			border.Content = child1;

			Assert.True(added);
			Assert.Equal(child1, border.Content);

			added = false;
			border.Content = child1;

			Assert.False(added);
		}

		[Fact]
		public void TestReplaceChild()
		{
			Border border = new Border();

			var child1 = new Label();
			var child2 = new Label();

			border.Content = child1;

			bool removed = false;
			bool added = false;

			border.ChildRemoved += (sender, e) => removed = true;
			border.ChildAdded += (sender, e) => added = true;

			border.Content = child2;

			Assert.True(removed);
			Assert.True(added);
			Assert.Equal(child2, border.Content);
		}
	}
}