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
		readonly IMauiContext _mauiContext;

		public ShellContent ShellContentTab { get; private set; }

		public ShellFragmentContainer(ShellContent shellContent, IMauiContext mauiContext)
		{
			ShellContentTab = shellContent;
			_mauiContext = mauiContext;
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			_page = ((IShellContentController)ShellContentTab).GetOrCreateContent();

			var pageMauiContext = _mauiContext.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager);

			return new ShellPageContainer(RequireContext(), (IPlatformViewHandler)_page.ToHandler(pageMauiContext), true)
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