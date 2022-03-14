using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface IAppActions
	{
		bool IsSupported { get; }

		string Type { get; }

		Task<IEnumerable<Maui.Essentials.AppAction>> GetAsync ();
		Task SetAsync (IEnumerable<AppAction> actions);	

		Task SetAsync (params AppAction[] actions);		
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppActions']/Docs" />
	public static partial class AppActions
	{
		internal static bool IsSupported
			=> Current.IsSupported;

		internal static string Type
			=> Current.Type;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="//Member[@MemberName='GetAsync']/Docs" />
		public static Task<IEnumerable<AppAction>> GetAsync()
			=> Current.GetAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="//Member[@MemberName='SetAsync'][1]/Docs" />
		public static Task SetAsync(params AppAction[] actions)
			=> Current.SetAsync(actions);

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="//Member[@MemberName='SetAsync'][2]/Docs" />
		public static Task SetAsync(IEnumerable<AppAction> actions)
			=> Current.SetAsync(actions);

		public static event EventHandler<AppActionEventArgs> OnAppAction;

		internal static void InvokeOnAppAction(object sender, AppAction appAction)
			=> OnAppAction?.Invoke(sender, new AppActionEventArgs(appAction));

#nullable enable
		static IAppActions? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IAppActions Current =>
			currentImplementation ??= new AppActionsImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IAppActions? implementation) =>
			currentImplementation = implementation;
#nullable disable
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
		public string Type => "XE_APP_ACTION_TYPE";

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="//Member[@MemberName='Title']/Docs" />
		public string Title { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="//Member[@MemberName='Subtitle']/Docs" />
		public string Subtitle { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="//Member[@MemberName='Id']/Docs" />
		public string Id { get; set; }

		internal string Icon { get; set; }

	}
}
