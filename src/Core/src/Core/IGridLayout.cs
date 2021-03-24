using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface IGridLayout : ILayout
	{
		IReadOnlyList<IGridRowDefinition> RowDefinitions { get; }
		IReadOnlyList<IGridColumnDefinition> ColumnDefinitions { get; }

		double RowSpacing { get; }
		double ColumnSpacing { get; }

		int GetRow(IView view);
		int GetRowSpan(IView view);

		int GetColumn(IView view);
		int GetColumnSpan(IView view);
	}
}