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
using Xamarin.Forms.Platform.Android;
using View = Android.Views.View;
using Button = Android.Widget.Button;

namespace Embedding.Droid
{
	[Activity(Label = "SecondActivity")]
	public class SecondActivity : FragmentActivity
	{
		Fragment _hello;
		Fragment _alertsAndActionSheets;
		Fragment _openUri;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView (Resource.Layout.Second);
			
			var ft = SupportFragmentManager.BeginTransaction();
			ft.Replace(Resource.Id.fragment_frame_layout, new SecondFragment(), "main");
			ft.Commit();
		}

		void ShowEmbeddedPageFragment(Fragment fragment)
		{
			FragmentTransaction ft = SupportFragmentManager.BeginTransaction();

			ft.AddToBackStack(null);
			ft.Replace(Resource.Id.fragment_frame_layout, fragment, "hello");
			
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

		public void ShowAlertsAndActionSheets()
		{
			if (_alertsAndActionSheets== null)
			{
				_alertsAndActionSheets = new AlertsAndActionSheets().CreateSupportFragment(this);
			}

			ShowEmbeddedPageFragment(_alertsAndActionSheets);
		}

		public void ShowOpenUri()
		{
			if (_openUri == null)
			{
				_openUri = new OpenUri().CreateSupportFragment(this);
			}

			ShowEmbeddedPageFragment(_openUri );
		}
	}

	public class SecondFragment : Fragment
	{
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var view =  inflater.Inflate(Resource.Layout.SecondFragment, container, false);
			var showEmbeddedButton = view.FindViewById<Button>(Resource.Id.showEmbeddedButton);
			var showAlertsActionSheets = view.FindViewById<Button>(Resource.Id.showAlertsActionSheets);
			var showOpenUri = view.FindViewById<Button>(Resource.Id.showOpenUri);

			showEmbeddedButton.Click += ShowEmbeddedClick;
			showAlertsActionSheets.Click += ShowAlertsActionSheetsClick;
			showOpenUri.Click += ShowOpenUriClick;

			return view;
		}

		void ShowAlertsActionSheetsClick(object sender, EventArgs eventArgs)
		{
			((SecondActivity)Activity).ShowAlertsAndActionSheets();
		}

		void ShowEmbeddedClick(object sender, EventArgs e)
		{
			((SecondActivity)Activity).ShowHello();
		}

		void ShowOpenUriClick(object sender, EventArgs e)
		{
			((SecondActivity)Activity).ShowOpenUri();
		}
	}
}