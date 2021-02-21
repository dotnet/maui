using System.ComponentModel;

namespace Xamarin.Forms
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IMenuItemController
	{
		bool IsEnabled { get; set; }
		void Activate();
	}
}
