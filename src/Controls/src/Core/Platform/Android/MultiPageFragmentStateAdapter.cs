using System.Collections.Generic;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;

namespace Microsoft.Maui.Controls.Platform
{
	internal class MultiPageFragmentStateAdapter<T> : FragmentStateAdapter where T : Page
	{
		MultiPage<T> _page;
		private int _countOverride;
		readonly IMauiContext _context;
		public Dictionary<int, FragmentContainer> Fragments { get; }

		public MultiPageFragmentStateAdapter(
			MultiPage<T> page, FragmentManager fragmentManager, IMauiContext context)
			: base(fragmentManager, context.GetActivity().Lifecycle)
		{
			_page = page;
			_context = context;
			Fragments = new Dictionary<int, FragmentContainer>();
		}

		public override int ItemCount => CountOverride;

		public int CountOverride
		{
			get => _countOverride;
			set
			{
				_countOverride = value;
				Fragments.Clear();
			}
		}

		public override Fragment CreateFragment(int position)
		{
			var fragment = FragmentContainer.CreateInstance(_page.Children[position], _context);

			Fragments[position] = fragment;

			return fragment;
		}

		public override long GetItemId(int position)
		{
			return _page.Children[position].GetHashCode();
		}
	}
}