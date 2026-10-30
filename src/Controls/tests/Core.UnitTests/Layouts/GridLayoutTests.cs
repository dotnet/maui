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

		[Theory]
		[InlineData("Array")]
		[InlineData("CopiedList")]
		[InlineData("OwnedList")]
		[InlineData("Converter")]
		public void ChangingConstructedColumnWidthInvalidatesGrid(string construction)
		{
			var grid = new Grid
			{
				ColumnDefinitions = CreateColumnDefinitions(construction)
			};

			var handler = ListenForInvalidation(grid);
			grid.ColumnDefinitions[0].Width = 0;
			AssertInvalidated(handler);
		}

		[Theory]
		[InlineData("Array")]
		[InlineData("CopiedList")]
		[InlineData("OwnedList")]
		[InlineData("Converter")]
		public void ChangingConstructedRowHeightInvalidatesGrid(string construction)
		{
			var definitions = new List<RowDefinition> { new RowDefinition(390) };
			var grid = new Grid
			{
				RowDefinitions = construction switch
				{
					"Array" => new RowDefinitionCollection(definitions.ToArray()),
					"CopiedList" => new RowDefinitionCollection(definitions, copy: true),
					"OwnedList" => new RowDefinitionCollection(definitions, copy: false),
					"Converter" => (RowDefinitionCollection)new RowDefinitionCollectionTypeConverter().ConvertFromInvariantString("390"),
					_ => throw new ArgumentOutOfRangeException(nameof(construction))
				}
			};

			var handler = ListenForInvalidation(grid);
			grid.RowDefinitions[0].Height = 0;
			AssertInvalidated(handler);
		}

		[Theory]
		[InlineData("Array", false)]
		[InlineData("CopiedList", false)]
		[InlineData("OwnedList", false)]
		[InlineData("Converter", false)]
		[InlineData("Array", true)]
		[InlineData("CopiedList", true)]
		[InlineData("OwnedList", true)]
		[InlineData("Converter", true)]
		public void RemovedConstructedColumnDoesNotInvalidateGrid(string construction, bool clear)
		{
			var grid = new Grid
			{
				ColumnDefinitions = CreateColumnDefinitions(construction)
			};
			var definition = grid.ColumnDefinitions[0];

			if (clear)
				grid.ColumnDefinitions.Clear();
			else
				grid.ColumnDefinitions.Remove(definition);

			var handler = ListenForInvalidation(grid);
			definition.Width = 0;
			handler.DidNotReceive().Invoke(Arg.Is(nameof(IView.InvalidateMeasure)), Arg.Any<object>());
		}

		static ColumnDefinitionCollection CreateColumnDefinitions(string construction)
		{
			var definitions = new List<ColumnDefinition> { new ColumnDefinition(390) };
			return construction switch
			{
				"Array" => new ColumnDefinitionCollection(definitions.ToArray()),
				"CopiedList" => new ColumnDefinitionCollection(definitions, copy: true),
				"OwnedList" => new ColumnDefinitionCollection(definitions, copy: false),
				"Converter" => (ColumnDefinitionCollection)new ColumnDefinitionCollectionTypeConverter().ConvertFromInvariantString("390"),
				_ => throw new ArgumentOutOfRangeException(nameof(construction))
			};
		}

		[Theory]
		[InlineData("Row", false)]
		[InlineData("RowSpan", false)]
		[InlineData("Column", false)]
		[InlineData("ColumnSpan", false)]
		[InlineData("Row", true)]
		[InlineData("RowSpan", true)]
		[InlineData("Column", true)]
		[InlineData("ColumnSpan", true)]
		public void ChangingChildGridPositionInvalidatesParent(string property, bool childIsGrid)
		{
			var parent = new Grid();
			View child = childIsGrid ? new Grid() : new Label();
			parent.Add(child);
			var handler = ListenForInvalidation(parent);

			switch (property)
			{
				case "Row":
					Grid.SetRow(child, 1);
					break;
				case "RowSpan":
					Grid.SetRowSpan(child, 2);
					break;
				case "Column":
					Grid.SetColumn(child, 1);
					break;
				case "ColumnSpan":
					Grid.SetColumnSpan(child, 2);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(property));
			}

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

		[Fact, Category(TestCategory.Memory)]
		public async Task ColumnDefinitionDoesNotLeak()
		{
			// Long-lived column, like from a Style in App.Resources
			var columnDefinition = new ColumnDefinition();

			WeakReference CreateReference()
			{
				var grid = new Grid();
				grid.ColumnDefinitions.Add(columnDefinition);
				return new(grid);
			}

			WeakReference reference = CreateReference();

			await TestHelpers.Collect();

			Assert.False(await reference.WaitForCollect(), "Grid should not be alive!");

			// Ensure that the ColumnDefinition isn't collected during the test
			GC.KeepAlive(columnDefinition);
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

			await TestHelpers.Collect();

			Assert.False(await reference.WaitForCollect(), "Grid should not be alive!");
		}
	}
}
