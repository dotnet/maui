using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppActions']/Docs" />
	public static partial class AppActions
	{
		internal static bool IsSupported
			=> PlatformIsSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="//Member[@MemberName='GetAsync']/Docs" />
		public static Task<IEnumerable<AppAction>> GetAsync()
			=> PlatformGetAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="//Member[@MemberName='SetAsync']/Docs" />
		public static Task SetAsync(params AppAction[] actions)
			=> PlatformSetAsync(actions);

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="//Member[@MemberName='SetAsync']/Docs" />
		public static Task SetAsync(IEnumerable<AppAction> actions)
			=> PlatformSetAsync(actions);

		public static event EventHandler<AppActionEventArgs> OnAppAction;

		internal static void InvokeOnAppAction(object sender, AppAction appAction)
			=> OnAppAction?.Invoke(sender, new AppActionEventArgs(appAction));
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/AppActionEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppActionEventArgs']/Docs" />
	public class AppActionEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActionEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public AppActionEventArgs(AppAction appAction)
			: base() => AppAction = appAction;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActionEventArgs.xml" path="//Member[@MemberName='AppAction']/Docs" />
		public AppAction AppAction { get; }
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppAction']/Docs" />
	public class AppAction
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public AppAction(string id, string title, string subtitle = null, string icon = null)
		{
			Id = id ?? throw new ArgumentNullException(nameof(id));
			Title = title ?? throw new ArgumentNullException(nameof(title));

			Subtitle = subtitle;
			Icon = icon;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="//Member[@MemberName='Title']/Docs" />
		public string Title { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="//Member[@MemberName='Subtitle']/Docs" />
		public string Subtitle { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="//Member[@MemberName='Id']/Docs" />
		public string Id { get; set; }

		internal string Icon { get; set; }
	}
}
