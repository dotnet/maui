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
using System.Threading.Tasks;

#endregion

namespace Xamarin.Forms.Platform.Android
{
	public class FormsAppCompatActivity : AppCompatActivity, IDeviceInfoProvider
	{
		public delegate bool BackButtonPressedEventHandler(object sender, EventArgs e);

		Application _application;

		AndroidApplicationLifecycleState _currentState;
		ARelativeLayout _layout;

		AppCompat.Platform _platform;

		AndroidApplicationLifecycleState _previousState;

		bool _renderersAdded;
		PowerSaveModeBroadcastReceiver _powerSaveModeBroadcastReceiver;

		// Override this if you want to handle the default Android behavior of restoring fragments on an application restart
		protected virtual bool AllowFragmentRestore => false;

		protected FormsAppCompatActivity()
		{
			_previousState = AndroidApplicationLifecycleState.Uninitialized;
			_currentState = AndroidApplicationLifecycleState.Uninitialized;
			PopupManager.Subscribe(this);
		}

		IApplicationController Controller => _application;

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

		protected void LoadApplication(Application application)
		{
			if (!_renderersAdded)
			{
				RegisterHandlerForDefaultRenderer(typeof(NavigationPage), typeof(NavigationPageRenderer), typeof(NavigationRenderer));
				RegisterHandlerForDefaultRenderer(typeof(TabbedPage), typeof(TabbedPageRenderer), typeof(TabbedRenderer));
				RegisterHandlerForDefaultRenderer(typeof(MasterDetailPage), typeof(MasterDetailPageRenderer), typeof(MasterDetailRenderer));
				RegisterHandlerForDefaultRenderer(typeof(Switch), typeof(AppCompat.SwitchRenderer), typeof(SwitchRenderer));
				RegisterHandlerForDefaultRenderer(typeof(Picker), typeof(AppCompat.PickerRenderer), typeof(PickerRenderer));
				RegisterHandlerForDefaultRenderer(typeof(CarouselPage), typeof(AppCompat.CarouselPageRenderer), typeof(CarouselPageRenderer));

				if (Forms.Flags.Contains(Flags.FastRenderersExperimental))
				{
					RegisterHandlerForDefaultRenderer(typeof(Button), typeof(FastRenderers.ButtonRenderer), typeof(ButtonRenderer));
					RegisterHandlerForDefaultRenderer(typeof(Label), typeof(FastRenderers.LabelRenderer), typeof(LabelRenderer));
					RegisterHandlerForDefaultRenderer(typeof(Image), typeof(FastRenderers.ImageRenderer), typeof(ImageRenderer));
					RegisterHandlerForDefaultRenderer(typeof(Frame), typeof(FastRenderers.FrameRenderer), typeof(FrameRenderer));
				}
				else
				{
					RegisterHandlerForDefaultRenderer(typeof(Button), typeof(AppCompat.ButtonRenderer), typeof(ButtonRenderer));
					RegisterHandlerForDefaultRenderer(typeof(Frame), typeof(AppCompat.FrameRenderer), typeof(FrameRenderer));
				}

				_renderersAdded = true;
			}

			_application = application ?? throw new ArgumentNullException(nameof(application));
			(application as IApplicationController)?.SetAppIndexingProvider(new AndroidAppIndexProvider(this));
			Xamarin.Forms.Application.SetCurrentApplication(application);

			if (Xamarin.Forms.Application.Current.OnThisPlatform().GetWindowSoftInputModeAdjust() != WindowSoftInputModeAdjust.Unspecified)
				SetSoftInputMode();

			CheckForAppLink(Intent);

			application.PropertyChanged += AppOnPropertyChanged;

			if (application?.MainPage != null)
			{
				var iver = Platform.GetRenderer(application.MainPage);
				if (iver != null)
				{
					iver.Dispose();
					application.MainPage.ClearValue(Platform.RendererProperty);
				}
			}

			SetMainPage();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			ActivityResultCallbackRegistry.InvokeCallback(requestCode, resultCode, data);
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
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
			PopupManager.Unsubscribe(this);
			_platform?.Dispose();

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

			if (_application != null && _application.OnThisPlatform().GetShouldPreserveKeyboardOnResume())
			{
				if (CurrentFocus != null && (CurrentFocus is EditText || CurrentFocus is TextView || CurrentFocus is SearchView))
				{
					CurrentFocus.ShowKeyboard();
				}
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
			if (args.PropertyName == "MainPage")
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

			if (_platform != null)
			{
				_platform.SetPage(page);
				return;
			}

			PopupManager.ResetBusyCount(this);

			_platform = new AppCompat.Platform(this);
			if (_application != null)
				_application.Platform = _platform;
			_platform.SetPage(page);
			_layout.AddView(_platform);
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

		void RegisterHandlerForDefaultRenderer(Type target, Type handler, Type filter)
		{
			Type current = Registrar.Registered.GetHandlerType(target);
			if (current != filter)
				return;

			Registrar.Registered.Register(target, handler);
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
