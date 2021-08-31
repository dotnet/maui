using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface INavigationView : IView
	{
		IReadOnlyList<IView> NavigationStack { get; }
		void RequestNavigation(MauiNavigationRequestedEventArgs eventArgs);
		void NavigationFinished(IReadOnlyList<IView> newStack);
	}
}
