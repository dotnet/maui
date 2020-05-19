using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
    public interface IWindowNavigation
    {
		Task OpenNewWindowAsync();
		void NavigateToAnotherPage(Page page);	
	}
}
