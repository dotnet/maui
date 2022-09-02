using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class NavigationViewStub : StubBase, IStackNavigationView
	{
		TaskCompletionSource<IReadOnlyList<IView>> _onNavigationFinished;
		public Task<IReadOnlyList<IView>> OnNavigationFinished => _onNavigationFinished.Task.WaitAsync(TimeSpan.FromSeconds(2));

		public void NavigationFinished(IReadOnlyList<IView> newStack)
		{
			var completion = _onNavigationFinished;
			_onNavigationFinished = null;
			completion.SetResult(newStack);
		}

		public void RequestNavigation(NavigationRequest eventArgs)
		{
			_onNavigationFinished = new TaskCompletionSource<IReadOnlyList<IView>>();
			Handler?.Invoke(nameof(IStackNavigationView.RequestNavigation), eventArgs);
		}

		public List<IView> NavigationStack { get; set; }

		public IToolbar Toolbar => null;
	}
}
