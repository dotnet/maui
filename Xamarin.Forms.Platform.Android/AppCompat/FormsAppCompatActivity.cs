#region

using System;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android.AppCompat;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using AToolbar = Android.Support.V7.Widget.Toolbar;
using AColor = Android.Graphics.Color;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using ARelativeLayout = Android.Widget.RelativeLayout;

#endregion

namespace Xamarin.Forms.Platform.Android
{
	public class FormsAppCompatActivity : AppCompatActivity, IDeviceInfoProvider, IStartActivityForResult
	{
		public delegate bool BackButtonPressedEventHandler(object sender, EventArgs e);

		readonly ConcurrentDictionary<int, Action<Result, Intent>> _activityResultCallbacks = new ConcurrentDictionary<int, Action<Result, Intent>>();

		Application _application;
		int _busyCount;
		AndroidApplicationLifecycleState _currentState;
		ARelativeLayout _layout;

		int _nextActivityResultCallbackKey;

		AppCompat.Platform _platform;

		AndroidApplicationLifecycleState _previousState;

		bool _renderersAdded;
		int _statusBarHeight = -1;
		global::Android.Views.View _statusBarUnderlay;

		// Override this if you want to handle the default Android behavior of restoring fragments on an application restart
		protected virtual bool AllowFragmentRestore => false;

		protected FormsAppCompatActivity()
		{
			_previousState = AndroidApplicationLifecycleState.Uninitialized;
			_currentState = AndroidApplicationLifecycleState.Uninitialized;
		}

		public event EventHandler ConfigurationChanged;

		int IStartActivityForResult.RegisterActivityResultCallback(Action<Result, Intent> callback)
		{
			int requestCode = _nextActivityResultCallbackKey;

			while (!_activityResultCallbacks.TryAdd(requestCode, callback))
			{
				_nextActivityResultCallbackKey += 1;
				requestCode = _nextActivityResultCallbackKey;
			}

			_nextActivityResultCallbackKey += 1;

			return requestCode;
		}

		void IStartActivityForResult.UnregisterActivityResultCallback(int requestCode)
		{
			Action<Result, Intent> callback;
			_activityResultCallbacks.TryRemove(requestCode, out callback);
		}

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
			_statusBarUnderlay.SetBackgroundColor(color);
		}

		protected void LoadApplication(Application application)
		{
			if (!_renderersAdded)
			{
				RegisterHandlerForDefaultRenderer(typeof(NavigationPage), typeof(NavigationPageRenderer), typeof(NavigationRenderer));
				RegisterHandlerForDefaultRenderer(typeof(TabbedPage), typeof(TabbedPageRenderer), typeof(TabbedRenderer));
				RegisterHandlerForDefaultRenderer(typeof(MasterDetailPage), typeof(MasterDetailPageRenderer), typeof(MasterDetailRenderer));
				RegisterHandlerForDefaultRenderer(typeof(Button), typeof(AppCompat.ButtonRenderer), typeof(ButtonRenderer));
				RegisterHandlerForDefaultRenderer(typeof(Switch), typeof(AppCompat.SwitchRenderer), typeof(SwitchRenderer));
				RegisterHandlerForDefaultRenderer(typeof(Picker), typeof(AppCompat.PickerRenderer), typeof(PickerRenderer));
				RegisterHandlerForDefaultRenderer(typeof(Frame), typeof(AppCompat.FrameRenderer), typeof(FrameRenderer));
				RegisterHandlerForDefaultRenderer(typeof(CarouselPage), typeof(AppCompat.CarouselPageRenderer), typeof(CarouselPageRenderer));

				_renderersAdded = true;
			}

			if (application == null)
				throw new ArgumentNullException("application");

			_application = application;
			(application as IApplicationController)?.SetAppIndexingProvider(new AndroidAppIndexProvider(this));
			Xamarin.Forms.Application.Current = application;

			SetSoftInputMode();

			CheckForAppLink(Intent);

			application.PropertyChanged += AppOnPropertyChanged;

			SetMainPage();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			Action<Result, Intent> callback;

			if (_activityResultCallbacks.TryGetValue(requestCode, out callback))
				callback(resultCode, data);
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

			AddStatusBarUnderlay();
		}

