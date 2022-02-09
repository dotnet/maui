using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, WindowHeader>
	{
		protected override WindowHeader CreateNativeElement()
		{
			return new WindowHeader();
		}
	}
}
