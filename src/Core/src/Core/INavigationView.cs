using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface INavigationView : IView
	{
		void RequestNavigation(MauiNavigationRequestedEventArgs eventArgs);
		void NavigationFinished(IReadOnlyList<IView> newStack);
	}
}
