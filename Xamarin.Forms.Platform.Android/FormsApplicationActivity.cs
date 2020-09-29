using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms.Platform.Android
{
	public class FormsApplicationActivity : Activity, IDeviceInfoProvider
	{
		public delegate bool BackButtonPressedEventHandler(object sender, EventArgs e);

		Application _application;
		AndroidApplicationLifecycleState _currentState;
		LinearLayout _layout;

		bool _powerSaveReceiverRegistered;
		PowerSaveModeBroadcastReceiver _powerSaveModeBroadcastReceiver;

		AndroidApplicationLifecycleState _previousState;

		internal Platform Platform { get; private set; }

		protected FormsApplicationActivity()
		{
			_previousState = AndroidApplicationLifecycleState.Uninitialized;
			_currentState = AndroidApplicationLifecycleState.Uninitialized;
			PopupManager.Subscribe(this);

			// We seem to get a different color from the theme than we use by default
			// Override to use the old color
			Forms.ColorButtonNormalOverride = Color.FromHex("#5a595b");
		}

		public event EventHandler ConfigurationChanged;

		public static event BackButtonPressedEventHandler BackPressed;

		public override void OnBackPressed()
		{
			if (BackPressed != null && BackPressed(this, EventArgs.Empty))
				return;
			base.OnBackPressed();
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);
			EventHandler handler = ConfigurationChanged;
			handler?.Invoke(this, new EventArgs());
		}

		// FIXME: THIS SHOULD NOT BE MANDATORY
		// or
		// This should be specified in an interface and formalized, perhaps even provide a stock AndroidActivity users
		// can derive from to avoid having to do any work.
		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			if (item.ItemId == global::Android.Resource.Id.Home)
				Platform.SendHomeClicked();
			return base.OnOptionsItemSelected(item);
		}

		public override bool OnPrepareOptionsMenu(IMenu menu)
		{
			Platform.PrepareMenu(menu);
			return base.OnPrepareOptionsMenu(menu);
		}

		[Obsolete("SetPage is obsolete as of version 1.3.0. Please use protected LoadApplication (Application app) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetPage(Page page)
		{
			var application = new DefaultApplication { MainPage = page };
			LoadApplication(application);
		}

		protected void LoadApplication(Application application)
		{
			if (_application != null)
				_application.PropertyChanged -= AppOnPropertyChanged;

			_application = application ?? throw new ArgumentNullException(nameof(application));
			((IApplicationController)application).SetAppIndexingProvider(new AndroidAppIndexProvider(this));
			Xamarin.Forms.Application.SetCurrentApplication(application);

			SetSoftInputMode();

			application.PropertyChanged += AppOnPropertyChanged;

			SetMainPage();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			ActivityResultCallbackRegistry.InvokeCallback(requestCode, resultCode, data);
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			Window.RequestFeature(WindowFeatures.IndeterminateProgress);

			base.OnCreate(savedInstanceState);

			_layout = new LinearLayout(BaseContext);
			SetContentView(_layout);

			Xamarin.Forms.Application.ClearCurrent();

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnCreate;

			if (Forms.IsLollipopOrNewer)
			{
				// Listen for the device going into power save mode so we can handle animations being disabled
				_powerSaveModeBroadcastReceiver = new PowerSaveModeBroadcastReceiver();
			}

			OnStateChanged();
		}

		protected override void OnDestroy()
		{
			// may never be called
			base.OnDestroy();

			if (_application != null)
				_application.PropertyChanged -= AppOnPropertyChanged;

			PopupManager.Unsubscribe(this);

			((IDisposable)Platform)?.Dispose();
		}

		protected override void OnPause()
		{
			_layout.HideKeyboard(true);

			// Stop animations or other ongoing actions that could consume CPU
			// Commit unsaved changes, build only if users expect such changes to be permanently saved when thy leave such as a draft email
			// Release system resources, such as broadcast receivers, handles to sensors (like GPS), or any resources that may affect battery life when your activity is paused.
			// Avoid writing to permanent storage and CPU intensive tasks
			base.OnPause();

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnPause;

			if (_powerSaveReceiverRegistered && Forms.IsLollipopOrNewer)
			{
				// Don't listen for power save mode changes while we're paused
				UnregisterReceiver(_powerSaveModeBroadcastReceiver);
				_powerSaveReceiverRegistered = false;
			}

			OnStateChanged();
		}

		protected override void OnRestart()
		{
			base.OnRestart();

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnRestart;

			OnStateChanged();
		}

		protected override void OnResume()
		{
			// counterpart to OnPause
			base.OnResume();

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnResume;

			if (!_powerSaveReceiverRegistered && Forms.IsLollipopOrNewer)
			{
				// Start listening for power save mode changes
				RegisterReceiver(_powerSaveModeBroadcastReceiver, new IntentFilter(
					PowerManager.ActionPowerSaveModeChanged));

				_powerSaveReceiverRegistered = true;
				_powerSaveModeBroadcastReceiver.CheckAnimationEnabledStatus();
			}

			OnStateChanged();
		}

		protected override void OnStart()
		{
			base.OnStart();

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnStart;

			OnStateChanged();
		}

		// Scenarios that stop and restart you app
		// -- Switches from your app to another app, activity restarts when clicking on the app again.
		// -- Action in your app that starts a new Activity, the current activity is stopped and the second is created, pressing back restarts the activity
		// -- The user recieves a phone call while using your app on his or her phone
		protected override void OnStop()
		{
			// writing to storage happens here!
			// full UI obstruction
			// users focus in another activity
			// perform heavy load shutdown operations
			// clean up resources
			// clean up everything that may leak memory
			base.OnStop();

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnStop;

			OnStateChanged();
		}

		void AppOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName == "MainPage")
				InternalSetPage(_application.MainPage);
			if (args.PropertyName == PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName)
				SetSoftInputMode();
		}

		void InternalSetPage(Page page)
		{
			if (!Forms.IsInitialized)
				throw new InvalidOperationException("Call Forms.Init (Activity, Bundle) before this");

			if (Platform != null)
			{
				Platform.SetPage(page);
				return;
			}

			PopupManager.ResetBusyCount(this);

			Platform = new Platform(this);

			if (_application != null)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				// The Platform property is no longer necessary, but we have to set it because some third-party
				// library might still be retrieving it and using it
				_application.Platform = Platform;
#pragma warning restore CS0618 // Type or member is obsolete
			}

			Platform.SetPage(page);
			_layout.AddView(Platform.GetViewGroup());
		}

		void OnStateChanged()
		{
			if (_application == null)
				return;

			if (_previousState == AndroidApplicationLifecycleState.OnCreate && _currentState == AndroidApplicationLifecycleState.OnStart)
				_application.SendStart();
			else if (_previousState == AndroidApplicationLifecycleState.OnRestart && _currentState == AndroidApplicationLifecycleState.OnStart)
				_application.SendResume();
			else if (_previousState == AndroidApplicationLifecycleState.OnPause && _currentState == AndroidApplicationLifecycleState.OnStop)
				_application.SendSleep();
		}

		void SetMainPage()
		{
			InternalSetPage(_application.MainPage);
		}

		void SetSoftInputMode()
		{
			SoftInput adjust = SoftInput.AdjustPan;

			if (Xamarin.Forms.Application.Current != null)
			{
				var elementValue = Xamarin.Forms.Application.Current.OnThisPlatform().GetWindowSoftInputModeAdjust();
				switch (elementValue)
				{
					default:
					case WindowSoftInputModeAdjust.Pan:
						adjust = SoftInput.AdjustPan;
						break;
					case WindowSoftInputModeAdjust.Resize:
						adjust = SoftInput.AdjustResize;
						break;
				}
			}

			Window.SetSoftInputMode(adjust);
		}

		internal class DefaultApplication : Application
		{
		}
	}
}