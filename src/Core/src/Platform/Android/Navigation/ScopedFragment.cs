using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.Platform
{
	class ScopedFragment : Fragment
	{
		readonly IMauiContext _mauiContext;

		public bool IsDestroyed { get; private set; }
		public IView? DetailView { get; private set; }

		public ScopedFragment(IView detailView, IMauiContext mauiContext)
		{
			DetailView = detailView;
			_mauiContext = mauiContext;
		}

		/// <summary>
		/// Explicitly disconnects the handler and clears the DetailView reference.
		/// This can be called proactively before the fragment is destroyed to allow
		/// the view to be garbage collected sooner.
		/// </summary>
		public void DisconnectDetailView()
		{
			if (DetailView?.Handler is IPlatformViewHandler pvh)
			{
				pvh.DisconnectHandler();
			}
			DetailView = null;
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
			return DetailView?.ToPlatform(pageMauiContext) ?? new View(inflater.Context!);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			IsDestroyed = true;

			// Disconnect the handler to allow the view to be garbage collected
			// (may have already been disconnected by DisconnectDetailView)
			if (DetailView?.Handler is IPlatformViewHandler pvh)
			{
				pvh.DisconnectHandler();
			}

			// Clear the reference to allow GC
			DetailView = null!;
		}
	}
}
