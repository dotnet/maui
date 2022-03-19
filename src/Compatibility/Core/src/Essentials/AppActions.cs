#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentialssss/AppActions.xml" path="Type[@FullName='Microsoft.Maui.Essentialssss.AppActions']/Docs" />
	public static partial class AppActions
	{
		/// <include file="../../docs/Microsoft.Maui.Essentialssss/AppActions.xml" path="//Member[@MemberName='GetAsync']/Docs" />
		public static Task<IEnumerable<AppAction>> GetAsync()
			=> Current.GetAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentialssss/AppActions.xml" path="//Member[@MemberName='SetAsync'][1]/Docs" />
		public static Task SetAsync(params AppAction[] actions)
			=> Current.SetAsync(actions);

		/// <include file="../../docs/Microsoft.Maui.Essentialssss/AppActions.xml" path="//Member[@MemberName='SetAsync'][2]/Docs" />
		public static Task SetAsync(IEnumerable<AppAction> actions)
			=> Current.SetAsync(actions);

		/// <include file="../../docs/Microsoft.Maui.Essentialssss/AppActions.xml" path="//Member[@MemberName='OnAppAction'][2]/Docs" />
		public static event EventHandler<AppActionEventArgs>? OnAppAction
		{
			add => Current.AppActionActivated += value;
			remove => Current.AppActionActivated -= value;
		}

		static IAppActions Current => ApplicationModel.AppActions.Current;
	}
}
