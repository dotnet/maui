using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.DeviceTests
{
	public class CustomTestAdapterHandler : TabbedViewHandler
	{
		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			(PlatformView as ViewPager2).Adapter = new TestAdapter((TabbedPage)VirtualView);
		}

		class TestAdapter : FragmentStateAdapter
		{
			readonly TabbedPage _tabbedPage;

			public TestAdapter(TabbedPage tabbedPage)
				: base(tabbedPage.Handler.MauiContext.GetFragmentManager(), (tabbedPage.Handler.MauiContext.GetActivity() as FragmentActivity)!.Lifecycle)
			{
				_tabbedPage = tabbedPage;
			}

			public override void OnAttachedToRecyclerView(RecyclerView recyclerView)
			{
				base.OnAttachedToRecyclerView(recyclerView);
			}

			public override int ItemCount => _tabbedPage.Children.Count;

			public override Fragment CreateFragment(int p0)
			{
				return new ScopedFragment(_tabbedPage.Children[p0], _tabbedPage.FindMauiContext());
			}

			public override long GetItemId(int position)
			{
				return position;
			}

			public override bool ContainsItem(long position)
			{
				return true;
			}
		}
	}
}
