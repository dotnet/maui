using System;
using Android.App;
using Android.OS;
#if __ANDROID_29__
using AndroidX.Fragment.App;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;
#else
using Android.Support.V4.App;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;
#endif
using Android.Views;
using Embedding.XF;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using View = Android.Views.View;
using Button = Android.Widget.Button;

namespace Embedding.Droid
{
	[Activity(Label = "Embedding.Droid", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : FragmentActivity
	{
		Fragment _hello;
		Fragment _alertsAndActionSheets;
		Fragment _webview;
		Fragment _openUri;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			Forms.Init(this, null);

			SetContentView (Resource.Layout.Main);
			
			var ft = SupportFragmentManager.BeginTransaction();
			ft.Replace(Resource.Id.fragment_frame_layout, new MainFragment(), "main");
			ft.Commit();
		}

		public void ShowHello()
		{
			if (_hello == null)
			{
				_hello = new Hello().CreateSupportFragment(this);
			}

			ShowEmbeddedPageFragment(_hello);
		}

		public void ShowWebView()
		{
			if (_webview == null)
			{
				_webview= new WebViewExample().CreateSupportFragment(this);
			}

			ShowEmbeddedPageFragment(_webview);
		}
		public void ShowOpenUri()
		{
			if (_openUri == null)
			{
				_openUri = new OpenUri().CreateSupportFragment(this);
			}

			ShowEmbeddedPageFragment(_openUri );
		}

		public void ShowAlertsAndActionSheets()
		{
			if (_alertsAndActionSheets== null)
			{
				_alertsAndActionSheets = new AlertsAndActionSheets().CreateSupportFragment(this);
			}

			ShowEmbeddedPageFragment(_alertsAndActionSheets);
		}

		void ShowEmbeddedPageFragment(Fragment fragment)
		{
			FragmentTransaction ft = SupportFragmentManager.BeginTransaction();

			ft.AddToBackStack(null);
			ft.Replace(Resource.Id.fragment_frame_layout, fragment, "hello");
			
			ft.Commit();
		}

		public void LaunchSecondActivity()
		{
			StartActivity(typeof(SecondActivity));
		}
	}

	public class MainFragment : Fragment
	{
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var view =  inflater.Inflate(Resource.Layout.MainFragment, container, false);
			var showEmbeddedButton = view.FindViewById<Button>(Resource.Id.showEmbeddedButton);
			var showAlertsActionSheets = view.FindViewById<Button>(Resource.Id.showAlertsActionSheets);
			var showWebView = view.FindViewById<Button>(Resource.Id.showWebView);
			var showOpenUri = view.FindViewById<Button>(Resource.Id.showOpenUri);
			var launchSecondActivity = view.FindViewById<Button>(Resource.Id.launchSecondActivity);

			showEmbeddedButton.Click += ShowEmbeddedClick;
			showAlertsActionSheets.Click += ShowAlertsActionSheetsClick;
			showWebView.Click += ShowWebViewOnClick;
			showOpenUri.Click += ShowOpenUriClick;
			launchSecondActivity.Click += LaunchSecondActivityOnClick;

			return view;
		}

		void ShowWebViewOnClick(object sender, EventArgs eventArgs)
		{
			((MainActivity)Activity).ShowWebView();
		}
		void LaunchSecondActivityOnClick(object sender, EventArgs e)
		{
			((MainActivity)Activity).LaunchSecondActivity();
		}

		void ShowAlertsActionSheetsClick(object sender, EventArgs eventArgs)
		{
			((MainActivity)Activity).ShowAlertsAndActionSheets();
		}

		void ShowEmbeddedClick(object sender, EventArgs e)
		{
			((MainActivity)Activity).ShowHello();
		}

		void ShowOpenUriClick(object sender, EventArgs e)
		{
			((MainActivity)Activity).ShowOpenUri();
		}
	}
}

