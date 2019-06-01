#region

using System;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android.AppCompat;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific.AppCompat;
using AToolbar = Android.Support.V7.Widget.Toolbar;
using AColor = Android.Graphics.Color;
using ARelativeLayout = Android.Widget.RelativeLayout;
using Xamarin.Forms.Internals;

#endregion

namespace Xamarin.Forms.Platform.Android
{
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
		PowerSaveModeBroadcastReceiver _powerSaveModeBroadcastReceiver;

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
			Type current = Registrar.Registered.GetHandlerType(target);
			if (current != filter)
				return;

			Registrar.Registered.Register(target, handler);
		}

		// This is currently being used by the previewer please do not change or remove this
		static void RegisterHandlers()
		{
			RegisterHandler(typeof(NavigationPage), typeof(NavigationPageRenderer), typeof(NavigationRenderer));
			RegisterHandler(typeof(TabbedPage), typeof(TabbedPageRenderer), typeof(TabbedRenderer));
			RegisterHandler(typeof(MasterDetailPage), typeof(MasterDetailPageRenderer), typeof(MasterDetailRenderer));
			RegisterHandler(typeof(Switch), typeof(AppCompat.SwitchRenderer), typeof(SwitchRenderer));
			RegisterHandler(typeof(Picker), typeof(AppCompat.PickerRenderer), typeof(PickerRenderer));
			RegisterHandler(typeof(CarouselPage), typeof(AppCompat.CarouselPageRenderer), typeof(CarouselPageRenderer));
			RegisterHandler(typeof(CheckBox), typeof(CheckBoxRenderer), typeof(CheckBoxDesignerRenderer));

			if (Forms.Flags.Contains(Flags.UseLegacyRenderers))
			{
				RegisterHandler(typeof(Button), typeof(AppCompat.ButtonRenderer), typeof(ButtonRenderer));
				RegisterHandler(typeof(Frame), typeof(AppCompat.FrameRenderer), typeof(FrameRenderer));
			}
			else
			{
				RegisterHandler(typeof(Button), typeof(FastRenderers.ButtonRenderer), typeof(ButtonRenderer));
				RegisterHandler(typeof(Label), typeof(FastRenderers.LabelRenderer), typeof(LabelRenderer));
				RegisterHandler(typeof(Image), typeof(FastRenderers.ImageRenderer), typeof(ImageRenderer));
				RegisterHandler(typeof(Frame), typeof(FastRenderers.FrameRenderer), typeof(FrameRenderer));
			}
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
				RegisterHandlers();
				_renderersAdded = true;
			}

			if (_application != null)
				_application.PropertyChanged -= AppOnPropertyChanged;

			_application = application ?? throw new ArgumentNullException(nameof(application));
			((IApplicationController)application).SetAppIndexingProvider(new AndroidAppIndexProvider(this));
			Xamarin.Forms.Application.SetCurrentApplication(application);

			if (Xamarin.Forms.Application.Current.OnThisPlatform().GetWindowSoftInputModeAdjust() != WindowSoftInputModeAdjust.Unspecified)
				SetSoftInputMode();

			CheckForAppLink(Intent);

			application.PropertyChanged += AppOnPropertyChanged;

			Profile.FramePartition(nameof(SetMainPage));

			SetMainPage();

			Profile.FrameEnd();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			ActivityResultCallbackRegistry.InvokeCallback(requestCode, resultCode, data);
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			_activityCreated = true;
			if (!AllowFragmentRestore)
			{
				// Remove the automatically persisted fragment structure; we don't need them
				// because we're rebuilding everything from scratch. This saves a bit of memory
				// and prevents loading errors from child fragment managers
				savedInstanceState?.Remove("android:support:fragments");
			}

			base.OnCreate(savedInstanceState);

			AToolbar bar;
			if (ToolbarResource != 0)
			{
				bar = LayoutInflater.Inflate(ToolbarResource, null).JavaCast<AToolbar>();
				if (bar == null)
					throw new InvalidOperationException("ToolbarResource must be set to a Android.Support.V7.Widget.Toolbar");
			}
			else
				bar = new AToolbar(this);

			SetSupportActionBar(bar);

			_layout = new ARelativeLayout(BaseContext);
			SetContentView(_layout);

			Xamarin.Forms.Application.ClearCurrent();

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnCreate;

			OnStateChanged();

			if (Forms.IsLollipopOrNewer)
			{
				// Allow for the status bar color to be changed
				Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
			}

			if (Forms.IsLollipopOrNewer)
			{
				// Listen for the device going into power save mode so we can handle animations being disabled
				_powerSaveModeBroadcastReceiver = new PowerSaveModeBroadcastReceiver();
			}
		}

		protected override void OnDestroy()
		{
			if (_application != null)
				_application.PropertyChanged -= AppOnPropertyChanged;

			PopupManager.Unsubscribe(this);

			Platform?.Dispose();

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

			// Stop animations or other ongoing actions that could consume CPU
			// Commit unsaved changes, build only if users expect such changes to be permanently saved when thy leave such as a draft email
			// Release system resources, such as broadcast receivers, handles to sensors (like GPS), or any resources that may affect battery life when your activity is paused.
			// Avoid writing to permanent storage and CPU intensive tasks
			base.OnPause();

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnPause;

			if (Forms.IsLollipopOrNewer)
			{
				// Don't listen for power save mode changes while we're paused
				UnregisterReceiver(_powerSaveModeBroadcastReceiver);
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

			if (_application != null && CurrentFocus != null && _application.OnThisPlatform().GetShouldPreserveKeyboardOnResume())
			{
				CurrentFocus.ShowKeyboard();
			}

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnResume;

			if (Forms.IsLollipopOrNewer)
			{
				// Start listening for power save mode changes
				RegisterReceiver(_powerSaveModeBroadcastReceiver, new IntentFilter(
					PowerManager.ActionPowerSaveModeChanged
				));
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
				return;
			}

			if (args.PropertyName == nameof(_application.MainPage))
				InternalSetPage(_application.MainPage);
			if (args.PropertyName == PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName)
				SetSoftInputMode();
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
			else if (_previousState == AndroidApplicationLifecycleState.OnStop && _currentState == AndroidApplicationLifecycleState.OnRestart)
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

		void SetSoftInputMode()
		{
			var adjust = SoftInput.AdjustPan;

			if (Xamarin.Forms.Application.Current != null)
			{
				WindowSoftInputModeAdjust elementValue = Xamarin.Forms.Application.Current.OnThisPlatform().GetWindowSoftInputModeAdjust();
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
