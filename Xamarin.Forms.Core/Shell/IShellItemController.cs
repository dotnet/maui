using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Xamarin.Forms
{
	public interface IShellItemController : IElementController
	{
		bool ProposeSection(ShellSection shellSection, bool setValue = true);

		ReadOnlyCollection<ShellSection> GetItems();
		event NotifyCollectionChangedEventHandler ItemsCollectionChanged;
	}
}