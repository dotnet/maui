namespace Xamarin.Forms.Pages
{
	public interface IDataSourceProvider
	{
		IDataSource DataSource { get; set; }

		void MaskKey(string key);

		void UnmaskKey(string key);
	}
}