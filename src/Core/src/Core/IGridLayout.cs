using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a layout that arranges views in rows and columns.
	/// </summary>
	public interface IGridLayout : ILayout
	{
		/// <summary>
		/// An IGridRowDefinition collection for the GridLayout instance.
		/// </summary>
		IReadOnlyList<IGridRowDefinition> RowDefinitions { get; }

		/// <summary>
		/// An IGridColumnDefinition collection for the GridLayout instance.
		/// </summary>
		IReadOnlyList<IGridColumnDefinition> ColumnDefinitions { get; }

		/// <summary>
		/// Gets the amount of space left between rows in the GridLayout.
		/// </summary>
		double RowSpacing { get; }

		/// <summary>
		/// Gets the amount of space left between columns in the GridLayout.
		/// </summary>
		double ColumnSpacing { get; }

		/// <summary>
		/// Gets the row of the child element.
		/// </summary>
		/// <param name="view">A view that belongs to the Grid layout.</param>
		/// <returns>An integer that represents the row in which the item will appear.</returns>
		int GetRow(IView view);

		/// <summary>
		/// Gets the row span of the child element.
		/// </summary>
		/// <param name="view">A view that belongs to the Grid layout.</param>
		/// <returns>The row that the child element is in.</returns>
		int GetRowSpan(IView view);

		/// <summary>
		/// Gets the column of the child element.
		/// </summary>
		/// <param name="view">A view that belongs to the Grid layout.</param>
		/// <returns>The column that the child element is in.</returns>
		int GetColumn(IView view);

		/// <summary>
		/// Gets the row span of the child element.
		/// </summary>
		/// <param name="view">A view that belongs to the Grid layout.</param>
		/// <returns>The row that the child element is in.</returns>
		int GetColumnSpan(IView view);
	}
}