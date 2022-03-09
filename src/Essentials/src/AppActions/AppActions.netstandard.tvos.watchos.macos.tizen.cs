using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppActions']/Docs" />
	public partial class AppActionsImplementation : IAppActions
	{
		public string Type => "XE_APP_ACTION_TYPE";
		public bool IsSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<IEnumerable<AppAction>> GetAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task SetAsync(IEnumerable<AppAction> actions) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task SetAsync(params AppAction[] actions) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
