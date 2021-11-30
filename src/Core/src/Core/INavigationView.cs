using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides top-level navigation for the .NET MAUI app.
	/// </summary>
	public interface INavigationView : IView
	{
		void RequestNavigation(NavigationRequest eventArgs);
		void NavigationFinished(IReadOnlyList<IView> newStack);
	}
}
