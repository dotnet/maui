#nullable disable
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	internal sealed class ShellFragmentContainer : Fragment
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
			_page.ToPlatform(_mauiContext, RequireContext(), inflater, ChildFragmentManager);

			return new ShellPageContainer(RequireContext(), (IPlatformViewHandler)_page.Handler, true)
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
	}
}