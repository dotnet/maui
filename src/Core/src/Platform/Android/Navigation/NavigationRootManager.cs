using System;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.AppBar;
using Google.Android.Material.Tabs;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public class NavigationRootManager
	{
		AppBarLayout? _appBar;
		Toolbar? _toolbar;
		NavigationLayout? _navigationLayout;
		IScopedMauiContext _scopedMauiContext;

		internal NavigationLayout NavigationLayout => _navigationLayout ??=
			LayoutInflater
				.Inflate(Resource.Layout.navigationlayout, null)
				.JavaCast<NavigationLayout>()
				?? throw new InvalidOperationException($"Resource.Layout.navigationlayout missing");

		internal Toolbar Toolbar =>
			_toolbar ??= NavigationLayout.FindViewById<MaterialToolbar>(Resource.Id.navigationlayout_toolbar)
			?? throw new InvalidOperationException($"Resource.Id.navigationlayout_toolbar missing");

		internal AppBarLayout AppBar =>
			_appBar ??= NavigationLayout.FindViewById<AppBarLayout>(Resource.Id.navigationlayout_appbar)
			?? throw new InvalidOperationException($"Resource.Id.navigationlayout_appbar missing");

		LayoutInflater LayoutInflater => _scopedMauiContext.GetLayoutInflater();

		FragmentManager FragmentManager => _scopedMauiContext.GetFragmentManager();

		public AView RootView => NavigationLayout;

		public NavigationRootManager(IMauiContext mauiContext)
		{
			_scopedMauiContext = (IScopedMauiContext)mauiContext;
		}

		// TODO MAUI: replace this with something else
		internal void SetContentView(AView content)
		{
			FragmentManager.BeginTransaction()
				.Replace(Resource.Id.navigationlayout_content, new FragmentView(content))
				.SetReorderingAllowed(true)
				.Commit();
		}

		internal class FragmentView : Fragment
		{
			readonly AView _aView;

			public FragmentView(AView aView)
			{
				_aView = aView;
				_aView.RemoveFromParent();
			}

			public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
			{
				_aView.RemoveFromParent();
				return _aView;
			}

			public override void OnViewCreated(AView view, Bundle savedInstanceState)
			{
				_aView.RemoveFromParent();
				base.OnViewCreated(view, savedInstanceState);
			}

			public override Animation OnCreateAnimation(int transit, bool enter, int nextAnim)
			{
				return base.OnCreateAnimation(transit, enter, nextAnim);
			}

			public override void OnDestroy()
			{
				base.OnDestroy();
			}

			public override void OnDestroyView()
			{
				base.OnDestroyView();
			}
		}
	}
}
