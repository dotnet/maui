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
		/// <remarks>
		/// Instance subscribers are held weakly. Keep the subscriber alive while it should receive app actions.
		/// </remarks>
		public static event EventHandler<AppActionEventArgs>? OnAppAction
		{
			add
			{
				if (value is not null)
					subscriptions.GetValue(Current, publisher => new AppActionSubscriptions(publisher)).Add(value);
			}
			remove
			{
				if (value is not null && subscriptions.TryGetValue(Current, out var handlers))
					handlers.Remove(value);
			}
		}

		static readonly ConditionalWeakTable<IAppActions, AppActionSubscriptions> subscriptions = new();

		sealed class AppActionSubscriptions
		{
			readonly IAppActions _publisher;
			readonly List<Subscription> _handlers = new();

			public AppActionSubscriptions(IAppActions publisher) => _publisher = publisher;

			public void Add(EventHandler<AppActionEventArgs> handler)
			{
				lock (_handlers)
				{
					Prune();
					foreach (EventHandler<AppActionEventArgs> item in handler.GetInvocationList())
					{
						var subscription = new Subscription(this, item);
						_handlers.Add(subscription);
						_publisher.AppActionActivated += subscription.Invoke;
					}
				}
			}

			public void Remove(EventHandler<AppActionEventArgs> handler)
			{
				var invocationList = handler.GetInvocationList();
				lock (_handlers)
				{
					Prune();
					for (int start = _handlers.Count - invocationList.Length; start >= 0; start--)
					{
						int offset = 0;
						while (offset < invocationList.Length &&
							_handlers[start + offset].Matches((EventHandler<AppActionEventArgs>)invocationList[offset]))
							offset++;

						if (offset != invocationList.Length)
							continue;

						for (int i = 0; i < invocationList.Length; i++)
							_publisher.AppActionActivated -= _handlers[start + i].Invoke;
						_handlers.RemoveRange(start, invocationList.Length);
						return;
					}
				}
			}

			void Prune()
			{
				for (int i = _handlers.Count - 1; i >= 0; i--)
				{
					if (_handlers[i].IsAlive)
						continue;

					_publisher.AppActionActivated -= _handlers[i].Invoke;
					_handlers.RemoveAt(i);
				}
			}

			sealed class Subscription
			{
				readonly AppActionSubscriptions _owner;
				readonly WeakReference<object>? _target;
				readonly MethodInfo _method;
				readonly EventHandler<AppActionEventArgs>? _staticHandler;

				public Subscription(AppActionSubscriptions owner, EventHandler<AppActionEventArgs> handler)
				{
					_owner = owner;
					_method = handler.Method;
					if (handler.Target is object target)
						_target = new WeakReference<object>(target);
					else
						_staticHandler = handler;
				}

				public bool IsAlive => _target is null || _target.TryGetTarget(out _);

				public bool Matches(EventHandler<AppActionEventArgs> handler) =>
					_method == handler.Method &&
					(_target is null ? handler.Target is null :
						_target.TryGetTarget(out var target) && ReferenceEquals(target, handler.Target));

				public void Invoke(object? sender, AppActionEventArgs args)
				{
					if (_staticHandler is not null)
						_staticHandler(sender, args);
					else if (_target!.TryGetTarget(out var target))
						((EventHandler<AppActionEventArgs>)_method.CreateDelegate(typeof(EventHandler<AppActionEventArgs>), target))(sender, args);
					else
					{
						lock (_owner._handlers)
							_owner.Prune();
					}
				}
			}
		}

		static IAppActions? currentImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IAppActions Current =>
			currentImplementation ??= new AppActionsImplementation();

		internal static void SetCurrent(IAppActions? implementation) =>
			currentImplementation = implementation;
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