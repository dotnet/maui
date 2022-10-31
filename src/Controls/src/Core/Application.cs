#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="Type[@FullName='Microsoft.Maui.Controls.Application']/Docs/*" />
	public partial class Application : Element, IResourcesProvider, IApplicationController, IElementConfiguration<Application>, IVisualTreeElement
	{
		readonly WeakEventManager _weakEventManager = new WeakEventManager();
		readonly Lazy<PlatformConfigurationRegistry<Application>> _platformConfigurationRegistry;

#pragma warning disable CS0612 // Type or member is obsolete
		readonly Lazy<IResourceDictionary> _systemResources;
#pragma warning restore CS0612 // Type or member is obsolete

		IAppIndexingProvider? _appIndexProvider;
		ReadOnlyCollection<Element>? _logicalChildren;
		bool _isStarted;

		static readonly SemaphoreSlim SaveSemaphore = new SemaphoreSlim(1, 1);

		/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Application() : this(true)
		{
		}

		internal Application(bool setCurrentApplication)
		{
			if (setCurrentApplication)
				SetCurrentApplication(this);

#pragma warning disable CS0612 // Type or member is obsolete
			_systemResources = new Lazy<IResourceDictionary>(() =>
			{
				var systemResources = DependencyService.Get<ISystemResourcesProvider>().GetSystemResources();
				systemResources.ValuesChanged += OnParentResourcesChanged;
				return systemResources;
			});
#pragma warning restore CS0612 // Type or member is obsolete

			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Application>>(() => new PlatformConfigurationRegistry<Application>(this));

			_lastAppTheme = PlatformAppTheme;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='Quit']/Docs/*" />
		public void Quit()
		{
			Handler?.Invoke(ApplicationHandler.TerminateCommandKey);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='AppLinks']/Docs/*" />
		public IAppLinks AppLinks
		{
			get
			{
				if (_appIndexProvider == null)
					throw new ArgumentException("No IAppIndexingProvider was provided");
				if (_appIndexProvider.AppLinks == null)
					throw new ArgumentException("No AppLinks implementation was found, if in Android make sure you installed the Microsoft.Maui.Controls.AppLinks");
				return _appIndexProvider.AppLinks;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='SetCurrentApplication']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrentApplication(Application value) => Current = value;

		/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='Current']/Docs/*" />
		public static Application? Current { get; set; }

		Page? _singleWindowMainPage;

		/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='MainPage']/Docs/*" />
		public Page? MainPage
		{
			get
			{
				if (Windows.Count == 0)
					return _singleWindowMainPage;

				return Windows[0].Page;
			}
			set
			{
				if (MainPage == value)
					return;

				OnPropertyChanging();

				_singleWindowMainPage = value;

				if (Windows.Count == 1)
				{
					Windows[0].Page = value;
				}

				OnPropertyChanged();
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='Properties']/Docs/*" />
		[Obsolete("Properties API is obsolete, use Microsoft.Maui.Storage.Preferences instead.", error: true)]
		public IDictionary<string, object> Properties => throw new NotSupportedException("Properties API is obsolete, use Microsoft.Maui.Storage.Preferences instead.");

		internal override IReadOnlyList<Element> LogicalChildrenInternal =>
			_logicalChildren ??= new ReadOnlyCollection<Element>(InternalChildren);

		/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='NavigationProxy']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public NavigationProxy? NavigationProxy { get; private set; }

		internal IResourceDictionary SystemResources => _systemResources.Value;

		ObservableCollection<Element> InternalChildren { get; } = new ObservableCollection<Element>();

		/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='SetAppIndexingProvider']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetAppIndexingProvider(IAppIndexingProvider provider)
		{
			_appIndexProvider = provider;
		}

		ResourceDictionary? _resources;
		bool IResourcesProvider.IsResourcesCreated => _resources != null;

		/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='Resources']/Docs/*" />
		public ResourceDictionary Resources
		{
			get
			{
				if (_resources != null)
					return _resources;

				_resources = new ResourceDictionary();
				((IResourceDictionary)_resources).ValuesChanged += OnResourcesChanged;
				return _resources;
			}
			set
			{
				if (_resources == value)
					return;
				OnPropertyChanging();
				if (_resources != null)
					((IResourceDictionary)_resources).ValuesChanged -= OnResourcesChanged;
				_resources = value;
				OnResourcesChanged(value);
				if (_resources != null)
					((IResourceDictionary)_resources).ValuesChanged += OnResourcesChanged;
				OnPropertyChanged();
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='UserAppTheme']/Docs/*" />
		public AppTheme UserAppTheme
		{
			get => _userAppTheme;
			set
			{
				_userAppTheme = value;
				TriggerThemeChangedActual();
			}
		}

		public AppTheme PlatformAppTheme => AppInfo.RequestedTheme;

		/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='RequestedTheme']/Docs/*" />
		public AppTheme RequestedTheme => UserAppTheme != AppTheme.Unspecified ? UserAppTheme : PlatformAppTheme;

		static Color? _accentColor;
		public static Color? AccentColor
		{
			get => _accentColor ??= GetAccentColor();
			set => _accentColor = value;
		}


		static Color? GetAccentColor()
		{
#if WINDOWS
			if (UI.Xaml.Application.Current.Resources.TryGetValue("SystemColorControlAccentBrush", out object accent) &&
				accent is UI.Xaml.Media.SolidColorBrush scb)
			{
				return scb.ToColor();
			}

			return null;
#elif ANDROID
			if (Current?.Windows?.Count > 0)
				return Current.Windows[0].MauiContext.Context?.GetAccentColor();

			return null;
#elif IOS
			return ColorExtensions.AccentColor.ToColor();
#else
			return Color.FromRgba(50, 79, 133, 255);
#endif
		}

		public event EventHandler<AppThemeChangedEventArgs> RequestedThemeChanged
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		bool _themeChangedFiring;
		AppTheme _lastAppTheme = AppTheme.Unspecified;
		AppTheme _userAppTheme = AppTheme.Unspecified;

		void TriggerThemeChangedActual()
		{
			var newTheme = RequestedTheme;

			// On iOS the event is triggered more than once.
			// To minimize that for us, we only do it when the theme actually changes and it's not currently firing
			if (_themeChangedFiring || newTheme == _lastAppTheme)
				return;

			try
			{
				_themeChangedFiring = true;
				_lastAppTheme = newTheme;

				_weakEventManager.HandleEvent(this, new AppThemeChangedEventArgs(newTheme), nameof(RequestedThemeChanged));
			}
			finally
			{
				_themeChangedFiring = false;
			}
		}

		public event EventHandler<ModalPoppedEventArgs>? ModalPopped;

		public event EventHandler<ModalPoppingEventArgs>? ModalPopping;

		public event EventHandler<ModalPushedEventArgs>? ModalPushed;

		public event EventHandler<ModalPushingEventArgs>? ModalPushing;

		internal void NotifyOfWindowModalEvent(EventArgs eventArgs)
		{
			switch (eventArgs)
			{
				case ModalPoppedEventArgs poppedEvents:
					ModalPopped?.Invoke(this, poppedEvents);
					break;
				case ModalPoppingEventArgs poppingEvents:
					ModalPopping?.Invoke(this, poppingEvents);
					break;
				case ModalPushedEventArgs pushedEvents:
					ModalPushed?.Invoke(this, pushedEvents);
					break;
				case ModalPushingEventArgs pushingEvents:
					ModalPushing?.Invoke(this, pushingEvents);
					break;
				default:
					break;
			}
		}

		public event EventHandler<Page>? PageAppearing;

		public event EventHandler<Page>? PageDisappearing;

		/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='SavePropertiesAsync']/Docs/*" />
		[Obsolete("Properties API is obsolete, use Microsoft.Maui.Storage.Preferences instead.", error: true)]
		public Task SavePropertiesAsync() => throw new NotSupportedException("Properties API is obsolete, use Microsoft.Maui.Storage.Preferences instead.");

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Application> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		protected virtual void OnAppLinkRequestReceived(Uri uri)
		{
		}

		protected override void OnParentSet()
		{
			throw new InvalidOperationException("Setting a Parent on Application is invalid.");
		}

		protected virtual void OnResume()
		{
		}

		protected virtual void OnSleep()
		{
		}

		protected virtual void OnStart()
		{
		}

		internal static void ClearCurrent() => Current = null;

		internal static bool IsApplicationOrNull(object? element) =>
			element == null || element is IApplication;

		internal static bool IsApplicationOrWindowOrNull(object? element) =>
			element == null || element is IApplication || element is IWindow;

		internal override void OnParentResourcesChanged(IEnumerable<KeyValuePair<string, object>> values)
		{
			if (!((IResourcesProvider)this).IsResourcesCreated || Resources.Count == 0)
			{
				base.OnParentResourcesChanged(values);
				return;
			}

			var innerKeys = new HashSet<string>();
			var changedResources = new List<KeyValuePair<string, object>>();
			foreach (KeyValuePair<string, object> c in Resources)
				innerKeys.Add(c.Key);
			foreach (KeyValuePair<string, object> value in values)
			{
				if (innerKeys.Add(value.Key))
					changedResources.Add(value);
			}
			OnResourcesChanged(changedResources);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='SendOnAppLinkRequestReceived']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendOnAppLinkRequestReceived(Uri uri)
		{
			OnAppLinkRequestReceived(uri);
		}

		internal void SendResume()
		{
			Current = this;
			OnResume();
		}

		internal void SendSleep()
		{
			OnSleep();
		}

		internal void SendStart()
		{
			if (_isStarted)
				return;

			_isStarted = true;
			OnStart();
		}

		internal void OnPageAppearing(Page page)
			=> PageAppearing?.Invoke(this, page);

		internal void OnPageDisappearing(Page page)
			=> PageDisappearing?.Invoke(this, page);

		protected internal virtual void CleanUp()
		{
			// Unhook everything that's referencing the main page so it can be collected
			// This only comes up if we're disposing of an embedded Forms app, and will
			// eventually go away when we fully support multiple windows
			// TODO MAUI
			//if (_mainPage != null)
			//{
			//	InternalChildren.Remove(_mainPage);
			//	_mainPage.Parent = null;
			//	_mainPage = null;
			//}

			NavigationProxy = null;
		}

		IReadOnlyList<Maui.IVisualTreeElement> IVisualTreeElement.GetVisualChildren() => this.Windows;
	}
}
