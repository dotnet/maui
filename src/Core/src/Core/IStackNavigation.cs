using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides stack based navigation for the .NET MAUI app.
	/// </summary>
	public interface IStackNavigation
	{
		void RequestNavigation(NavigationRequest eventArgs);
		void NavigationFinished(IReadOnlyList<IView> newStack);
	}
}
