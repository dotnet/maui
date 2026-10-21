#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
#if ANDROID
using Android.Content;
#endif

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// The AppActions API lets you create and respond to app shortcuts from the app icon.
	/// </summary>
	public interface IAppActions
	{
		/// <summary>
		/// Gets if app actions are supported on this device.
		/// </summary>
		bool IsSupported { get; }

		/// <summary>
		/// Retrieves all the currently available <see cref="AppAction"/> instances.
		/// </summary>
		/// <returns>A collection of <see cref="AppAction"/> available for this app.</returns>
		Task<IEnumerable<AppAction>> GetAsync();

		/// <summary>
		/// Sets the app actions that will be available for this app.
		/// </summary>
		/// <param name="actions">A collection of <see cref="AppAction"/> that is to be set for this app.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task SetAsync(IEnumerable<AppAction> actions);

		/// <summary>
		/// Event triggered when an app action is activated.
		/// </summary>
		event EventHandler<AppActionEventArgs>? AppActionActivated;
	}

	/// <summary>
	/// Provides abstractions for the platform lifecycle events that are triggered when using App Actions.
	/// </summary>
	public interface IPlatformAppActions
	{
#if WINDOWS
		/// <summary>
		/// The lifecycle event that is triggered when this app is launched.
		/// </summary>
		/// <param name="e">Event arguments containing information about the launch of the application.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task OnLaunched(UI.Xaml.LaunchActivatedEventArgs e);
#elif IOS || MACCATALYST
		/// <summary>
		/// The lifecycle event that is triggered when the user selects an app action.
		/// </summary>
		/// <param name="application">The <see cref="UIKit.UIApplication"/> instance this action is performed for.</param>
		/// <param name="shortcutItem">The shortcut item that was chosen from the app icon.</param>
		/// <param name="completionHandler">Invoked once to report whether the action was handled.</param>
		/// <remarks>Handler failures are reported as unsuccessful before their exceptions are propagated.</remarks>
		void PerformActionForShortcutItem(UIKit.UIApplication application, UIKit.UIApplicationShortcutItem shortcutItem, UIKit.UIOperationHandler completionHandler);
#elif ANDROID
		/// <summary>
		/// The lifecycle event that is triggered when this app is launched.
		/// </summary>
		/// <param name="intent">The provided <see cref="Intent"/> to launch this app with.</param>
		void OnNewIntent(Intent? intent);

		/// <summary>
		/// The lifecycle event that is triggered when this app is launched.
		/// </summary>
		/// <param name="intent">The provided <see cref="Intent"/> to resume this app with.</param>
		void OnResume(Intent? intent);
#endif
	}

	/// <summary>
	/// The AppActions API lets you create and respond to app shortcuts from the app icon.
	/// </summary>
	public static class AppActions
	{
		/// <summary>
		/// Gets if app actions are supported on this device.
		/// </summary>
		public static bool IsSupported
			=> Current.IsSupported;

		/// <summary>
		/// Retrieves all the currently available <see cref="AppAction"/> instances.
		/// </summary>
		/// <returns>A collection of <see cref="AppAction"/> available for this app.</returns>
		public static Task<IEnumerable<AppAction>> GetAsync()
			=> Current.GetAsync();

		/// <summary>
		/// Sets the app actions that will be available for this app.
		/// </summary>
		/// <param name="actions"><see cref="AppAction"/> objects that will be set for this app.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task SetAsync(params AppAction[] actions)
			=> Current.SetAsync(actions);

		/// <summary>
		/// Sets the app actions that will be available for this app.
		/// </summary>
		/// <param name="actions">A collection of <see cref="AppAction"/> that is to be set for this app.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task SetAsync(IEnumerable<AppAction> actions)
			=> Current.SetAsync(actions);

		/// <summary>
		/// Occurs when an app action is activated.
		/// </summary>
		/// <remarks>Instance subscribers are held weakly and must be kept alive by the application.</remarks>
		public static event EventHandler<AppActionEventArgs>? OnAppAction
		{
			add
			{
				if (value is null)
					return;

				var publisher = Current;
				var subscriptions = appActionSubscriptions.GetOrCreateValue(publisher);
				lock (subscriptions)
				{
					foreach (EventHandler<AppActionEventArgs> handler in value.GetInvocationList())
					{
						var subscription = new AppActionSubscription(publisher, subscriptions, handler);
						subscriptions.Add(subscription);
						publisher.AppActionActivated += subscription.OnAppAction;
					}
				}
			}
			remove
			{
				if (value is null)
					return;

				var publisher = Current;
				if (!appActionSubscriptions.TryGetValue(publisher, out var subscriptions))
					return;

				var handlers = value.GetInvocationList();
				lock (subscriptions)
				{
					for (int i = subscriptions.Count - handlers.Length; i >= 0; i--)
					{
						int j = 0;
						while (j < handlers.Length && subscriptions[i + j].Matches(handlers[j]))
							j++;

						if (j != handlers.Length)
							continue;

						for (j = 0; j < handlers.Length; j++)
							publisher.AppActionActivated -= subscriptions[i + j].OnAppAction;

						subscriptions.RemoveRange(i, handlers.Length);
						break;
					}
				}
			}
		}

		static readonly ConditionalWeakTable<IAppActions, List<AppActionSubscription>> appActionSubscriptions = new();
		static IAppActions? currentImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IAppActions Current =>
			currentImplementation ??= new AppActionsImplementation();

		internal static void SetCurrent(IAppActions? implementation) =>
			currentImplementation = implementation;

		sealed class AppActionSubscription
		{
			readonly WeakReference<IAppActions> publisher;
			readonly List<AppActionSubscription> subscriptions;
			readonly WeakReference<object>? target;
			readonly MethodInfo method;
			readonly EventHandler<AppActionEventArgs>? staticHandler;

			public AppActionSubscription(IAppActions publisher, List<AppActionSubscription> subscriptions, EventHandler<AppActionEventArgs> handler)
			{
				this.publisher = new(publisher);
				this.subscriptions = subscriptions;
				method = handler.Method;
				if (handler.Target is object handlerTarget)
					target = new(handlerTarget);
				else
					staticHandler = handler;
			}

			public bool Matches(Delegate handler) =>
				staticHandler is not null ? staticHandler.Equals(handler) :
				target!.TryGetTarget(out var handlerTarget) && ReferenceEquals(handlerTarget, handler.Target) && method.Equals(handler.Method);

			public void OnAppAction(object? sender, AppActionEventArgs args)
			{
				if (staticHandler is not null)
				{
					staticHandler(sender, args);
				}
				else if (target!.TryGetTarget(out var handlerTarget))
				{
					// Recreate a typed delegate so subscriber exceptions propagate without reflection wrapping.
					var handler = (EventHandler<AppActionEventArgs>)method.CreateDelegate(typeof(EventHandler<AppActionEventArgs>), handlerTarget);
					handler(sender, args);
				}
				else if (publisher.TryGetTarget(out var source))
				{
					lock (subscriptions)
					{
						source.AppActionActivated -= OnAppAction;
						subscriptions.Remove(this);
					}
				}
			}
		}
	}

	/// <summary>
	/// Supporting extension methods for the AppActions API.
	/// </summary>
	public static partial class AppActionsExtensions
	{
		static IPlatformAppActions AsPlatform(this IAppActions appActions)
		{
			if (appActions is not IPlatformAppActions platform)
				throw new PlatformNotSupportedException("This implementation of IAppActions does not implement IPlatformAppActions.");

			return platform;
		}

#if WINDOWS
		/// <summary>
		/// The lifecycle event that is triggered when this app is launched.
		/// </summary>
		/// <param name="appActions">Instance of the <see cref="IAppActions"/> object this event is invoked on.</param>
		/// <param name="e">Event arguments containing information about the launch of the application.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task OnLaunched(this IAppActions appActions, UI.Xaml.LaunchActivatedEventArgs e) =>
			appActions.AsPlatform().OnLaunched(e);
#elif IOS || MACCATALYST
		/// <summary>
		/// The lifecycle event that is triggered when the user selects an app action.
		/// </summary>
		/// <param name="appActions">Instance of the <see cref="IAppActions"/> object this event is invoked on.</param>
		/// <param name="application">The <see cref="UIKit.UIApplication"/> instance this action is performed for.</param>
		/// <param name="shortcutItem">The shortcut item that was chosen from the app icon.</param>
		/// <param name="completionHandler">Invoked once to report whether the action was handled.</param>
		public static void PerformActionForShortcutItem(this IAppActions appActions, UIKit.UIApplication application, UIKit.UIApplicationShortcutItem shortcutItem, UIKit.UIOperationHandler completionHandler) =>
			appActions.AsPlatform().PerformActionForShortcutItem(application, shortcutItem, completionHandler);
#elif ANDROID
		/// <summary>
		/// The lifecycle event that is triggered when this app is launched.
		/// </summary>
		/// <param name="appActions">Instance of the <see cref="IAppActions"/> object this event is invoked on.</param>
		/// <param name="intent">The provided <see cref="Intent"/> to launch this app with.</param>
		public static void OnNewIntent(this IAppActions appActions, Intent? intent) =>
			appActions.AsPlatform().OnNewIntent(intent);

		/// <summary>
		/// The lifecycle event that is triggered when this app is launched.
		/// </summary>
		/// <param name="appActions">Instance of the <see cref="IAppActions"/> object this event is invoked on.</param>
		/// <param name="intent">The provided <see cref="Intent"/> to resume this app with.</param>
		public static void OnResume(this IAppActions appActions, Intent? intent) =>
			appActions.AsPlatform().OnResume(intent);
#endif
	}

	/// <summary>
	/// Event arguments containing data that is used when the app started through an <see cref="AppAction"/>.
	/// </summary>
	public class AppActionEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AppActionEventArgs"/> class.
		/// </summary>
		/// <param name="appAction">The <see cref="AppAction"/> that triggered this event.</param>
		public AppActionEventArgs(AppAction appAction)
			: base() => AppAction = appAction;

		/// <summary>
		/// Gets the <see cref="AppAction"/> that triggered this event.
		/// </summary>
		public AppAction AppAction { get; }
	}

	/// <summary>
	/// The <see cref="AppAction"/> class lets you create and respond to app shortcuts from the app icon.
	/// </summary>
	public class AppAction
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AppAction"/> class.
		/// </summary>
		/// <param name="id">A unique identifier used to respond to the action tap.</param>
		/// <param name="title">The visible title to display on the app icon.</param>
		/// <param name="subtitle">If supported, a sub-title to display under the title.</param>
		/// <param name="icon">An icon that is shown next to the title.</param>
		/// <exception cref="ArgumentNullException">Thrown when either <paramref name="id"/> or <paramref name="title"/> is <see langword="null"/>.</exception>
		public AppAction(string id, string title, string? subtitle = null, string? icon = null)
		{
			Id = id ?? throw new ArgumentNullException(nameof(id));
			Title = title ?? throw new ArgumentNullException(nameof(title));

			Subtitle = subtitle;
			Icon = icon;
		}

		/// <summary>
		/// Gets or sets the visible title to display on the app icon.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Gets or sets a sub-title to display under the <see cref="Title"/>.
		/// </summary>
		/// <remarks>Not supported on all platforms.</remarks>
		public string? Subtitle { get; set; }

		/// <summary>
		/// Gets or sets the unique identifier used to respond to the action tap.
		/// </summary>
		public string Id { get; set; }

		internal string? Icon { get; set; }
	}
}