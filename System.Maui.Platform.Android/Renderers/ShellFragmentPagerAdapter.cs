using Android.Database;
using Android.OS;
#if __ANDROID_29__
using AndroidX.Fragment.App;
#else
using Android.Support.V4.App;
#endif
using Java.Lang;
using System.Collections.Specialized;
using Xamarin.Forms.Platform.Android.AppCompat;

namespace Xamarin.Forms.Platform.Android
{
	internal class ShellFragmentPagerAdapter : FragmentPagerAdapter
	{
		bool _disposed;
		ShellSection _shellSection;
		IShellSectionController SectionController => (IShellSectionController)_shellSection;
		
#if __ANDROID_29__
		public ShellFragmentPagerAdapter(ShellSection shellSection, FragmentManager fragmentManager) : base(fragmentManager, FragmentStatePagerAdapter.BehaviorResumeOnlyCurrentFragment)
#else
		public ShellFragmentPagerAdapter(ShellSection shellSection, FragmentManager fragmentManager) : base(fragmentManager)
#endif
		{
			_shellSection = shellSection;
			SectionController.ItemsCollectionChanged += OnItemsCollectionChanged;
		}

		protected virtual void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			NotifyDataSetChanged();
		}

		public int CountOverride { get; set; }
		public override int Count => SectionController.GetItems().Count;

		public override Fragment GetItem(int position)
		{
			var shellContent = SectionController.GetItems()[position];
			return new ShellFragmentContainer(shellContent) { Arguments = Bundle.Empty };
		}

		public override long GetItemId(int position)
		{
			return SectionController.GetItems()[position].GetHashCode();
		}

#pragma warning disable RCS1168 // Parameter name differs from base name.
		public override int GetItemPosition(Object objectValue)
#pragma warning restore RCS1168 // Parameter name differs from base name.
		{
			var fragContainer = objectValue as ShellFragmentContainer;
			var shellContent = fragContainer?.ShellContentTab;
			if (shellContent != null)
			{
				int index = SectionController.GetItems().IndexOf(shellContent);
				if (index >= 0)
					return index;
			}
			return PositionNone;
		}

		public override ICharSequence GetPageTitleFormatted(int position)
		{
			return new String(SectionController.GetItems()[position].Title);
		}

		// http://stackoverflow.com/questions/18642890/fragmentstatepageradapter-with-childfragmentmanager-fragmentmanagerimpl-getfra/19099987#19099987
		public override void RestoreState(IParcelable state, ClassLoader loader)
		{
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
			}

			base.Dispose(disposing);
		}
	}
}
