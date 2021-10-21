using System;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;
using AColorRes = Android.Resource.Color;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.Controls.Platform
{
	internal class ShellFragmentContainer : Fragment
	{
		Page _page;
		readonly IMauiContext _mauiContext;
		AView _view;

		public ShellContent ShellContentTab { get; private set; }

		public ShellFragmentContainer(ShellContent shellContent, IMauiContext mauiContext)
		{
			ShellContentTab = shellContent;
			_mauiContext = mauiContext;
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			_page = ((IShellContentController)ShellContentTab).GetOrCreateContent();
			_view = _page.ToNative(_mauiContext);
			return new ShellPageContainer(RequireContext(), (INativeViewHandler)_page.Handler, true)
			{
				LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
			};
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();			
			((IShellContentController)ShellContentTab).RecyclePage(_page);
			_page = null;
			_view?.RemoveFromParent();
			_view = null;
		}

		public override void OnDestroy()
		{
			Device.BeginInvokeOnMainThread(Dispose);

			base.OnDestroy();
		}
	}
}