using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{

	/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppActions']/Docs" />
	partial class AppActionsImplementation : IAppActions
	{

		public bool IsSupported => false;

		public Task<IEnumerable<AppAction>> GetAsync() => Task.FromResult<IEnumerable<AppAction>>(null);

		public Task SetAsync(IEnumerable<AppAction> actions) =>
			Task.CompletedTask;

#pragma warning disable CS0067 // The event is never used
		public event EventHandler<AppActionEventArgs> AppActionActivated;
#pragma warning restore CS0067 // The event is never used

	}

}