#nullable enable
using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity;
using AndroidX.Activity.Result;
using AndroidX.Browser.Auth;
using AndroidX.Browser.CustomTabs;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Authentication
{
	[Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, Exported = false)]
	class WebAuthenticatorIntermediateActivity : ComponentActivity
	{
		const string ModeExtra = "maui.webauthenticator.mode";
		const string RequestIdExtra = "maui.webauthenticator.request_id";
		const string UrlExtra = "maui.webauthenticator.url";
		const string CallbackUrlExtra = "maui.webauthenticator.callback_url";
		const string ProviderExtra = "maui.webauthenticator.provider";
		const string EphemeralExtra = "maui.webauthenticator.ephemeral";
		const string FallbackAvailableExtra = "maui.webauthenticator.fallback_available";
		const string LaunchedExtra = "maui.webauthenticator.launched";

		const int ModeAuthTab = 1;
		const int ModeCustomTab = 2;
		const int ModeCallback = 3;
		const int ModeCleanup = 4;
		const int ModeBrowser = 5;

		static readonly object liveOwnerLock = new();
		static WeakReference<WebAuthenticatorIntermediateActivity>? liveOwner;

		readonly ActivityResultCallback<AuthTabIntent.AuthResult> authTabResultCallback;
		readonly ActivityResultLauncher? authTabLauncher;

		long requestId;
		int mode;
		bool launched;
		bool prefersEphemeral;
		bool fallbackAvailable;
		string? url;
		string? callbackUrl;
		string? provider;

		public WebAuthenticatorIntermediateActivity()
		{
			authTabResultCallback = new ActivityResultCallback<AuthTabIntent.AuthResult>(OnAuthTabResult);
			authTabLauncher = AuthTabIntent.RegisterActivityResultLauncher(this, authTabResultCallback);
		}

		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			var extras = savedInstanceState ?? Intent?.Extras;
			if (extras is null)
			{
				Finish();
				return;
			}

			mode = extras.GetInt(ModeExtra);
			if (mode == ModeCallback)
			{
				Finish();
				RouteCallback(Intent);
				return;
			}

			requestId = extras.GetLong(RequestIdExtra);
			if (requestId <= 0 || !WebAuthenticatorRequestManager.IsActive(requestId))
			{
				Finish();
				return;
			}

			if (mode == ModeCleanup)
			{
				Finish();
				return;
			}

			launched = extras.GetBoolean(LaunchedExtra);
			prefersEphemeral = extras.GetBoolean(EphemeralExtra);
			fallbackAvailable = extras.GetBoolean(FallbackAvailableExtra);
			url = extras.GetString(UrlExtra);
			callbackUrl = extras.GetString(CallbackUrlExtra);
			provider = extras.GetString(ProviderExtra);

			if ((mode != ModeAuthTab && mode != ModeCustomTab && mode != ModeBrowser) ||
				string.IsNullOrEmpty(url) ||
				(mode != ModeBrowser && string.IsNullOrEmpty(provider)) ||
				(mode == ModeAuthTab && string.IsNullOrEmpty(callbackUrl)))
			{
				Finish();
				WebAuthenticatorRequestManager.TryFail(
					requestId,
					new InvalidOperationException("The Android WebAuthenticator launch state was incomplete."));
				return;
			}

			RegisterLiveOwner(this);
		}

		protected override void OnResume()
		{
			base.OnResume();

			if (IsFinishing || requestId <= 0 || !WebAuthenticatorRequestManager.IsActive(requestId))
			{
				FinishAndReleaseOwner();
				return;
			}

			if (!launched)
			{
				launched = true;
				if (mode == ModeAuthTab)
					LaunchAuthTabOrFallback();
				else if (mode == ModeCustomTab)
					LaunchCustomTabOrBrowser();
				else if (mode == ModeBrowser)
					LaunchBrowser();
				return;
			}

			if (mode == ModeCustomTab || mode == ModeBrowser)
			{
				FinishAndReleaseOwner();
				WebAuthenticatorRequestManager.TryCancelFromPlatform(requestId);
			}
		}

		protected override void OnNewIntent(Intent? intent)
		{
			base.OnNewIntent(intent);
			if (intent is null)
				return;

			Intent = intent;
			var incomingMode = intent.GetIntExtra(ModeExtra, 0);
			if (incomingMode == ModeCallback)
			{
				FinishAndReleaseOwner();
				RouteCallback(intent);
			}
			else if (incomingMode == ModeCleanup &&
				intent.GetLongExtra(RequestIdExtra, 0) == requestId)
			{
				FinishAndReleaseOwner();
			}
		}

		protected override void OnDestroy()
		{
			ReleaseLiveOwner(this);
			base.OnDestroy();
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			outState.PutInt(ModeExtra, mode);
			outState.PutLong(RequestIdExtra, requestId);
			outState.PutBoolean(LaunchedExtra, launched);
			outState.PutBoolean(EphemeralExtra, prefersEphemeral);
			outState.PutBoolean(FallbackAvailableExtra, fallbackAvailable);
			outState.PutString(UrlExtra, url);
			outState.PutString(CallbackUrlExtra, callbackUrl);
			outState.PutString(ProviderExtra, provider);

			base.OnSaveInstanceState(outState);
		}

		void LaunchAuthTabOrFallback()
		{
			var selectedProvider = provider;
			if (!WebAuthenticatorImplementation.IsAuthTabSupported(this, selectedProvider))
			{
				var replacementProvider = WebAuthenticatorImplementation.TryGetCustomTabsProvider(this);
				if (WebAuthenticatorImplementation.IsAuthTabSupported(this, replacementProvider))
				{
					selectedProvider = replacementProvider;
					provider = replacementProvider;
				}
				else
				{
					if (!fallbackAvailable)
					{
						FinishAndReleaseOwner();
						WebAuthenticatorRequestManager.TryFail(
							requestId,
							new InvalidOperationException("The selected Android Auth Tab provider is no longer available."));
						return;
					}

					provider = replacementProvider;
					if (string.IsNullOrEmpty(provider))
					{
						mode = ModeBrowser;
						LaunchBrowser();
					}
					else
					{
						mode = ModeCustomTab;
						LaunchCustomTabOrBrowser();
					}
					return;
				}
			}

			try
			{
				using var builder = new AuthTabIntent.Builder();
				if (prefersEphemeral && WebAuthenticatorImplementation.IsEphemeralBrowsingSupported(this, selectedProvider))
					builder.SetEphemeralBrowsingEnabled(true);

				if (authTabLauncher is null)
					throw new InvalidOperationException();

				using var authTabIntent = builder.Build() ?? throw new InvalidOperationException();
				var launchIntent = authTabIntent.Intent ?? throw new InvalidOperationException();
				var packageManager = PackageManager ?? throw new InvalidOperationException();
				launchIntent.SetPackage(selectedProvider!);
				var nativeUrl = global::Android.Net.Uri.Parse(url!);
				var expectedCallback = new Uri(callbackUrl!);

				if (nativeUrl is null || launchIntent.ResolveActivity(packageManager) is null)
					throw new ActivityNotFoundException();

				if (expectedCallback.Scheme == "https")
				{
					authTabIntent.Launch(
						authTabLauncher,
						nativeUrl,
						expectedCallback.Host,
						expectedCallback.AbsolutePath);
				}
				else
				{
					authTabIntent.Launch(authTabLauncher, nativeUrl, expectedCallback.Scheme);
				}
			}
			catch (Exception)
			{
				if (fallbackAvailable)
				{
					mode = ModeCustomTab;
					LaunchCustomTabOrBrowser();
				}
				else
				{
					FinishAndReleaseOwner();
					WebAuthenticatorRequestManager.TryFail(
						requestId,
						new InvalidOperationException("The selected Android Auth Tab provider could not be launched."));
				}
			}
		}

		void LaunchCustomTabOrBrowser()
		{
			if (string.IsNullOrEmpty(provider))
			{
				mode = ModeBrowser;
				LaunchBrowser();
				return;
			}

			try
			{
				using var builder = new CustomTabsIntent.Builder();
				builder.SetShowTitle(true);
				if (prefersEphemeral && WebAuthenticatorImplementation.IsEphemeralBrowsingSupported(this, provider))
					builder.SetEphemeralBrowsingEnabled(true);

				using var customTabsIntent = builder.Build() ?? throw new InvalidOperationException();
				var launchIntent = customTabsIntent.Intent ?? throw new InvalidOperationException();
				var packageManager = PackageManager ?? throw new InvalidOperationException();
				launchIntent.SetPackage(provider);
				launchIntent.SetData(global::Android.Net.Uri.Parse(url!));

				if (launchIntent.ResolveActivity(packageManager) is null)
				{
					mode = ModeBrowser;
					LaunchBrowser();
					return;
				}

				StartActivity(launchIntent);
			}
			catch (Exception)
			{
				mode = ModeBrowser;
				LaunchBrowser();
			}
		}

		void LaunchBrowser()
		{
			try
			{
				WebAuthenticatorImplementation.LaunchSystemBrowser(this, new Uri(url!));
			}
			catch (Exception)
			{
				FinishAndReleaseOwner();
				WebAuthenticatorRequestManager.TryFail(
					requestId,
					new InvalidOperationException("No browser could be opened for web authentication."));
			}
		}

		void OnAuthTabResult(AuthTabIntent.AuthResult? result)
		{
			FinishAndReleaseOwner();
			HandleAuthTabResult(requestId, result?.ResultCode, result?.ResultUri?.ToString());
		}

		void FinishAndReleaseOwner()
		{
			ReleaseLiveOwner(this);
			Finish();
		}

		internal static void HandleAuthTabResult(long requestId, int? resultCode, string? callbackUri)
		{
			if (requestId <= 0 || resultCode is null)
			{
				if (requestId > 0)
				{
					WebAuthenticatorRequestManager.TryFail(
						requestId,
						new InvalidOperationException("Android Auth Tab did not return a result."));
				}
				return;
			}

			if (resultCode == AuthTabIntent.ResultOk)
			{
				if (string.IsNullOrEmpty(callbackUri))
				{
					WebAuthenticatorRequestManager.TryHandleTerminalCallback(requestId, null);
					return;
				}

				try
				{
					WebAuthenticatorRequestManager.TryHandleTerminalCallback(requestId, new Uri(callbackUri));
				}
				catch (Exception)
				{
					WebAuthenticatorRequestManager.TryFail(
						requestId,
						new InvalidOperationException("Android Auth Tab returned an invalid callback URI."));
				}
			}
			else if (resultCode == AuthTabIntent.ResultCanceled)
			{
				WebAuthenticatorRequestManager.TryCancelFromPlatform(requestId);
			}
			else if (resultCode == AuthTabIntent.ResultVerificationFailed)
			{
				WebAuthenticatorRequestManager.TryFail(
					requestId,
					new InvalidOperationException("Android Auth Tab could not verify the HTTPS callback ownership."));
			}
			else if (resultCode == AuthTabIntent.ResultVerificationTimedOut)
			{
				WebAuthenticatorRequestManager.TryFail(
					requestId,
					new InvalidOperationException("Android Auth Tab timed out while verifying the HTTPS callback ownership."));
			}
			else
			{
				WebAuthenticatorRequestManager.TryFail(
					requestId,
					new InvalidOperationException("Android Auth Tab returned an unknown result."));
			}
		}

		static void RouteCallback(Intent? intent)
		{
			if (intent is null)
				return;

			try
			{
				WebAuthenticator.Default.OnResume(intent);
			}
			catch (Exception)
			{
				// Lifecycle routing must never crash the callback activity.
			}
		}

		internal static void StartAuthTab(
			Activity activity,
			long requestId,
			Uri url,
			Uri callbackUrl,
			string provider,
			bool prefersEphemeral,
			bool fallbackAvailable)
		{
			var intent = CreateLaunchIntent(activity, ModeAuthTab, requestId, url, provider, prefersEphemeral);
			intent.PutExtra(CallbackUrlExtra, callbackUrl.OriginalString);
			intent.PutExtra(FallbackAvailableExtra, fallbackAvailable);
			activity.StartActivity(intent);
		}

		internal static void StartCustomTab(
			Activity activity,
			long requestId,
			Uri url,
			string provider,
			bool prefersEphemeral)
		{
			activity.StartActivity(CreateLaunchIntent(activity, ModeCustomTab, requestId, url, provider, prefersEphemeral));
		}

		internal static void StartBrowser(Activity activity, long requestId, Uri url) =>
			activity.StartActivity(CreateLaunchIntent(activity, ModeBrowser, requestId, url, null, false));

		internal static void StartCallback(Context context, global::Android.Net.Uri? callbackUri)
		{
			var intent = new Intent(context, typeof(WebAuthenticatorIntermediateActivity));
			intent.PutExtra(ModeExtra, ModeCallback);
			intent.SetData(callbackUri);
			intent.AddFlags(global::Android.Content.ActivityFlags.ClearTop | global::Android.Content.ActivityFlags.SingleTop);
			if (context is not Activity)
				intent.AddFlags(global::Android.Content.ActivityFlags.NewTask);
			context.StartActivity(intent);
		}

		internal static bool TryRequestCleanup(long requestId)
		{
			if (!TryGetLiveOwner(requestId, out var activity))
				return false;

			var intent = new Intent(activity, typeof(WebAuthenticatorIntermediateActivity));
			intent.PutExtra(ModeExtra, ModeCleanup);
			intent.PutExtra(RequestIdExtra, requestId);
			intent.AddFlags(global::Android.Content.ActivityFlags.ClearTop | global::Android.Content.ActivityFlags.SingleTop);
			activity.StartActivity(intent);
			return true;
		}

		static Intent CreateLaunchIntent(
			Activity activity,
			int mode,
			long requestId,
			Uri url,
			string? provider,
			bool prefersEphemeral)
		{
			var intent = new Intent(activity, typeof(WebAuthenticatorIntermediateActivity));
			intent.PutExtra(ModeExtra, mode);
			intent.PutExtra(RequestIdExtra, requestId);
			intent.PutExtra(UrlExtra, url.OriginalString);
			if (!string.IsNullOrEmpty(provider))
				intent.PutExtra(ProviderExtra, provider);
			intent.PutExtra(EphemeralExtra, prefersEphemeral);
			return intent;
		}

		static void RegisterLiveOwner(WebAuthenticatorIntermediateActivity activity)
		{
			lock (liveOwnerLock)
				liveOwner = new WeakReference<WebAuthenticatorIntermediateActivity>(activity);
		}

		static bool TryGetLiveOwner(long requestId, out WebAuthenticatorIntermediateActivity activity)
		{
			lock (liveOwnerLock)
			{
				if (liveOwner is null || !liveOwner.TryGetTarget(out var currentActivity))
				{
					liveOwner = null;
					activity = null!;
					return false;
				}

				if (currentActivity.requestId == requestId &&
					!currentActivity.IsFinishing &&
					!currentActivity.IsDestroyed)
				{
					activity = currentActivity;
					return true;
				}
			}

			activity = null!;
			return false;
		}

		static void ReleaseLiveOwner(WebAuthenticatorIntermediateActivity activity)
		{
			lock (liveOwnerLock)
			{
				if (liveOwner is null)
					return;

				if (liveOwner.TryGetTarget(out var currentActivity) && !ReferenceEquals(currentActivity, activity))
					return;

				liveOwner = null;
			}
		}
	}
}
