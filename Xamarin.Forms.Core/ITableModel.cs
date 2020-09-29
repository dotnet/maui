namespace Xamarin.Forms
{
	public interface ITableModel
	{
		Cell GetCell(int section, int row);
		Cell GetHeaderCell(int section);
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