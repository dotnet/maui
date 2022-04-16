// error CS0618: 'FragmentStatePagerAdapter' is obsolete:
#pragma warning disable 618
using System.Collections.Generic;
using System.Collections.Specialized;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;
using Java.Lang;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	internal class ShellFragmentStateAdapter : FragmentStateAdapter
	{
		bool _disposed;
		ShellSection _shellSection;
		IShellSectionController SectionController => (IShellSectionController)_shellSection;
		IMauiContext _mauiContext;
		IList<ShellContent> _items;
		Dictionary<long, ShellContent> _createdShellContent = new Dictionary<long, ShellContent>();
		long _id;

		public ShellFragmentStateAdapter(
			ShellSection shellSection,
			AndroidX.Fragment.App.FragmentManager fragmentManager,
			IMauiContext mauiContext) : base(fragmentManager, (mauiContext.Context.GetActivity() as AppCompatActivity).Lifecycle)
		{
			_mauiContext = mauiContext;
			_shellSection = shellSection;
			SectionController.ItemsCollectionChanged += OnItemsCollectionChanged;
			_items = SectionController.GetItems();
		}

		protected virtual void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_items = SectionController.GetItems();
			var removeList = new List<long>();

			foreach (var created in _createdShellContent)
				if (!_items.Contains(created.Value))
					removeList.Add(created.Key);

			foreach (var remove in removeList)
				_createdShellContent.Remove(remove);

			NotifyDataSetChanged();
		}

		public int CountOverride { get; set; }

		public override int ItemCount => _items.Count;

		public override Fragment CreateFragment(int position)
		{
			var shellContent = _items[position];
			return new ShellFragmentContainer(shellContent, _mauiContext) { Arguments = Bundle.Empty };
		}

		public override long GetItemId(int position)
		{
			var shellContent = _items[position];
			foreach (var item in _createdShellContent)
				if (item.Value == shellContent)
					return item.Key;

			var id = _id;
			_createdShellContent.Add(_id++, shellContent);
			return id;
		}

		public override bool ContainsItem(long itemId)
		{
			return _createdShellContent.ContainsKey(itemId);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				SectionController.ItemsCollectionChanged -= OnItemsCollectionChanged;
				_shellSection = null;

				_items = null;

				_createdShellContent?.Clear();
				_createdShellContent = null;
			}

			base.Dispose(disposing);
		}
	}
}
