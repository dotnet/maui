#nullable disable
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
	internal sealed class ShellFragmentStateAdapter : FragmentStateAdapter
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
			_items = SectionController.GetItems();
		}

		public void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_items = SectionController.GetItems();
			var removeList = new List<long>();

			foreach (var created in _createdShellContent)
				if (!_items.Contains(created.Value))
					removeList.Add(created.Key);

			foreach (var remove in removeList)
				_createdShellContent.Remove(remove);
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
			if (_createdShellContent.TryGetValue(itemId, out var shellContent) &&
				!_items.Contains(shellContent))
			{
				// This means a data set change was triggered but the INCC change hasn't
				// propagated from our xplat code to here yet
				_createdShellContent.Remove(itemId);
			}

			return _createdShellContent.ContainsKey(itemId);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				_shellSection = null;

				_items = null;

				_createdShellContent?.Clear();
				_createdShellContent = null;
			}

			base.Dispose(disposing);
		}
	}
}
