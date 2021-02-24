using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IMenuItemController
	{
		bool IsEnabled { get; set; }
		void Activate();
	}
}
