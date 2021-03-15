using System.Collections.Generic;
using Microsoft.Maui.Layouts;

// This is a temporary namespace until we rename everything and move the legacy layouts
namespace Microsoft.Maui.Controls.Layout2
{
	public class GridLayout : Layout, IGridLayout
	{
		List<IGridRowDefinition> _rowDefinitions = new List<IGridRowDefinition>();
		List<IGridColumnDefinition> _columnDefinitions = new List<IGridColumnDefinition>();

		public IReadOnlyList<IGridRowDefinition> RowDefinitions => _rowDefinitions;
		public IReadOnlyList<IGridColumnDefinition> ColumnDefinitions => _columnDefinitions;

		public double RowSpacing { get; set; }
		public double ColumnSpacing { get; set; }

		Dictionary<IView, GridInfo> _viewInfo = new Dictionary<IView, GridInfo>();

		// TODO ezhart This needs to override Remove and clean up any row/column/span info for the removed child

		public int GetColumn(IView view)
		{
			if (_viewInfo.TryGetValue(view, out GridInfo gridInfo))
			{
				return gridInfo.Col;
			}

			return 0;
		}

		public int GetColumnSpan(IView view)
		{
			if (_viewInfo.TryGetValue(view, out GridInfo gridInfo))
			{
				return gridInfo.ColSpan;
			}

			return 1;
		}

		public int GetRow(IView view)
		{
			if (_viewInfo.TryGetValue(view, out GridInfo gridInfo))
			{
				return gridInfo.Row;
			}

			return 0;
		}

		public int GetRowSpan(IView view)
		{
			if (_viewInfo.TryGetValue(view, out GridInfo gridInfo))
			{
				return gridInfo.RowSpan;
			}

			return 1;
		}

		protected override ILayoutManager CreateLayoutManager() => new GridLayoutManager(this);

		public void AddRowDefinition(IGridRowDefinition gridRowDefinition)
		{
			_rowDefinitions.Add(gridRowDefinition);
		}

		public void AddColumnDefinition(IGridColumnDefinition gridColumnDefinition)
		{
			_columnDefinitions.Add(gridColumnDefinition);
		}

		public void SetRow(IView view, int row)
		{
			if (_viewInfo.TryGetValue(view, out GridInfo gridInfo))
			{
				gridInfo.Row = row;
			}
			else
			{
				_viewInfo[view] = new GridInfo { Row = row };
			}
		}

		public void SetRowSpan(IView view, int span)
		{
			if (_viewInfo.TryGetValue(view, out GridInfo gridInfo))
			{
				gridInfo.RowSpan = span;
			}
			else
			{
				_viewInfo[view] = new GridInfo { RowSpan = span };
			}
		}

		public void SetColumn(IView view, int col)
		{
			if (_viewInfo.TryGetValue(view, out GridInfo gridInfo))
			{
				gridInfo.Col = col;
			}
			else
			{
				_viewInfo[view] = new GridInfo { Col = col };
			}
		}

		public void SetColumnSpan(IView view, int span)
		{
			if (_viewInfo.TryGetValue(view, out GridInfo gridInfo))
			{
				gridInfo.ColSpan = span;
			}
			else
			{
				_viewInfo[view] = new GridInfo { ColSpan = span };
			}
		}

		class GridInfo
		{
			public int Row { get; set; }
			public int Col { get; set; }
			public int RowSpan { get; set; } = 1;
			public int ColSpan { get; set; } = 1;
		}
	}
}
