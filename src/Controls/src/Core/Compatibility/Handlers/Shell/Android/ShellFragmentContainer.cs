using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	internal class ShellFragmentContainer : Fragment
	{
		Page _page;
		IMauiContext _mauiContext;

		public ShellContent ShellContentTab { get; private set; }

		public ShellFragmentContainer(ShellContent shellContent, IMauiContext mauiContext)
		{
			ShellContentTab = shellContent;
			_mauiContext = mauiContext;
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			_page = ((IShellContentController)ShellContentTab).GetOrCreateContent();

			IMauiContext mauiContext = null;

			// If the page has already been created with a handler then we just let it retain the same
			// Handler and MauiContext
			// But we want to update the inflater and ChildFragmentManager to match
			// the handlers new home			
			if (_page?.Handler?.MauiContext is MauiContext scopedMauiContext)
			{
				scopedMauiContext.AddWeakSpecific(ChildFragmentManager);
				scopedMauiContext.AddWeakSpecific(inflater);
				mauiContext = scopedMauiContext;
			}

			mauiContext ??= _mauiContext.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager);

			return new ShellPageContainer(RequireContext(), (IPlatformViewHandler)_page.ToHandler(mauiContext), true)
			{
				LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
			};
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			((IShellContentController)ShellContentTab).RecyclePage(_page);
			_page = null;
		}

		public override void OnDestroy()
		{
			_mauiContext
				.GetDispatcher()
				.Dispatch(Dispose);

			base.OnDestroy();
		}
	}
}