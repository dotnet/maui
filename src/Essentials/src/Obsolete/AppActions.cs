#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppActions']/Docs" />
	public static partial class AppActions
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="//Member[@MemberName='GetAsync']/Docs" />
		[Obsolete($"Use {nameof(AppActions)}.{nameof(Current)} instead.", true)]
		public static Task<IEnumerable<AppAction>> GetAsync()
			=> Current.GetAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="//Member[@MemberName='SetAsync'][1]/Docs" />
		[Obsolete($"Use {nameof(AppActions)}.{nameof(Current)} instead.", true)]
		public static Task SetAsync(params AppAction[] actions)
			=> Current.SetAsync(actions);

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="//Member[@MemberName='SetAsync'][2]/Docs" />
		[Obsolete($"Use {nameof(AppActions)}.{nameof(Current)} instead.", true)]
		public static Task SetAsync(IEnumerable<AppAction> actions)
			=> Current.SetAsync(actions);

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="//Member[@MemberName='OnAppAction'][2]/Docs" />
		[Obsolete($"Use {nameof(AppActions)}.{nameof(Current)} instead.", true)]
		public static event EventHandler<AppActionEventArgs>? OnAppAction
		{
			add => Current.AppActionActivated += value;
			remove => Current.AppActionActivated -= value;
		}
	}
}
