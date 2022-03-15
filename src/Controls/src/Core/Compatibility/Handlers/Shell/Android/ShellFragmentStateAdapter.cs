// error CS0618: 'FragmentStatePagerAdapter' is obsolete:
#pragma warning disable 618
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

		public ShellFragmentStateAdapter(
			ShellSection shellSection,
			AndroidX.Fragment.App.FragmentManager fragmentManager,
			IMauiContext mauiContext) : base(fragmentManager, (mauiContext.Context.GetActivity() as AppCompatActivity).Lifecycle)
		{
			_mauiContext = mauiContext;
			_shellSection = shellSection;
			SectionController.ItemsCollectionChanged += OnItemsCollectionChanged;
		}

		protected virtual void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			NotifyDataSetChanged();
		}

		public int CountOverride { get; set; }

		public override int ItemCount => SectionController.GetItems().Count;

		public override Fragment CreateFragment(int position)
		{
			var shellContent = SectionController.GetItems()[position];
			return new ShellFragmentContainer(shellContent, _mauiContext) { Arguments = Bundle.Empty };
		}

		public override long GetItemId(int position)
		{
			return SectionController.GetItems()[position].GetHashCode();
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
