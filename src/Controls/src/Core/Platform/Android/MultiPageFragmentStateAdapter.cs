using System.Collections.Generic;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;
using FragmentTransit = Android.App.FragmentTransit;

namespace Microsoft.Maui.Controls.Platform
{
	internal class MultiPageFragmentStateAdapter<T> : FragmentStateAdapter where T : Page
	{
		MultiPage<T> _page;
		FragmentManager _fragmentManager;
		readonly IMauiContext _context;
		List<Fragment> _fragments;

		public MultiPageFragmentStateAdapter(
			MultiPage<T> page, FragmentManager fragmentManager, IMauiContext context) 
			: base(fragmentManager, (context.GetActivity() as AppCompatActivity).Lifecycle)
		{
			_page = page;
			_fragmentManager = fragmentManager;
			_context = context;
			_fragments = new List<Fragment>();
		}

		public override int ItemCount => CountOverride;

		public int CountOverride { get; set; }

		public override Fragment CreateFragment(int position)
		{
			var fragment = FragmentContainer.CreateInstance(_page.Children[position], _context);

			_fragments.Add(fragment);

			return fragment;
		}

		public override long GetItemId(int position)
		{
			return _page.Children[position].GetHashCode();
		}
	}
}