namespace Xamarin.Forms.Platform.WinPhone
{
	public interface ICellRenderer : IRegisterable
	{
		System.Windows.DataTemplate GetTemplate(Cell cell);
	}
}