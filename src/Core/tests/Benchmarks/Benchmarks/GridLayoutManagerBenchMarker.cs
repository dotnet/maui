using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Diagnostics.Tracing.Parsers.IIS_Trace;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NSubstitute;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class GridLayoutManagerBenchMarker
	{
		IGridLayout _gridLayout;
		GridLayoutManager _manager;

		void BasicSetup()
		{
			_gridLayout = Substitute.For<IGridLayout>();

			_manager = new GridLayoutManager(_gridLayout);

			var view0 = CreateTestView();
			var view1 = CreateTestView();
			var view2 = CreateTestView();
			var view3 = CreateTestView();

			SubstituteChildren(_gridLayout, view0, view1, view2, view3);

			SetLocation(_gridLayout, view0, row: 0, col: 0, rowSpan: 1, colSpan: 1);
			SetLocation(_gridLayout, view1, row: 0, col: 1, rowSpan: 1, colSpan: 1);
			SetLocation(_gridLayout, view2, row: 1, col: 0, rowSpan: 1, colSpan: 1);
			SetLocation(_gridLayout, view3, row: 1, col: 1, rowSpan: 1, colSpan: 1);
		}

		[GlobalSetup(Target = nameof(AllAbsolute))]
		public void AbsoluteSetup()
		{
			BasicSetup();

			SubRowDefs(_gridLayout, CreateTestRows("200, 200"));
			SubColDefs(_gridLayout, CreateTestColumns("200, 200"));
		}

		[GlobalSetup(Target = nameof(AllStars))]
		public void AllStarsSetup()
		{
			BasicSetup();

			SubRowDefs(_gridLayout, CreateTestRows("*, *"));
			SubColDefs(_gridLayout, CreateTestColumns("*, *"));
		}

		[GlobalSetup(Target = nameof(AllAuto))]
		public void AllAutoSetup()
		{
			BasicSetup();

			SubRowDefs(_gridLayout, CreateTestRows("auto, auto"));
			SubColDefs(_gridLayout, CreateTestColumns("auto, auto"));
		}

		static IView CreateTestView()
		{
			var viewSize = new Size(100, 100);

			var view = Substitute.For<IView>();
			view.Height.Returns(-1);
			view.Width.Returns(-1);
			view.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(viewSize);
			view.DesiredSize.Returns(viewSize);

			return view;
		}

		public static void SubstituteChildren(ILayout layout, params IView[] views)
		{
			var children = new List<IView>(views);

			SubstituteChildren(layout, children);
		}

		public static void SubstituteChildren(ILayout layout, IList<IView> children)
		{
			layout[Arg.Any<int>()].Returns(args => children[(int)args[0]]);
			layout.GetEnumerator().Returns(children.GetEnumerator());
			layout.Count.Returns(children.Count);
		}

		static void SetLocation(IGridLayout grid, IView view, int row = 0, int col = 0, int rowSpan = 1, int colSpan = 1)
		{
			grid.GetRow(view).Returns(row);
			grid.GetRowSpan(view).Returns(rowSpan);
			grid.GetColumn(view).Returns(col);
			grid.GetColumnSpan(view).Returns(colSpan);
		}

		static void SubRowDefs(IGridLayout grid, IEnumerable<IGridRowDefinition> rows = null)
		{
			if (rows == null)
			{
				var rowDefs = new List<IGridRowDefinition>();
				grid.RowDefinitions.Returns(rowDefs);
			}
			else
			{
				grid.RowDefinitions.Returns(rows);
			}
		}

		static void SubColDefs(IGridLayout grid, IEnumerable<IGridColumnDefinition> cols = null)
		{
			if (cols == null)
			{
				var colDefs = new List<IGridColumnDefinition>();
				grid.ColumnDefinitions.Returns(colDefs);
			}
			else
			{
				grid.ColumnDefinitions.Returns(cols);
			}
		}

		static List<IGridColumnDefinition> CreateTestColumns(string cols)
		{
			return CreateTestColumnsFromStrings(cols.Split(","));
		}

		static List<IGridColumnDefinition> CreateTestColumnsFromStrings(params string[] columnWidths)
		{
			var colDefs = new List<IGridColumnDefinition>();

			foreach (var width in columnWidths)
			{
				var gridLength = GridLengthFromString(width);
				var colDef = Substitute.For<IGridColumnDefinition>();
				colDef.Width.Returns(gridLength);
				colDefs.Add(colDef);
			}

			return colDefs;
		}

		static List<IGridRowDefinition> CreateTestRows(string rows)
		{
			return CreateTestRowsFromStrings(rows.Split(","));
		}

		static List<IGridRowDefinition> CreateTestRowsFromStrings(params string[] rowHeights)
		{
			var rowDefs = new List<IGridRowDefinition>();

			foreach (var height in rowHeights)
			{
				var gridLength = GridLengthFromString(height);
				var rowDef = Substitute.For<IGridRowDefinition>();
				rowDef.Height.Returns(gridLength);
				rowDefs.Add(rowDef);
			}

			return rowDefs;
		}

		static GridLength GridLengthFromString(string gridLength)
		{
			gridLength = gridLength.Trim();

			if (gridLength.EndsWith("*"))
			{
				gridLength = gridLength.Substring(0, gridLength.Length - 1);

				if (gridLength.Length == 0)
				{
					return GridLength.Star;
				}

				return new GridLength(double.Parse(gridLength), GridUnitType.Star);
			}

			if (gridLength.ToLower() == "auto")
			{
				return GridLength.Auto;
			}

			return new GridLength(double.Parse(gridLength));
		}

		[Benchmark]
		public void AllAbsolute()
		{
			var result = _manager.Measure(500, 500);
			_manager.ArrangeChildren(new Rect(Point.Zero, result));
		}

		[Benchmark]
		public void AllStars()
		{
			var result = _manager.Measure(500, 500);
			_manager.ArrangeChildren(new Rect(Point.Zero, result));
		}

		[Benchmark]
		public void AllAuto()
		{
			var result = _manager.Measure(500, 500);
			_manager.ArrangeChildren(new Rect(Point.Zero, result));
		}
	}
}
