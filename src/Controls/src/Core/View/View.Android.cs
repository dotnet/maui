using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class View
	{
		partial void HandlerChangedPartial()
		{
			this.AddOrRemoveControlsAccessibilityDelegate();
		}
	}
}
