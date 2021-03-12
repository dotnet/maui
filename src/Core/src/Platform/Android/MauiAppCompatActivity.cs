using System;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui
{
	public class MauiAppCompatActivity : AppCompatActivity
	{
		App? _app;

		AndroidApplicationLifecycleState _currentState;
		AndroidApplicationLifecycleState _previousState;

		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			if (App.Current == null)
				throw new InvalidOperationException($"App is not {nameof(App)}");

			_app = App.Current;

			if (_app.Services == null)
				throw new InvalidOperationException("App was not initialized");

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnCreate;

			var mauiContext = new MauiContext(_app.Services, this);
			var state = new ActivationState(mauiContext, savedInstanceState);
			var window = _app.CreateWindow(state);

			window.MauiContext = mauiContext;

			//Hack for now we set this on the App Static but this should be on IFrameworkElement
			App.Current.SetHandlerContext(window.MauiContext);

			var content = (window.Page as IView) ??
				window.Page.View;

			CoordinatorLayout parent = new CoordinatorLayout(this);

			SetContentView(parent, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

			//AddToolbar(parent);

			parent.AddView(content.ToNative(window.MauiContext), new CoordinatorLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
		}

		protected override void OnStart()
		{
			base.OnStart();

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnStart;

			UpdateApplicationLifecycleState();
		}

		protected override void OnPause()
		{
			base.OnPause();

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnPause;

			UpdateApplicationLifecycleState();
		}

		protected override void OnResume()
		{
			base.OnResume();

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnResume;

			UpdateApplicationLifecycleState();
		}

		protected override void OnRestart()
		{
			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnRestart;

			UpdateApplicationLifecycleState();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			_previousState = _currentState;
			_currentState = AndroidApplicationLifecycleState.OnDestroy;

			UpdateApplicationLifecycleState();
		}

		void AddToolbar(ViewGroup parent)
		{
			Toolbar toolbar = new Toolbar(this);
			var appbarLayout = new AppBarLayout(this);

			appbarLayout.AddView(toolbar, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, Android.Resource.Attribute.ActionBarSize));
			SetSupportActionBar(toolbar);
			parent.AddView(appbarLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
		}

		void UpdateApplicationLifecycleState()
		{
			if (_previousState == AndroidApplicationLifecycleState.OnCreate && _currentState == AndroidApplicationLifecycleState.OnStart)
				_app?.OnCreated();
			else if (_previousState == AndroidApplicationLifecycleState.OnRestart && _currentState == AndroidApplicationLifecycleState.OnStart)
				_app?.OnResumed();
			else if (_previousState == AndroidApplicationLifecycleState.OnPause && _currentState == AndroidApplicationLifecycleState.OnStop)
				_app?.OnPaused();
			else if (_currentState == AndroidApplicationLifecycleState.OnDestroy)
				_app?.OnStopped();
		}
	}
}
