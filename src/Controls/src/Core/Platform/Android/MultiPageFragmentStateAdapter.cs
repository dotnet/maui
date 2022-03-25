using System.Diagnostics.CodeAnalysis;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;

namespace Microsoft.Maui.Controls.Platform
{
	internal class MultiPageFragmentStateAdapter<[DynamicallyAccessedMembers(BindableProperty.DeclaringTypeMembers)] T> : FragmentStateAdapter where T : Page
	{
		MultiPage<T> _page;
		readonly IMauiContext _context;

		public MultiPageFragmentStateAdapter(
			MultiPage<T> page, FragmentManager fragmentManager, IMauiContext context)
			: base(fragmentManager, context.GetActivity().Lifecycle)
		{
			_page = page;
			_context = context;
		}

		public override int ItemCount => CountOverride;

		public int CountOverride { get; set; }

		public override Fragment CreateFragment(int position)
		{
			var fragment = FragmentContainer.CreateInstance(_page.Children[position], _context);
			return fragment;
		}

		public override long GetItemId(int position)
		{
			return _page.Children[position].GetHashCode();
		}
	}
}