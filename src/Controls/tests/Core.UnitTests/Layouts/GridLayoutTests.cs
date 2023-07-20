using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	public class GridLayoutTests
	{
		[Fact]
		public void RemovedMauiViewsHaveNoRowColumnInfo()
		{
			var gl = new Grid();
			var view = NSubstitute.Substitute.For<IView>();

			gl.Add(view);
			gl.SetRow(view, 2);

			// Check our assumptions
			Assert.Equal(2, gl.GetRow(view));

			// Okay, removing the View from the Grid should mean that any attempt to get row/column info
			// for that View should fail
			gl.Remove(view);

			Assert.Throws<KeyNotFoundException>(() => gl.GetRow(view));
			Assert.Throws<KeyNotFoundException>(() => gl.GetRowSpan(view));
			Assert.Throws<KeyNotFoundException>(() => gl.GetColumn(view));
			Assert.Throws<KeyNotFoundException>(() => gl.GetColumnSpan(view));
		}

		[Fact]
		public void AddedViewGetsDefaultRowAndColumn()
		{
			var gl = new Grid();
			var view = new Label();

			gl.Add(view);
			Assert.Equal(0, gl.GetRow(view));
			Assert.Equal(0, gl.GetColumn(view));
			Assert.Equal(1, gl.GetRowSpan(view));
			Assert.Equal(1, gl.GetColumnSpan(view));
		}

		[Fact]
		public void AddedMauiViewGetsDefaultRowAndColumn()
		{
			var gl = new Grid();
			var view = NSubstitute.Substitute.For<IView>();

			gl.Add(view);
			Assert.Equal(0, gl.GetRow(view));
			Assert.Equal(0, gl.GetColumn(view));
			Assert.Equal(1, gl.GetRowSpan(view));
			Assert.Equal(1, gl.GetColumnSpan(view));
		}

		[Fact]
		public void ChangingRowSpacingInvalidatesGrid()
		{
			var grid = new Grid();

			var handler = ListenForInvalidation(grid);
			grid.RowSpacing = 100;
			AssertInvalidated(handler);
		}

		[Fact]
		public void ChangingColumnSpacingInvalidatesGrid()
		{
			var grid = new Grid();

			var handler = ListenForInvalidation(grid);
			grid.ColumnSpacing = 100;
			AssertInvalidated(handler);
		}

		[Fact]
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

		[Fact]
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

		[Fact]
		public void RowDefinitionsGetBindingContext()
		{
			var def = new RowDefinition();
			var def2 = new RowDefinition();

			var grid = new Grid()
			{
				RowDefinitions = new RowDefinitionCollection
				{
					def
				}
			};

			var context = new object();

			Assert.Null(def.BindingContext);
			Assert.Null(def2.BindingContext);

			grid.BindingContext = context;

			Assert.Equal(def.BindingContext, context);

			grid.RowDefinitions.Add(def2);

			Assert.Equal(def2.BindingContext, context);
		}

		[Fact]
		public void ColumnDefinitionsGetBindingContext()
		{
			var def = new ColumnDefinition();
			var def2 = new ColumnDefinition();

			var grid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitionCollection
				{
					def
				}
			};

			var context = new object();

			Assert.Null(def.BindingContext);
			Assert.Null(def2.BindingContext);

			grid.BindingContext = context;

			Assert.Equal(def.BindingContext, context);

			grid.ColumnDefinitions.Add(def2);

			Assert.Equal(def2.BindingContext, context);
		}

		[Fact]
		public async Task ColumnDefinitionDoesNotLeak()
		{
			// Long-lived column, like from a Style in App.Resources
			var column = new ColumnDefinition();
			WeakReference reference;

			{
				var grid = new Grid();
				grid.ColumnDefinitions.Add(column);
				reference = new(grid);
			}

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.False(reference.IsAlive, "Grid should not be alive!");
		}

		[Fact]
		public async Task RowDefinitionDoesNotLeak()
		{
			// Long-lived row, like from a Style in App.Resources
			var row = new RowDefinition();
			WeakReference reference;

			{
				var grid = new Grid();
				grid.RowDefinitions.Add(row);
				reference = new(grid);
			}

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.False(reference.IsAlive, "Grid should not be alive!");
		}
	}
}
