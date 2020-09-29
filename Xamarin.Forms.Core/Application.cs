using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	public class Application : Element, IResourcesProvider, IApplicationController, IElementConfiguration<Application>
	{
		readonly WeakEventManager _weakEventManager = new WeakEventManager();
		Task<IDictionary<string, object>> _propertiesTask;
		readonly Lazy<PlatformConfigurationRegistry<Application>> _platformConfigurationRegistry;

		public override IDispatcher Dispatcher => this.GetDispatcher();

		IAppIndexingProvider _appIndexProvider;
		ReadOnlyCollection<Element> _logicalChildren;
		Page _mainPage;

		static readonly SemaphoreSlim SaveSemaphore = new SemaphoreSlim(1, 1);

		[Obsolete("Assign the LogWarningsListener")]
		public static bool LogWarningsToApplicationOutput { get; set; }

		public Application()
		{
			var f = false;
			if (f)
				Loader.Load();

			SetCurrentApplication(this);
			NavigationProxy = new NavigationImpl(this);
			SystemResources = DependencyService.Get<ISystemResourcesProvider>().GetSystemResources();
			SystemResources.ValuesChanged += OnParentResourcesChanged;
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Application>>(() => new PlatformConfigurationRegistry<Application>(this));
			// Initialize this value, when the app loads
			_lastAppTheme = RequestedTheme;
		}

		public void Quit()
		{
			Device.PlatformServices?.QuitApplication();
		}

		public IAppLinks AppLinks
		{
			get
			{
				if (_appIndexProvider == null)
					throw new ArgumentException("No IAppIndexingProvider was provided");
				if (_appIndexProvider.AppLinks == null)
					throw new ArgumentException("No AppLinks implementation was found, if in Android make sure you installed the Xamarin.Forms.AppLinks");
				return _appIndexProvider.AppLinks;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrentApplication(Application value) => Current = value;

		public static Application Current { get; set; }

		public Page MainPage
		{
			get { return _mainPage; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				if (_mainPage == value)
					return;

				OnPropertyChanging();
				if (_mainPage != null)
				{
					InternalChildren.Remove(_mainPage);

					_mainPage.Parent = null;
				}

				_mainPage = value;

				if (_mainPage != null)
				{
					_mainPage.Parent = this;
					_mainPage.NavigationProxy.Inner = NavigationProxy;
					InternalChildren.Add(_mainPage);
				}
				OnPropertyChanged();
			}
		}

		public IDictionary<string, object> Properties
		{
			get
			{
				if (_propertiesTask == null)
				{
					_propertiesTask = GetPropertiesAsync();
				}

				return _propertiesTask.Result;
			}
		}

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal
		{
			get { return _logicalChildren ?? (_logicalChildren = new ReadOnlyCollection<Element>(InternalChildren)); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public NavigationProxy NavigationProxy { get; private set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public int PanGestureId { get; set; }

		internal IResourceDictionary SystemResources { get; }

		ObservableCollection<Element> InternalChildren { get; } = new ObservableCollection<Element>();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetAppIndexingProvider(IAppIndexingProvider provider)
		{
			_appIndexProvider = provider;
		}

		ResourceDictionary _resources;
		bool IResourcesProvider.IsResourcesCreated => _resources != null;

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

		public OSAppTheme UserAppTheme
		{
			get => _userAppTheme;
			set
			{
				_userAppTheme = value;
				TriggerThemeChangedActual(new AppThemeChangedEventArgs(value));
			}
		}
		public OSAppTheme RequestedTheme => UserAppTheme == OSAppTheme.Unspecified ? Device.PlatformServices.RequestedTheme : UserAppTheme;

		public event EventHandler<AppThemeChangedEventArgs> RequestedThemeChanged
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		bool _themeChangedFiring;
		OSAppTheme _lastAppTheme;
		OSAppTheme _userAppTheme = OSAppTheme.Unspecified;


		[EditorBrowsable(EditorBrowsableState.Never)]
		public void TriggerThemeChanged(AppThemeChangedEventArgs args)
		{
			if (UserAppTheme != OSAppTheme.Unspecified)
				return;
			TriggerThemeChangedActual(args);
		}

		void TriggerThemeChangedActual(AppThemeChangedEventArgs args)
		{
			// On iOS the event is triggered more than once.
			// To minimize that for us, we only do it when the theme actually changes and it's not currently firing
			if (_themeChangedFiring || RequestedTheme == _lastAppTheme)
				return;

			try
			{
				_themeChangedFiring = true;
				_lastAppTheme = RequestedTheme;

				_weakEventManager.HandleEvent(this, args, nameof(RequestedThemeChanged));
			}
			finally
			{
				_themeChangedFiring = false;
			}
		}

		public event EventHandler<ModalPoppedEventArgs> ModalPopped;

		public event EventHandler<ModalPoppingEventArgs> ModalPopping;

		public event EventHandler<ModalPushedEventArgs> ModalPushed;

		public event EventHandler<ModalPushingEventArgs> ModalPushing;

		public event EventHandler<Page> PageAppearing;

		public event EventHandler<Page> PageDisappearing;

		async void SaveProperties()
		{
			try
			{
				await SetPropertiesAsync();
			}
			catch (Exception exc)
			{
				Internals.Log.Warning(nameof(Application), $"Exception while saving Application Properties: {exc}");
			}
		}

		public async Task SavePropertiesAsync()
		{
			if (Dispatcher.IsInvokeRequired)
			{
				Dispatcher.BeginInvokeOnMainThread(SaveProperties);
			}
			else
			{
				await SetPropertiesAsync();
			}
		}

		// Don't use this unless there really is no better option
		internal void SavePropertiesAsFireAndForget()
		{
			if (Dispatcher.IsInvokeRequired)
			{
				Dispatcher.BeginInvokeOnMainThread(SaveProperties);
			}
			else
			{
				SaveProperties();
			}
		}

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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void ClearCurrent() => Current = null;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsApplicationOrNull(Element element)
		{
			return element == null || element is Application;
		}

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

		internal event EventHandler PopCanceled;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendOnAppLinkRequestReceived(Uri uri)
		{
			OnAppLinkRequestReceived(uri);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendResume()
		{
			Current = this;
			OnResume();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendSleep()
		{
			OnSleep();
			SavePropertiesAsFireAndForget();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public Task SendSleepAsync()
		{
			OnSleep();
			return SavePropertiesAsync();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendStart()
		{
			OnStart();
		}

		async Task<IDictionary<string, object>> GetPropertiesAsync()
		{
			var deserializer = DependencyService.Get<IDeserializer>();
			if (deserializer == null)
			{
				Log.Warning("Startup", "No IDeserialzier was found registered");
				return new Dictionary<string, object>(4);
			}

			IDictionary<string, object> properties = await deserializer.DeserializePropertiesAsync().ConfigureAwait(false);
			if (properties == null)
				properties = new Dictionary<string, object>(4);

			return properties;
		}

		internal void OnPageAppearing(Page page)
			=> PageAppearing?.Invoke(this, page);

		internal void OnPageDisappearing(Page page)
			=> PageDisappearing?.Invoke(this, page);

		void OnModalPopped(Page modalPage)
			=> ModalPopped?.Invoke(this, new ModalPoppedEventArgs(modalPage));

		bool OnModalPopping(Page modalPage)
		{
			var args = new ModalPoppingEventArgs(modalPage);
			ModalPopping?.Invoke(this, args);
			return args.Cancel;
		}

		void OnModalPushed(Page modalPage)
			=> ModalPushed?.Invoke(this, new ModalPushedEventArgs(modalPage));

		void OnModalPushing(Page modalPage)
			=> ModalPushing?.Invoke(this, new ModalPushingEventArgs(modalPage));

		void OnPopCanceled()
			=> PopCanceled?.Invoke(this, EventArgs.Empty);

		async Task SetPropertiesAsync()
		{
			await SaveSemaphore.WaitAsync();
			try
			{
				await DependencyService.Get<IDeserializer>().SerializePropertiesAsync(Properties);
			}
			finally
			{
				SaveSemaphore.Release();
			}

		}

		protected internal virtual void CleanUp()
		{
			// Unhook everything that's referencing the main page so it can be collected
			// This only comes up if we're disposing of an embedded Forms app, and will
			// eventually go away when we fully support multiple windows
			if (_mainPage != null)
			{
				InternalChildren.Remove(_mainPage);
				_mainPage.Parent = null;
				_mainPage = null;
			}

			NavigationProxy = null;
		}

		class NavigationImpl : NavigationProxy
		{
			readonly Application _owner;

			public NavigationImpl(Application owner)
			{
				_owner = owner;
			}

			protected override async Task<Page> OnPopModal(bool animated)
			{
				Page modal = ModalStack[ModalStack.Count - 1];
				if (_owner.OnModalPopping(modal))
				{
					_owner.OnPopCanceled();
					return null;
				}
				Page result = await base.OnPopModal(animated);
				result.Parent = null;
				_owner.OnModalPopped(result);
				return result;
			}

			protected override async Task OnPushModal(Page modal, bool animated)
			{
				_owner.OnModalPushing(modal);

				modal.Parent = _owner;

				if (modal.NavigationProxy.ModalStack.Count == 0)
				{
					modal.NavigationProxy.Inner = this;
					await base.OnPushModal(modal, animated);
				}
				else
				{
					await base.OnPushModal(modal, animated);
					modal.NavigationProxy.Inner = this;
				}

				_owner.OnModalPushed(modal);
			}
		}
	}
}