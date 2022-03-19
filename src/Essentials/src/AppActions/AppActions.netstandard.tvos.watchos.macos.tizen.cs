using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppActions']/Docs" />
	partial class AppActionsImplementation : IAppActions
	{
		public bool IsSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<IEnumerable<AppAction>> GetAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task SetAsync(IEnumerable<AppAction> actions) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public event EventHandler<AppActionEventArgs> AppActionActivated;
	}
}
