using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.UnitTests.Views
{
	[Category(TestCategory.Core, TestCategory.View)]
	public class ShapeTests
	{
		[Fact]
		public async Task ShapeFillSubscribed()
		{
			var fill = new SolidColorBrush { Color = Colors.Red };
			var shape = new Rectangle { Fill = fill };

			bool fired = false;
			shape.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(Shape.Fill))
					fired = true;
			};

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.KeepAlive(shape);

			fill.Color = Colors.Green;

			Assert.True(fired, "PropertyChanged did not fire!");
		}

		[Fact]
		public async Task ShapeFillDoesNotLeak()
		{
			var fill = new SolidColorBrush(Colors.Red);
			var reference = new WeakReference(new Rectangle { Fill = fill });

			Assert.False(await reference.WaitForCollect(), "Shape should not be alive!");
		}

		[Fact]
		public async Task ShapeStrokeSubscribed()
		{
			var stroke = new SolidColorBrush { Color = Colors.Red };
			var shape = new Rectangle { Stroke = stroke };

			bool fired = false;
			shape.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(Shape.Stroke))
					fired = true;
			};

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.KeepAlive(shape);

			stroke.Color = Colors.Green;

			Assert.True(fired, "PropertyChanged did not fire!");
		}

		[Fact]
		public async Task ShapeStrokeDoesNotLeak()
		{
			var stroke = new SolidColorBrush(Colors.Red);
			var reference = new WeakReference(new Rectangle { Stroke = stroke });

			Assert.False(await reference.WaitForCollect(), "Shape should not be alive!");
		}
	}
}