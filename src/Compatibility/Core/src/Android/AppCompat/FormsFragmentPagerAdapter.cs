// error CS0618: 'FragmentStatePagerAdapter' is obsolete:
#pragma warning disable 618
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Android.OS;
using AndroidX.Fragment.App;
using Java.Lang;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using FragmentTransit = Android.App.FragmentTransit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat
{
	[System.Obsolete]
	internal class FormsFragmentPagerAdapter<[DynamicallyAccessedMembers(BindableProperty.DeclaringTypeMembers
#if NET8_0 // IL2091
	| BindableProperty.ReturnTypeMembers
#endif
	)] T> : FragmentPagerAdapter where T : Page
	{
		MultiPage<T> _page;
		FragmentManager _fragmentManager;
		List<Fragment> _fragments;
		bool _disposed;

		public FormsFragmentPagerAdapter(MultiPage<T> page, FragmentManager fragmentManager) : base(fragmentManager, FragmentStatePagerAdapter.BehaviorResumeOnlyCurrentFragment)
		{
			_page = page;
			_fragmentManager = fragmentManager;
			_fragments = new List<Fragment>();
		}

		public override int Count => CountOverride;

		public int CountOverride { get; set; }

		public override Fragment GetItem(int position)
		{
			var fragment = FragmentContainer.CreateInstance(_page.Children[position]);

			_fragments.Add(fragment);

			return fragment;
		}

		public override long GetItemId(int position)
		{
			return _page.Children[position].GetHashCode();
		}

		public override int GetItemPosition(Object objectValue)
		{
			var fragContainer = objectValue as FragmentContainer;

			if (fragContainer?.Page != null)
			{
				int index = _page.Children.IndexOf(fragContainer.Page);

				if (index >= 0)
					return index;
			}

			_fragments.Remove(fragContainer);

			return PositionNone;
		}

		public override ICharSequence GetPageTitleFormatted(int position)
		{
			return new String(_page.Children[position].Title);
		}

		// http://stackoverflow.com/questions/18642890/fragmentstatepageradapter-with-childfragmentmanager-fragmentmanagerimpl-getfra/19099987#19099987
		public override void RestoreState(IParcelable state, ClassLoader loader)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				_disposed = true;

				_page = null;

				if (!_fragmentManager.IsDestroyed(_page?.Handler?.MauiContext?.Context))
				{
					FragmentTransaction transaction = _fragmentManager.BeginTransactionEx();

					foreach (Fragment fragment in _fragments)
					{
						transaction.RemoveEx(fragment);
						transaction.SetTransitionEx((int)FragmentTransit.None);
					}

					transaction.CommitAllowingStateLossEx();

					_fragmentManager.ExecutePendingTransactionsEx();

					_fragments = null;
					_fragmentManager = null;
				}
			}

			base.Dispose(disposing);
		}
	}
}