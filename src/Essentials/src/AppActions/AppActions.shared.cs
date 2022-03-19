#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.ApplicationModel
{
	public interface IAppActions
	{
		bool IsSupported { get; }

		Task<IEnumerable<AppAction>> GetAsync();

		Task SetAsync(IEnumerable<AppAction> actions);

		event EventHandler<AppActionEventArgs>? AppActionActivated;
	}

	public interface IPlatformAppActions
	{
#if WINDOWS
		Task OnLaunched(LaunchActivatedEventArgs e);
#elif IOS || MACCATALYST
		void PerformActionForShortcutItem(UIKit.UIApplication application, UIKit.UIApplicationShortcutItem shortcutItem, UIKit.UIOperationHandler completionHandler);
#elif ANDROID
		void OnNewIntent(Android.Content.Intent? intent);
#endif
	}

	public static class AppActions
	{
		static IAppActions? currentImplementation;

		public static IAppActions Current =>
			currentImplementation ??= new AppActionsImplementation();

		internal static void SetCurrent(IAppActions? implementation) =>
			currentImplementation = implementation;
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
		public AppAction(string id, string title, string? subtitle = null, string? icon = null)
		{
			Id = id ?? throw new ArgumentNullException(nameof(id));
			Title = title ?? throw new ArgumentNullException(nameof(title));

			Subtitle = subtitle;
			Icon = icon;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="//Member[@MemberName='Title']/Docs" />
		public string Title { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="//Member[@MemberName='Subtitle']/Docs" />
		public string? Subtitle { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="//Member[@MemberName='Id']/Docs" />
		public string Id { get; set; }

		internal string? Icon { get; set; }
	}
}
