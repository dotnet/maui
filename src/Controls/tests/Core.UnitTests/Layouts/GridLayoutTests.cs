using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	public class GridLayoutTests
	{
		[Test]
		public void RemovedMauiViewsHaveNoRowColumnInfo()
		{
			var gl = new Grid();
			var view = NSubstitute.Substitute.For<IView>();

			gl.Add(view);
			gl.SetRow(view, 2);

			// Check our assumptions
			Assert.AreEqual(2, gl.GetRow(view));

			// Okay, removing the View from the Grid should mean that any attempt to get row/column info
			// for that View should fail
			gl.Remove(view);

			Assert.Throws(typeof(KeyNotFoundException), () => gl.GetRow(view));
			Assert.Throws(typeof(KeyNotFoundException), () => gl.GetRowSpan(view));
			Assert.Throws(typeof(KeyNotFoundException), () => gl.GetColumn(view));
			Assert.Throws(typeof(KeyNotFoundException), () => gl.GetColumnSpan(view));
		}

		[Test]
		public void AddedViewGetsDefaultRowAndColumn()
		{
			var gl = new Grid();
			var view = new Label();

			gl.Add(view);
			Assert.AreEqual(0, gl.GetRow(view));
			Assert.AreEqual(0, gl.GetColumn(view));
			Assert.AreEqual(1, gl.GetRowSpan(view));
			Assert.AreEqual(1, gl.GetColumnSpan(view));
		}

		[Test]
		public void AddedMauiViewGetsDefaultRowAndColumn()
		{
			var gl = new Grid();
			var view = NSubstitute.Substitute.For<IView>();

			gl.Add(view);
			Assert.AreEqual(0, gl.GetRow(view));
			Assert.AreEqual(0, gl.GetColumn(view));
			Assert.AreEqual(1, gl.GetRowSpan(view));
			Assert.AreEqual(1, gl.GetColumnSpan(view));
		}

		[Test]
		public void ChangingRowSpacingInvalidatesGrid()
		{
			var grid = new Grid();

			var handler = ListenForInvalidation(grid);
			grid.RowSpacing = 100;
			AssertInvalidated(handler);
		}

		[Test]
		public void ChangingColumnSpacingInvalidatesGrid()
		{
			var grid = new Grid();

			var handler = ListenForInvalidation(grid);
			grid.ColumnSpacing = 100;
			AssertInvalidated(handler);
		}

		[Test]
		public void ChangingChildRowInvalidatesGrid()
		{
			var grid = new Grid()
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition(), new RowDefinition()
				}
			};

			var view = Substitute.For<IView>();
			grid.Add(view);

			var handler = ListenForInvalidation(grid);

			grid.SetRow(view, 1);

			AssertInvalidated(handler);
		}

		[Test]
		public void ChangingChildColumnInvalidatesGrid()
		{
			var grid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitionCollection
				{
					new ColumnDefinition(), new ColumnDefinition()
				}
			};

			var view = Substitute.For<IView>();
			grid.Add(view);

			var handler = ListenForInvalidation(grid);

			grid.SetColumn(view, 1);

			AssertInvalidated(handler);
		}

		static IViewHandler ListenForInvalidation(IView view)
		{
			var handler = Substitute.For<IViewHandler>();
			view.Handler = handler;
			handler.ClearReceivedCalls();
			return handler;
		}

		static void AssertInvalidated(IViewHandler handler)
		{
			handler.Received().Invoke(Arg.Is(nameof(IView.InvalidateMeasure)), Arg.Any<object>());
		}
	}
}
