using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface INavigationView : IView
	{
		void RequestNavigation(NavigationRequest eventArgs);
		void NavigationFinished(IReadOnlyList<IView> newStack);
	}
}
