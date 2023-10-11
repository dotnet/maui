using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.Platform
{
	class ScopedFragment : Fragment
	{
		readonly IMauiContext _mauiContext;

		public bool IsDestroyed { get; private set; }
		public IView DetailView { get; private set; }

		public ScopedFragment(IView detailView, IMauiContext mauiContext)
		{
			DetailView = detailView;
			_mauiContext = mauiContext;
		}

		public override void OnViewStateRestored(Bundle? savedInstanceState)
		{
			// Fragments have the potential to "undestroy" if you reuse them
			IsDestroyed = false;
			base.OnViewStateRestored(savedInstanceState);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
		{
			// Fragments have the potential to "undestroy" if you reuse them
			IsDestroyed = false;
			var pageMauiContext = _mauiContext.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager);
			return DetailView.ToPlatform(pageMauiContext);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			IsDestroyed = true;
		}
	}
}
