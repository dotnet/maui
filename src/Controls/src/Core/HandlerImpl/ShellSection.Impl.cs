using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class ShellSection : IStackNavigation
	{
		IToolbar IStackNavigation.Toolbar { get; }

		void IStackNavigation.RequestNavigation(NavigationRequest eventArgs)
		{

		}

		void IStackNavigation.NavigationFinished(IReadOnlyList<IView> newStack)
		{

		}
	}
}
