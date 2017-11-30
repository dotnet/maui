using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	public class Application : Element, IResourcesProvider, IApplicationController, IElementConfiguration<Application>
	{
		static Application s_current;
		Task<IDictionary<string, object>> _propertiesTask;
		readonly Lazy<PlatformConfigurationRegistry<Application>> _platformConfigurationRegistry;

		IAppIndexingProvider _appIndexProvider;

		ReadOnlyCollection<Element> _logicalChildren;

		Page _mainPage;

		static SemaphoreSlim SaveSemaphore = new SemaphoreSlim(1, 1);

		protected Application()
		{
			var f = false;
			if (f)
				Loader.Load();
			NavigationProxy = new NavigationImpl(this);
			SetCurrentApplication(this);

			SystemResources = DependencyService.Get<ISystemResourcesProvider>().GetSystemResources();
			SystemResources.ValuesChanged += OnParentResourcesChanged;
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Application>>(() => new PlatformConfigurationRegistry<Application>(this));
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

		public static Application Current
		{
			get { return s_current; }
			set 
			{
				if (s_current == value)
					return;
				if (value == null)
					s_current = null; //Allow to reset current for unittesting
				s_current = value;
			}
		}

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
		public NavigationProxy NavigationProxy { get; }

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
			get {
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

		public event EventHandler<ModalPoppedEventArgs> ModalPopped;

		public event EventHandler<ModalPoppingEventArgs> ModalPopping;

		public event EventHandler<ModalPushedEventArgs> ModalPushed;

		public event EventHandler<ModalPushingEventArgs> ModalPushing;

		public async Task SavePropertiesAsync()
		{
			if (Device.IsInvokeRequired)
				Device.BeginInvokeOnMainThread(async () => await SetPropertiesAsync());
			else
				await SetPropertiesAsync();
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
		public static void ClearCurrent()
		{
			s_current = null;
		}

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
			s_current = this;
			OnResume();
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

		void OnModalPopped(Page modalPage)
		{
			EventHandler<ModalPoppedEventArgs> handler = ModalPopped;
			if (handler != null)
				handler(this, new ModalPoppedEventArgs(modalPage));
		}

		bool OnModalPopping(Page modalPage)
		{
			EventHandler<ModalPoppingEventArgs> handler = ModalPopping;
			var args = new ModalPoppingEventArgs(modalPage);
			if (handler != null)
				handler(this, args);
			return args.Cancel;
		}

		void OnModalPushed(Page modalPage)
		{
			EventHandler<ModalPushedEventArgs> handler = ModalPushed;
			if (handler != null)
				handler(this, new ModalPushedEventArgs(modalPage));
		}

		void OnModalPushing(Page modalPage)
		{
			EventHandler<ModalPushingEventArgs> handler = ModalPushing;
			if (handler != null)
				handler(this, new ModalPushingEventArgs(modalPage));
		}

		void OnPopCanceled()
		{
			EventHandler handler = PopCanceled;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

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