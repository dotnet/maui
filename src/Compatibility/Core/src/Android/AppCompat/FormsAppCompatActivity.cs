using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat;
using AColor = Android.Graphics.Color;
using ARelativeLayout = Android.Widget.RelativeLayout;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[Flags]
	public enum ActivationFlags : long
	{
		DisableSetStatusBarColor = 1 << 0,
	}

	public struct ActivationOptions
	{
		public ActivationOptions(Bundle bundle)
		{
			this = default(ActivationOptions);
			this.Bundle = bundle;
		}
		public Bundle Bundle;
		public ActivationFlags Flags;
	}

	public class FormsAppCompatActivity : AppCompatActivity, IDeviceInfoProvider
	{
		public delegate bool BackButtonPressedEventHandler(object sender, EventArgs e);

		Application _application;

		AndroidApplicationLifecycleState _currentState;
		ARelativeLayout _layout;

		internal AppCompat.Platform Platform { get; private set; }

		AndroidApplicationLifecycleState _previousState;

		bool _renderersAdded;
		bool _activityCreated;
		bool _needMainPageAssign;
		bool _powerSaveReceiverRegistered;
		PowerSaveModeBroadcastReceiver _powerSaveModeBroadcastReceiver;

		static readonly ManualResetEventSlim PreviousActivityDestroying = new ManualResetEventSlim(true);

		// Override this if you want to handle the default Android behavior of restoring fragments on an application restart
		protected virtual bool AllowFragmentRestore => false;

		protected FormsAppCompatActivity()
		{
			_previousState = AndroidApplicationLifecycleState.Uninitialized;
			_currentState = AndroidApplicationLifecycleState.Uninitialized;
			PopupManager.Subscribe(this);
		}

		public event EventHandler ConfigurationChanged;

		public override void OnBackPressed()
		{
			if (BackPressed != null && BackPressed(this, EventArgs.Empty))
				return;
			base.OnBackPressed();
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);
			ConfigurationChanged?.Invoke(this, new EventArgs());

			Microsoft.Maui.Controls.Application.Current?.TriggerThemeChanged(new AppThemeChangedEventArgs(Microsoft.Maui.Controls.Application.Current.RequestedTheme));
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			if (item.ItemId == global::Android.Resource.Id.Home)
				BackPressed?.Invoke(this, EventArgs.Empty);

			return base.OnOptionsItemSelected(item);
		}

		public void SetStatusBarColor(AColor color)
		{
			if (Forms.IsLollipopOrNewer)
			{
				Window.SetStatusBarColor(color);
			}
		}

		static void RegisterHandler(Type target, Type handler, Type filter)
		{
			Profile.FrameBegin();

			Profile.FramePartition(target.Name);
			Type current = Registrar.Registered.GetHandlerType(target);

			if (current == filter)
			{
				Profile.FramePartition("Register");
				Registrar.Registered.Register(target, handler);
			}

			Profile.FrameEnd();
		}

		// This is currently being used by the previewer please do not change or remove this
		static void RegisterHandlers()
		{
		}

		protected void LoadApplication(Application application)
		{
			Profile.FrameBegin();
			if (!_activityCreated)
			{
				throw new InvalidOperationException("Activity OnCreate was not called prior to loading the application. Did you forget a base.OnCreate call?");
			}

			if (!_renderersAdded)
			{
				Profile.FramePartition("RegisterHandlers");
				RegisterHandlers();
				_renderersAdded = true;
			}

			if (_application != null)
			{
				_application.PropertyChanging -= AppOnPropertyChanging;
				_application.PropertyChanged -= AppOnPropertyChanged;
			}

			Profile.FramePartition("SetAppIndexingProvider");
			_application = application ?? throw new ArgumentNullException(nameof(application));
			((IApplicationController)application).SetAppIndexingProvider(new AndroidAppIndexProvider(this));

			Profile.FramePartition("SetCurrentApplication");
			Microsoft.Maui.Controls.Application.SetCurrentApplication(application);

			Profile.FramePartition("SetSoftInputMode");
			if (Microsoft.Maui.Controls.Application.Current.OnThisPlatform().GetWindowSoftInputModeAdjust() != WindowSoftInputModeAdjust.Unspecified)
				SetSoftInputMode();

			Profile.FramePartition("CheckForAppLink");
			CheckForAppLink(Intent);

			application.PropertyChanged += AppOnPropertyChanged;
			application.PropertyChanging += AppOnPropertyChanging;

			// Wait if old activity destroying is not finished
			PreviousActivityDestroying.Wait();

			Profile.FramePartition(nameof(SetMainPage));

			SetMainPage();

			Profile.FrameEnd();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			ActivityResultCallbackRegistry.InvokeCallback(requestCode, resultCode, data);
		}

		protected void OnCreate(ActivationOptions options)
		{
			OnCreate(options.Bundle, options.Flags);
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			OnCreate(savedInstanceState, default(ActivationFlags));
		}

		void OnCreate(
			Bundle savedInstanceState,
			ActivationFlags flags)
		{
			Profile.FrameBegin();
			_activityCreated = true;
			if (!AllowFragmentRestore)
			{
				// Remove the automatically persisted fragment structure; we don't need them
				// because we're rebuilding everything from scratch. This saves a bit of memory
				// and prevents loading errors from child fragment managers
				savedInstanceState?.Remove("android:support:fragments");
			}

			Profile.FramePartition("Xamarin.Android.OnCreate");
			base.OnCreate(savedInstanceState);

			Profile.FramePartition("SetSupportActionBar");
			AToolbar bar = null;

			if (ToolbarResource == 0)
			{
				ToolbarResource = Resource.Layout.toolbar;
			}

			if (TabLayoutResource == 0)
			{
				TabLayoutResource = Resource.Layout.tabbar;
			}

			if (ToolbarResource != 0)
			{
				try
				{
					bar = LayoutInflater.Inflate(ToolbarResource, null).JavaCast<AToolbar>();
				}
				catch (global::Android.Views.InflateException ie)
				{
					throw new InvalidOperationException("ToolbarResource must be set to a androidx.appcompat.widget.Toolbar", ie);
				}

				if (bar == null)
					throw new InvalidOperationException("ToolbarResource must be set to a androidx.appcompat.widget.Toolbar");
			}
			else
			{
				bar = new AToolbar(this);
			}

			SetSupportActionBar(bar);

			Profile.FramePartition("SetContentView");
			_layout = new ARelativeLayout(BaseContext);
			SetContentView(_layout);

			Profile.FramePartition("OnStateChanged");
			Microsoft.Maui.Controls.Application.Current = null;

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnCreate;

			OnStateChanged();

			Profile.FramePartition("Forms.IsLollipopOrNewer");
			if (Forms.IsLollipopOrNewer)
			{
				// Allow for the status bar color to be changed
				if ((flags & ActivationFlags.DisableSetStatusBarColor) == 0)
				{
					Profile.FramePartition("Set DrawsSysBarBkgrnds");
					Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
				}
			}
			if (Forms.IsLollipopOrNewer)
			{
				// Listen for the device going into power save mode so we can handle animations being disabled
				Profile.FramePartition("Allocate PowerSaveModeReceiver");
				_powerSaveModeBroadcastReceiver = new PowerSaveModeBroadcastReceiver();
			}

			Profile.FrameEnd();
		}

		protected override void OnDestroy()
		{
			PreviousActivityDestroying.Reset();

			if (_application != null)
			{
				_application.PropertyChanging -= AppOnPropertyChanging;
				_application.PropertyChanged -= AppOnPropertyChanged;
			}

			PopupManager.Unsubscribe(this);

			if (Platform != null)
			{
				_layout.RemoveView(Platform);
				Platform.Dispose();
			}

			PreviousActivityDestroying.Set();

			// call at the end to avoid race conditions with Platform dispose
			base.OnDestroy();
		}

		protected override void OnNewIntent(Intent intent)
		{
			base.OnNewIntent(intent);
			CheckForAppLink(intent);
		}

		protected override void OnPause()
		{
			_layout.HideKeyboard(true);

			if (_powerSaveReceiverRegistered && Forms.IsLollipopOrNewer)
			{
				// Don't listen for power save mode changes while we're paused
				UnregisterReceiver(_powerSaveModeBroadcastReceiver);
				_powerSaveReceiverRegistered = false;
			}

			// Stop animations or other ongoing actions that could consume CPU
			// Commit unsaved changes, build only if users expect such changes to be permanently saved when thy leave such as a draft email
			// Release system resources, such as broadcast receivers, handles to sensors (like GPS), or any resources that may affect battery life when your activity is paused.
			// Avoid writing to permanent storage and CPU intensive tasks
			base.OnPause();

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnPause;

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
			Profile.FrameBegin();

			// counterpart to OnPause
			base.OnResume();

			if (_application != null && CurrentFocus != null && _application.OnThisPlatform().GetShouldPreserveKeyboardOnResume())
			{
				CurrentFocus.ShowKeyboard();
			}

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnResume;

			if (_needMainPageAssign)
			{
				_needMainPageAssign = false;
				SettingMainPage();
				SetMainPage();
			}

			if (!_powerSaveReceiverRegistered && Forms.IsLollipopOrNewer)
			{
				// Start listening for power save mode changes
				RegisterReceiver(_powerSaveModeBroadcastReceiver, new IntentFilter(
					PowerManager.ActionPowerSaveModeChanged
				));

				_powerSaveReceiverRegistered = true;
			}

			OnStateChanged();

			Profile.FrameEnd();
		}

		protected override void OnStart()
		{
			Profile.FrameBegin();

			Profile.FramePartition("Android OnStart");
			base.OnStart();

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnStart;

			Profile.FramePartition("OnStateChanged");
			OnStateChanged();

			Profile.FrameEnd();
		}

		// Scenarios that stop and restart your app
		// -- Switches from your app to another app, activity restarts when clicking on the app again.
		// -- Action in your app that starts a new Activity, the current activity is stopped and the second is created, pressing back restarts the activity
		// -- The user receives a phone call while using your app on his or her phone
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
			// Activity in pause must not react to application changes
			if (_currentState >= AndroidApplicationLifecycleState.OnPause)
			{
				// If the main page is set after the activity has been paused, delay it to resume step
				if (args.PropertyName == nameof(_application.MainPage))
				{
					_needMainPageAssign = true;
				}

				return;
			}

			if (args.PropertyName == nameof(_application.MainPage))
				SetMainPage();
			if (args.PropertyName == PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName)
				SetSoftInputMode();
		}

		void AppOnPropertyChanging(object sender, PropertyChangingEventArgs args)
		{
			// Activity in pause must not react to application changes
			if (_currentState >= AndroidApplicationLifecycleState.OnPause)
			{
				return;
			}

			if (args.PropertyName == nameof(_application.MainPage))
			{
				SettingMainPage();
			}
		}

		void CheckForAppLink(Intent intent)
		{
			string action = intent.Action;
			string strLink = intent.DataString;
			if (Intent.ActionView != action || string.IsNullOrWhiteSpace(strLink))
				return;

			var link = new Uri(strLink);
			_application?.SendOnAppLinkRequestReceived(link);
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

			Platform = new AppCompat.Platform(this);
			Platform.SetPage(page);
			_layout.AddView(Platform);
			_layout.BringToFront();
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

		// This is currently being used by the previewer please do not change or remove this
		void RegisterHandlerForDefaultRenderer(Type target, Type handler, Type filter)
		{
			RegisterHandler(target, handler, filter);
		}

		void SetMainPage()
		{
			InternalSetPage(_application.MainPage);
		}

		void SettingMainPage()
		{
			Platform.SettingNewPage();
		}

		void SetSoftInputMode()
		{
			var adjust = SoftInput.AdjustPan;

			if (Microsoft.Maui.Controls.Application.Current != null)
			{
				WindowSoftInputModeAdjust elementValue = Microsoft.Maui.Controls.Application.Current.OnThisPlatform().GetWindowSoftInputModeAdjust();
				switch (elementValue)
				{
					case WindowSoftInputModeAdjust.Resize:
						adjust = SoftInput.AdjustResize;
						break;
					case WindowSoftInputModeAdjust.Unspecified:
						adjust = SoftInput.AdjustUnspecified;
						break;
					default:
						adjust = SoftInput.AdjustPan;
						break;
				}
			}

			Window.SetSoftInputMode(adjust);
		}

		internal class DefaultApplication : Application
		{
		}

		#region Statics

		public static event BackButtonPressedEventHandler BackPressed;

		public static int TabLayoutResource { get; set; }

		public static int ToolbarResource { get; set; }

		#endregion
	}
}
