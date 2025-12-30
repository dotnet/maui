#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public interface ITableModel
	{
#pragma warning disable CS0618 // Type or member is obsolete
		Cell GetCell(int section, int row);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		Cell GetHeaderCell(int section);
#pragma warning restore CS0618 // Type or member is obsolete
		object GetItem(int section, int row);
		int GetRowCount(int section);
		int GetSectionCount();
		string[] GetSectionIndexTitles();
		string GetSectionTitle(int section);
		Color GetSectionTextColor(int section);
		void RowLongPressed(int section, int row);
		void RowSelected(object item);
		void RowSelected(int section, int row);
	}
}
