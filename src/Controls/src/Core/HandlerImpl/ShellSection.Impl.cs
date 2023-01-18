using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	public partial class ShellSection : IStackNavigation
	{
		// This code only runs for shell bits that are running through a proper
		// ShellHandler
		TaskCompletionSource<object>? _handlerBasedNavigationCompletionSource;
		internal Task? PendingNavigationTask => _handlerBasedNavigationCompletionSource?.Task;

		void IStackNavigation.RequestNavigation(NavigationRequest eventArgs)
		{
			if (_handlerBasedNavigationCompletionSource != null)
				throw new InvalidOperationException("Pending Navigations still processing");

			_handlerBasedNavigationCompletionSource = new TaskCompletionSource<object>();
			Handler.Invoke(nameof(IStackNavigation.RequestNavigation), eventArgs);
		}

		void IStackNavigation.NavigationFinished(IReadOnlyList<IView> newStack)
		{
			_ = _handlerBasedNavigationCompletionSource ?? throw new InvalidOperationException("Mismatched Navigation finished");
			var source = _handlerBasedNavigationCompletionSource;
			_handlerBasedNavigationCompletionSource = null;
			source?.SetResult(true);
		}
	}
}
