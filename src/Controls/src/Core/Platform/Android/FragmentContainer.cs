using System;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using Microsoft.Maui.Controls.Platform;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal class FragmentContainer : Fragment
	{
		AView? _pageContainer;
		readonly IMauiContext _mauiContext;
		Action<AView>? _onCreateCallback;
		ViewGroup? _parent;
		AdapterItemKey _adapterItemKey;

		public FragmentContainer(AdapterItemKey adapterItemKey, IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
			_adapterItemKey = adapterItemKey;
		}

		public Page Page => _adapterItemKey.Page;

		public static FragmentContainer CreateInstance(AdapterItemKey adapterItemKey, IMauiContext mauiContext)
		{
			return new FragmentContainer(adapterItemKey, mauiContext) { Arguments = new Bundle() };
		}

		public void SetOnCreateCallback(Action<AView> callback)
		{
			_onCreateCallback = callback;
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
		{
			_parent = container ?? _parent;

			_pageContainer = Page.ToPlatform(_mauiContext, RequireContext(), inflater, ChildFragmentManager);
			_adapterItemKey.SetToStableView();
			_parent = _parent ?? (_pageContainer.Parent as ViewGroup);
			_onCreateCallback?.Invoke(_pageContainer);

			return _pageContainer;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			if (Context.IsDestroyed())
				Page?.Handler?.DisconnectHandler();
		}

		public override void OnResume()
		{
			if (_pageContainer == null)
				return;

			_parent = (_pageContainer.Parent as ViewGroup) ?? _parent;
			if (_pageContainer.Parent == null && _parent != null)
			{
				// Re-add the view to the container if Android removed it
				// Because we are re-using views inside OnCreateView Android
				// will remove the "previous" view from the parent but since our
				// "previous" view and "current" view are the same we have to re-add it
				_parent.AddView(_pageContainer);
			}

			base.OnResume();
		}
	}
}