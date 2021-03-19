using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public interface IWindowNavigation
	{
		Task OpenNewWindowAsync();
		void NavigateToAnotherPage(Page page);
	}
}
