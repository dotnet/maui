#nullable disable
namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public interface ICellRenderer : IRegisterable
	{
#pragma warning disable CS0618 // Type or member is obsolete
		Microsoft.UI.Xaml.DataTemplate GetTemplate(Cell cell);
#pragma warning restore CS0618 // Type or member is obsolete
	}
}