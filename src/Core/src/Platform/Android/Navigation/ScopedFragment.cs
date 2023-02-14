using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.Platform
{
	class ScopedFragment : Fragment
	{
		readonly IMauiContext _mauiContext;

		public IView DetailView { get; private set; }

		public ScopedFragment(IView detailView, IMauiContext mauiContext)
		{
			DetailView = detailView;
			_mauiContext = mauiContext;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
		{
			return DetailView.ToPlatform(_mauiContext, RequireContext(), inflater, ChildFragmentManager);
		}
	}
}
