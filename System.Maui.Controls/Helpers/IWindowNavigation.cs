using System.Threading.Tasks;

namespace System.Maui.Controls
{
    public interface IWindowNavigation
    {
		Task OpenNewWindowAsync();
		void NavigateToAnotherPage(Page page);	
	}
}
