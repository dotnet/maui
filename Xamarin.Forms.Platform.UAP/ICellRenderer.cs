namespace Xamarin.Forms.Platform.UWP
{
	public interface ICellRenderer : IRegisterable
	{
		Windows.UI.Xaml.DataTemplate GetTemplate(Cell cell);
	}
}