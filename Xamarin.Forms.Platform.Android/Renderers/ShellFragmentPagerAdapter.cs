using Android.Database;
using Android.OS;
using Android.Support.V4.App;
using Java.Lang;
using System.Collections.Specialized;
using Xamarin.Forms.Platform.Android.AppCompat;

namespace Xamarin.Forms.Platform.Android
{
	internal class ShellFragmentPagerAdapter : FragmentPagerAdapter
	{
		bool _disposed;
		ShellSection _shellSection;

		public ShellFragmentPagerAdapter(ShellSection shellSection, FragmentManager fragmentManager) : base(fragmentManager)
		{
			_shellSection = shellSection;
			((INotifyCollectionChanged)shellSection.Items).CollectionChanged += OnItemsCollectionChanged;
		}

		protected virtual void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			NotifyDataSetChanged();
		}

		public int CountOverride { get; set; }
		public override int Count => _shellSection.Items.Count;

		public override Fragment GetItem(int position)
		{
			var shellContent = _shellSection.Items[position];
			return new ShellFragmentContainer(shellContent) { Arguments = Bundle.Empty };
		}

		public override long GetItemId(int position)
		{
			return _shellSection.Items[position].GetHashCode();
		}

#pragma warning disable RCS1168 // Parameter name differs from base name.
		public override int GetItemPosition(Object objectValue)
#pragma warning restore RCS1168 // Parameter name differs from base name.
		{
			var fragContainer = objectValue as ShellFragmentContainer;
			var shellContent = fragContainer?.ShellContentTab;
			if (shellContent != null)
			{
				int index = _shellSection.Items.IndexOf(shellContent);
				if (index >= 0)
					return index;
			}
			return PositionNone;
		}

		public override ICharSequence GetPageTitleFormatted(int position)
		{
			return new String(_shellSection.Items[position].Title);
		}

		// http://stackoverflow.com/questions/18642890/fragmentstatepageradapter-with-childfragmentmanager-fragmentmanagerimpl-getfra/19099987#19099987
		public override void RestoreState(IParcelable state, ClassLoader loader)
		{
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && !_disposed)
			{
				((INotifyCollectionChanged)_shellSection.Items).CollectionChanged -= OnItemsCollectionChanged;
				_shellSection = null;
				_disposed = true;

			}
		}
	}
}