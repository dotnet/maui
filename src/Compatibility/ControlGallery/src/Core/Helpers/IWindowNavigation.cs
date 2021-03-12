using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public interface IWindowNavigation
	{
		Task OpenNewWindowAsync();
		void NavigateToAnotherPage(Page page);
	}
}