		protected override void OnDestroy()
		{
			// may never be called
			base.OnDestroy();

			MessagingCenter.Unsubscribe<Page, AlertArguments>(this, Page.AlertSignalName);
			MessagingCenter.Unsubscribe<Page, bool>(this, Page.BusySetSignalName);
			MessagingCenter.Unsubscribe<Page, ActionSheetArguments>(this, Page.ActionSheetSignalName);

			if (_platform != null)
				_platform.Dispose();
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

		internal int GetStatusBarHeight()
		{
			if (_statusBarHeight >= 0)
				return _statusBarHeight;

			var result = 0;
			int resourceId = Resources.GetIdentifier("status_bar_height", "dimen", "android");
			if (resourceId > 0)
				result = Resources.GetDimensionPixelSize(resourceId);
			return _statusBarHeight = result;
		}

		void AddStatusBarUnderlay()
		{
			_statusBarUnderlay = new global::Android.Views.View(this);

			var layoutParameters = new ARelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, GetStatusBarHeight()) { AlignWithParent = true };
			layoutParameters.AddRule(LayoutRules.AlignTop);
			_statusBarUnderlay.LayoutParameters = layoutParameters;
			_layout.AddView(_statusBarUnderlay);

			if (Forms.IsLollipopOrNewer)
			{
				Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
				Window.SetStatusBarColor(AColor.Transparent);

				int primaryColorDark = GetColorPrimaryDark();

				if (primaryColorDark != 0)
				{
					int r = AColor.GetRedComponent(primaryColorDark);
					int g = AColor.GetGreenComponent(primaryColorDark);
					int b = AColor.GetBlueComponent(primaryColorDark);
					int a = AColor.GetAlphaComponent(primaryColorDark);
					SetStatusBarColor(AColor.Argb(a, r, g, b));
				}
			}
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

		int GetColorPrimaryDark()
		{
			FormsAppCompatActivity context = this;
			int id = global::Android.Resource.Attribute.ColorPrimaryDark;
			using (var value = new TypedValue())
			{
				try
				{
					Resources.Theme theme = context.Theme;
					if (theme != null && theme.ResolveAttribute(id, value, true))
					{
						if (value.Type >= DataType.FirstInt && value.Type <= DataType.LastInt)
							return value.Data;
						if (value.Type == DataType.String)
							return ContextCompat.GetColor(context, value.ResourceId);
					}
				}
				catch (Exception ex)
				{
					Log.Warning("Xamarin.Forms.Platform.Android.FormsAppCompatActivity", "Error retrieving color resource: {0}", ex);
				}

				return -1;
			}
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

			_busyCount = 0;
			MessagingCenter.Subscribe<Page, bool>(this, Page.BusySetSignalName, OnPageBusy);
			MessagingCenter.Subscribe<Page, AlertArguments>(this, Page.AlertSignalName, OnAlertRequested);
			MessagingCenter.Subscribe<Page, ActionSheetArguments>(this, Page.ActionSheetSignalName, OnActionSheetRequested);

			_platform = new AppCompat.Platform(this);
			if (_application != null)
				_application.Platform = _platform;
			_platform.SetPage(page);
			_layout.AddView(_platform);
			_layout.BringToFront();
		}

		void OnActionSheetRequested(Page sender, ActionSheetArguments arguments)
		{
			var builder = new AlertDialog.Builder(this);
			builder.SetTitle(arguments.Title);
			string[] items = arguments.Buttons.ToArray();
			builder.SetItems(items, (o, args) => arguments.Result.TrySetResult(items[args.Which]));

			if (arguments.Cancel != null)
				builder.SetPositiveButton(arguments.Cancel, (o, args) => arguments.Result.TrySetResult(arguments.Cancel));

			if (arguments.Destruction != null)
				builder.SetNegativeButton(arguments.Destruction, (o, args) => arguments.Result.TrySetResult(arguments.Destruction));

			AlertDialog dialog = builder.Create();
			builder.Dispose();
			//to match current functionality of renderer we set cancelable on outside
			//and return null
			dialog.SetCanceledOnTouchOutside(true);
			dialog.CancelEvent += (o, e) => arguments.SetResult(null);
			dialog.Show();
		}

		void OnAlertRequested(Page sender, AlertArguments arguments)
		{
			AlertDialog alert = new AlertDialog.Builder(this).Create();
			alert.SetTitle(arguments.Title);
			alert.SetMessage(arguments.Message);
			if (arguments.Accept != null)
				alert.SetButton((int)DialogButtonType.Positive, arguments.Accept, (o, args) => arguments.SetResult(true));
			alert.SetButton((int)DialogButtonType.Negative, arguments.Cancel, (o, args) => arguments.SetResult(false));
			alert.CancelEvent += (o, args) => { arguments.SetResult(false); };
			alert.Show();
		}

		void OnPageBusy(Page sender, bool enabled)
		{
			_busyCount = Math.Max(0, enabled ? _busyCount + 1 : _busyCount - 1);

			UpdateProgressBarVisibility(_busyCount > 0);
		}

		async void OnStateChanged()
		{
			if (_application == null)
				return;

			if (_previousState == AndroidApplicationLifecycleState.OnCreate && _currentState == AndroidApplicationLifecycleState.OnStart)
				_application.SendStart();
			else if (_previousState == AndroidApplicationLifecycleState.OnStop && _currentState == AndroidApplicationLifecycleState.OnRestart)
				_application.SendResume();
			else if (_previousState == AndroidApplicationLifecycleState.OnPause && _currentState == AndroidApplicationLifecycleState.OnStop)
				await _application.SendSleepAsync();
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
			SetStatusBarVisibility(adjust);
		}

		void SetStatusBarVisibility(SoftInput mode)
		{
			if (!Forms.IsLollipopOrNewer)
				return;

			if (mode == SoftInput.AdjustResize)
			{
				Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.Immersive);
			}
			else
				Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.LayoutFullscreen | SystemUiFlags.LayoutStable);

			_layout?.Invalidate();
		}

		void UpdateProgressBarVisibility(bool isBusy)
		{
			if (!Forms.SupportsProgress)
				return;
#pragma warning disable 612, 618
			SetProgressBarIndeterminate(true);
			SetProgressBarIndeterminateVisibility(isBusy);
#pragma warning restore 612, 618
		}

		internal class DefaultApplication : Application
		{
		}

		#region Statics

		public static event BackButtonPressedEventHandler BackPressed;

		public static int TabLayoutResource { get; set; }

		public static int ToolbarResource { get; set; }

		internal static int GetUniqueId()
		{
			// getting unique Id's is an art, and I consider myself the Jackson Pollock of the field
			if ((int)Build.VERSION.SdkInt >= 17)
				return global::Android.Views.View.GenerateViewId();

			// Numbers higher than this range reserved for xml
			// If we roll over, it can be exceptionally problematic for the user if they are still retaining things, android's internal implementation is
			// basically identical to this except they do a lot of locking we don't have to because we know we only do this
			// from the UI thread
			if (s_id >= 0x00ffffff)
				s_id = 0x00000400;
			return s_id++;
		}

		static int s_id = 0x00000400;

		#endregion
	}
}