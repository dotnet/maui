#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
		Task OnLaunched(UI.Xaml.LaunchActivatedEventArgs e);
#elif IOS || MACCATALYST
		void PerformActionForShortcutItem(UIKit.UIApplication application, UIKit.UIApplicationShortcutItem shortcutItem, UIKit.UIOperationHandler completionHandler);
#elif ANDROID
		void OnNewIntent(Android.Content.Intent? intent);
		void OnResume(Android.Content.Intent? intent);
#endif
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppActions']/Docs/*" />
	public static class AppActions
	{
		public static bool IsSupported
			=> Current.IsSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="//Member[@MemberName='GetAsync']/Docs/*" />
		public static Task<IEnumerable<AppAction>> GetAsync()
			=> Current.GetAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="//Member[@MemberName='SetAsync'][1]/Docs/*" />
		public static Task SetAsync(params AppAction[] actions)
			=> Current.SetAsync(actions);

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActions.xml" path="//Member[@MemberName='SetAsync'][2]/Docs/*" />
		public static Task SetAsync(IEnumerable<AppAction> actions)
			=> Current.SetAsync(actions);

		public static event EventHandler<AppActionEventArgs>? OnAppAction
		{
			add => Current.AppActionActivated += value;
			remove => Current.AppActionActivated -= value;
		}

		static IAppActions? currentImplementation;

		public static IAppActions Current =>
			currentImplementation ??= new AppActionsImplementation();

		internal static void SetCurrent(IAppActions? implementation) =>
			currentImplementation = implementation;
	}

	public static partial class AppActionsExtensions
	{
		static IPlatformAppActions AsPlatform(this IAppActions appActions)
		{
			if (appActions is not IPlatformAppActions platform)
				throw new PlatformNotSupportedException("This implementation of IAppActions does not implement IPlatformAppActions.");

			return platform;
		}

#if WINDOWS
		public static Task OnLaunched(this IAppActions appActions, UI.Xaml.LaunchActivatedEventArgs e) =>
			appActions.AsPlatform().OnLaunched(e);
#elif IOS || MACCATALYST
		public static void PerformActionForShortcutItem(this IAppActions appActions, UIKit.UIApplication application, UIKit.UIApplicationShortcutItem shortcutItem, UIKit.UIOperationHandler completionHandler) =>
			appActions.AsPlatform().PerformActionForShortcutItem(application, shortcutItem, completionHandler);
#elif ANDROID
		public static void OnNewIntent(this IAppActions appActions, Android.Content.Intent? intent) =>
			appActions.AsPlatform().OnNewIntent(intent);

		public static void OnResume(this IAppActions appActions, Android.Content.Intent? intent) =>
			appActions.AsPlatform().OnResume(intent);
#endif
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/AppActionEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppActionEventArgs']/Docs/*" />
	public class AppActionEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActionEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public AppActionEventArgs(AppAction appAction)
			: base() => AppAction = appAction;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppActionEventArgs.xml" path="//Member[@MemberName='AppAction']/Docs/*" />
		public AppAction AppAction { get; }
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppAction']/Docs/*" />
	public class AppAction
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public AppAction(string id, string title, string? subtitle = null, string? icon = null)
		{
			Id = id ?? throw new ArgumentNullException(nameof(id));
			Title = title ?? throw new ArgumentNullException(nameof(title));

			Subtitle = subtitle;
			Icon = icon;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="//Member[@MemberName='Title']/Docs/*" />
		public string Title { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="//Member[@MemberName='Subtitle']/Docs/*" />
		public string? Subtitle { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppAction.xml" path="//Member[@MemberName='Id']/Docs/*" />
		public string Id { get; set; }

		internal string? Icon { get; set; }
	}
}
