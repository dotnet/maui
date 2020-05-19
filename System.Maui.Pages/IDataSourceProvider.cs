namespace System.Maui.Pages
{
	public interface IDataSourceProvider
	{
		IDataSource DataSource { get; set; }

		void MaskKey(string key);

		void UnmaskKey(string key);
	}
}