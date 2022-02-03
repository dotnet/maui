#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	public partial class ShellSection : IStackNavigation
	{
		IToolbar IStackNavigation.Toolbar { get; }

		// This code only runs for shell bits that are running through a proper
		// ShellHandler
		TaskCompletionSource<object>? _handlerBasedNavigationCompletionSource;

		void IStackNavigation.RequestNavigation(NavigationRequest eventArgs)
		{
			_handlerBasedNavigationCompletionSource = new TaskCompletionSource<object>();
			Handler.Invoke(nameof(IStackNavigation.RequestNavigation), eventArgs);
		}

		void IStackNavigation.NavigationFinished(IReadOnlyList<IView> newStack)
		{
			var source = _handlerBasedNavigationCompletionSource;
			_handlerBasedNavigationCompletionSource = null;
			source?.SetResult(true);
		}
	}
}
