namespace System.Maui.Platform.UWP
{
	public interface ICellRenderer : IRegisterable
	{
		global::Windows.UI.Xaml.DataTemplate GetTemplate(Cell cell);
	}
}