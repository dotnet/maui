#nullable disable
namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public interface ICellRenderer : IRegisterable
	{
		Microsoft.UI.Xaml.DataTemplate GetTemplate(Cell cell);
	}
}