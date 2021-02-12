namespace Xamarin.Forms.Platform.UWP
{
	public interface ICellRenderer : IRegisterable
	{
		Microsoft.UI.Xaml.DataTemplate GetTemplate(Cell cell);
	}
}